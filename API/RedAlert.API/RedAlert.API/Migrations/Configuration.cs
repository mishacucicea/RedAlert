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

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );
            //

            //adding a few serial numbers for testing
            context.SerialNumbers.AddOrUpdate(x => x.Code, new SerialNumber { Code = "xxxx-xxxx", Activated = false });
            context.SerialNumbers.AddOrUpdate(x => x.Code, new SerialNumber { Code = "yyyy-yyyy", Activated = false });
            context.SerialNumbers.AddOrUpdate(x => x.Code, new SerialNumber { Code = "zzzz-zzzz", Activated = false });
        }
    }
}
