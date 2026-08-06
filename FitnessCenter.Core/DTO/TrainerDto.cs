namespace FitnessCenterr.Core.DTOs.Trainers;

public class TrainerDto
{
    public int TrainerID { get; set; }
    public string Name { get; set; } = string.Empty;
    public int ClassCount { get; set; }
    public int MemberCount { get; set; }
}

public class CreateTrainerDto
{
    public string Name { get; set; } = string.Empty;
}

public class UpdateTrainerDto
{
    public string Name { get; set; } = string.Empty;
}