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
        public DbSet<BodyMeasurement> BodyMeasurements { get; set; }
        public DbSet<FitnessGoal> FitnessGoals { get; set; }
        public DbSet<GymClass> GymClasses { get; set; }
        public DbSet<ClassBooking> ClassBookings { get; set; }
        public DbSet<TrainerFeedback> TrainerFeedbacks { get; set; }
        public DbSet<Equipment> Equipment { get; set; }
        public DbSet<EquipmentBooking> EquipmentBookings { get; set; }
        public DbSet<WorkoutLog> WorkoutLogs { get; set; }
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

            modelBuilder.Entity<BodyMeasurement>()
                .HasRequired(m => m.User)
                .WithMany()
                .HasForeignKey(m => m.UserId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<FitnessGoal>()
                .HasRequired(g => g.User)
                .WithMany()
                .HasForeignKey(g => g.UserId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<GymClass>()
                .HasRequired(c => c.Trainer)
                .WithMany()
                .HasForeignKey(c => c.TrainerId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ClassBooking>()
                .HasRequired(b => b.GymClass)
                .WithMany(c => c.Bookings)
                .HasForeignKey(b => b.GymClassId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ClassBooking>()
                .HasRequired(b => b.User)
                .WithMany()
                .HasForeignKey(b => b.UserId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TrainerFeedback>()
                .HasRequired(f => f.Trainer)
                .WithMany()
                .HasForeignKey(f => f.TrainerId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TrainerFeedback>()
                .HasRequired(f => f.Member)
                .WithMany()
                .HasForeignKey(f => f.MemberId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<EquipmentBooking>()
                .HasRequired(b => b.Equipment)
                .WithMany(e => e.Bookings)
                .HasForeignKey(b => b.EquipmentId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<EquipmentBooking>()
                .HasRequired(b => b.User)
                .WithMany()
                .HasForeignKey(b => b.UserId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<WorkoutLog>()
                .HasRequired(w => w.User)
                .WithMany()
                .HasForeignKey(w => w.UserId)
                .WillCascadeOnDelete(false);
        }

    }
}