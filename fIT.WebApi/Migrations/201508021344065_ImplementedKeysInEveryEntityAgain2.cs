namespace fIT.WebApi.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ImplementedKeysInEveryEntityAgain2 : DbMigration
    {
        public override void Up()
        {
            DropPrimaryKey("dbo.Practices");
            AlterColumn("dbo.Practices", "Id", c => c.Int(nullable: false, identity: true));
            AddPrimaryKey("dbo.Practices", "Id");
        }
        
        public override void Down()
        {
            DropPrimaryKey("dbo.Practices");
            AlterColumn("dbo.Practices", "Id", c => c.Int(nullable: false));
            AddPrimaryKey("dbo.Practices", new[] { "Id", "ScheduleId", "ExerciseId" });
        }
    }
}
