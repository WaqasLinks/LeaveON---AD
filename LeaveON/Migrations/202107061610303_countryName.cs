namespace LeaveON.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class countryName : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "CountryName", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "CountryName");
        }
    }
}
