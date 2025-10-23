using System;
using System.Collections.Generic;

namespace TourPlatform.Domain.Entities;

public partial class Touroperator
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Contactemail { get; set; }

    public virtual ICollection<Route> Routes { get; set; } = new List<Route>();

    public virtual ICollection<Uploadhistory> Uploadhistories { get; set; } = new List<Uploadhistory>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
