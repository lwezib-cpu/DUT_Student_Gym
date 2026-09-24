namespace GymNet.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddTrainerClassesEquipment : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ClassBookings",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        GymClassId = c.Int(nullable: false),
                        UserId = c.String(nullable: false, maxLength: 128),
                        BookedAt = c.DateTime(nullable: false),
                        Status = c.String(maxLength: 20),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.GymClasses", t => t.GymClassId)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => t.GymClassId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.GymClasses",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TrainerId = c.String(nullable: false, maxLength: 128),
                        Title = c.String(nullable: false, maxLength: 100),
                        Description = c.String(maxLength: 500),
                        StartTime = c.DateTime(nullable: false),
                        DurationMinutes = c.Int(nullable: false),
                        Capacity = c.Int(nullable: false),
                        Status = c.String(maxLength: 20),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.TrainerId)
                .Index(t => t.TrainerId);
            
            CreateTable(
                "dbo.Equipments",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 100),
                        Description = c.String(maxLength: 300),
                        TotalQuantity = c.Int(nullable: false),
                        ReservationFee = c.Decimal(nullable: false, precision: 18, scale: 2),
                        OveragePerHourFee = c.Decimal(nullable: false, precision: 18, scale: 2),
                        IsActive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.EquipmentBookings",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        EquipmentId = c.Int(nullable: false),
                        UserId = c.String(nullable: false, maxLength: 128),
                        ReservedAt = c.DateTime(nullable: false),
                        ExpectedDurationMinutes = c.Int(nullable: false),
                        ExpectedReturnAt = c.DateTime(nullable: false),
                        ActualReturnAt = c.DateTime(),
                        ReservationFeeCharged = c.Decimal(nullable: false, precision: 18, scale: 2),
                        OverageFeeCharged = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Status = c.String(maxLength: 20),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Equipments", t => t.EquipmentId)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => t.EquipmentId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.TrainerFeedbacks",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TrainerId = c.String(nullable: false, maxLength: 128),
                        MemberId = c.String(nullable: false, maxLength: 128),
                        Comment = c.String(nullable: false, maxLength: 1000),
                        Rating = c.Int(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.MemberId)
                .ForeignKey("dbo.AspNetUsers", t => t.TrainerId)
                .Index(t => t.TrainerId)
                .Index(t => t.MemberId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.TrainerFeedbacks", "TrainerId", "dbo.AspNetUsers");
            DropForeignKey("dbo.TrainerFeedbacks", "MemberId", "dbo.AspNetUsers");
            DropForeignKey("dbo.EquipmentBookings", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.EquipmentBookings", "EquipmentId", "dbo.Equipments");
            DropForeignKey("dbo.ClassBookings", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.ClassBookings", "GymClassId", "dbo.GymClasses");
            DropForeignKey("dbo.GymClasses", "TrainerId", "dbo.AspNetUsers");
            DropIndex("dbo.TrainerFeedbacks", new[] { "MemberId" });
            DropIndex("dbo.TrainerFeedbacks", new[] { "TrainerId" });
            DropIndex("dbo.EquipmentBookings", new[] { "UserId" });
            DropIndex("dbo.EquipmentBookings", new[] { "EquipmentId" });
            DropIndex("dbo.GymClasses", new[] { "TrainerId" });
            DropIndex("dbo.ClassBookings", new[] { "UserId" });
            DropIndex("dbo.ClassBookings", new[] { "GymClassId" });
            DropTable("dbo.TrainerFeedbacks");
            DropTable("dbo.EquipmentBookings");
            DropTable("dbo.Equipments");
            DropTable("dbo.GymClasses");
            DropTable("dbo.ClassBookings");
        }
    }
}
