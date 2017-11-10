using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PumpDiagnosticsSystem.Core;
using PumpDiagnosticsSystem.Core.Parser;
using PumpDiagnosticsSystem.Core.Parser.Base;
using PumpDiagnosticsSystem.Util;

namespace PumpDiagnosticsSystem.Tests
{
    [TestClass]
    public class SpectrumTest
    {
        [Flags]
        public enum MyEnum
        {
            A = 1,
            B = 2,
            C = 4,
            D = 8
        }

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

        [TestMethod]
        public void ParseEnumFlagsTest()
        {
            var flags = 3;
            var ftNameList = PubFuncs.ParseEnumFlags<MyEnum>(flags);
            Assert.AreEqual(2, ftNameList.Count);
            Assert.AreEqual(MyEnum.A, ftNameList[0]);
            Assert.AreEqual(MyEnum.B, ftNameList[1]);
        }

        [TestMethod]
        public void IntConvertToEnumTest()
        {
            var conda = (MyEnum) 1; //正常
            Assert.AreEqual(MyEnum.A, conda);

            var condb = (MyEnum) 3; //组合
            Assert.IsTrue(condb.HasFlag(MyEnum.A));
            Assert.IsTrue(condb.HasFlag(MyEnum.B));

            var condc = (MyEnum) 0; //无
            Assert.AreEqual(0, (int)condc);

            var condd = (MyEnum) (-1); //错误
            Assert.AreEqual(-1, (int)condd);

            Assert.AreEqual(MyEnum.A | MyEnum.B | MyEnum.C, MyEnum.B | MyEnum.C | MyEnum.A);
        }
    }
}
