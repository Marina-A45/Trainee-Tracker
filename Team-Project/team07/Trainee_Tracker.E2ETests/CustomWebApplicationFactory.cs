using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Trainee_Tracker.Repository;

namespace Trainee_Tracker.Tests;

// Author: Lukas Nowak

/// <summary>
/// WebApplicationFactory that hosts the application for end-to-end tests
/// with an isolated, temporary SQLite database instead of the real one.
/// </summary>
/// <typeparam name="TProgram">Entry point of the application under test.</typeparam>
public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class {
    /// <summary>
    /// Path of the temporary SQLite database used for this test run.
    /// </summary>
    private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"e2e_test_{Guid.NewGuid()}.db");

    /// <summary>
    /// Runs before the standard Program.cs and makes sure a fresh database
    /// with exactly the data from the DataSeeder is used.
    /// </summary>
    /// <param name="builder">Web host builder of the application under test.</param>
    protected override void ConfigureWebHost(IWebHostBuilder builder) {
        // Force the app to run in the 'Testing' environment
        builder.UseEnvironment("Testing");
        builder.ConfigureServices(services => {
            // The app registers TraineeTrackerContext as a plain scoped service that
            // configures itself in OnConfiguring (pointing at the real persistence DB).
            // Swap it for a context that uses an isolated, temporary SQLite file instead.
            services.RemoveAll<TraineeTrackerContext>();
            services.AddScoped<TraineeTrackerContext>(_ => new E2ETraineeTrackerContext(_dbPath));
            // Program.cs does its normal startup sequence (EnsureCreated + DataSeeder).
        });
    }

    /// <summary>
    /// Cleans up the temporary SQLite files when the test run is complete.
    /// </summary>
    /// <param name="disposing">Whether managed resources should be disposed.</param>
    protected override void Dispose(bool disposing) {
        base.Dispose(disposing);
        SqliteConnection.ClearAllPools();
        foreach (var file in new[] { _dbPath, $"{_dbPath}-wal", $"{_dbPath}-shm" }) {
            try { File.Delete(file); } catch { /* Ignore */ }
        }
    }
}

/// <summary>
/// TraineeTrackerContext that stores its data in an isolated SQLite file
/// instead of persistence/TraineeTracker.db.
/// </summary>
public class E2ETraineeTrackerContext : TraineeTrackerContext {
    /// <summary>
    /// Path of the SQLite database file used by this context.
    /// </summary>
    private readonly string _dbPath;

    /// <summary>
    /// Creates a context that stores its data at the given SQLite file path.
    /// </summary>
    /// <param name="dbPath">Path of the SQLite database file.</param>
    public E2ETraineeTrackerContext(string dbPath) {
        _dbPath = dbPath;
    }

    /// <summary>
    /// Points the context at the isolated test database.
    /// </summary>
    /// <param name="optionsBuilder">Options builder to configure.</param>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
        optionsBuilder.UseSqlite($"Data Source={_dbPath}");
    }
}
