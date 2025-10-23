using CsvHelper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Formats.Asn1;
using System.Globalization;
using System.Threading.Channels;
using TourPlatform.Application.Contracts;
using TourPlatform.Application.Dtos.FileProcess;
using TourPlatform.Application.Settings;
using TourPlatform.Domain.Entities;
using TourPlatform.Infrastructure.Entities;
using TourPlatform.Infrastructure.Hubs;
using TourPlatform.Infrastructure.Repositories;

namespace TourPlatform.Application.Concrete;

public class FileProcessorService : IFileProcessorService
{
    private readonly AppDbContext _db;
    private readonly IHubContext<UploadProgressHub> _hub;
    private readonly IPricingRepository _pricingRepo;
    private readonly ILogger<FileProcessorService> _logger;
    private readonly BulkUploadSettings _settings;
    private readonly IRouteRepository _routeRepository;
    private readonly ISeasonRepository _seasonRepository;
    private readonly IPricingRepository _pricingRepository;

    public FileProcessorService(AppDbContext db,
                                        IHubContext<UploadProgressHub> hub,
                                        IPricingRepository pricingRepo,
                                        IOptions<BulkUploadSettings> settings,
                                        IRouteRepository routeRepository,
                                        ISeasonRepository seasonRepository,
                                        IPricingRepository pricingRepository,
                                        ILogger<FileProcessorService> log)
    {
        _db = db;
        _hub = hub;
        _pricingRepo = pricingRepo;
        _settings = settings.Value;
        _routeRepository = routeRepository;
        _seasonRepository = seasonRepository;
        _pricingRepository = pricingRepository;
        _logger = log;
    }

    public async Task ProcessCsvAsync(Stream file, int tourOperatorId, string connectionId, CancellationToken ct)
    {
        var upload = await CreateUploadHistoryAsync(tourOperatorId, ct);
        var progressInterval = TimeSpan.FromSeconds(_settings.ProgressIntervalSeconds);

        try
        {
            var channel = Channel.CreateBounded<Pricingrecord>(new BoundedChannelOptions(20000)
            {
                SingleReader = false,
                SingleWriter = true
            });
            

            var initialState = (TotalProcessed: 0, TotalInserted: 0, ErrorCount: 0, LastProgress: DateTime.UtcNow);
            int userId = await _db.Users
                        .Where(u => u.Touroperatorid == tourOperatorId)
                        .Select(u => u.Id)
                        .FirstOrDefaultAsync(ct);

            var workers = Enumerable.Range(0, _settings.MaxWorkers)
                .Select(_ => Task.Run(() => ProcessBatchesAsync(channel, connectionId, upload, userId, progressInterval, initialState, ct)))
                .ToList();

            await _hub.Clients.Client(connectionId)
                .SendAsync("UploadProgress", new { message = "Validation started..." }, ct);

            var csvState = await ReadCsvAsync(file, tourOperatorId, channel, connectionId, (initialState.TotalProcessed, initialState.ErrorCount), ct);
            channel.Writer.Complete();

            var results = await Task.WhenAll(workers);

            var finalState = results.Aggregate(
                (TotalProcessed: csvState.TotalProcessed, TotalInserted: 0, ErrorCount: csvState.ErrorCount, LastProgress: DateTime.UtcNow),
                (acc, t) => (
                    TotalProcessed: acc.TotalProcessed + t.TotalProcessed,
                    TotalInserted: acc.TotalInserted + t.TotalInserted,
                    ErrorCount: acc.ErrorCount + t.ErrorCount,
                    LastProgress: t.LastProgress > acc.LastProgress ? t.LastProgress : acc.LastProgress
                )
            );

            await FinalizeUploadAsync(upload, finalState.TotalProcessed, finalState.TotalInserted, finalState.ErrorCount, connectionId, ct);
        }
        catch (Exception ex)
        {
            await HandleUploadErrorAsync(upload, ex, connectionId, ct);
        }
    }

    private async Task<Uploadhistory> CreateUploadHistoryAsync(int tourOperatorId, CancellationToken ct)
    {
        var upload = new Uploadhistory
        {
            Filename = $"upload_{DateTime.UtcNow:yyyyMMddHHmmss}.csv",
            Status = "Processing",
            Touroperatorid = tourOperatorId,
            Uploadedat = DateTime.Now
        };
        _db.Uploadhistories.Add(upload);
        await _db.SaveChangesAsync(ct);
        return upload;
    }

