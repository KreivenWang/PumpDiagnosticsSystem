using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PumpDiagnosticsSystem.Core.Parser;
using PumpDiagnosticsSystem.Core.Parser.Base;
using PumpDiagnosticsSystem.Util;

namespace PumpDiagnosticsSystem.Tests
{
    [TestClass]
    public class SpectrumTest
    {
        [TestMethod]
        public void FindContinuesTest()
        {
            var data = new List<int> {1, 3, 5, 6, 7, 9, 10, 11, 12, 14, 16, 17, 19, 20};
            var findResult = PubFuncs.FindContinues(data);
            Assert.AreEqual(5, findResult[0].Item1);
            Assert.AreEqual(7, findResult[0].Item2);

            Assert.AreEqual(9, findResult[1].Item1);
            Assert.AreEqual(12, findResult[1].Item2);

            Assert.AreEqual(19, findResult[3].Item1);
            Assert.AreEqual(20, findResult[3].Item2);
        }
    }
}
