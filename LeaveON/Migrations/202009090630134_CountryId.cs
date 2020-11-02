namespace LeaveON.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CountryId : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "CountryId", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "CountryId");
        }
    }
}
