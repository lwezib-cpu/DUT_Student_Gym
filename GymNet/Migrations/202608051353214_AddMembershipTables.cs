namespace GymNet.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddMembershipTables : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.MemberMemberships",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        MembershipPlanId = c.Int(nullable: false),
                        StartDate = c.DateTime(nullable: false),
                        EndDate = c.DateTime(nullable: false),
                        Status = c.String(nullable: false, maxLength: 20),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.MembershipPlans", t => t.MembershipPlanId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => t.UserId)
                .Index(t => t.MembershipPlanId);
            
            CreateTable(
                "dbo.MembershipPlans",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 100),
                        Description = c.String(nullable: false, maxLength: 500),
                        Price = c.Decimal(nullable: false, precision: 18, scale: 2),
                        DurationInMonths = c.Int(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Payments",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        MemberMembershipId = c.Int(nullable: false),
                        PaymentMethod = c.String(nullable: false, maxLength: 50),
                        Amount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Status = c.String(nullable: false, maxLength: 50),
                        PaymentDate = c.DateTime(nullable: false),
                        CardHolderName = c.String(maxLength: 100),
                        CardNumber = c.String(maxLength: 20),
                        ExpiryDate = c.String(maxLength: 5),
                        BankName = c.String(maxLength: 100),
                        AccountNumber = c.String(maxLength: 20),
                        TransactionReference = c.String(maxLength: 50),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.MemberMemberships", t => t.MemberMembershipId)
                .Index(t => t.MemberMembershipId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.MemberMemberships", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Payments", "MemberMembershipId", "dbo.MemberMemberships");
            DropForeignKey("dbo.MemberMemberships", "MembershipPlanId", "dbo.MembershipPlans");
            DropIndex("dbo.Payments", new[] { "MemberMembershipId" });
            DropIndex("dbo.MemberMemberships", new[] { "MembershipPlanId" });
            DropIndex("dbo.MemberMemberships", new[] { "UserId" });
            DropTable("dbo.Payments");
            DropTable("dbo.MembershipPlans");
            DropTable("dbo.MemberMemberships");
        }
    }
}
