using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PumpDiagnosticsSystem.Models;
using PumpDiagnosticsSystem.Models.DbEntities;

namespace PumpDiagnosticsSystem.Dbs
{
    public class PumpSystemContext : DbContext
    {
        private static readonly string connStr = ConfigurationManager.ConnectionStrings["PPSysDbContext"].ConnectionString;
        public PumpSystemContext()
            : base(connStr)
        {
            
        }

        public DbSet<GraphArchive> GraphArchives { get; set; }

        public DbSet<FaultItemReport> FaultItemReports { get; set; }

        public DbSet<InferComboReport> InferComboReports { get; set; }

        public DbSet<MainSpec> MainSpecs { get; set; }

        public int AddGraph(GraphArchive ga)
        {
            GraphArchives.Add(ga);
            SaveChanges();
            return ga.Id;
        }
    }
}

namespace PumpDiagnosticsSystem.Dbs.Migrations
{
    internal sealed class Configuration : DbMigrationsConfiguration<PumpSystemContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
        }

        protected override void Seed(PumpSystemContext context)
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
        }
    }
}