using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PumpDiagnosticsSystem.Business;
using PumpDiagnosticsSystem.Core.Parser;
using PumpDiagnosticsSystem.Core.Parser.Base;
using PumpDiagnosticsSystem.Datas;
using PumpDiagnosticsSystem.Models;
using PumpDiagnosticsSystem.Util;

namespace PumpDiagnosticsSystem.Tests
{
    [TestClass]
    public class ParserTest
    {
        internal class MockParser : BaseParser
        {
            public new bool EvaluateExpression(string expression)
            {
                return base.EvaluateExpression(expression);
            }

            public new double Calculate(string expression)
            {
                return base.Calculate(expression);
            }
        }

        internal class MockCriterionParser : CriterionParser
        {
            public new bool EvaluateExpression(string expression)
            {
                return base.EvaluateExpression(expression);
            }
            public new double Calculate(string expression)
            {
                return base.Calculate(expression);
            }
        }

        internal class MockInferComboParser : InferComboParser
        {
            public new bool EvaluateExpression(string expression)
            {
                return base.EvaluateExpression(expression);
            }
        }

        [TestMethod]
        public void BasicTest()
        {
            var mkParser = new MockParser();

            Assert.IsFalse(mkParser.EvaluateExpression("1>2"));
            Assert.IsTrue(mkParser.EvaluateExpression("2>1"));

            Assert.IsFalse(mkParser.EvaluateExpression("0"));
            Assert.IsTrue(mkParser.EvaluateExpression("1"));
            Assert.IsTrue(mkParser.EvaluateExpression("2"));
            Assert.IsTrue(mkParser.EvaluateExpression("-1"));

            //！： //定义为<=0的数为真， >0的为假
            Assert.IsFalse(mkParser.EvaluateExpression("!1")); 
            Assert.IsFalse(mkParser.EvaluateExpression("!-1"));//表达式无效
            Assert.IsTrue(mkParser.EvaluateExpression("!(-1)"));
            Assert.IsTrue(mkParser.EvaluateExpression("!0"));

            //字符串不解析
            Assert.IsFalse(mkParser.EvaluateExpression("true"));
            Assert.IsFalse(mkParser.EvaluateExpression("True"));

            Assert.IsTrue(mkParser.EvaluateExpression("1&&1"));
            Assert.IsTrue(mkParser.EvaluateExpression("1||0"));
            Assert.IsFalse(mkParser.EvaluateExpression("0||0"));
            Assert.IsTrue(mkParser.EvaluateExpression("!0||0"));

            Assert.IsTrue(mkParser.EvaluateExpression("2>1.1*1.5"));

            Assert.IsTrue(mkParser.EvaluateExpression("1&&0||1"));
            Assert.IsFalse(mkParser.EvaluateExpression("0||0&&1"));
            Assert.IsTrue(mkParser.EvaluateExpression("1&&1||1"));

            //问题出在！ not 后面跟 || 的时候必须大括号， 否则先计算||
            Assert.IsTrue(mkParser.EvaluateExpression("0||1"));

            //错误写法
            Assert.IsFalse(mkParser.EvaluateExpression("!(1)||1"));

            //错误写法！！！
            Assert.IsFalse(mkParser.EvaluateExpression("!1||1"));

            //正确写法
            Assert.IsTrue(mkParser.EvaluateExpression("(!1)||1"));
            Assert.IsTrue(mkParser.EvaluateExpression("1&&(!1)||1"));

            Assert.IsTrue(mkParser.EvaluateExpression("(!1&&1) || (1&&1)"));
            Assert.IsTrue(mkParser.EvaluateExpression("(1&&!1) || (1&&1)"));
            Assert.IsTrue(mkParser.EvaluateExpression("(1&&1) || (!1&&1)"));
            Assert.IsTrue(mkParser.EvaluateExpression("(1&&1) || (1&&!1)"));
            
            Assert.IsTrue(mkParser.EvaluateExpression("((!1)||1) && (1||1)"));
            Assert.IsTrue(mkParser.EvaluateExpression("(1||!1) && (1||1)"));
            Assert.IsTrue(mkParser.EvaluateExpression("(1||!1) && ((!1)||1)"));
            Assert.IsTrue(mkParser.EvaluateExpression("(1||!1) && (1||!1)"));

            //实际遇到问题， 应该是false的
            Assert.IsTrue(mkParser.EvaluateExpression("!0&&!0&&0&&!0"));
            //问题应该还是！表达式， 加上括号试试
            Assert.IsFalse(mkParser.EvaluateExpression("(!0)&&(!0)&&0&&(!0)"));
            //可以了，！表达式最好都加上括号吧

        }

        [TestMethod]
        public void FunctionsTest()
        {
            var mkParser = new MockParser();

            Assert.IsFalse(mkParser.EvaluateExpression("min(1,2) == 2"));
            Assert.IsTrue(mkParser.EvaluateExpression("min(1,2) == 1"));
        }

