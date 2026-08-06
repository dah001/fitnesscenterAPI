namespace FitnessCenterr.Core.Models;

public class Trainer
{
    public int TrainerID { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? AiBio { get; set; }          // AI-generated biography
    public DateTime? AiBioGeneratedAt { get; set; }

    public ICollection<Member> Members { get; set; } = new List<Member>();
    public ICollection<Class> Classes { get; set; } = new List<Class>();
}
