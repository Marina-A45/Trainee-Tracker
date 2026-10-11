using Microsoft.Playwright;
using NUnit.Framework;

namespace Trainee_Tracker.Tests;

/// <summary>
/// End-to-end test for administrator access to trainee lesson progress.
/// </summary>
[TestFixture]
public class AdminTraineeProgressE2ETest : PlaywrightTestBase {
    /// <summary>
    /// Logs in with the administrator account seeded by the DataSeeder.
    /// Author: Dennis Axt
    /// </summary>
    private async Task LoginAsAdmin() {
        await Page.GotoAsync($"{BaseUrl}/Home/Login");

        await Page.FillAsync(
            "[data-testid='lesson-email-input']",
            "admin@makandra.de");

        await Page.FillAsync(
            "[data-testid='lesson-password-input']",
            "Admin1!");

        await Page.ClickAsync(
            "[data-testid='submit-login']");

        await Page.WaitForLoadStateAsync(
            LoadState.NetworkIdle);
    }

    /// <summary>
    /// Verifies that an administrator can select an active trainee and view
    /// their lesson progress without a Mentor-Trainee assignment.
    /// Author: Dennis Axt
    /// </summary>
    [Test]
    public async Task Admin_Should_View_Progress_Of_Selected_Trainee() {
        // Log in as an administrator without a Mentor-Trainee assignment.
        await LoginAsAdmin();

        // Open the shared Mentor/Admin trainee-progress page.
        await Page.GotoAsync(
            $"{BaseUrl}/Mentor/TraineeProgress");

        Assert.That(
            Page.Url,
            Does.Not.Contain("AccessDenied"));

        var heading = Page.Locator("h1.page-title");

        Assert.That(
            await heading.TextContentAsync(),
            Does.Contain("ADMIN TRAINEE PROGRESS"));

        // Select the seeded E2E trainee from all active trainees.
        await Page.SelectOptionAsync(
            "#trainee-filter",
            new SelectOptionValue {
                Label = "E2E Trainee"
            });

        await Page.WaitForLoadStateAsync(
            LoadState.NetworkIdle);

        var selectedTrainee =
            Page.Locator("#trainee-filter option:checked");

        Assert.That(
            await selectedTrainee.TextContentAsync(),
            Does.Contain("E2E Trainee"));

        // Confirm that the selected trainee's persisted progress is displayed.
        var lessonCards = Page.Locator(
            "#board-container .lesson-card");

        Assert.That(
            await lessonCards.CountAsync(),
            Is.GreaterThan(0));
    }
}