using Microsoft.EntityFrameworkCore;
using System.IO;
using Trainee_Tracker.Core.Entity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
namespace Trainee_Tracker.Repository;

public class TraineeTrackerContext
    : IdentityDbContext<User>{
    public DbSet<User> users { get; set; }
    public DbSet<MentorAssignment> assignments { get; set; }
    public DbSet<LessonProgress> progresses { get; set; }
    public DbSet<Lesson> lessons { get; set; }
    public DbSet<Curriculum> curriculums { get; set; }
    public DbSet<Feedback> feedbacks { get; set; }
    public DbSet<WorkingHoursCache> workingHoursCache { get; set; }
    public DbSet<TraineeProgress> TraineeProgress { get; set; }
    public DbSet<TraineeProgressSnapshot> TraineeProgressSnapshots { get; set; }
    private bool useInMemory;
private readonly string inMemoryDatabaseName;
    public TraineeTrackerContext() {
        useInMemory = false;
    }
    public TraineeTrackerContext(bool useInMemory) {
        this.useInMemory = useInMemory;
    this.inMemoryDatabaseName = Guid.NewGuid().ToString();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
        if (useInMemory) {
        optionsBuilder.UseInMemoryDatabase(inMemoryDatabaseName);
        }
        else {
            optionsBuilder.UseSqlite($"Data Source={CreateDbPathAndEnsureCreated()}");
        }
    }

    private static string CreateDbPathAndEnsureCreated() {
        var projectRoot = Environment.CurrentDirectory;
        var dbPath = Path.Combine(projectRoot, "persistence", "TraineeTracker.db");
        var directory = Path.GetDirectoryName(dbPath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }
        return dbPath;
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

    modelBuilder.Entity<MentorAssignment>()
        .HasKey(ma => new { ma.MentorId, ma.TraineeId });

modelBuilder.Entity<MentorAssignment>()
    .HasOne(ma => ma.mentor)
    .WithMany(m => m.Assignments)
    .HasForeignKey(ma => ma.MentorId);

modelBuilder.Entity<MentorAssignment>()
    .HasOne(ma => ma.trainee)
    .WithMany(t => t.MentorAssignments)
    .HasForeignKey(ma => ma.TraineeId);

    modelBuilder.Entity<User>()
        .HasAlternateKey(u => u.guid);

    modelBuilder.Entity<TraineeProgressSnapshot>()
        .HasOne(s => s.Trainee)
        .WithMany(t => t.ProgressSnapshots)
        .HasPrincipalKey(t => t.guid)
        .HasForeignKey(s => s.TraineeId);

    modelBuilder.Entity<TraineeProgress>()
        .HasOne(p => p.Trainee)
        .WithMany()
        .HasPrincipalKey(u => u.guid)
        .HasForeignKey(p => p.TraineeId);
}
}