using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Data;
using ApplicationCore.Interfaces;
using Infrastructure;
using Infrastructure.Services;
using Microsoft.AspNetCore.Identity.UI.Services;

var builder = WebApplication.CreateBuilder(args);


var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();


builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddRazorPages();


builder.Services.AddScoped<UnitOfWork>();
//builder.Services.AddScoped<DbInitializer>();
//builder.Services.AddSingleton<IEmailSender, EmailSender>();

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();


//SeedDatabase();

/*void SeedDatabase()
{
    using var scope = app.Services.CreateScope();
    var initializer = scope.ServiceProvider.GetRequiredService<DbInitializer>();
    initializer.Initialize();
}*/

app.Run();
