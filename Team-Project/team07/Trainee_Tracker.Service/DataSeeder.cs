using System.Globalization;
using Trainee_Tracker.Core.Entity;
using Trainee_Tracker.Core.Service;
using Trainee_Tracker.Core.Repository;
using Trainee_Tracker.Core.Enums;
using Trainee_Tracker.Repository;
using Microsoft.Extensions.DependencyInjection;

namespace Trainee_Tracker.Service;

public static class DataSeeder {

    /// <summary>
    /// Seeds the development database with users, curricula, assignments,
    /// and lesson progress data.
    /// Originally implemented by Lukas Nowak.
    /// Extended by Dennis Axt with repository-based lesson status seeding
    /// for execution without an authenticated user.
    /// </summary>
    /// <param name="serviceProvider">
    /// The application service provider used to resolve seeding dependencies.
    /// </param>
    /// <returns>A task representing the asynchronous seeding operation.</returns>
    public static async Task SeedDataAsync(IServiceProvider serviceProvider) {
        using var scope = serviceProvider.CreateScope();
        var curriculumService = scope.ServiceProvider.GetRequiredService<ICurriculumService>();
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

        var lessonProgressService =
            scope.ServiceProvider.GetRequiredService<ILessonProgressService>();

        var snapshotRepository =
            scope.ServiceProvider
                .GetRequiredService<ITraineeProgressSnapshotRepository>();

        // Seed lesson statuses directly through the repository because data
        // seeding runs as a system operation without an authenticated user.
        var lessonProgressRepository =
            scope.ServiceProvider.GetRequiredService<ILessonProgressRepository>();

        Console.WriteLine("[Service]");
        var currs = curriculumService.getAll();

        if (!curriculumService.getAll().Any()) {
            string filePath = Path.Combine(AppContext.BaseDirectory, "SeedData", "lehrplan_devop.json");

            if (File.Exists(filePath)) {
                using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                await curriculumService.CreateWithJson("Default Trainee Curriculum", fileStream);
                Console.WriteLine("[Success] Development data seeded successfully.");
            }
            else {
                Console.WriteLine($"[Warning] Seed file missing at: {filePath}. Make sure 'Copy to Output Directory' is set.");
            }
        }
        else {
            Console.WriteLine($"[Curriculums] Seeded");
        }


        if (!userService.getAll().Any()) {
            Console.WriteLine("[SEED] Seeding users into database...");

            Guid? firstCurriculumId = curriculumService.getAll().FirstOrDefault()?.id;

            if (firstCurriculumId == null) {
                Console.WriteLine("[Warning] No curriculum found to link to Trainee Vanessa. Progresses won't auto-map.");
            }

            // 1. Seed Trainees
            await userService.Create(new CreateUser
            (
                "vanessa.vital@makandra.de",
                "Vanessa",
                "Vital",
                DateOnly.ParseExact("17.07.2026", "dd.MM.yyyy", CultureInfo.InvariantCulture),
                DateOnly.ParseExact("31.03.2027", "dd.MM.yyyy", CultureInfo.InvariantCulture),
                "Trainee"
            ));
            await userService.setPassword(new SetPassword((await userService.GetByEmail("vanessa.vital@makandra.de")).guid, "VVital13!","VVital13!"));
            await userService.Create(new CreateUser
            (
                "stefan.schnupfen@makandra.de",
                "Stefan",
                "Schnupfen",
                DateOnly.ParseExact("01.05.2026", "dd.MM.yyyy", CultureInfo.InvariantCulture),
                DateOnly.ParseExact("31.10.2026", "dd.MM.yyyy", CultureInfo.InvariantCulture),
                "Trainee"
            ));
            await userService.setPassword(new SetPassword((await userService.GetByEmail("stefan.schnupfen@makandra.de")).guid, "Stefan1!","Stefan1!"));
            await userService.Create(new CreateUser
            (
                "ursula.urlaub@makandra.de",
                "Ursula",
                "Urlaub",
                DateOnly.ParseExact("01.01.2026", "dd.MM.yyyy", CultureInfo.InvariantCulture),
                DateOnly.ParseExact("31.07.2026", "dd.MM.yyyy", CultureInfo.InvariantCulture),
                "Trainee"
            ));
            await userService.setPassword(new SetPassword((await userService.GetByEmail("ursula.urlaub@makandra.de")).guid, "U1laub!","U1laub!"));

            //Test-Trainee for E2E Tests
            await userService.Create(new CreateUser
            (
                "e2e.trainee@test.com",
                "E2E",
                "Trainee",
                DateOnly.ParseExact("01.01.2026", "dd.MM.yyyy", CultureInfo.InvariantCulture),
                DateOnly.ParseExact("31.12.2026", "dd.MM.yyyy", CultureInfo.InvariantCulture),
                "Trainee"
            ));
            await userService.setPassword(new SetPassword((await userService.GetByEmail("e2e.trainee@test.com")).guid, "Test123!","Test123!"));

            // 2. Seed Mentors and Admins
            await userService.Create(new CreateUser { firstName = "Manfred", lastName = "Mental", EMail = "manfred.mental@makandra.de", role = "Mentor" });
            await userService.setPassword(new SetPassword((await userService.GetByEmail("manfred.mental@makandra.de")).guid, "Pssssst1!","Pssssst1!"));
            await userService.Create(new CreateUser { firstName = "Hans", lastName = "Hilfreich", EMail = "hans.hilfreich@makandra.de", role = "Mentor" });
            await userService.setPassword(new SetPassword((await userService.GetByEmail("hans.hilfreich@makandra.de")).guid, "Hilfe123!","Hilfe123!"));
            await userService.Create(new CreateUser { firstName = "Admin", lastName = "der echte", EMail = "admin@makandra.de", role = "Admin" });
            await userService.setPassword(new SetPassword((await userService.GetByEmail("admin@makandra.de")).guid, "Admin1!","Admin1!"));
            await userService.Create(new CreateUser { firstName = "E2E", lastName = "Mentor", EMail = "e2e.mentor@test.com", role = "Mentor" });
            await userService.setPassword(new SetPassword((await userService.GetByEmail("e2e.mentor@test.com")).guid, "Mentor123!","Mentor123!"));

            // 3. Cross-assign every Trainee to every Mentor manually
            Console.WriteLine("[SEED] Mapping Trainees to Mentors...");

            // Re-fetch fresh tracking instances from database
            var allUsers = userService.getAll().ToList();
            var mentors = allUsers.OfType<Mentor>().ToList();
            var trainees = allUsers.OfType<Trainee>().ToList();

            foreach (var mentor in mentors) {
                foreach (var trainee in trainees) {
                    userService.assignTrainee(mentor, trainee);
                }
            }
            int countAfter = userService.getAll().Count();
            Console.WriteLine($"[SEED] Done! New user count in DB: {countAfter} with all trainees cross-assigned.");


            var stefan = trainees.FirstOrDefault(t => t.Email == "stefan.schnupfen@makandra.de");
            if (stefan != null && stefan.progresses != null && stefan.progresses.Any())
            {
                int progressCountToFormat = (int)Math.Ceiling(stefan.progresses.Count * 0.3);
                var progressesToFinish = stefan.progresses.Take(progressCountToFormat);

                Console.WriteLine($"[SEED] Setting {progressCountToFormat} lessons for Stefan to Finished...");
                foreach (var progress in progressesToFinish)
                {
                    progress.status = LessonStatus.Finished;
                    lessonProgressRepository.Edit(progress);
                }
            }

            var ursula = trainees.FirstOrDefault(t => t.Email == "ursula.urlaub@makandra.de");
            if (ursula != null && ursula.progresses != null && ursula.progresses.Any())
            {
                int progressCountToFormat = (int)Math.Ceiling(ursula.progresses.Count * 0.9);
                var progressesToRated = ursula.progresses.Take(progressCountToFormat);

                Console.WriteLine($"[SEED] Setting {progressCountToFormat} lessons for Stefan to Finished...");
                foreach (var progress in progressesToRated)
                {
                    progress.status = LessonStatus.Rated;
                    lessonProgressRepository.Edit(progress);
                }
                //Künstlich Snapshots für das Chart
                var ursulaActualRemainingWork = new decimal[]
                {
                    96.25m, 95.50m, 94.75m, 94.00m, 93.00m,
                    91.75m, 90.50m, 89.00m, 87.50m, 85.75m,
                    84.00m, 82.00m, 79.75m, 76.50m, 72.75m,
                    68.50m, 63.75m, 58.50m, 52.75m, 46.50m,
                    39.75m, 33.00m, 27.00m, 21.50m, 17.00m,
                    13.50m, 11.00m, 9.50m, 8.50m
                };

                var snapshots = ursulaActualRemainingWork
                    .Select((actualProgress, weekIndex) =>
                    {
                        var weekStart = ursula.startDate.AddDays(weekIndex * 7);

                        return new TraineeProgressSnapshot
                        {
                            Id = Guid.NewGuid(),
                            TraineeId = ursula.guid,
                            Trainee = ursula,
                            WeekIndex = weekIndex,
                            WeekStartDate = weekStart,
                            ActualProgress = actualProgress,
                            CreatedAt = weekStart.ToDateTime(TimeOnly.MinValue)
                        };
                     })
                    .ToList();

                await snapshotRepository.AddRangeAsync(snapshots);
                await snapshotRepository.SaveChangesAsync();
            }

            var e2e = trainees.FirstOrDefault(t => t.Email == "e2e.trainee@test.com");
            if (e2e != null && e2e.progresses != null && e2e.progresses.Any())
            {
                var allStatuses = Enum.GetValues(typeof(LessonStatus)).Cast<LessonStatus>().ToArray();
                var progressList = e2e.progresses.ToList();

                Console.WriteLine($"[SEED] Distributing {allStatuses.Length} unique statuses across {progressList.Count} lessons for e2eTrainee...");

                for (int i = 0; i < progressList.Count; i++)
                {
                    var statusToAssign = allStatuses[i % allStatuses.Length];
                    progressList[i].status = statusToAssign;
                    lessonProgressRepository.Edit(progressList[i]);
                }
            }
        }
    }
}