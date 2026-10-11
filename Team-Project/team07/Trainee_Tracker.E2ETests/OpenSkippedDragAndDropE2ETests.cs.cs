using Microsoft.Playwright;
using NUnit.Framework;

namespace Trainee_Tracker.Tests;

[TestFixture]
[NonParallelizable]

public class OpenSkippedDragAndDropE2ETests : PlaywrightTestBase {
    // By Patrick Plechinger
    // Login with the E2E mentor account
    private async Task LoginAsE2EMentor() {
        // Use a viewport that fits the whole mentor dashboard including the
        // skip container below the board. Otherwise DragToAsync scrolls the
        // page in the middle of the drag (mouse-down happens before the
        // scroll), so a different lesson card ends up under the pointer and
        // the wrong lesson gets dragged.
        await Page.SetViewportSizeAsync(1280, 2400);

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

    // By Patrick Plechinger
    // Opens the mentor board and selects the E2E Trainee.
    private async Task OpenBoardForE2ETrainee() {
        await Page.GotoAsync($"{BaseUrl}/Mentor/TraineeProgress");
        await Page.Locator("#trainee-filter")
            .SelectOptionAsync(new SelectOptionValue {
                Label = "E2E Trainee"
            });

        // Selecting a trainee triggers a full page navigation
        // (window.location.href in TraineeProgress.cshtml), so wait for the
        // new page before touching any locators.
        await Page.WaitForURLAsync("**/Mentor/TraineeProgress?traineeId=*");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    // By Patrick Plechinger
    // Tests Mentor: LessonProgress open -> skipped
    //[Test]
    public async Task Should_Move_Lesson_From_Open_To_Skipped() {
        await LoginAsE2EMentor();

        // Open Mentors lesson status board for E2E Trainee
        await OpenBoardForE2ETrainee();

        // Find lessons in Open column that are not skipped
        var openLessons = Page.Locator("[data-status='open'] .lesson-card:not(.is-skipped)");
        Assert.That(
            await openLessons.CountAsync(),
            Is.GreaterThan(0)
        );
        var lessonId = await openLessons.First.GetAttributeAsync("id");
        var lesson = Page.Locator($"[id='{lessonId}']");

        // Find Skip container
        var skippedContainer = Page.Locator(
            "#skip-container"
        );

        // The skip container is display:none until a drag is in progress,
        // but DragToAsync refuses to start while its target is hidden.
        // Make it visible up front; the dragstart handler keeps it visible
        // during the real drag anyway.
        await Page.EvaluateAsync("""
() => {
    document.querySelector("#skip-container")
            .classList.add("is-visible", "is-valid-target");
}
""");
        // Wait until the target becomes visible.
        await skippedContainer.WaitForAsync(new LocatorWaitForOptions {
            State = WaitForSelectorState.Visible
        });

        await lesson.DragToAsync(
            skippedContainer
        );

        // Verify lesson tagged successfully
        Assert.That(
            await Page
                .Locator($"[id='{lessonId}']")
                .GetAttributeAsync("class"),
            Does.Contain("is-skipped")
        );
    }

    // By Patrick Plechinger
    // Tests Mentor: LessonProgress skipped -> open
    //[Test]
    public async Task Should_Move_Lesson_From_Skipped_To_Open() {
        await LoginAsE2EMentor();

        // Open Mentors lesson status board for E2E Trainee
        await OpenBoardForE2ETrainee();

        // Find lessons with skipped class
        var skippedLessons = Page.Locator(
            ".columns--column[data-status='open'] .is-skipped"
        );
        Assert.That(
            await skippedLessons.CountAsync(),
            Is.GreaterThan(0)
        );
        var lessonId = await skippedLessons.First.GetAttributeAsync("id");
        var lesson = Page.Locator($"[id='{lessonId}']");

        // Find Open column
        var openColumn = Page.Locator(
            ".columns--column[data-status='open']"
        );

        // Drag lesson with skipped class to Open
        await lesson.DragToAsync(
            openColumn
        );

        // Verify lesson tagged successfully
        Assert.That(
            await openColumn
                .Locator($"[id='{lessonId}']")
                .GetAttributeAsync("class"),
            Does.Not.Contain("is-skipped")
        );
    }
}
