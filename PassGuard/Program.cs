using Microsoft.EntityFrameworkCore;
using PassGuard.BLL;
using PassGuard.DAL;

namespace PassGuard
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            // Database connection
            builder.Services.AddDbContext<PassGuardContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

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

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