    private async Task<(int TotalProcessed, int ErrorCount)> ReadCsvAsync(
        Stream file,
        int tourOperatorId,
        Channel<Pricingrecord> channel,
        string connectionId,
        (int TotalProcessed, int ErrorCount) state,
        CancellationToken ct)
    {
        using var reader = new StreamReader(file);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        await csv.ReadAsync();
        csv.ReadHeader();

        while (await csv.ReadAsync())
        {
            try
            {
                var routeCode = csv.GetField("RouteCode");
                var seasonCode = csv.GetField("SeasonCode");

                var routeId = await _routeRepository.GetOrCreateAsync(routeCode, tourOperatorId, ct);
                var seasonId = await _seasonRepository.GetOrCreateAsync(seasonCode, ct);

                var record = new Pricingrecord
                {
                    Routeid = routeId,
                    Seasonid = seasonId,
                    Recorddate = DateOnly.Parse(csv.GetField("Date")),
                    Economyprice = csv.GetField<decimal>("EconomyPrice"),
                    Businessprice = csv.GetField<decimal>("BusinessPrice"),
                    Economyseats = csv.GetField<int>("EconomySeats"),
                    Businessseats = csv.GetField<int>("BusinessSeats"),
                    Uploadedat = DateTime.Now,
                    Uploadedby = tourOperatorId
                };

                await channel.Writer.WriteAsync(record, ct);

                state.TotalProcessed++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing CSV row");
                state.ErrorCount++;
            }
        }

        return state;
    }

    private async Task<(int TotalProcessed, int TotalInserted, int ErrorCount, DateTime LastProgress)> ProcessBatchesAsync(
        Channel<Pricingrecord> channel,
        string connectionId,
        Uploadhistory upload,
        int userId,
        TimeSpan progressInterval,
        (int TotalProcessed, int TotalInserted, int ErrorCount, DateTime LastProgress) state,
        CancellationToken ct)
    {
        var batch = new List<Pricingrecord>(_settings.BatchSize);

        await foreach (var record in channel.Reader.ReadAllAsync(ct))
        {
            record.Uploadedby = userId;
            batch.Add(record);

            if (batch.Count >= _settings.BatchSize)
            {
                state = await InsertBatchAsync(batch, connectionId, state, ct);
                batch.Clear();
            }

            if (connectionId != null && DateTime.Now - state.LastProgress > progressInterval)
            {
                state.LastProgress = DateTime.Now;
                await SendProgressAsync(connectionId, state.TotalProcessed, state.TotalInserted, state.ErrorCount, "Processing...");
            }
        }

        if (batch.Count > 0)
            state = await InsertBatchAsync(batch, connectionId, state, ct);

        return state;
    }

    private async Task<(int TotalProcessed, int TotalInserted, int ErrorCount, DateTime LastProgress)> InsertBatchAsync(
        List<Pricingrecord> batch,
        string connectionId,
        (int TotalProcessed, int TotalInserted, int ErrorCount, DateTime LastProgress) state,
        CancellationToken ct)
    {
        await _pricingRepository.BulkInsertAsync(batch, ct);
        state.TotalInserted += batch.Count;
        return state;
    }

    private async Task SendProgressAsync(string connectionId, int processed, int inserted, int errors, string message)
    {
        await _hub.Clients.Client(connectionId).SendAsync("UploadProgress", new
        {
            processed,
            inserted,
            errors,
            message
        });
    }

    private async Task FinalizeUploadAsync(Uploadhistory upload, int processed, int inserted, int errors, string connectionId, CancellationToken ct)
    {
        upload.Status = errors > 0 ? "CompletedWithErrors" : "Completed";
        upload.Totalrows = processed;
        await _db.SaveChangesAsync(ct);

        await _hub.Clients.Client(connectionId).SendAsync("UploadCompleted", new
        {
            processed,
            inserted,
            errors,
            message = "Upload completed successfully."
        });
    }

    private async Task HandleUploadErrorAsync(Uploadhistory upload, Exception ex, string connectionId, CancellationToken ct)
    {
        upload.Status = "Failed";
        await _db.SaveChangesAsync(ct);

        _logger.LogError(ex, "CSV upload failed");
        await _hub.Clients.Client(connectionId).SendAsync("UploadFailed", new
        {
            message = ex.Message
        }, ct);
    }

}
