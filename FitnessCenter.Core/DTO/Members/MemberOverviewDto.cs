namespace FitnessCenterr.Core.DTOs.Members;

public class MemberOverviewDto
{
    public int       MemberID          { get; set; }
    public string    MemberName        { get; set; } = string.Empty;
    public string?   Email             { get; set; }
    public DateTime? BirthDate         { get; set; }
    public string?   TrainerName       { get; set; }
    public string?   SubscriptionType  { get; set; }
    public decimal?  SubscriptionPrice { get; set; }
    public DateTime? MemberSince       { get; set; }
    public int       TotalBookings     { get; set; }
    public decimal?  TotalPaid         { get; set; }
}
