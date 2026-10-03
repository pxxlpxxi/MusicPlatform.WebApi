using MusicPlatform.Application.Services;
using MusicPlatform.Data;
using MusicPlatform.Services;
using MusicPlatform.WebApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy("WebAdmin", policy =>
    {
        var webAdminUrl = builder.Configuration["WebAdminUrl"];

        policy
            .WithOrigins(webAdminUrl!)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});


builder.Services.AddScoped<IMusicPlatformContext, MusicPlatformContext>();

builder.Services.AddScoped<SongService>();
builder.Services.AddScoped<SongApplicationService>();

builder.Services.AddScoped<DatabaseTableService>();

var app = builder.Build();

app.UseCors("WebAdmin");

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
