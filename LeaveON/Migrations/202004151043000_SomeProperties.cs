namespace LeaveON.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SomeProperties : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "DateCreated", c => c.DateTime());
            AddColumn("dbo.AspNetUsers", "DateModified", c => c.DateTime());
            AddColumn("dbo.AspNetUsers", "Remarks", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "Remarks");
            DropColumn("dbo.AspNetUsers", "DateModified");
            DropColumn("dbo.AspNetUsers", "DateCreated");
        }
    }
}
