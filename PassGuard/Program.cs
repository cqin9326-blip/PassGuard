using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PassGuard.BLL;
using PassGuard.DAL;
using PassGuard.Infrastructure;

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
            builder.Services.AddScoped<GateCheckInService>();



            // Services
            builder.Services.AddScoped<EstateRepository>();
            builder.Services.AddScoped<HomeRepository>();
            builder.Services.AddScoped<VisitorRepository>();
            builder.Services.AddScoped<VisitPassRepository>();
            builder.Services.AddScoped<GateCheckInRepository>();
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
            app.UseAuthorization();

            await IdentitySeedData.SeedAsync(app.Services);

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=VisitPass}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
