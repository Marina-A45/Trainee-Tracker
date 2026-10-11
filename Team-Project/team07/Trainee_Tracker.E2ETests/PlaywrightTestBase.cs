using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Playwright;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Trainee_Tracker.Tests;

// Author: Marina Achner

/// <summary>
/// Base class for end-to-end tests using Playwright.
/// Provides shared browser setup, application factory configuration,
/// and cleanup logic for derived test classes.
/// </summary>
public class PlaywrightTestBase {
    /// <summary>
    /// Factory used to host the web application on a real Kestrel server.
    /// </summary>
    protected CustomWebApplicationFactory<Program> Factory = null!;

    /// <summary>
    /// Playwright instance used to control browser automation.
    /// </summary>
    protected IPlaywright PlaywrightInstance = null!;

    /// <summary>
    /// Browser instance used during end-to-end tests.
    /// </summary>
    protected IBrowser Browser = null!;

    /// <summary>
    /// Browser context containing isolated browser state for a test run.
    /// </summary>
    protected IBrowserContext Context = null!;

    /// <summary>
    /// Page instance used to interact with the application UI.
    /// </summary>
    protected IPage Page = null!;


    /// <summary>
    /// Base URL used by the end-to-end tests to access the application.
    /// </summary>
    protected const string BaseUrl = "http://localhost:5210";


    /// <summary>
    /// Starts the application once per test class.
    /// UseKestrel makes the factory host the app on a real TCP port
    /// (instead of the in-memory TestServer), so the Playwright browser
    /// can actually reach it.
    /// </summary>
    [OneTimeSetUp]
    public void OneTimeSetUp() {
        Factory = new CustomWebApplicationFactory<Program>();
        Factory.UseKestrel(5210);
        Factory.StartServer();
    }


    /// <summary>
    /// Initializes the test environment before each test execution.
    /// Starts Playwright, launches the browser and creates a new
    /// browser context and page.
    /// </summary>
    /// <returns>
    /// A task representing the asynchronous setup operation.
    /// </returns>
    [SetUp]
    public async Task Setup() {
        // Initialize Playwright automation.
        PlaywrightInstance = await Playwright.CreateAsync();

        // Launch Chromium browser in headless mode.
        Browser = await PlaywrightInstance.Chromium.LaunchAsync(
            new BrowserTypeLaunchOptions {
                Headless = true,
                Channel = "chrome"
            });

        // Create isolated browser context and new page for testing.
        Context = await Browser.NewContextAsync();
        Page = await Context.NewPageAsync();
    }


    /// <summary>
    /// Cleans up browser resources after each test execution.
    /// </summary>
    /// <returns>
    /// A task representing the asynchronous cleanup operation.
    /// </returns>
    [TearDown]
    public async Task TearDown() {
        // Close browser instance after test execution.
        await Browser.CloseAsync();

        // Release Playwright resources.
        PlaywrightInstance.Dispose();
    }


    /// <summary>
    /// Shuts down the application and cleans up the test database
    /// once all tests of the class have finished.
    /// </summary>
    [OneTimeTearDown]
    public void OneTimeTearDown() {
        Factory.Dispose();
    }
}
