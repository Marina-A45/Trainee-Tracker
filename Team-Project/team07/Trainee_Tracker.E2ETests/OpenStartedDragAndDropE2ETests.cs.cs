using Microsoft.Playwright;
using NUnit.Framework;

namespace Trainee_Tracker.Tests;

// Author: Marina Achner

/// <summary>
/// End-to-end tests for changing lesson progress states
/// using drag and drop functionality.
/// </summary>
//[TestFixture]
//[NonParallelizable]
public class OpenStartedDragAndDropE2ETests : PlaywrightTestBase {
    /// <summary>
    /// Logs in using the predefined E2E trainee account.
    /// </summary>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// </returns>
    private async Task LoginAsE2ETrainee() {
        // Open login page using the BaseUrl from PlaywrightTestBase
        await Page.GotoAsync($"{BaseUrl}/Home/Login");

        // Enter email
        await Page.FillAsync(
            "[data-testid='lesson-email-input']",
            "e2e.trainee@test.com"
        );

        // Enter password
        await Page.FillAsync(
            "[data-testid='lesson-password-input']",
            "Test123!"
        );

        // Submit login form
        await Page.ClickAsync(
            "[data-testid='submit-login']"
        );

        // Wait until login is finished
        await Page.WaitForLoadStateAsync(
            LoadState.NetworkIdle
        );
    }

    //Author: Marina Achner

    /// <summary>
    /// Logs in using the predefined E2E mentor account.
    /// </summary>
    /// <returns>
    /// A task representing the asynchronous login operation.
    /// </returns>
    private async Task LoginAsE2EMentor() {
        await Page.GotoAsync($"{BaseUrl}/Home/Login");

        await Page.FillAsync(
            "[data-testid='lesson-email-input']",
            "e2e.mentor@test.com"
        );

        await Page.FillAsync(
            "[data-testid='lesson-password-input']",
            "Mentor123!"
        );

        await Page.ClickAsync(
            "[data-testid='submit-login']"
        );

        await Page.WaitForLoadStateAsync(
            LoadState.NetworkIdle
        );
    }

    // Author: Marina Achner

    /// <summary>
    /// Verifies that a trainee can move a lesson progress
    /// from Open status to Started status using drag and drop.
    /// </summary>
    /// <returns>
    /// A task representing the asynchronous test execution.
    /// </returns>
    //[Test]
    public async Task TShouldMoveLessonFromOpenToStarted() {
        await LoginAsE2ETrainee();

        // Open lesson status board
        await Page.GotoAsync($"{BaseUrl}/Trainee/LessonStatus");

        // Find lessons in Open column
        var openLessons = Page.Locator(
            ".columns--column[data-status='open'] .lesson-card"
        );

        Assert.That(
            await openLessons.CountAsync(),
            Is.GreaterThan(0)
        );

        var lesson = openLessons.First;
        var lessonId = await lesson.GetAttributeAsync("id");

        // Find Started column
        var startedColumn = Page.Locator(
            ".columns--column[data-status='started']"
        );

        // Drag lesson from Open to Started
        await lesson.DragToAsync(
            startedColumn
        );

        // Verify lesson moved successfully
        Assert.That(
            await startedColumn
                .Locator($"[id='{lessonId}']")
                .IsVisibleAsync(),
            Is.True
        );
    }

    // Author: Marina Achner

    /// <summary>
    /// Verifies that a trainee can move a lesson progress
    /// from Started status back to Open status using drag and drop.
    /// </summary>
    /// <returns>
    /// A task representing the asynchronous test execution.
    /// </returns>
    //[Test]
    public async Task TShouldMoveLessonFromStartedToOpen() {
        await LoginAsE2ETrainee();

        // Open lesson status board
        await Page.GotoAsync($"{BaseUrl}/Trainee/LessonStatus");

        // Find lessons in Started column
        var startedLessons = Page.Locator(
            ".columns--column[data-status='started'] .lesson-card"
        );

        Assert.That(
            await startedLessons.CountAsync(),
            Is.GreaterThan(0)
        );

        var lesson = startedLessons.First;
        var lessonId = await lesson.GetAttributeAsync("id");

        // Find Open column
        var openColumn = Page.Locator(
            ".columns--column[data-status='open']"
        );

        // Drag lesson from Started back to Open
        await lesson.DragToAsync(
            openColumn
        );

        // Verify lesson moved successfully
        Assert.That(
            await openColumn
                .Locator($"[id='{lessonId}']")
                .IsVisibleAsync(),
            Is.True
        );
    }


