namespace RedAlert.API.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.DeviceGroups",
                c => new
                    {
                        DeviceGroupId = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Password = c.String(),
                    })
                .PrimaryKey(t => t.DeviceGroupId);
            
            CreateTable(
                "dbo.Devices",
                c => new
                    {
                        DeviceId = c.Int(nullable: false, identity: true),
                        SerialNumber = c.String(maxLength: 40),
                        HubDeviceId = c.String(),
                        DeviceKey = c.String(maxLength: 20),
                        SenderKey = c.String(maxLength: 20),
                    })
                .PrimaryKey(t => t.DeviceId)
                .Index(t => t.SerialNumber, unique: true)
                .Index(t => t.DeviceKey, unique: true)
                .Index(t => t.SenderKey, unique: true);
            
            CreateTable(
                "dbo.SerialNumbers",
                c => new
                    {
                        Code = c.String(nullable: false, maxLength: 40),
                        Activated = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Code);
            
        }
        
        public override void Down()
        {
            DropIndex("dbo.Devices", new[] { "SenderKey" });
            DropIndex("dbo.Devices", new[] { "DeviceKey" });
            DropIndex("dbo.Devices", new[] { "SerialNumber" });
            DropTable("dbo.SerialNumbers");
            DropTable("dbo.Devices");
            DropTable("dbo.DeviceGroups");
        }
    }
}
