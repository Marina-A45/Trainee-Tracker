using Trainee_Tracker.Core.Entity;
using Trainee_Tracker.Core.Enums;
using Trainee_Tracker.Core.Service;
using Trainee_Tracker.Repository;
using Microsoft.EntityFrameworkCore;
using Trainee_Tracker.Core.Repository;

namespace Trainee_Tracker.Service;

/// <summary>
/// Author: Tim Glaser
/// Service class for calculating and saving trainee progress based on lesson progress and time tracking data.
/// </summary>
public class TraineeProgressService : ITraineeProgressService {
    // Repositories and API client used to access trainee progress, lesson progress, trainee data, and time tracking data
    private readonly ITraineeProgressRepository _traineeProgressRepository;
    private readonly ILessonProgressRepository _lessonProgressRepository;
    private readonly ITraineeRepository _traineeRepository;
    private readonly IDateTimeTrackingApiClient _timeTrackingApiClient;
    private readonly IWorkingHoursRepository _workingHoursRepository;
    // Initializes the service with the required repositories and API client
    public TraineeProgressService(ITraineeProgressRepository traineeProgressRepository, ILessonProgressRepository lessonProgressRepository, ITraineeRepository traineeRepository, IDateTimeTrackingApiClient timeTrackingApiClient, IWorkingHoursRepository workingHoursRepository) {
        _traineeProgressRepository = traineeProgressRepository;
        _lessonProgressRepository = lessonProgressRepository;
        _traineeRepository = traineeRepository;
        _timeTrackingApiClient = timeTrackingApiClient;
        _workingHoursRepository = workingHoursRepository;
    }

    /// <summary>
    /// Calculates the current progress metrics for a trainee and saves them to the database.
    /// The calculation compares weighted completed lesson work against recorded attendance in workdays.
    /// </summary>
    /// <param name="traineeId">The unique identifier of the trainee.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous save operation.</param>
    /// <returns>The created or updated trainee progress record.</returns>
    /// <exception cref="Exception">Thrown when the trainee cannot be found or is not a trainee.</exception>
    public async Task<TraineeProgress> calcAndSaveProgress(Guid traineeId, CancellationToken cancellationToken = default) {
        // Trainee check
        var trainee = _traineeRepository.GetTraineeById(traineeId);

        if (trainee == null || trainee.role != Role.Trainee) {
            throw new Exception("Trainee not found");
        }
        // get working hours from time tracking API or from database cache if API is unavailable
        var responseDto = await GetWorkingHours(traineeId, trainee.Email, trainee.startDate, DateOnly.FromDateTime(DateTime.Now));

        double attendance = responseDto.Sum(e => e.working_hours) / 8.0;

        // Lessonprogress laden
        var lessons = _lessonProgressRepository.GetAll(traineeId).ToList();
        
        // Calculate progress 
        double weightedProgressEstimate = calculateWeightedProgressEstimate(lessons);
        double openWork = calculateOpenWork(lessons, weightedProgressEstimate);
        double buffer = calculateBuffer(weightedProgressEstimate, attendance);
        double workSpeed = calculateWorkSpeed(weightedProgressEstimate, attendance);
        double estimatedBuffer = calculateEstimatedBuffer(buffer, openWork, workSpeed);

        var traineeProgress = new TraineeProgress {
            TraineeId = traineeId,
            Trainee = trainee,
            WorkSpeed = (decimal)workSpeed,
            Buffer = (decimal)buffer,
            EstimatedBuffer = (decimal)estimatedBuffer,
            OpenWork = (decimal)openWork,
            WeightedProgressEstimate = (decimal)weightedProgressEstimate,
        };

        return await _traineeProgressRepository.UpsertAsync(traineeProgress, cancellationToken);
    }    

    public Task<TraineeProgress?> GetTraineeProgressByTraineeId(Guid traineeId, CancellationToken cancellationToken = default) {
        return _traineeProgressRepository.GetByTraineeIdAsync(traineeId, cancellationToken);
    }
    /// By Tim Glaser
    /// <summary>
    /// Retrieves the working hours for a specific trainee within a date range.
    /// </summary>
    public  async Task<List<WorkingHoursEntryDto>> GetWorkingHours(Guid traineeId, string email, DateOnly startDate, DateOnly endDate) {
        try {
            var result = await _timeTrackingApiClient.GetWorkinghours(email, startDate, endDate);
            // If the API returns no data, fallback to the latest cached working hours from the database
            if (result == null || result.entries.Count == 0 || result.entries == null) {
                var cached = await _workingHoursRepository.GetLatestWorkingHoursByTraineeId(traineeId);
                // Return the cached data or an empty list if no cached data is available
                return cached ?? new List<WorkingHoursEntryDto>();

            } 
            // Store the successful API response in the database as fallback data
            await _workingHoursRepository.UpdateWorkingHours(
                traineeId,
                result.entries
            );

            // Filter out entries after endDate (entry.date is a string)
            var filtered = result.entries.Where(e => {
                if (DateOnly.TryParse(e.date, out var d)) {
                    return d <= endDate;
                }
                // if parsing fails, keep the entry
                return true;
            }).ToList();

            return filtered;
        
    }
    catch {
            // API is unavailable (e.g. timeout, offline, server error)
            // Load the latest stored working hours from database cache    
            var cached = await _workingHoursRepository.GetLatestWorkingHoursByTraineeId(
                traineeId
            );

            return cached ?? new List<WorkingHoursEntryDto>();
        }
    }

    /// By Tim Glaser
    /// <summary>
    /// Returns % Speed of done work divided by worked hours
    /// </summary>
    public double calculateWorkSpeed(double weightedProgress, double attendance) {
        if(attendance == 0) {
            return 0;
        }
        return (weightedProgress / attendance) * 100;
    }

    /// By Tim Glaser
    /// <summary>
    /// Returns estimated time buffer based on current speed and remaining lessons estimated time effort
    /// </summary>
    public double calculateEstimatedBuffer(double buffer, double openWork, double workSpeed) {
        if(workSpeed == 0) {
            return buffer;
        }
        double workSpeedFactor = workSpeed / 100.0;
        double estimatedActualDaysForOpenWork = openWork / workSpeedFactor;
        return buffer + openWork - estimatedActualDaysForOpenWork;
    }

    /// By Tim Glaser
    /// <summary>
    /// Returns the estimated time of every open lesson for a trainee
    /// </summary>
    public double calculateOpenWork(IEnumerable<LessonProgress> lessons, double weightedProgress) {
        double totalEstimate = lessons.Sum(x => x.timeEstimate);
        return totalEstimate - weightedProgress;
    }

    /// By Tim Glaser
    /// <summary>
    /// Returns how much faster the trainee is than the estimated time effort for the finished Lessons
    /// </summary>
    public double calculateBuffer(double weightedProgress, double attendance) {
        return weightedProgress - attendance;
    }

    /// By Tim Glaser
    /// <summary>
    /// Returns Estimated lesson effort multiplied with the status-dependent weight factor for every lesson
    /// </summary>
    public double calculateWeightedProgressEstimate(IEnumerable<LessonProgress> lessons) {
    double estimate = 0;

    foreach (var lesson in lessons) {
        double factor = lesson.status switch {

            LessonStatus.Open => 0,
            LessonStatus.Started => 0,
            LessonStatus.Finished => 0.7,
            LessonStatus.Accepted => 1.0,
            LessonStatus.Rejected => 0.8,
            LessonStatus.Rated => 1.0,
            _ => 0
        };

        estimate += lesson.timeEstimate * factor;
    }

    return estimate;
    }

    
}
