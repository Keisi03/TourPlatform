using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Sinks.PostgreSQL;
using TourPlatform.Api;
using TourPlatform.Infrastructure;
using TourPlatform.Infrastructure.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.AddApplication(builder.Configuration);

var columnWriters = new Dictionary<string, ColumnWriterBase>
{
    { "Timestamp", new TimestampColumnWriter() },
    { "Level", new LevelColumnWriter(true, NpgsqlTypes.NpgsqlDbType.Varchar) },
    { "Message", new MessageTemplateColumnWriter() },
    { "MessageTemplate", new MessageTemplateColumnWriter() },
    { "Exception", new ExceptionColumnWriter() },
    { "Properties", new PropertiesColumnWriter(NpgsqlTypes.NpgsqlDbType.Jsonb) }
};

// Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.PostgreSQL(
        connectionString: builder.Configuration.GetConnectionString("DatabaseConnection"),
        tableName: "Logs",
        columnOptions: columnWriters,
        needAutoCreateTable: false 
    )
    .CreateLogger();

builder.Host.UseSerilog();
builder.Services.AddSignalR();


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http, 
        Scheme = "bearer",                                        
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter 'Bearer' [space] and then your valid token."
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
}); 

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Tour Platform API v1");
        c.RoutePrefix = string.Empty; 
    });
}

app.UseAuthentication();
app.UseAuthorization();

app.MapHub<UploadProgressHub>("/hubs/uploadProgress");
app.MapControllers();
app.Run();
