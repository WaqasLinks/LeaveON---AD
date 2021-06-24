namespace LeaveON.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Department1 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "DepartmentName", c => c.String());
            DropColumn("dbo.AspNetUsers", "Department");
        }
        
        public override void Down()
        {
            AddColumn("dbo.AspNetUsers", "Department", c => c.String());
            DropColumn("dbo.AspNetUsers", "DepartmentName");
        }
    }
}
