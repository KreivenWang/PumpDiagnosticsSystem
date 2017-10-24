using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;

namespace PumpDiagnosticsSystem.Tests
{
    public class EFQueryTestDbContext : DbContext
    {
        private static readonly string connStr = ConfigurationManager.ConnectionStrings["EFQueryTestDbContext"].ConnectionString;
        public EFQueryTestDbContext()
            : base(connStr)
        {

        }

        public DbSet<Person> Persons { get; set; }
    }

    [Table("Person")]
    public class Person
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
    }

    internal sealed class Configuration : DbMigrationsConfiguration<EFQueryTestDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
        }

        protected override void Seed(EFQueryTestDbContext context)
        {
            if (!context.Persons.Any()) {
                var nameStrs = "AaBbCcDdEeFfGgHhIiJjKkLlMmNn";
                var nameStrsLen = nameStrs.Length;
                var persons = new List<Person>();
                Random rdm = new Random(DateTime.Now.Millisecond);
                for (int i = 0; i < 100000; i++) {
                    var name = nameStrs[rdm.Next(0, nameStrsLen - 1)].ToString() +
                               nameStrs[rdm.Next(0, nameStrsLen - 1)];
                    persons.Add(new Person() { Name = name });
                }
                context.Persons.AddRange(persons);
                context.SaveChanges();
            }
        }
    }
}