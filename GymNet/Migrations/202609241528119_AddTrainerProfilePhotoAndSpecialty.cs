namespace GymNet.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddTrainerProfilePhotoAndSpecialty : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "ProfilePhotoUrl", c => c.String());
            AddColumn("dbo.AspNetUsers", "Specialty", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "Specialty");
            DropColumn("dbo.AspNetUsers", "ProfilePhotoUrl");
        }
    }
}
