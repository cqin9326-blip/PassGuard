using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PassGuard.BLL;
using PassGuard.DAL;
using PassGuard.Infrastructure;
using System.Security.Claims;

namespace PassGuard
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            // Database connection
            builder.Services.AddDbContext<PassGuardContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services
                .AddIdentity<ApplicationUser, IdentityRole>(options =>
                {
                    options.Password.RequireDigit = true;
                    options.Password.RequireUppercase = true;
                    options.Password.RequireLowercase = true;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequiredLength = 8;
                })
                .AddEntityFrameworkStores<PassGuardContext>()
                .AddDefaultTokenProviders();

            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Account/Login";
                options.AccessDeniedPath = "/Account/AccessDenied";
            });

            // Repositories（数据层）
            builder.Services.AddScoped<EstateService>();
            builder.Services.AddScoped<HomeService>();
            builder.Services.AddScoped<VisitorService>();
            builder.Services.AddScoped<VisitPassService>();
            builder.Services.AddScoped<PassCodeService>();
            builder.Services.AddScoped<GateCheckInService>();
            builder.Services.AddScoped<AuditLogService>();



            // Services
            builder.Services.AddScoped<EstateRepository>();
            builder.Services.AddScoped<HomeRepository>();
            builder.Services.AddScoped<VisitorRepository>();
            builder.Services.AddScoped<VisitPassRepository>();
            builder.Services.AddScoped<GateCheckInRepository>();
            builder.Services.AddScoped<AuditLogRepository>();
            builder.Services.AddScoped<DashboardService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();

            app.Use(async (context, next) =>
            {
                if (context.User.Identity?.IsAuthenticated == true)
                {
                    string? path = context.Request.Path.Value;
                    bool isPasswordChangePath = string.Equals(path, "/Account/ChangePassword", StringComparison.OrdinalIgnoreCase);
                    bool isLogoutPath = string.Equals(path, "/Account/Logout", StringComparison.OrdinalIgnoreCase);
                    bool isAccessDeniedPath = string.Equals(path, "/Account/AccessDenied", StringComparison.OrdinalIgnoreCase);

                    if (!isPasswordChangePath && !isLogoutPath && !isAccessDeniedPath)
                    {
                        string? userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

                        if (!string.IsNullOrWhiteSpace(userId))
                        {
                            using IServiceScope scope = app.Services.CreateScope();
                            UserManager<ApplicationUser> userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                            ApplicationUser? user = await userManager.FindByIdAsync(userId);

                            if (user?.MustChangePassword == true)
                            {
                                context.Response.Redirect("/Account/ChangePassword");
                                return;
                            }
                        }
                    }
                }

                await next();
            });

            app.UseAuthorization();

            await IdentitySeedData.SeedAsync(app.Services);

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=VisitPass}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
