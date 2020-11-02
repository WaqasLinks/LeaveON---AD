namespace LeaveON.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedDepartmnetId : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "DepartmentId", c => c.Int(nullable: false));
            DropColumn("dbo.AspNetUsers", "CountryId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.AspNetUsers", "CountryId", c => c.Int(nullable: false));
            DropColumn("dbo.AspNetUsers", "DepartmentId");
        }
    }
}
