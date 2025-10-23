namespace TourPlatform.Domain.Entities;

public partial class User
{
    public int Id { get; set; }

    public string Username { get; set; } = null!;

    public byte[] Passwordhash { get; set; } = null!;

    public string Role { get; set; } = null!;

    public int? Touroperatorid { get; set; }

    public virtual ICollection<Pricingrecord> Pricingrecords { get; set; } = new List<Pricingrecord>();

    public virtual Touroperator? Touroperator { get; set; }
}
