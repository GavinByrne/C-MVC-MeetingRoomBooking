namespace BookingSystemData.Migrations
{
    using DbModels;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<BookingSystemData.DbModels.BookingSystemContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
        }

        protected override void Seed(BookingSystemContext context)
        {
            BookingSystemInitializer.SeedData(context);
        }
    }
}
