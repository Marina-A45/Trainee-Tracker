using Microsoft.Playwright;
using NUnit.Framework;
using Trainee_Tracker.Core.Entity;
using Trainee_Tracker.Core.Enums;
using Trainee_Tracker.Repository;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
namespace Trainee_Tracker.Tests;

[TestFixture]
public class LessonStatusTests : PlaywrightTestBase
{
    /// <summary>
    /// Login as the E2ETrainee
    /// </summary>
    /// <returns></returns>
    private async Task LoginAsE2ETrainee()
    {
        await Page.GotoAsync($"{BaseUrl}/Home/Login");
        await Page.FillAsync("[data-testid='lesson-email-input']", "e2e.trainee@test.com");
        await Page.FillAsync("[data-testid='lesson-password-input']", "Test123!");
        await Page.ClickAsync("[data-testid='submit-login']");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    /// <summary>
    /// Login as the E2EMentor
    /// </summary>
    /// <returns></returns>
    private async Task LoginAsE2EMentor()
    {
        await Page.GotoAsync($"{BaseUrl}/Home/Login");
        await Page.FillAsync("[data-testid='lesson-email-input']", "e2e.mentor@test.com");
        await Page.FillAsync("[data-testid='lesson-password-input']", "Mentor123!");
        await Page.ClickAsync("[data-testid='submit-login']");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    private async Task OpenBoardForE2ETrainee()
    {
        await Page.GotoAsync($"{BaseUrl}/Mentor/TraineeProgress");
        await Page.Locator("#trainee-filter")
            .SelectOptionAsync(new SelectOptionValue
            {
                Label = "E2E Trainee"
            });
        await Page.WaitForURLAsync("**/Mentor/TraineeProgress?traineeId=*");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    /// <summary>
    /// Tests if a Trainee Can move the allowed transitions
    /// Author Lukas Nowak
    /// </summary>
    /// <param name="fromState">the original state</param>
    /// <param name="toState">the state where the card gets dragged to</param>
    [TestCase("open", "started")]
    [TestCase("started", "open")]
    [TestCase("started", "finished")]
    [TestCase("finished", "started")]
    [TestCase("rejected", "finished")]
    [TestCase("accepted", "rated")]
    public async Task Trainee_Should_Move_States(string fromState, string toState)
    {
        await LoginAsE2ETrainee();
        var response = await Page.GotoAsync($"{BaseUrl}/Trainee/LessonStatus");
        Assert.That(response, Is.Not.Null, "Page load failed and returned a null response.");
        Assert.That(response!.Ok, Is.True, $"Page load failed with status code: {response?.Status}");

        var sourceColumn = Page.Locator($".columns--column[data-status='{fromState}']");
        var targetColumn = Page.Locator($".columns--column[data-status='{toState}']");

        // Take the first card from the column
        var lessonCard = sourceColumn.Locator(".lesson-card").First;
        await lessonCard.WaitForAsync(new() { State = WaitForSelectorState.Visible });

        var lessonId = await lessonCard.GetAttributeAsync("id");
        Assert.That(lessonId, Is.Not.Null.And.Not.Empty, $"No lesson card found in '{fromState}' column.");

        // Drag the card to the target column and wait for the API response
        var responseTask = Page.WaitForResponseAsync("**/api/lesson/update-status");
        var dropTarget = targetColumn.Locator(".column-title");
        await lessonCard.DragToAsync(dropTarget);
        var apiResponse = await responseTask;
        Assert.That(apiResponse.Ok, Is.True, "Status update API failed.");

        // Verify the card now exists inside the destination column UI
        var movedCard = targetColumn.Locator($"[id='{lessonId}']");
        await movedCard.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 5000 });
        Assert.That(await movedCard.IsVisibleAsync(), Is.True, $"Card '{lessonId}' did not appear in the '{toState}' column.");

        // Assert DB updated correctly
        var parsedLessonId = Guid.Parse(lessonId!);
        using (var scope = Factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<TraineeTrackerContext>();
            var allProgresses = await dbContext.progresses.AsNoTracking().ToListAsync();
            var lessonProgress = allProgresses.FirstOrDefault(lp => lp.id == parsedLessonId || lp.lessonId == parsedLessonId);
            Assert.That(lessonProgress, Is.Not.Null, $"LessonProgress record not found for ID '{lessonId}'.");

            var parsed = Enum.TryParse<LessonStatus>(toState, ignoreCase: true, out var expectedStatus);
            Assert.That(parsed, Is.True, $"Unknown lesson status '{toState}'.");
            Assert.That(lessonProgress!.status, Is.EqualTo(expectedStatus));
        }
    }
}