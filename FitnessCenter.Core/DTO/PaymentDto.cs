namespace FitnessCenterr.Core.DTOs.Payments;

public class PaymentDto
{
    public int PaymentID { get; set; }
    public int MemberID { get; set; }
    public string MemberName { get; set; } = string.Empty;
    public decimal? Amount { get; set; }
    public DateTime? PaymentDate { get; set; }
    public string? PaymentType { get; set; }
}

public class CreatePaymentDto
{
    public int MemberID { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public string? PaymentType { get; set; }
}
