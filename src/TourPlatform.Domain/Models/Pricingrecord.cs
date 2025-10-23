using System;
using System.Collections.Generic;

namespace TourPlatform.Domain.Entities;

public partial class Pricingrecord
{
    public long Id { get; set; }

    public int Routeid { get; set; }

    public int Seasonid { get; set; }

    public DateOnly Recorddate { get; set; }

    public decimal? Economyprice { get; set; }

    public decimal? Businessprice { get; set; }

    public int? Economyseats { get; set; }

    public int? Businessseats { get; set; }

    public DateTime? Uploadedat { get; set; }

    public int Uploadedby { get; set; }

    public virtual Route Route { get; set; } = null!;

    public virtual Season Season { get; set; } = null!;

    public virtual User UploadedbyNavigation { get; set; } = null!;
}
