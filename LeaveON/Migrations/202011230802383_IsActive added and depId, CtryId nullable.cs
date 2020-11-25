namespace LeaveON.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class IsActiveaddedanddepIdCtryIdnullable : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "IsActive", c => c.Boolean());
            AlterColumn("dbo.AspNetUsers", "DepartmentId", c => c.Int());
            AlterColumn("dbo.AspNetUsers", "CountryId", c => c.Int());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.AspNetUsers", "CountryId", c => c.Int(nullable: false));
            AlterColumn("dbo.AspNetUsers", "DepartmentId", c => c.Int(nullable: false));
            DropColumn("dbo.AspNetUsers", "IsActive");
        }
    }
}
