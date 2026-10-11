using Microsoft.Extensions.DependencyInjection;
using Trainee_Tracker.Core.Service;

namespace Trainee_Tracker.Service;

// Author: Marin Achner

/// <summary>
/// Service responsible for synchronizing trainees' working hours
/// from the external time tracking API into the local repository.
/// </summary>
public class DateTimeTrackingService : IDateTimeTrackingService {
    private readonly IServiceScopeFactory _scopeFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="DateTimeTrackingService"/> class.
    /// </summary>
    /// <param name="scopeFactory">
    /// Factory used to create a new dependency injection scope for each update operation.
    /// </param>
    public DateTimeTrackingService(IServiceScopeFactory scopeFactory) {
        _scopeFactory = scopeFactory;
    }

    /// <summary>
    /// Updates the stored working hours for all trainees.
    /// </summary>
    /// <remarks>
    /// For each trainee, the method retrieves the current working hours from the
    /// external time tracking API and stores the returned entries in the repository.
    /// If retrieving the working hours for a trainee fails, the exception is ignored
    /// and processing continues with the next trainee.
    /// </remarks>
    /// <returns>A task representing the asynchronous update operation.</returns>
    public async Task UpdateWorkingtime() {
        using var scope = _scopeFactory.CreateAsyncScope();

        IDateTimeTrackingApiClient dateTimeTrackingApiClient = scope.ServiceProvider.GetRequiredService<IDateTimeTrackingApiClient>();
        ITraineeService traineeService = scope.ServiceProvider.GetRequiredService<ITraineeService>();
        IWorkingHoursRepository workingHoursRepository = scope.ServiceProvider.GetRequiredService<IWorkingHoursRepository>();

        var trainees = traineeService.GetAllTrainees();

        foreach (var trainee in trainees) {
            try {
                var result = await dateTimeTrackingApiClient.GetWorkinghours(
                    trainee.Email,
                    trainee.startDate,
                    trainee.endDate
                );

                await workingHoursRepository.UpdateWorkingHours(
                    Guid.Parse(trainee.Id),
                    result.entries
                );
            }
            catch {
                continue;
            }
        }
    }
}