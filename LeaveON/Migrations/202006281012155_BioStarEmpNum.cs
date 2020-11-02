namespace LeaveON.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class BioStarEmpNum : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "BioStarEmpNum", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "BioStarEmpNum");
        }
    }
}
