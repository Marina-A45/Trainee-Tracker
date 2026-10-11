using Trainee_Tracker.Core.Entity;

namespace Trainee_Tracker.Core.Service;

public interface IDateTimeTrackingApiClient
{
    // Calls the external time tracking API and returns working hours for a given user and time range
    public Task<WorkingHoursResponseDto?> GetWorkinghours(string email, DateOnly startDate, DateOnly endDate);
}