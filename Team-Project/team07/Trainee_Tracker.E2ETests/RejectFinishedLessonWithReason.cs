using Microsoft.Playwright;
using NUnit.Framework;

namespace Trainee_Tracker.Tests;

/// By Tim Glaser
/// <summary>
/// Verifyes the mentor rejection workflow.
/// Checks that a mentor can reject a finished lesson with a required reason
/// and that the rejected lesson is shown as rejected on the trainee progress page.
/// </summary>
[TestFixture]
public class RejectFinishedLessonWithReason : PlaywrightTestBase
{
    private async Task LoginAsE2EMentor() {
        
        // Open login page using the BaseUrl from PlaywrightTestBase
        await Page.GotoAsync($"{BaseUrl}/Home/Login");

        // Enter email
        await Page.FillAsync(
            "[data-testid='lesson-email-input']",
            "e2e.mentor@test.com"
        );

        // Enter password
        await Page.FillAsync(
            "[data-testid='lesson-password-input']",
            "Mentor123!"
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

    //[Test]
    public async Task Should_Reject_Finished_Lesson_With_Reason()
    {
        await LoginAsE2EMentor();

        await Page.GotoAsync($"{BaseUrl}/Mentor/MissingFeedback");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var reviewCards = Page.Locator(".lesson-card.review-mode");
        Assert.That(await reviewCards.CountAsync(), Is.GreaterThan(0));

        var card = reviewCards.First;
        var lessonId = await card.GetAttributeAsync("id");

        await card.Locator(".reject-toggle").ClickAsync();

        var rejectForm = card.Locator(".reject-form");
        await rejectForm.Locator("textarea[name='rejectionReason']")
        .FillAsync("E2E rejection reason");

        await rejectForm.Locator("button[type='submit']").ClickAsync();

        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await Page.GotoAsync($"{BaseUrl}/Mentor/TraineeProgress");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var rejectedLesson = Page.Locator($".lesson-card.is-rejected[id='{lessonId}']");
        Assert.That(await rejectedLesson.IsVisibleAsync(), Is.True);
    }
}