        [TestMethod]
        public void ParseCriterionTest()
        {
            Repo.Initialize();
            RuntimeRepo.RtData = new RtData();
            var ctParser = new MockCriterionParser();

            var ct = @"(SpectrumIntegration(@Spectrum_Bearing_In_Y,0.8*@Speed,1.2*@Speed,#SPECTRUMINTERVAL*60)*1.5)<SpectrumIntegration(@Spectrum_Bearing_In_Y,1.8*@Speed,2.2*@Speed,#SPECTRUMINTERVAL*60)";
            //简单的开始测 没有实时数据，最后SpectrumIntegration应该返回-1
            ct = @"SpectrumIntegration(@Spectrum_Bearing_In_Y,0.8*@Speed,1.2*@Speed,#SPECTRUMINTERVAL*60)==-1";

            var vars = CriterionParser.MatchConsts(ct);
            var consts = Repo.Consts;
            var missingVars = new List<string>();
            foreach (var vr in vars) {
                if (consts.ContainsKey(vr)) {
                    ct = ct.Replace(vr, consts[vr].ToString());
                } else {
                    missingVars.AddSingle(vr);
                }
            }
            Assert.IsTrue(!missingVars.Any());

            ct = ct.Replace("@Spectrum_Bearing_In_Y", "2");
            ct = ct.Replace("@Speed", "0");

            var a = ctParser.EvaluateExpression(ct);
            Assert.IsTrue(a);
        }

        [TestMethod]
        public void ParseInferComboTest()
        {
            var icParser = new MockInferComboParser();

            var dict = new Dictionary<int, int> {
                {1, 1},
                {2, 1},
                {3, 1}
            };

            var icItem1 = new InferComboItem {
                Expression = @"A1&&(!A2)||A3"
            };

            var icItem2 = new InferComboItem {
                Expression = @"A1&&((!A2)||A3)"
            };

            icParser.ParseInferComboItem(icItem1,dict);
            icParser.ParseInferComboItem(icItem2,dict);

            Assert.IsTrue(icItem1.IsHappening);
            Assert.IsTrue(icItem2.IsHappening);
        }

        [TestMethod]
        public void ParseCriterionRtDataTest()
        {
            const string mockCtRtData = @"故障级别:3, @Spectrum_Bearing_In_X:11, @Speed:567.0051234, #MAX_A_VIBRATION:11.42, SpectrumIntegration:2.18131412";
            const string mockInferComboItemRtData = @"!A10&&!A13&&A28&&!A31";

            var drCtrler = new DiagnoseReportController();
            var dict1 = drCtrler.ParseCriterionRtDataStr(mockCtRtData);
            var dict2 = drCtrler.ParseCriterionRtDataStr(mockInferComboItemRtData);

            Assert.AreEqual(5, dict1.Count);

            Assert.IsTrue(dict1.ContainsKey("故障级别"));
            Assert.AreEqual(3, dict1["故障级别"]);

            Assert.IsTrue(dict1.ContainsKey("@Spectrum_Bearing_In_X"));
            Assert.AreEqual(11, dict1["@Spectrum_Bearing_In_X"]);

            Assert.IsTrue(dict1.ContainsKey("SpectrumIntegration"));
            Assert.AreEqual(2.18131412, dict1["SpectrumIntegration"]);


            Assert.AreEqual(0, dict2.Count);


            const string realDataTest =
                "Grade:1, @Spectrum_Bearing_In_X:1, @Speed:525.565979, #SPECTRUMINTERVAL:1.25, #MAX_A_VIBRATION:7.613333, SpectrumIntegration:1.58606, SpectrumIntegration:1.58606|||" +
                "Grade:1, @Spectrum_Bearing_In_Y:3, @Speed:525.565979, #SPECTRUMINTERVAL:1.25, #MAX_A_VIBRATION:7.613333, SpectrumIntegration:1.42109, SpectrumIntegration:1.42109|||" +
                "Grade:1, @Spectrum_Bearing_In_Z:17, @Speed:525.565979, #SPECTRUMINTERVAL:1.25, #MAX_A_VIBRATION:7.613333, SpectrumIntegration:2.21644, SpectrumIntegration:2.21644|||" +
                "Grade:1, @Spectrum_Bearing_Out_Z:19, @Speed:525.565979, #SPECTRUMINTERVAL:1.25, #MAX_A_VIBRATION:7.613333, SpectrumIntegration:1.15875, SpectrumIntegration:1.15875";
            var dict3 = drCtrler.ParseCriterionRtDataStr(realDataTest);
            Assert.AreEqual(28, dict3.Count);
        }

        [TestMethod]
        public void FuncParseResult_CompareTest()
        {
            FuncParseResult r1 = new FuncParseResult("func1", 3.6502999999999997);
            FuncParseResult r2 = new FuncParseResult("func1", 3.6502999999999997);
            Assert.IsTrue(r1.Equals(r2));
        }


        [TestMethod]
        public void BoolAndOpTest()
        {
            var cond1 = true;
            var cond2 = false;
            var cond3 = true;
            var t = cond1;
            t &= cond2;
            t &= cond3;
            
            Assert.IsFalse(t);

            cond1 = true;
            cond2 = true;
            cond3 = true;
            t = cond1;
            t &= cond2;
            t &= cond3;

            Assert.IsTrue(t);
        }

        [TestMethod]
        public void CalcTest()
        {
            var mockParser = new MockParser();
            var result= mockParser.Calculate("3*4*5");

            Assert.AreEqual(60, result);
        }

        [TestMethod]
        public void StringFuncTest()
        {
            var mockParser = new MockCriterionParser();
            var result = mockParser.Calculate("TestStringFunc(\"string\",1)");
            Debug.WriteLine("calc finished");
            Assert.AreEqual(7, result);
        }
    }
}
