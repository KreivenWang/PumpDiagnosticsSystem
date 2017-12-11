using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PumpDiagnosticsSystem.Models.DbEntities;

namespace PumpDiagnosticsSystem.Tests
{
    [TestClass]
    public class EFQueryTest
    {
        [TestMethod]
        public void QueryTest()
        {
            using (var context = new EFQueryTestDbContext()) {
                var allPersons = context.Persons;
                var persons1 = allPersons.ToList().Where(p => p.Name.Contains("bc")).ToList(); //>1s
                var persons2 = allPersons.Where(p => p.Name.Contains("bc")).ToList(); //<100ms
                var persons3 = from p in allPersons
                    where p.Name.Contains("bc")
                    select p; //<5ms
                var a = persons3.Expression; //1ms
                var b = persons3.Count(); //<100ms
                var c = persons3.ToList(); //<100ms
                Assert.IsTrue(persons1.Count == persons2.Count);
                Assert.IsTrue(persons2.Count == persons3.Count());
            }
        }
    }
}
