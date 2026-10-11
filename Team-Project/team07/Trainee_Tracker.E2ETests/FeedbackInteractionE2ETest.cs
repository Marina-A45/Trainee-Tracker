using Microsoft.Playwright;
using NUnit.Framework;

namespace Trainee_Tracker.Tests;

/// <summary>
/// Author: Carl Böhm-Sturm
/// </summary>
[TestFixture]
public class FeedbackInteractionE2ETest : PlaywrightTestBase
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
    /// Covers the full feedback interaction: opening the form from an accepted
    /// lesson, submitting it, the lesson moving from Accepted to Rated, and the
    /// feedback showing up in the feedback overview.
    /// Author: Carl Böhm-Sturm
    /// </summary>
    [Test]
    public async Task Should_Rate_Lesson_And_Show_Feedback_In_Overview()
    {
        await LoginAsE2ETrainee();

        await Page.GotoAsync($"{BaseUrl}/Trainee/LessonStatus");

        // Only lessons in the Accepted column offer the feedback button
        var acceptedLessons = Page.Locator(".columns--column[data-status='accepted'] .lesson-card");
        Assert.That(await acceptedLessons.CountAsync(), Is.GreaterThan(0));

        var lesson = acceptedLessons.First;
        var lessonId = await lesson.GetAttributeAsync("id");

        await lesson.Locator("a.feedback-btn").ClickAsync();

        // Unique marker so the overview assertion cannot match seeded feedback
        var feedbackText = $"E2E feedback {Guid.NewGuid()}";

        // Fill Form
        await Page.Locator("label[for='btn-diff-3']").ClickAsync();
        await Page.Locator("input[name='actualEffort']").ClearAsync();
        await Page.Locator("input[name='actualEffort']").FillAsync("2");
        await Page.Locator("textarea[name='knowledge']").FillAsync("Some prior C# knowledge.");
        await Page.Locator("textarea[name='text']").FillAsync(feedbackText);

        await Page.ClickAsync("button.submit-button");
        await Page.WaitForURLAsync("**/Trainee/LessonStatus");

        // Submitting feedback moves the lesson from Accepted to Rated
        var ratedCard = Page.Locator(
            $".columns--column[data-status='rated'] .lesson-card[id='{lessonId}']"
        );
        Assert.That(await ratedCard.IsVisibleAsync(), Is.True);

        // The submitted feedback is listed in the feedback overview
        await Page.GotoAsync($"{BaseUrl}/Feedback/AllFeedbacks");
        var feedbackEntry = Page.Locator($".feedback-container:has-text(\"{feedbackText}\")");
        Assert.That(await feedbackEntry.CountAsync(), Is.EqualTo(1));
    }
}
