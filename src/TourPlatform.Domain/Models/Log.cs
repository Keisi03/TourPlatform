using System;
using System.Collections.Generic;

namespace TourPlatform.Domain.Entities;

public partial class Log
{
    public int Id { get; set; }

    public DateTime Timestamp { get; set; }

    public string Level { get; set; } = null!;

    public string Message { get; set; } = null!;

    public string? Messagetemplate { get; set; }

    public string? Exception { get; set; }

    public string? Properties { get; set; }
}
