using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PassGuard.Models;

namespace PassGuard.DAL
{
    public class PassGuardContext : IdentityDbContext<ApplicationUser>
    {
        public PassGuardContext(DbContextOptions<PassGuardContext> options)
            : base(options)
        {
        }

        public DbSet<Estate> Estates { get; set; }
        public DbSet<Home> Homes { get; set; }
        public DbSet<Visitor> Visitors { get; set; }
        public DbSet<VisitPass> VisitPasses { get; set; }
        public DbSet<GateCheckIn> GateCheckIns { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Estate>()
                .HasKey(e => e.EstateId);

            modelBuilder.Entity<Estate>()
                .Property(e => e.EstateName)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<Home>()
                .HasKey(h => h.HomeId);

            modelBuilder.Entity<Home>()
                .Property(h => h.OwnerUserId)
                .IsRequired()
                .HasMaxLength(450);

            modelBuilder.Entity<Home>()
                .Property(h => h.Address)
                .IsRequired()
                .HasMaxLength(200);

            modelBuilder.Entity<Home>()
                .HasOne(h => h.Estate)
                .WithMany(e => e.Homes)
                .HasForeignKey(h => h.EstateId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Visitor>()
                .HasKey(v => v.VisitorId);

            modelBuilder.Entity<Visitor>()
                .Property(v => v.FullName)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<Visitor>()
                .Property(v => v.Phone)
                .IsRequired()
                .HasMaxLength(30);

            modelBuilder.Entity<VisitPass>()
                .HasKey(v => v.VisitPassId);

            modelBuilder.Entity<VisitPass>()
                .Property(v => v.CodeHash)
                .IsRequired()
                .HasMaxLength(256);

            modelBuilder.Entity<VisitPass>()
                .Property(v => v.CreatedByUserId)
                .IsRequired()
                .HasMaxLength(450);

            modelBuilder.Entity<VisitPass>()
                .Property(v => v.Status)
                .IsRequired()
                .HasMaxLength(30);

            modelBuilder.Entity<VisitPass>()
                .HasOne(v => v.Visitor)
                .WithMany(v => v.VisitPasses)
                .HasForeignKey(v => v.VisitorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<VisitPass>()
                .HasOne(v => v.Home)
                .WithMany(h => h.VisitPasses)
                .HasForeignKey(v => v.HomeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<GateCheckIn>()
                .HasKey(g => g.GateCheckInId);

            modelBuilder.Entity<GateCheckIn>()
                .Property(g => g.Result)
                .IsRequired()
                .HasMaxLength(30);

            modelBuilder.Entity<GateCheckIn>()
                .Property(g => g.Note)
                .HasMaxLength(300);

            modelBuilder.Entity<GateCheckIn>()
                .Property(g => g.SecurityUserId)
                .IsRequired()
                .HasMaxLength(450);

            modelBuilder.Entity<GateCheckIn>()
                .HasOne(g => g.VisitPass)
                .WithOne(v => v.GateCheckIn)
                .HasForeignKey<GateCheckIn>(g => g.VisitPassId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<GateCheckIn>()
                .HasIndex(g => g.VisitPassId)
                .IsUnique();
        }
    }
}
