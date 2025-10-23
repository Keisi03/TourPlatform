using System;
using System.Collections.Generic;

namespace TourPlatform.Domain.Entities;

public partial class Season
{
    public int Id { get; set; }

    public string Seasoncode { get; set; } = null!;

    public DateOnly Startdate { get; set; }

    public DateOnly Enddate { get; set; }

    public virtual ICollection<Pricingrecord> Pricingrecords { get; set; } = new List<Pricingrecord>();
}
