﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PumpDiagnosticsSystem.Core;
using PumpDiagnosticsSystem.Core.Parser;
using PumpDiagnosticsSystem.Core.Parser.Base;
using PumpDiagnosticsSystem.Datas;
using PumpDiagnosticsSystem.Models;
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
            var data = new List<int> { 1, 3, 5, 6, 7, 9, 10, 11, 12, 14, 16, 17, 19, 20 };
            var findResult = PubFuncs.FindContinues(data);
            Assert.AreEqual(5, findResult[0].Item1);
            Assert.AreEqual(7, findResult[0].Item2);

            Assert.AreEqual(9, findResult[1].Item1);
            Assert.AreEqual(12, findResult[1].Item2);

            Assert.AreEqual(19, findResult[3].Item1);
            Assert.AreEqual(20, findResult[3].Item2);

            data = new List<int> { 1, 5, 6, 7, 9, 10, 11, 14, 16, 17, 19, 20 };
            findResult = PubFuncs.FindContinues(data, 2);
            Assert.AreEqual(5, findResult[0].Item1);
            Assert.AreEqual(20, findResult[0].Item2);

            findResult = PubFuncs.FindContinues(data, 1);
            Assert.AreEqual(5, findResult[0].Item1);
            Assert.AreEqual(11, findResult[0].Item2);

            Assert.AreEqual(14, findResult[1].Item1);
            Assert.AreEqual(20, findResult[1].Item2);
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
            var conda = (MyEnum)1; //正常
            Assert.AreEqual(MyEnum.A, conda);

            var condb = (MyEnum)3; //组合
            Assert.IsTrue(condb.HasFlag(MyEnum.A));
            Assert.IsTrue(condb.HasFlag(MyEnum.B));

            var condc = (MyEnum)0; //无
            Assert.AreEqual(0, (int)condc);

            var condd = (MyEnum)(-1); //错误
            Assert.AreEqual(-1, (int)condd);

            Assert.AreEqual(MyEnum.A | MyEnum.B | MyEnum.C, MyEnum.B | MyEnum.C | MyEnum.A);
        }

        [TestMethod]
        public void SpectFeatureTest()
        {
            var ppguid = Guid.Parse("A67C1178-3B96-412F-BBEF-045B926F9999");//实验台1# .9 jibeng-ui数据库查询用
            var ssguid = Guid.Parse("05EE47E6-50FD-4171-8C68-D4A874F99011");//水泵非驱动端Y
            var graphNum = 1;
            var feature = 1;
            var rpm = 2923.675D;
            var fmax = 3120.743D;

            #region data

            var data =
                @"0.188003,0.845209,0.750463,0.189743,0.078223,0.379352,0.075067,0.237072,0.065852,0.385731,
0.054142,0.221125,0.117253,0.116442,0.500214,9.155606,0.180701,0.233452,0.131543,0.445333,0.469843,0.200683,
0.311307,0.258318,0.236894,0.183563,0.107750,0.053108,0.036749,0.042691,0.041061,0.312380,0.079526,0.056078,
0.035673,0.052789,0.016284,0.086343,0.029348,0.020693,0.043938,0.033337,0.067634,0.013386,0.053786,0.032443,
0.009935,0.074248,0.097000,0.103960,0.034541,0.106931,0.011022,0.012024,0.042175,0.066154,0.069429,0.013415,
0.041547,0.017143,0.021483,0.012891,0.017635,0.026239,0.016225,0.010197,0.025534,0.016050,0.010387,0.004352,
0.030276,0.006325,0.061519,0.038068,0.038742,0.051175,0.068443,0.049890,0.033891,0.073807,0.040037,0.018792,
0.053921,0.042474,0.020813,0.002202,0.043956,0.033790,0.008289,0.058311,0.030555,0.042259,0.067997,0.074685,
0.105237,0.919985,0.150277,0.020516,0.092165,0.024891,0.018296,0.025522,0.003536,0.012979,0.024958,0.016406,
0.020931,0.014944,0.003980,0.049801,0.010399,0.105517,0.023204,0.022557,0.011543,0.017823,0.006731,0.014917,
0.030145,0.016318,0.018310,0.006785,0.033151,0.009659,0.019033,0.035177,0.025414,0.050680,0.008323,0.007635,
0.013184,0.019991,0.039877,0.028395,0.016778,0.004003,0.033719,0.019695,0.044808,0.031122,0.007771,0.009356,
0.030245,0.058929,0.075020,0.037302,0.027231,0.034120,0.051953,0.032556,0.028697,0.060886,0.053499,0.017511,
0.036943,0.028708,0.014856,0.011140,0.023818,0.069944,0.020309,0.009336,0.009649,0.026330,0.024273,0.013648,
0.027691,0.026829,0.024921,0.038921,0.017752,0.047398,0.019060,0.009775,0.038700,0.030505,0.047518,0.114175,
0.034598,0.042522,0.013975,0.018749,0.027707,0.034598,0.011639,0.027760,0.006709,0.023950,0.008663,0.033699,
0.010365,0.066528,0.041639,0.026340,0.041207,0.012555,0.012500,0.051059,0.023688,0.015341,0.013337,0.035125,
0.036196,0.032503,0.027756,0.009590,0.020778,0.104206,0.047561,0.033240,0.010883,0.079687,0.043049,0.017533,
0.017271,0.035712,0.022656,0.005147,0.002497,0.019930,0.044528,0.022267,0.035938,0.094336,0.020936,0.054509,
0.029910,0.047540,0.055510,0.141703,0.034310,0.054397,0.037998,0.065453,0.002006,0.014186,0.027932,0.114446,
0.053787,0.152343,0.066317,0.051945,0.098264,0.059676,0.130957,0.054661,0.144596,0.074791,0.120948,0.121728,
0.088976,0.026914,0.010142,0.033558,0.098397,0.025575,0.043867,0.022377,0.034204,0.031325,0.018479,0.029787,
0.009606,0.042921,0.045839,0.057989,0.073217,0.048265,0.040283,0.024123,0.028603,0.047601,0.044907,0.044771,
0.044840,0.034074,0.020233,0.011135,0.013901,0.029706,0.055342,0.024923,0.055102,0.021653,0.047891,0.026276,
0.061499,0.022768,0.032446,0.016689,0.023642,0.024886,0.007090,0.023168,0.025326,0.026765,0.004805,0.018319,
0.003999,0.034109,0.012871,0.019246,0.014273,0.011367,0.008205,0.003056,0.009931,0.009347,0.012803,0.015008,
0.019182,0.009908,0.011090,0.008000,0.009486,0.013416,0.004612,0.008334,0.010530,0.020070,0.007793,0.008344,
0.011345,0.015470,0.011830,0.003504,0.005340,0.012850,0.005598,0.015722,0.005629,0.018576,0.020570,0.001057,
0.016647,0.017232,0.015257,0.004600,0.005471,0.008295,0.002184,0.014078,0.005477,0.006756,0.005015,0.012490,
0.015254,0.013710,0.011283,0.018836,0.006024,0.002498,0.006858,0.013111,0.009580,0.006629,0.006670,0.012670,
0.003246,0.005927,0.002805,0.015845,0.014476,0.021719,0.033156,0.027992,0.030168,0.014866,0.027804,0.028051,
0.026686,0.016352,0.004202,0.003985,0.019336,0.019831,0.006957,0.005627,0.029508,0.012290,0.028485,0.031239,
0.021319,0.031663,0.035931,0.030230,0.027660,0.023023,0.014464,0.011653,0.023948,0.016269,0.029229,0.002529,
0.030047,0.015210,0.018749,0.046202,0.010865,0.046957,0.011756,0.005349,0.040159,0.014054,0.004276,0.023644,
0.003651,0.024226,0.024186,0.002065,0.004363,0.015007,0.016441,0.025016,0.024636,0.020101,0.011272,0.007310,
0.006347,0.025718,0.021350,0.030039,0.019832,0.010352,0.014156,0.011269,0.018440,0.002426,0.013333,0.011306,
0.011623,0.035293,0.018273,0.004996,0.026451,0.008548,0.022202,0.015968,0.021975,0.006871,0.009646,0.017182,
0.007403,0.010432,0.023614,0.004722,0.012275,0.009131,0.015102,0.010879,0.006230,0.015519,0.004465,0.014026,
0.013111,0.017296,0.012069,0.007405,0.002335,0.006359,0.011437,0.007611,0.006137,0.010452,0.010891,0.003273,
0.008291,0.007022,0.003249,0.016460,0.010969,0.001644,0.008819,0.002147,0.005381,0.003139,0.005205,0.007723,
0.015494,0.012542,0.005469,0.006568,0.007165,0.000418,0.006987,0.006314,0.002935,0.006385,0.004292,0.010220,
0.007573,0.006243,0.008734,0.003711,0.009480,0.011057,0.004744,0.003750,0.008786,0.017088,0.011390,0.005896,
0.010550,0.006720,0.003913,0.003982,0.006250,0.004504,0.011065,0.002462,0.009033,0.001245,0.004474,0.007192,
0.008953,0.007717,0.003845,0.009432,0.011505,0.003530,0.012388,0.007794,0.002665,0.011287,0.010714,0.008813,
0.015263,0.002756,0.007166,0.010132,0.013163,0.005466,0.005955,0.011697,0.004551,0.009237,0.005080,0.005250,
0.004527,0.008788,0.005599,0.004910,0.005613,0.004185,0.009341,0.008170,0.005282,0.014546,0.003191,0.002164,
0.004828,0.005111,0.004150,0.013527,0.010090,0.000245,0.009066,0.014128,0.009673,0.009553,0.003947,0.013151,
0.010279,0.006941,0.005616,0.015669,0.010305,0.006674,0.001451,0.003029,0.010789,0.002017,0.019696,0.003920,
0.009112,0.010508,0.004008,0.014357,0.010864,0.007968,0.005932,0.007007,0.004646,0.002604,0.008574,0.006133,
0.009484,0.008351,0.005487,0.005194,0.004032,0.006150,0.002627,0.015266,0.002429,0.003453,0.006591,0.003721,
0.007618,0.004817,0.007895,0.007848,0.011396,0.001694,0.005906,0.003992,0.009573,0.000713,0.007743,0.016370,
0.008068,0.005206,0.010436,0.005386,0.009564,0.004526,0.017352,0.009619,0.018546,0.012089,0.014634,0.007207,
0.010796,0.013131,0.009681,0.019506,0.023579,0.011549,0.006405,0.006429,0.003608,0.005793,0.003452,0.010044,
0.003572,0.002889,0.005115,0.014268,0.013532,0.004745,0.012218,0.007688,0.003791,0.006438,0.006539,0.007928,
0.002145,0.014509,0.006685,0.005855,0.000896,0.005742,0.010528,0.005644,0.008019,0.010356,0.002638,0.008664,
0.004807,0.007757,0.007028,0.003987,0.003545,0.007512,0.006786,0.006714,0.011149,0.007969,0.002356,0.014762,
0.004884,0.006485,0.009935,0.004337,0.001199,0.012953,0.011517,0.003457,0.002732,0.006307,0.005682,0.004810,
0.004975,0.007637,0.005286,0.007030,0.004753,0.007393,0.006704,0.005664,0.002318,0.004713,0.005689,0.006035,
0.002056,0.002022,0.016437,0.003467,0.005722,0.005750,0.006469,0.005563,0.008727,0.008886,0.007778,0.008437,
0.004532,0.006234,0.007046,0.004173,0.003304,0.007251,0.004698,0.008799,0.003614,0.005202,0.011737,0.005274,
0.001734,0.009646,0.002502,0.002951,0.005625,0.005074,0.006631,0.003502,0.003455,0.002051,0.001548,0.001670,
0.003376,0.002519,0.001709,0.002059,0.001591,0.004445,0.003939,0.003607,0.000910,0.002580,0.004339,0.002067,
0.004049,0.003526,0.000786,0.002871,0.002147,0.002166,0.002605,0.002130,0.003655,0.006709,0.003726,0.002112,
0.002490,0.002233,0.004543,0.002273,0.002262,0.002261,0.000655,0.003715,0.003788,0.003193,0.002474,0.001110,
0.004919,0.003357,0.003130,0.000747,0.004025,0.002550,0.001819,0.001286,0.003126,0.002472,0.003308,0.002990,
0.006059,0.000857,0.001681,0.005264,0.004594,0.002440,0.003269,0.002806,0.005408,0.001352,0.001711,0.002633,
0.001495,0.003353,0.002771,0.000933,0.006821,0.004027,0.002550,0.004107,0.002011,0.002253,0.002348,0.000803,
0.001684,0.001671,0.002273,0.001061,0.004571,0.005572,0.001044,0.002472,0.000684,0.002771,0.004209,0.002574,
0.001184,0.004598,0.004134,0.002570,0.002555,0.002218,0.000458,0.005409,0.001430,0.001621,0.002031,0.005115,
0.002947,0.000836,0.001646,0.001146,0.003028,0.002947,0.003290,0.002093,0.005011,0.001799,0.003330,0.001898,
0.002297,0.001929,0.002980,0.001426,0.003144,0.001078,0.006659,0.003473,0.002598,0.004256,0.003591,0.004514,
0.004494,0.004253,0.000616,0.002848,0.001491,0.003485,0.001020,0.003106,0.000739,0.002913,0.001316,0.002366,
0.001857,0.001848,0.001018,0.001376,0.001806,0.000475,0.004183,0.002545,0.001777,0.001283,0.003041,0.001602,
0.001723,0.000261,0.004058,0.002083,0.003103,0.005627,0.002797,0.006026,0.004803,0.000956,0.003077,0.001635,
0.001366,0.005156,0.001603,0.003136,0.005000,0.003541,0.005410,0.002182,0.003742,0.001553,0.004136,0.003506,
0.000648,0.001848,0.002299,0.001265,0.003584,0.008829,0.004568,0.003843,0.004989,0.001220,0.003926,0.001935,
0.002575,0.001953,0.004954,0.001997,0.000862,0.003602,0.003616,0.002214,0.003137,0.003922,0.005291,0.000405,
0.000562,0.006376,0.003594,0.003121,0.001182,0.003360,0.001349,0.001961,0.004973,0.003987,0.001285,0.001322,
0.004191,0.001092,0.004440,0.003812,0.003000,0.003520,0.001823,0.002203,0.003754,0.000799,0.000878,0.003949,
0.001385,0.001048,0.002388,0.001486,0.001002,0.001236,0.002317,0.002501,0.002570,0.002488,0.002088,0.003125,
0.004757,0.001587,0.004665,0.004548,0.002455,0.003272,0.003314,0.002367,0.000781,0.000755,0.000580,0.000263,
0.002308,0.003142,0.002046,0.001855,0.001309,0.001264,0.001389,0.002233,0.001552,0.001465,0.001185,0.003088,
0.000976,0.001940,0.000853,0.000489,0.001874,0.002228,0.000493,0.001175,0.001238,0.002830,0.002698,0.001636,
0.002160,0.001105,0.002966,0.002544,0.002293,0.003341,0.002593,0.002840,0.001117,0.004872,0.002146,0.001739,
0.000820,0.001916,0.000841,0.000361,0.003362,0.001940,0.001057,0.002102,0.001680,0.002887,0.001463,0.003264,
0.002630,0.000452,0.002888,0.001651,0.001849";

            #endregion

            var spec = new Spectrum(ppguid, ssguid, graphNum, rpm, fmax, data.Split(',').Select(double.Parse), "PUMPOUT_Y");
            RuntimeRepo.RtData = new RtData();
            RuntimeRepo.SpecAnalyser.UpdateSpecs(new List<Guid> {ppguid}, new List<Spectrum> {spec});
            var parser = new ExpressionParser();

            var result = parser.SpecFeature(graphNum, feature, rpm, parser.GetDpl(graphNum) * 60, 0);

            Assert.AreEqual(9, Math.Floor(result));
        }

        [TestMethod]
        public void MathIntegerTest()
        {
            var num = 1.0001D;
            Assert.AreEqual(1, Math.Floor(num)); //向下取整
            Assert.AreEqual(2, Math.Ceiling(num)); //向上取整

            num = 1.9998D;
            Assert.AreEqual(1, Math.Floor(num));
            Assert.AreEqual(2, Math.Ceiling(num));
        }

        public static object InvokePrivateMethod(object obj, string methodName, params object[] args)
        {
            PrivateObject pobj = new PrivateObject(obj);
            return pobj.Invoke(methodName, args);
        }
    }
}
