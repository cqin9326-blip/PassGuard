using Microsoft.EntityFrameworkCore;
using PassGuard.Models;

namespace PassGuard.DAL
{
    public class PassGuardContext : DbContext
    {
        public PassGuardContext(DbContextOptions<PassGuardContext> options)
            : base(options)
        {
        }

        public DbSet<Estate> Estates { get; set; }
        public DbSet<Home> Homes { get; set; }
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
                .Property(h => h.OwnerName)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<Home>()
                .Property(h => h.Address)
                .IsRequired()
                .HasMaxLength(200);

            modelBuilder.Entity<Home>()
                .HasOne(h => h.Estate)
                .WithMany(e => e.Homes)
                .HasForeignKey(h => h.EstateId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<VisitPass>()
                .HasKey(v => v.VisitPassId);

            modelBuilder.Entity<VisitPass>()
                .Property(v => v.VisitorName)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<VisitPass>()
                .Property(v => v.VisitorPhone)
                .HasMaxLength(30);

            modelBuilder.Entity<VisitPass>()
                .Property(v => v.Status)
                .IsRequired()
                .HasMaxLength(30);

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
                .HasOne(g => g.VisitPass)
                .WithMany(v => v.GateCheckIns)
                .HasForeignKey(g => g.VisitPassId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}