    // Author: Marina Achner

    /// <summary>
    /// Verifies that a mentor can move a lesson progress
    /// from Open status to Started status using drag and drop.
    /// </summary>
    /// <returns>
    /// A task representing the asynchronous test execution.
    /// </returns>
    //[Test]
    public async Task MShouldMoveLessonFromOpenToStarted() {
        await LoginAsE2EMentor();

        // Open the mentor trainee progress page
        await Page.GotoAsync(
            $"{BaseUrl}/Mentor/TraineeProgress"
        );

        // Select the E2E trainee because mentors can manage multiple assigned trainees
        // and the test requires predictable lesson progress data.
        await Page.SelectOptionAsync(
            "#trainee-filter",
            new SelectOptionValue {
                Label = "E2E Trainee"
            }
        );

        // Wait until the trainee dashboard has finished loading.
        await Page.WaitForLoadStateAsync(
            LoadState.NetworkIdle
        );

        // Find lessons currently in the Open column.
        var openLessons = Page.Locator(
            ".columns--column[data-status='open'] .lesson-card"
        );

        // Ensure that at least one lesson can be moved.
        Assert.That(
            await openLessons.CountAsync(),
            Is.GreaterThan(0)
        );

        var lesson = openLessons.First;
        var lessonId = await lesson.GetAttributeAsync("id");

        // Find the target Started column.
        var startedColumn = Page.Locator(
            ".columns--column[data-status='started']"
        );

        // Drag the lesson from Open to Started.
        await lesson.DragToAsync(
            startedColumn
        );

        // Verify that the lesson was moved successfully.
        Assert.That(
            await startedColumn
                .Locator($"[id='{lessonId}']")
                .IsVisibleAsync(),
            Is.True
        );
    }


    // Author: Marina Achner

    /// <summary>
    /// Verifies that a mentor can move a lesson progress
    /// from Started status back to Open status using drag and drop.
    /// </summary>
    /// <returns>
    /// A task representing the asynchronous test execution.
    /// </returns>
   // [Test]
    public async Task MShouldMoveLessonFromStartedToOpen() {
        await LoginAsE2EMentor();

        // Open the mentor trainee progress page.
        await Page.GotoAsync(
            $"{BaseUrl}/Mentor/TraineeProgress"
        );

        // Select the E2E trainee because mentors can manage multiple assigned trainees
        // and the test requires predictable lesson progress data.
        await Page.SelectOptionAsync(
            "#trainee-filter",
            new SelectOptionValue {
                Label = "E2E Trainee"
            }
        );

        // Wait until the trainee progress dashboard is fully loaded.
        await Page.WaitForLoadStateAsync(
            LoadState.NetworkIdle
        );

        // Find lessons currently in the Started column.
        var startedLessons = Page.Locator(
            ".columns--column[data-status='started'] .lesson-card"
        );

        // Ensure that at least one lesson can be moved back to Open.
        Assert.That(
            await startedLessons.CountAsync(),
            Is.GreaterThan(0)
        );

        var lesson = startedLessons.First;
        var lessonId = await lesson.GetAttributeAsync("id");

        // Find the target Open column.
        var openColumn = Page.Locator(
            ".columns--column[data-status='open']"
        );

        // Drag the lesson from Started back to Open.
        await lesson.DragToAsync(
            openColumn
        );

        // Verify that the moved lesson arrived in the Open column.
        Assert.That(
            await openColumn
                .Locator($"[id='{lessonId}']")
                .IsVisibleAsync(),
            Is.True
        );
    }
}