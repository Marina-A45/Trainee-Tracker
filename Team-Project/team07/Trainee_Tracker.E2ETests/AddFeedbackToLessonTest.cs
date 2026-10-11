using Microsoft.Playwright;
using NUnit.Framework;

namespace Trainee_Tracker.Tests;

[TestFixture]
public class AddFeedbackToLessonTest : PlaywrightTestBase
{
    private async Task LoginAsE2ETrainee()
    {
        // Use the BaseUrl constant defined in the base class
        await Page.GotoAsync($"{BaseUrl}/Home/Login");

        await Page.FillAsync("[data-testid='lesson-email-input']", "e2e.trainee@test.com");
        await Page.FillAsync("[data-testid='lesson-password-input']", "Test123!");
        await Page.ClickAsync("[data-testid='submit-login']");

        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    [Test]
    public async Task Should_Save_Feedback()
    {
        await LoginAsE2ETrainee();

        await Page.GotoAsync($"{BaseUrl}/Trainee/LessonStatus");

        var acceptedLessons = Page.Locator(".columns--column[data-status='accepted'] .lesson-card");

        Assert.That(await acceptedLessons.CountAsync(), Is.GreaterThan(0));

        var lesson = acceptedLessons.First;
        var lessonId = await lesson.GetAttributeAsync("id");
        var feedbackButton = lesson.Locator("a.feedback-btn");
        var href = await feedbackButton.GetAttributeAsync("href");

        Assert.That(href, Does.Contain($"/Trainee/Feedback?lessonId="));
        Assert.That(href, Does.Contain($"&progressId={lessonId}"));

        await feedbackButton.ClickAsync();

        // Fill Form
        await Page.Locator("label[for='btn-diff-4']").ClickAsync();
        await Page.Locator("input[name='actualEffort']").ClearAsync();
        await Page.Locator("input[name='actualEffort']").FillAsync("3.5");
        await Page.Locator("textarea[name='knowledge']").FillAsync("I knew a little bit of c#.");
        await Page.Locator("textarea[name='text']").FillAsync("The lesson was well structured.");

        await Page.ClickAsync("button.submit-button");
        await Page.WaitForURLAsync("**/Trainee/LessonStatus");

        var lessonCardAfterFeedback = Page.Locator(
            $".columns--column[data-status='accepted'] .lesson-card[id='{lessonId}']"
        );
        Assert.That(await lessonCardAfterFeedback.IsVisibleAsync(), Is.False);
    }
}