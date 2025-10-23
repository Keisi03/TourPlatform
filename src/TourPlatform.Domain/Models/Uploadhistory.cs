using System;
using System.Collections.Generic;

namespace TourPlatform.Domain.Entities;

public partial class Uploadhistory
{
    public long Id { get; set; }

    public int Touroperatorid { get; set; }

    public string? Filename { get; set; }

    public DateTime? Uploadedat { get; set; }

    public int? Totalrows { get; set; }

    public string? Status { get; set; }

    public string? Logpath { get; set; }

    public virtual Touroperator Touroperator { get; set; } = null!;
}
