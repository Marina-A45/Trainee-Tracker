using Moq;
using Trainee_Tracker.Service;
using Trainee_Tracker.Core.Entity;
using Trainee_Tracker.Core.Service;
using Trainee_Tracker.Core.Repository;
using Trainee_Tracker.Core.Enums;

namespace Trainee_Tracker.UnitTests;

/// <summary>
/// Tests the Functionality of the TraineeProgressService
/// Author: Lukas Nowak
/// </summary>
public class TraineeProgressServiceTests
{
    private readonly Mock<ITraineeProgressRepository> mockProgressRepo;
    private readonly Mock<ILessonProgressRepository> mockLessonRepo;
    private readonly Mock<ITraineeRepository> mockTraineeRepo;
    private readonly Mock<IDateTimeTrackingApiClient> mockApiClient;
    private readonly Mock<IWorkingHoursRepository> mockWorkHoursRepo;
    private readonly TraineeProgressService service;

    public TraineeProgressServiceTests()
    {
        mockProgressRepo = new Mock<ITraineeProgressRepository>();
        mockLessonRepo = new Mock<ILessonProgressRepository>();
        mockTraineeRepo = new Mock<ITraineeRepository>();
        mockApiClient = new Mock<IDateTimeTrackingApiClient>();
        mockWorkHoursRepo = new Mock<IWorkingHoursRepository>();

        service = new TraineeProgressService(
            mockProgressRepo.Object,
            mockLessonRepo.Object,
            mockTraineeRepo.Object,
            mockApiClient.Object,
            mockWorkHoursRepo.Object
        );
    }

    [Fact]
    public async Task CalcAndSaveProgress()
    {
        // Arrange
        var traineeId = Guid.NewGuid();
        var trainee = new Trainee
        {
            guid = traineeId,                     
            active = true,                        
            knowledge = "",                      
            Id = traineeId.ToString(),            
            role = Role.Trainee,
            Email = "trainee@makandra.de",
            startDate = new DateOnly(2026, 3, 1)
        };

        var mockApiResponse = new WorkingHoursResponseDto
        {
            entries = new List<WorkingHoursEntryDto>
            {
                new() { date = "2026-03-02", working_hours = 8.0 },
                new() { date = "2026-03-03", working_hours = 7.0 }
            }
        };

        var lessons = new List<LessonProgress>
        {
            new()
            {
                traineeId = traineeId,
                status = LessonStatus.Accepted,
                timeEstimate = 10,
                isActive = true,
                title = "Lesson 1",               
                url = "https://example.com/1",    
                position = 1                      
            },
            new()
            {
                traineeId = traineeId,
                status = LessonStatus.Finished,
                timeEstimate = 10,
                isActive = true,
                title = "Lesson 2",               
                url = "https://example.com/2",    
                position = 2                      
            }
        };

        mockTraineeRepo.Setup(r => r.GetTraineeById(traineeId)).Returns(trainee);
        mockApiClient.Setup(api => api.GetWorkinghours(trainee.Email, trainee.startDate, It.IsAny<DateOnly>()))
            .ReturnsAsync(mockApiResponse);
        mockLessonRepo.Setup(r => r.GetAll(traineeId)).Returns(lessons);

        TraineeProgress? savedProgress = null;
        mockProgressRepo.Setup(r => r.UpsertAsync(It.IsAny<TraineeProgress>(), It.IsAny<CancellationToken>()))
            .Callback<TraineeProgress, CancellationToken>((p, c) => savedProgress = p)
            .ReturnsAsync(() => savedProgress!);

        // Act
        var result = await service.calcAndSaveProgress(traineeId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(17, (double)result.WeightedProgressEstimate); // 10 + 7
        Assert.Equal(3, (double)result.OpenWork); // 20 - 17
        Assert.Equal(15.125, (double)result.Buffer); // 17 estimated Workdays - 15/8 (attendence in workdays)

        mockProgressRepo.Verify(r => r.UpsertAsync(It.IsAny<TraineeProgress>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void CalculateWorkSpeed_ReturnsZero()
    {
        // Act
        var result = service.calculateWorkSpeed(50.0, 0.0);

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void CalculateWorkSpeed_ReturnsPercentage()
    {
        // Act & Assert
        // 10 geschafft / 8 anwesend * 100 = 125%
        Assert.Equal(125.0, service.calculateWorkSpeed(10.0, 8.0));
    }

    [Fact]
    public void CalculateEstimatedBuffer_ReturnsCurrentBuffer()
    {
        // Act
        var result = service.calculateEstimatedBuffer(5.0, 10.0, 0.0);

        // Assert
        Assert.Equal(5.0, result);
    }

    [Fact]
    public void CalculateOpenWork()
    {
        // Arrange
        var lessons = new List<LessonProgress>
        {
            new()
            {
                timeEstimate = 10.0,
                status = LessonStatus.Accepted,    
                title = "Lesson 1",               
                url = "https://example.com/1",    
                isActive = true,                  
                position = 1                      
            },
            new()
            {
                timeEstimate = 15.0,
                status = LessonStatus.Accepted,    
                title = "Lesson 2",               
                url = "https://example.com/2",    
                isActive = true,                  
                position = 2                      
            }
        };
        double weightedProgress = 12.0;

        // Act
        var result = service.calculateOpenWork(lessons, weightedProgress);

        // Assert
        Assert.Equal(13.0, result); // 25.0 - 12.0
    }
}