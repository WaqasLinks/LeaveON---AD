namespace LeaveON.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class countryName1 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "CntryName", c => c.String());
            DropColumn("dbo.AspNetUsers", "CountryName");
        }
        
        public override void Down()
        {
            AddColumn("dbo.AspNetUsers", "CountryName", c => c.String());
            DropColumn("dbo.AspNetUsers", "CntryName");
        }
    }
}
