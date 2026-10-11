using Microsoft.Playwright;
using NUnit.Framework;

namespace Trainee_Tracker.Tests;

//[TestFixture]
//[NonParallelizable]
public class FinishedStartedDragAndDropE2ETest
    : PlaywrightTestBase {

    /// <summary>
    /// Logs in with the trainee account provided by the E2E seed data.
    /// Author: Dennis Axt
    /// </summary>
    /// <returns>A task representing the asynchronous login operation.</returns>
    private async Task LoginAsE2ETrainee() {
        await Page.GotoAsync($"{BaseUrl}/Home/Login");

        await Page.FillAsync(
            "[data-testid='lesson-email-input']",
            "e2e.trainee@test.com");

        await Page.FillAsync(
            "[data-testid='lesson-password-input']",
            "Test123!");

        await Page.ClickAsync(
            "[data-testid='submit-login']");

        await Page.WaitForLoadStateAsync(
            LoadState.NetworkIdle);
    }

    /// <summary>
    /// Verifies that a trainee can move a lesson from Finished back
    /// to Started and that the new status remains after reloading.
    /// Author: Dennis Axt
    /// </summary>
    /// <returns>A task representing the asynchronous E2E test.</returns>
    //[Test]
    public async Task Should_Move_Lesson_From_Finished_To_Started() {
        await LoginAsE2ETrainee();

        // Open the trainee lesson status board.
        await Page.GotoAsync(
            $"{BaseUrl}/Trainee/LessonStatus");

        // Select a real Finished lesson and exclude Rejected lessons,
        // which are displayed in the same column.
        var finishedLessons = Page.Locator(
            ".columns--column[data-status='finished'] " +
            ".lesson-card:not(.is-rejected)");

        Assert.That(
            await finishedLessons.CountAsync(),
            Is.GreaterThan(0));

        var lesson = finishedLessons.First;
        var lessonId = await lesson.GetAttributeAsync("id");

        Assert.That(
            lessonId,
            Is.Not.Null.And.Not.Empty);

        var startedColumn = Page.Locator(
            ".columns--column[data-status='started']");

        // Drag the lesson back to Started and wait for persistence.
        var response = await Page.RunAndWaitForResponseAsync(
            async () => await lesson.DragToAsync(startedColumn),
            "**/api/lesson/update-status");

        Assert.That(response.Ok, Is.True);

        Assert.That(
            await startedColumn
                .Locator($".lesson-card[id='{lessonId}']")
                .IsVisibleAsync(),
            Is.True);

        // Verify that the changed status remains stored after reloading.
        await Page.ReloadAsync();

        Assert.That(
            await Page.Locator(
                ".columns--column[data-status='started'] " +
                $".lesson-card[id='{lessonId}']")
                .IsVisibleAsync(),
            Is.True);
    }
}