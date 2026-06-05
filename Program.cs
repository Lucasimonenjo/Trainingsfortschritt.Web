using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.JSInterop;
using Trainingsfortschritt.Core.Abstractions;
using Trainingsfortschritt.Core.Services;
using Trainingsfortschritt.Web;
using Trainingsfortschritt.Web.Services;
using Trainingsfortschritt.Web.ViewModels;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// =========================
// BASE PATH FIX (WICHTIG)
// =========================
var baseHref = "/Trainingsfortschritt.Web/";
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// =========================
// HTTP (MUSS BaseAddress korrekt haben)
// =========================
builder.Services.AddScoped(sp =>
    new HttpClient
    {
        BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
    });

// =========================
// CORE SERVICES
// =========================
builder.Services.AddSingleton<IPlatformServices, WebPlatformServices>();
builder.Services.AddScoped<IndexedDbService>();
builder.Services.AddScoped<IDatabaseService, WebDatabaseService>();
builder.Services.AddScoped<IStorageService, WebStorageService>();

// =========================
// ACHIEVEMENTS
// =========================
builder.Services.AddScoped<AchievementService>();

// =========================
// VIEWMODELS
// =========================
builder.Services.AddScoped<HomeViewModel>();
builder.Services.AddScoped<ExerciseDetailViewModel>();
builder.Services.AddScoped<AddSetViewModel>();
builder.Services.AddScoped<SetTargetViewModel>();
builder.Services.AddScoped<SettingsViewModel>();
builder.Services.AddScoped<EditSetViewModel>();
builder.Services.AddScoped<ProgressChartViewModel>();
builder.Services.AddScoped<PersonalRecordsViewModel>();
builder.Services.AddScoped<StatisticsDashboardViewModel>();
builder.Services.AddScoped<GoalHistoryViewModel>();
builder.Services.AddScoped<AchievementsViewModel>();

// =========================
// NOTIFICATION
// =========================
builder.Services.AddScoped<INotificationService, WebNotificationService>();
builder.Services.AddScoped<AchievementPopupService>();

// =========================
// JS CONFIRM
// =========================
builder.Services.AddScoped<Func<string, Task<bool>>>(sp =>
{
    var js = sp.GetRequiredService<IJSRuntime>();
    return message => js.InvokeAsync<bool>("confirm", message).AsTask();
});

// =========================
// ERROR HANDLING
// =========================
AppDomain.CurrentDomain.UnhandledException += (s, e) =>
{
    Console.WriteLine(e.ExceptionObject);
};

var app = builder.Build();
await app.RunAsync();
