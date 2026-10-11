using Microsoft.Playwright;
using NUnit.Framework;

namespace Trainee_Tracker.Tests;

/// By Tim Glaser
/// <summary>
/// End-to-end tests for the mentor workflow between Finished and Rejected lessons.
/// The class verifies that a mentor can reject a finished lesson with a reason
/// and can move a rejected lesson back to the Finished column.
/// </summary>
//[TestFixture]
//[NonParallelizable]
public class RejectFinishedE2ETest : PlaywrightTestBase
{

    /// <summary>
    /// Logs in with the predefined E2E mentor account.
    /// </summary>
    private async Task LoginAsE2EMentor()
    {
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

    /// <summary>
    /// Opens the mentor trainee progress board and selects the predefined E2E trainee.
    /// </summary>
    private async Task OpenBoardForE2ETrainee()
    {
        await Page.GotoAsync($"{BaseUrl}/Mentor/TraineeProgress");

        await Page.Locator("#trainee-filter")
            .SelectOptionAsync(new SelectOptionValue {
                Label = "E2E Trainee"
            });

        await Page.WaitForURLAsync("**/Mentor/TraineeProgress?traineeId=*");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }


    /// <summary>
    /// Verifies that a mentor can reject a finished lesson by selecting the E2E trainee,
    /// submitting a rejection reason, and checking that the lesson is shown as rejected
    /// on the trainee progress board.
    /// </summary>
    //[Test]
    public async Task Should_Move_Lesson_From_Finished_To_Rejected()
    {
        await LoginAsE2EMentor();

        await Page.GotoAsync($"{BaseUrl}/Mentor/MissingFeedback");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await Page.Locator("#trainee-filter")
            .SelectOptionAsync(new SelectOptionValue {
                Label = "E2E Trainee"
            });

        var reviewCards = Page.Locator(".lesson-card.review-mode:visible");

        Assert.That(
            await reviewCards.CountAsync(),
            Is.GreaterThan(0)
        );

        var card = reviewCards.First;
        var lessonId = await card.GetAttributeAsync("id");

        Assert.That(
            lessonId,
            Is.Not.Null.And.Not.Empty
        );

        await card.Locator(".reject-toggle").ClickAsync();

        var rejectForm = card.Locator(".reject-form");

        await rejectForm
            .Locator("textarea[name='rejectionReason']")
            .FillAsync("E2E rejection reason");

        await rejectForm
            .Locator("button[type='submit']")
            .ClickAsync();

        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await OpenBoardForE2ETrainee();

        var rejectedLesson = Page.Locator(
            ".columns--column[data-status='finished'] " +
            $".lesson-card.is-rejected[id='{lessonId}']"
        );

        Assert.That(
            await rejectedLesson.IsVisibleAsync(),
            Is.True
        );

        Assert.That(
            await rejectedLesson.GetAttributeAsync("class"),
            Does.Contain("is-rejected")
        );
    }

    /// <summary>
    /// Verifies that a mentor can move a rejected lesson back to the Finished column
    /// by dragging it into the Finished column and checking that the rejected styling is removed.
    /// </summary>
    //[Test]
    public async Task Should_Move_Lesson_From_Rejected_To_Finished()
    {
        await LoginAsE2EMentor();

        await OpenBoardForE2ETrainee();

        var rejectedLessons = Page.Locator(
            ".columns--column[data-status='finished'] .lesson-card.is-rejected"
        );

        Assert.That(
            await rejectedLessons.CountAsync(),
            Is.GreaterThan(0)
        );

        var lessonId = await rejectedLessons.First.GetAttributeAsync("id");
        var lesson = Page.Locator($"[id='{lessonId}']");

        Assert.That(
            lessonId,
            Is.Not.Null.And.Not.Empty
        );

        var finishedColumn = Page.Locator(
            ".columns--column[data-status='finished']"
        );

        await lesson.DragToAsync(
            finishedColumn
        );

        var movedLesson = finishedColumn.Locator(
            $".lesson-card[id='{lessonId}']"
        );

        Assert.That(
            await movedLesson.IsVisibleAsync(),
            Is.True
        );

        Assert.That(
            await movedLesson.GetAttributeAsync("class"),
            Does.Not.Contain("is-rejected")
        );
    }
}