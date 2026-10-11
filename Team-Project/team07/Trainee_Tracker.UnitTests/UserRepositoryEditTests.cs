using Trainee_Tracker.Core.Entity;
using Trainee_Tracker.Repository;

namespace Trainee_Tracker.UnitTests;

/// <summary>
/// Regression tests for UserRepository.Edit, which was rewritten from a
/// delete+recreate strategy to in-place field updates.
/// Author: Carl Böhm-Sturm
/// </summary>
public class UserRepositoryEditTests : IDisposable {
    private readonly TraineeTrackerContext context;
    private readonly UserRepository repository;

    public UserRepositoryEditTests() {
        context = new TraineeTrackerContext(true);
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();

        repository = new UserRepository(context);
    }

    [Fact]
    public void Edit_ExistingUser_UpdatesFieldsAndPersists() {
        var admin = new Admin("original@test.com", "Original", "Name");
        context.users.Add(admin);
        context.SaveChanges();

        admin.Email = "changed@test.com";
        admin.UserName = "changed@test.com";
        admin.firstName = "Changed";
        admin.lastName = "Person";
        admin.active = false;

        repository.Edit(admin);

        var reloaded = repository.GetById(admin.guid);
        Assert.Equal("changed@test.com", reloaded.Email);
        Assert.Equal("Changed", reloaded.firstName);
        Assert.Equal("Person", reloaded.lastName);
        Assert.False(reloaded.active);
    }

    [Fact]
    public void Edit_UserNotInDatabase_ThrowsKeyNotFoundException() {
        // A detached user that was never persisted - simulates a stale
        // or forged Id reaching the repository.
        var ghost = new Admin("ghost@test.com", "Ghost", "User");

        Assert.Throws<KeyNotFoundException>(() => repository.Edit(ghost));
    }

    public void Dispose() {
        context.Database.EnsureDeleted();
        context.Dispose();
    }
}
