namespace GymNet.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddCheckOutTracking : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.BodyMeasurements",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        RecordedAt = c.DateTime(nullable: false),
                        WeightKg = c.Decimal(nullable: false, precision: 18, scale: 2),
                        HeightCm = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Bmi = c.Decimal(nullable: false, precision: 18, scale: 2),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.FitnessGoals",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        GoalType = c.String(nullable: false, maxLength: 20),
                        StartWeightKg = c.Decimal(nullable: false, precision: 18, scale: 2),
                        TargetWeightKg = c.Decimal(nullable: false, precision: 18, scale: 2),
                        StartDate = c.DateTime(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => t.UserId);
            
            AddColumn("dbo.CheckIns", "CheckOutTime", c => c.DateTime());
            AddColumn("dbo.CheckIns", "DurationMinutes", c => c.Int());
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.FitnessGoals", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.BodyMeasurements", "UserId", "dbo.AspNetUsers");
            DropIndex("dbo.FitnessGoals", new[] { "UserId" });
            DropIndex("dbo.BodyMeasurements", new[] { "UserId" });
            DropColumn("dbo.CheckIns", "DurationMinutes");
            DropColumn("dbo.CheckIns", "CheckOutTime");
            DropTable("dbo.FitnessGoals");
            DropTable("dbo.BodyMeasurements");
        }
    }
}
