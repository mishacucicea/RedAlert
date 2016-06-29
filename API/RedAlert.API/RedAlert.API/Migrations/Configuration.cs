namespace RedAlert.API.Migrations
{
    using Models;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<RedAlert.API.DAL.RedAlertContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
        }

        protected override void Seed(RedAlert.API.DAL.RedAlertContext context)
        {
            //  This method will be called after migrating to the latest version.
        }
    }
}
