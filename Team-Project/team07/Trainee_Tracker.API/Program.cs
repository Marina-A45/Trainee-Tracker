using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Timers;
using Trainee_Tracker.Core.Entity;
using Trainee_Tracker.Core.Repository;
using Trainee_Tracker.Core.Service;
using Trainee_Tracker.Repository;
using Trainee_Tracker.Service;
using Timer = System.Threading.Timer;
// using Trainee_Tracker.Reposi
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.WebHost.UseStaticWebAssets();
builder.Services.AddSignalR();
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<TraineeTrackerContext>();
builder.Services.AddTransient<ICurriculumRepository, CurriculumRepository>();
builder.Services.AddTransient<ILessonProgressRepository, LessonProgressRepository>();
builder.Services.AddTransient<ITraineeRepository, TraineeRepository>();
builder.Services.AddTransient<IPlannerRepository, PlannerRepository>();
builder.Services.AddTransient<IWorkingHoursRepository, WorkingHoursRepository>();
builder.Services.AddTransient<IUserRepository, UserRepository>();
builder.Services.AddTransient<ITraineeProgressRepository, TraineeProgressRepository>();
builder.Services.AddTransient<IFeedbackRepository, FeedbackRepository>();
builder.Services.AddTransient<ITraineeProgressSnapshotRepository, TraineeProgressSnapshotRepository>();

builder.Services.AddTransient<ICurriculumService, CurriculumService>();
builder.Services.AddTransient<ILessonProgressService, LessonProgressService>();
builder.Services.AddTransient<IFeedbackService, FeedbackService>();
builder.Services.AddTransient<ITraineeService, TraineeService>();
builder.Services.AddTransient<IPlannerService, PlannerService>();
builder.Services.AddTransient<ILoginService, LoginService>();
builder.Services.AddTransient<IUserService, UserService>();
builder.Services.AddTransient<ITraineeProgressService, TraineeProgressService>();
builder.Services.AddTransient<IBurnDownChartService, BurnDownChartService>();

builder.Services.AddTransient<IDateTimeTrackingApiClient, DateTimeTrackingApiClient>();


builder.Services.AddSingleton<IDateTimeTrackingService, DateTimeTrackingService>();

builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();


builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<TraineeTrackerContext>()
    .AddDefaultTokenProviders();

builder.Services.Configure<TimeTrackingApiSettings>(
    builder.Configuration.GetSection("TimeTrackingApi"));

builder.Services.AddAntiforgery(options => options.HeaderName = "X-CSRF-TOKEN");

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<TraineeTrackerContext>();
    var service = scope.ServiceProvider.GetRequiredService<IUserService>();
    context.Database.EnsureCreated();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    var roles = new[] { "Admin", "Mentor", "Trainee" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }
}
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
    Console.WriteLine($"[INFO] In Development");
}
else
{
    Console.WriteLine($"[INFO] Not In Development");
}

await DataSeeder.SeedDataAsync(app.Services);

var dateTimeTrackingService = app.Services.GetRequiredService<IDateTimeTrackingService>();

var timer = new Timer(
    async _ => await dateTimeTrackingService.UpdateWorkingtime(),
    null,
    TimeSpan.Zero,
    TimeSpan.FromHours(2));

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapHub<CurriculumHub>("/curriculumHub");

app.MapControllers();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
