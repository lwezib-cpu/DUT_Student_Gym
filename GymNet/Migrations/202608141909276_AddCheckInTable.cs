namespace GymNet.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddCheckInTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CheckIns",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        CheckInTime = c.DateTime(nullable: false),
                        CheckInMethod = c.String(maxLength: 50),
                        QRCode = c.String(maxLength: 100),
                        Notes = c.String(maxLength: 200),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => t.UserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.CheckIns", "UserId", "dbo.AspNetUsers");
            DropIndex("dbo.CheckIns", new[] { "UserId" });
            DropTable("dbo.CheckIns");
        }
    }
}
