namespace FitnessCenterr.Core.Models;

public class Class
{
    public int ClassID { get; set; }
    public string Name { get; set; } = string.Empty;
    public int TrainerID { get; set; }
    public int? HallID { get; set; }
    public int? LocationID { get; set; }
    public DateTime ClassDate { get; set; }
    public string? AiDescription { get; set; }          // AI-generated class description
    public DateTime? AiDescriptionGeneratedAt { get; set; }

    public Trainer Trainer { get; set; } = null!;
    public Hall? Hall { get; set; }
    public Location? Location { get; set; }
    public ICollection<ClassBooking> ClassBookings { get; set; } = new List<ClassBooking>();
}
