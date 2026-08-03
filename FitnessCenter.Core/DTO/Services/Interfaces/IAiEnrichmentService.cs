namespace FitnessCenterr.Core.Services.Interfaces;

public interface IAiEnrichmentService
{
    Task EnrichTrainerBioAsync(int trainerId);
    Task EnrichClassDescriptionAsync(int classId);
    Task EnrichAllTrainersAsync();
    Task EnrichAllClassesAsync();
}
