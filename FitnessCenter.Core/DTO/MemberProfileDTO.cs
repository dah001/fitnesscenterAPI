namespace FitnessCenter.Core.DTO;

public class MemberProfileDTO
{
    public int       MemberID          { get; set; }
    public string    Name              { get; set; } = string.Empty;
    public string?   Email             { get; set; }
    public DateTime? BirthDate         { get; set; }
    public string?   TrainerName       { get; set; }
    public string?   SubscriptionType  { get; set; }
    public decimal?  SubscriptionPrice { get; set; }
    public DateTime? MemberSince       { get; set; }
}