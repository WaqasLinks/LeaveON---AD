namespace LeaveON.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UserLeavePolicyId : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "UserLeavePolicyId", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "UserLeavePolicyId");
        }
    }
}
