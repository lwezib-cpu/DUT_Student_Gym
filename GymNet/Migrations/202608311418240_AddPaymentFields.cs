namespace GymNet.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddPaymentFields : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Payments", "AccountType", c => c.String());
            AddColumn("dbo.Payments", "BranchCode", c => c.String());
            AlterColumn("dbo.Payments", "PaymentMethod", c => c.String(nullable: false));
            AlterColumn("dbo.Payments", "Status", c => c.String(nullable: false));
            AlterColumn("dbo.Payments", "CardHolderName", c => c.String());
            AlterColumn("dbo.Payments", "CardNumber", c => c.String());
            AlterColumn("dbo.Payments", "ExpiryDate", c => c.String());
            AlterColumn("dbo.Payments", "BankName", c => c.String());
            AlterColumn("dbo.Payments", "AccountNumber", c => c.String());
            AlterColumn("dbo.Payments", "TransactionReference", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Payments", "TransactionReference", c => c.String(maxLength: 50));
            AlterColumn("dbo.Payments", "AccountNumber", c => c.String(maxLength: 20));
            AlterColumn("dbo.Payments", "BankName", c => c.String(maxLength: 100));
            AlterColumn("dbo.Payments", "ExpiryDate", c => c.String(maxLength: 5));
            AlterColumn("dbo.Payments", "CardNumber", c => c.String(maxLength: 20));
            AlterColumn("dbo.Payments", "CardHolderName", c => c.String(maxLength: 100));
            AlterColumn("dbo.Payments", "Status", c => c.String(nullable: false, maxLength: 50));
            AlterColumn("dbo.Payments", "PaymentMethod", c => c.String(nullable: false, maxLength: 50));
            DropColumn("dbo.Payments", "BranchCode");
            DropColumn("dbo.Payments", "AccountType");
        }
    }
}
