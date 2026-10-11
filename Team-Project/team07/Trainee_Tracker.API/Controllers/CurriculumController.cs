using Microsoft.AspNetCore.Mvc;
using Trainee_Tracker.Core.Entity;
using Trainee_Tracker.Core.Service;
using System.Text.Json;
namespace Trainee_Tracker.Controllers;

public class CurriculumController : Controller
{
    private readonly ICurriculumService service;

    public CurriculumController(ICurriculumService curriculumService)
    {
        service = curriculumService;
    }

    /// <summary>
    /// Creates a new curriculum using a provided name and a JSON file containing the curriculum structure.
    /// Author: Lukas Nowak
    /// </summary>
    /// <param name="name">The name of the new curriculum.</param>
    /// <param name="file">The uploaded JSON file representing the curriculum lessons.</param>
    /// <returns>A redirect to the Mentor Curriculums page, with TempData["ErrorMessage"] populated on failure.</returns>
    [HttpPost]
    public async Task<IActionResult> CreateWithJsonFile(string name, IFormFile file)
    {
        if (string.IsNullOrEmpty(name))
        {
            TempData["ErrorMessage"] = "Invalid name";
            return Redirect("/Mentor/Curriculums");
        }
        if (file == null || file.Length == 0)
        {
            TempData["ErrorMessage"] = "Invalid file payload.";
            return Redirect("/Mentor/Curriculums");
        }

        try
        {
            using var stream = file.OpenReadStream();
            var result = await service.CreateWithJson(name, stream);
            return Redirect("/Mentor/Curriculums");
        }
        catch (JsonException)
        {
            TempData["ErrorMessage"] = "The uploaded file is not a valid JSON or does not match the Lesson structure.";
            return Redirect("/Mentor/Curriculums");
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] = $"Something went wrong please try again";
            return Redirect("/Mentor/Curriculums");
        }
    }

    /// <summary>
    /// Updates an existing curriculum using its unique identifier and a JSON file containing the updated structure.
    /// Author: Lukas Nowak
    /// </summary>
    /// <param name="id">The unique identifier (GUID) of the curriculum to edit.</param>
    /// <param name="file">The uploaded JSON file containing the updated lesson structure.</param>
    /// <returns>A redirect to the Mentor Curriculums page, with TempData["ErrorMessage"] populated on failure.</returns>
    [HttpPost]
    public async Task<IActionResult> EditWithJsonFile(Guid id, IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            TempData["ErrorMessage"] = "Invalid file payload.";
            return Redirect("/Mentor/Curriculums");
        }

        try
        {
            using var stream = file.OpenReadStream();
            var result = await service.EditWithJson(id, stream);
            return Redirect("/Mentor/Curriculums");
        }
        catch (JsonException)
        {
            TempData["ErrorMessage"] = "The uploaded file is not a valid JSON or does not match the Lesson structure.";
            return Redirect("/Mentor/Curriculums");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            TempData["ErrorMessage"] = $"Something went wrong please try again";
            return Redirect("/Mentor/Curriculums");
        }
    }

    /// <summary>
    /// Deletes a curriculum based on its unique identifier.
    /// Author: Lukas Nowak
    /// </summary>
    /// <param name="id">The unique identifier (GUID) of the curriculum to be removed.</param>
    /// <returns>A redirect to the Mentor Curriculums page, with TempData["ErrorMessage"] populated on failure.</returns>
    [HttpPost]
    public async Task<IActionResult> Delete(Guid id)
    {
        if (id == Guid.Empty)
        {
            TempData["ErrorMessage"] = "Invalid Curriculum Id given.";
            return Redirect("/Mentor/Curriculums");
        }

        try
        {
            var result = await service.Delete(id);
            return Redirect("/Mentor/Curriculums");
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] = $"Something went wrong please try again";
            return Redirect("/Mentor/Curriculums");
        }
    }

}