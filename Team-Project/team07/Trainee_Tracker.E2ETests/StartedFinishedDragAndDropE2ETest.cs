using Microsoft.Playwright;
using NUnit.Framework;

namespace Trainee_Tracker.Tests;

/// <summary>
/// Author: Carl Böhm-Sturm
/// </summary>
[TestFixture]
public class StartedFinishedDragAndDropE2ETest : PlaywrightTestBase
{
    /// <summary>
    /// Logs in with the E2E trainee account seeded by the DataSeeder.
    /// Author: Carl Böhm-Sturm
    /// </summary>
    private async Task LoginAsE2ETrainee()
    {
        await Page.GotoAsync($"{BaseUrl}/Home/Login");

        await Page.FillAsync("[data-testid='lesson-email-input']", "e2e.trainee@test.com");
        await Page.FillAsync("[data-testid='lesson-password-input']", "Test123!");
        await Page.ClickAsync("[data-testid='submit-login']");

        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    /// <summary>
    /// Tests Trainee: LessonProgress started -> finished.
    /// Author: Carl Böhm-Sturm
    /// </summary>
    //[Test]
    public async Task Should_Move_Lesson_From_Started_To_Finished()
    {
        await LoginAsE2ETrainee();

        // Open lesson status board
        await Page.GotoAsync($"{BaseUrl}/Trainee/LessonStatus");

        // Find lessons in Started column
        var startedLessons = Page.Locator(".columns--column[data-status='started'] .lesson-card");
        Assert.That(await startedLessons.CountAsync(), Is.GreaterThan(0));

        var lesson = startedLessons.First;
        var lessonId = await lesson.GetAttributeAsync("id");

        // Find Finished column
        var finishedColumn = Page.Locator(".columns--column[data-status='finished']");

        // Drag lesson from Started to Finished; the drop fires a background
        // POST that persists the new status, so wait for that response before
        // asserting anything server-side
        var response = await Page.RunAndWaitForResponseAsync(
            async () => await lesson.DragToAsync(finishedColumn),
            "**/api/lesson/update-status"
        );
        Assert.That(response.Ok, Is.True);

        // Verify lesson moved in the UI
        Assert.That(
            await finishedColumn.Locator($"[id='{lessonId}']").IsVisibleAsync(),
            Is.True
        );

        // Verify the transition was persisted: still in Finished after reload
        await Page.ReloadAsync();
        Assert.That(
            await Page.Locator($".columns--column[data-status='finished'] [id='{lessonId}']")
                .IsVisibleAsync(),
            Is.True
        );
    }
}
