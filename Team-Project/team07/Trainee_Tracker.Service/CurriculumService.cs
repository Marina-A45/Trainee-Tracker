using Trainee_Tracker.Core.Service;
using Trainee_Tracker.Core.Repository;
using Trainee_Tracker.Core.Entity;
using Trainee_Tracker.Core.DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using System.Text.Json;

namespace Trainee_Tracker.Service;
/// <summary>
/// Author: Lukas Nowak
/// </summary>
public class CurriculumService : ICurriculumService
{
    private ICurriculumRepository repo;
    private readonly IHubContext<CurriculumHub> hubContext;
    public CurriculumService(ICurriculumRepository repo, IHubContext<CurriculumHub> hubContext)
    {
        this.repo = repo;
        this.hubContext = hubContext;
    }

    /// <summary>
    /// Creates a new curriculum record directly from a domain entity.
    /// </summary>
    /// <param name="curriculum">The curriculum entity instance to create.</param>
    public void create(Curriculum curriculum)
    {
        repo.Create(curriculum);
    }

    /// <summary>
    /// Updates an existing curriculum's lessons by processing an incoming JSON stream.
    /// </summary>
    /// <param name="curriculumId">The unique identifier of the curriculum to edit.</param>
    /// <param name="jsonStream">The stream containing JSON data for the updated lessons list.</param>
    /// <returns>A task that represents the asynchronous operation, returning true if successful.</returns>
    /// <exception cref="Exception">Thrown if database persistence or JSON deserialization fails.</exception>
    public async Task<Boolean> EditWithJson(Guid curriculumId, Stream jsonStream)
    {
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        try
        {
            var DTOLessons = await JsonSerializer.DeserializeAsync<List<LessonDTO>>(jsonStream, options);
            var lessons = new List<Lesson>();
            if (DTOLessons != null)
            {
                for (int i = 0; i < DTOLessons.Count; i++)
                {
                    var dto = DTOLessons[i];
                    // Maps the incoming Data Transfer Object (DTO) into a concrete Lesson entity
                    lessons.Add(new Lesson
                    {
                        // Formats the identifier into a clean 32-character hex representation before parsing to Guid
                        id = Guid.Parse(dto.id.ToString("x8").PadLeft(32, '0')),
                        title = dto.title,
                        timeEstimate = dto.estimate ?? 0.0,
                        url = dto.url,
                        isActive = !dto.deprecated,
                        defaultPosition = i + 1
                    });
                }
            }
            try
            {
                repo.Edit(curriculumId, lessons);
            }
            catch (DbUpdateException ex)
            {
                var innerMessage = ex.InnerException?.Message ?? ex.Message;

                throw new Exception($"Database Save Failed! Cause: {innerMessage}", ex);
            }
            await hubContext.Clients.All.SendAsync("RefreshCurriculumData", curriculumId);
            return true;
        }
        catch (JsonException)
        {
            // Preserve the exception type so the controller can display
            // a specific warning for invalid JSON.
            throw;
        }
    }

    /// <summary>
    /// Generates and persists a new curriculum object populated by lesson definitions parsed from a JSON stream.
    /// </summary>
    /// <param name="name">The descriptive name to give the new curriculum.</param>
    /// <param name="jsonStream">The stream containing JSON data for the lessons collection.</param>
    /// <returns>A task returning the newly created <see cref="Curriculum"/> structure.</returns>
    /// <exception cref="Exception">Thrown if data layer persistence or JSON formatting criteria fails.</exception>
    public async Task<Curriculum> CreateWithJson(string name, Stream jsonStream)
    {
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        try
        {
            var DTOLessons = await JsonSerializer.DeserializeAsync<List<LessonDTO>>(jsonStream, options);
            var lessons = new List<Lesson>();
            if (DTOLessons != null)
            {
                for (int i = 0; i < DTOLessons.Count; i++)
                {
                    var dto = DTOLessons[i];
                    lessons.Add(new Lesson
                    {
                        id = Guid.Parse(dto.id.ToString("x8").PadLeft(32, '0')),
                        title = dto.title,
                        timeEstimate = dto.estimate?? 0.0,
                        url = dto.url,
                        isActive = !dto.deprecated,
                        defaultPosition = i + 1
                    });
                }
            }
            var newCurriculum = new Curriculum
            {   
                id =  Guid.NewGuid(),
                name = name,
                updated = DateTime.UtcNow,
                lessons = lessons ?? new List<Lesson>()
            };

            try
            {
                repo.Create(newCurriculum);
            }
            catch (DbUpdateException ex)
            {
                var innerMessage = ex.InnerException?.Message ?? ex.Message;

                throw new Exception($"Database Save Failed! Cause: {innerMessage}", ex);
            }

            return newCurriculum;
        }
        catch (JsonException)
        {
            // Preserve the exception type so the controller can display
            // a specific warning for invalid JSON.
            throw;
        }
    }
    /// <summary>
    /// Removes a specified curriculum from the database by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier (GUID) of the target curriculum.</param>
    /// <returns>A task returning true if the deletion completes successfully.</returns>
    /// <exception cref="Exception">Thrown if a database cascading rule or constraint prevents deletion.</exception>
    public async Task<Boolean> Delete(Guid id)
    {
        try
        {
            repo.DeleteById(id);
            return true;
        }
        catch (DbUpdateException ex)
        {
            var innerMessage = ex.InnerException?.Message ?? ex.Message;
            throw new Exception($"Database Save Failed! Cause: {innerMessage}", ex);
        }
    }

    /// <summary>
    /// Returns a list of all existing curriculums stored in the data repository.
    /// </summary>
    /// <returns>A collection of <see cref="Curriculum"/> records.</returns>
    public IEnumerable<Curriculum> getAll()
    {
        return repo.GetAll();
    }
}