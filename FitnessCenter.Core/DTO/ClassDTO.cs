namespace FitnessCenterr.Core.DTOs.Classes;

public class ClassDto
{
    public int ClassID { get; set; }
    public string Name { get; set; } = string.Empty;
    public int TrainerID { get; set; }
    public string? TrainerName { get; set; }
    public DateTime ClassDate { get; set; }
    public int? HallID { get; set; }
    public string? HallName { get; set; }
    public int? LocationID { get; set; }
    public string? City { get; set; }
    public int BookingCount { get; set; }
}

public class CreateClassDto
{
    public string Name { get; set; } = string.Empty;
    public int TrainerID { get; set; }
    public DateTime ClassDate { get; set; }
    public int? HallID { get; set; }
    public int? LocationID { get; set; }
}

public class UpdateClassDto
{
    public string Name { get; set; } = string.Empty;
    public int TrainerID { get; set; }
    public DateTime ClassDate { get; set; }
    public int? HallID { get; set; }
    public int? LocationID { get; set; }
}