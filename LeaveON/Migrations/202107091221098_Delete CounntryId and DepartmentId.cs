namespace LeaveON.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DeleteCounntryIdandDepartmentId : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.AspNetUsers", "DepartmentId");
            DropColumn("dbo.AspNetUsers", "CountryId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.AspNetUsers", "CountryId", c => c.Int());
            AddColumn("dbo.AspNetUsers", "DepartmentId", c => c.Int());
        }
    }
}
