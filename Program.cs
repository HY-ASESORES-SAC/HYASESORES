using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using proyectoIngSoft.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>() // ✅ Habilitar roles
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews();

// 👇 Configurar sesiones antes del builder.Build()
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// 👇 Habilitar sesión
app.UseSession();

// ✅ Asignar el rol "Jefe" al usuario oskar@oskar
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

    // Crear el rol "Jefe" si no existe
    if (!await roleManager.RoleExistsAsync("Jefe"))
    {
        await roleManager.CreateAsync(new IdentityRole("Jefe"));
    }

    // Asignar el rol "Jefe" al usuario oskar@oskar
    var jefe = await userManager.FindByEmailAsync("oskar@oskar");
    if (jefe != null && !await userManager.IsInRoleAsync(jefe, "Jefe"))
    {
        await userManager.AddToRoleAsync(jefe, "Jefe");
    }
}

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
