using System;
using System.Collections.Generic;

namespace TourPlatform.Domain.Entities;

public partial class Route
{
    public int Id { get; set; }

    public int Touroperatorid { get; set; }

    public string Routecode { get; set; } = null!;

    public string? Origin { get; set; }

    public string? Destination { get; set; }

    public virtual ICollection<Pricingrecord> Pricingrecords { get; set; } = new List<Pricingrecord>();

    public virtual Touroperator Touroperator { get; set; } = null!;
}
