namespace GymNet.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddDeclineReasonMaintenanceWorkoutLog : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.WorkoutLogs",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        ExerciseName = c.String(nullable: false, maxLength: 100),
                        Sets = c.Int(nullable: false),
                        Reps = c.Int(nullable: false),
                        WeightKg = c.Decimal(precision: 18, scale: 2),
                        LoggedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => t.UserId);
            
            AddColumn("dbo.Equipments", "IsUnderMaintenance", c => c.Boolean(nullable: false));
            AddColumn("dbo.Equipments", "MaintenanceNote", c => c.String(maxLength: 200));
            AddColumn("dbo.EquipmentBookings", "DeclineReason", c => c.String(maxLength: 300));
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.WorkoutLogs", "UserId", "dbo.AspNetUsers");
            DropIndex("dbo.WorkoutLogs", new[] { "UserId" });
            DropColumn("dbo.EquipmentBookings", "DeclineReason");
            DropColumn("dbo.Equipments", "MaintenanceNote");
            DropColumn("dbo.Equipments", "IsUnderMaintenance");
            DropTable("dbo.WorkoutLogs");
        }
    }
}
