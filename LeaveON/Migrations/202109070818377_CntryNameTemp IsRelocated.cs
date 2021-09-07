namespace LeaveON.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CntryNameTempIsRelocated : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "CntryNameTemp", c => c.String());
            AddColumn("dbo.AspNetUsers", "IsRelocated", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "IsRelocated");
            DropColumn("dbo.AspNetUsers", "CntryNameTemp");
        }
    }
}
