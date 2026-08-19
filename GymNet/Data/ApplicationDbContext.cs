using Microsoft.AspNet.Identity.EntityFramework;
using GymNet.Models;
using System.Data.Entity;

namespace GymNet.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext() : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        // Add new DbSets
        public DbSet<MembershipPlan> MembershipPlans { get; set; }
        public DbSet<MemberMembership> MemberMemberships { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<CheckIn> CheckIns { get; set; }
        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure relationships
            modelBuilder.Entity<MemberMembership>()
                .HasRequired(m => m.User)
                .WithMany()
                .HasForeignKey(m => m.UserId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Payment>()
                .HasRequired(p => p.MemberMembership)
                .WithMany(m => m.Payments)
                .HasForeignKey(p => p.MemberMembershipId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<CheckIn>()
                .HasRequired(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .WillCascadeOnDelete(false);
        }

    }
}