using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using PumpDiagnosticsSystem.Core.Parser.Base;
using PumpDiagnosticsSystem.Models;
using PumpDiagnosticsSystem.Util;

namespace PumpDiagnosticsSystem.Core.Parser
{
    /// <summary>
    /// 判据解析器
    /// </summary>
    public class CriterionParser : BaseParser
    {
        /// <summary>
        /// 当前判据中解析出的实时数据
        /// </summary>
        private readonly Dictionary<string, double> _currentData = new Dictionary<string, double>();
        
        /// <summary>
        /// 当前在解析的判据
        /// </summary>
        private Criterion _ct;

        /// <summary>
        /// 【@变量，变量值】的字典
        /// </summary>
        private Dictionary<string, string> _dict = new Dictionary<string, string>();

        private string _compCode;

        /// <summary>
        /// 传感器数据更新时间
        /// </summary>
        public DateTime TdUpdateTime { get; set; }

        #region static funcs

        public static List<string> MatchConsts(string expressoin)
        {
            const string ConstMatchRegex = @"#\w{0,50}";
            return RegexMatch(expressoin, ConstMatchRegex);
        }

        /// <summary>
        ///     匹配 @ (间接参数)符号 、$(直接参数)符号、#（常数）符号
        /// </summary>
        /// <param name="expressoin"></param>
        /// <returns></returns>
        public static List<string> MatchVars(string expressoin)
        {
            // 0-20个单词数字组合 + @ + 0-20个单词数字组合
            const string IndirectParaMatchRegex = @"\w{0,150}@\w{0,150}";
            // $+字母 数字 下划线 点组合 当个数字或者字母长度不超过20, 0个或1个点
            const string DirectParaMatchRegex = @"\$(\w{0,150}\.{0,1}){0,5}";
            // # +0-20个单词数字组合
            const string ConstMatchRegex = @"#\w{0,150}";

            const string RegexString = IndirectParaMatchRegex + "|" + DirectParaMatchRegex + "|" + ConstMatchRegex;

            return RegexMatch(expressoin, RegexString);
        }

        public static List<string> MatchCriterionRtDataDict(string ctRtDataStr)
        {
            return RegexMatch(ctRtDataStr, @"[^:, |]+:[^:, |]+");
        }

        #endregion

        public CriterionParser()
        {
            _exParser.SetArgumentsSeparator(',');
            _exParser.SetDecimalSeparator('.');
        }

        public void ParseCriterion(Criterion ct, string compCode, Dictionary<string, string> dict)
        {
            //设置字段
            _ct = ct;
            _compCode = compCode;
            _dict = dict;

            //初始化
            ResetParser();
            _ct.ResetStatus();

            if (ct is GradedCriterion) {
                ParseGradedCriterion();
            } else {
                ParseNormalCriterion();
            }
        }

        private void ParseNormalCriterion()
        {
            string parsedExp;
            if (TryParseVariables(out parsedExp)) {
                _ct.IsHappening = EvaluateExpression(parsedExp);

                RecordRtData();
                var outputDetail = $"  组件：{_compCode}  事件模式：{_ct.EventMode}  实时数据： {_ct.RtDataStr}";
                if (_ct.IsHappening) {
                    _ct.Time = TdUpdateTime;
                    Log.Inform($">>>> 判据{_ct.LibId} 已发生 {outputDetail}");
                } else {
                    _ct.Time = null;
                    Log.Inform($"---- 判据{_ct.LibId} 未发生 {outputDetail}");
                }
            }
        }

        private void ParseGradedCriterion()
        {
            var gct = _ct as GradedCriterion;
            if(gct == null) return;

            string outputDetail = string.Empty;

            //先尝试判断判据能否正常解析
            string parsedExp;
            if (!TryParseVariables(out parsedExp)) {
                return;
            }

            var curIsPassed = true;
            //判据无误 能解析，那就进行分级解析
            GradedCriterion.ForEachValidGradeRange(lvs =>
            {
                //取一个最小的作为迭代条件
                var lv = lvs.Min();

                //当前等级没通过的话，之后的等级也就不用判断了，跳出循环
                if (!curIsPassed) return;

                var gradePercent = Convert.ToDouble(lv) / Convert.ToDouble(GradedCriterion.GradeCount);
                //解析判据
                TryParseVariables(out parsedExp, gradePercent);
                curIsPassed = EvaluateExpression(parsedExp);

                if (curIsPassed) {
                    gct.HappeningGrade = (SeverityGrade)lv;
                }

                //不管通过与否，都要记录和输出。 通过的话，设置级别要在记录之前；没通过的话，跳出要在记录之后
                RecordRtData();
                outputDetail = $"  组件：{_compCode} 事件模式：{gct.EventMode}  实时数据： {gct.RtDataStr}";
            });

            var notHappenAction = new Action(() =>
            {
                gct.IsHappening = false;
                gct.Time = null;
                Log.Inform($"---- 分级判据{_ct.LibId} 未发生 {outputDetail}");
            });

            var happenAction = new Action(() =>
            {
                gct.IsHappening = true;
                gct.Time = TdUpdateTime;
                Log.Inform($">>>> 分级判据{_ct.LibId} 已发生 {outputDetail}");
            });

            //都没通过，判据没发生
            if (gct.HappeningGrade == SeverityGrade.NotHappen) {
                notHappenAction();
            } else {
                //                if (Repo.ValidGrades.Contains((int) gct.HappeningGrade)) {
                //                    happenAction();
                //                } else {
                //                    notHappenAction();
                //                }
                happenAction();
            }
        }

        /// <summary>
        /// 记录判据的实时数据（只要判据能够正常解析，就要记录）
        /// </summary>
        private void RecordRtData()
        {
//            _ct.RtDataDict.Clear();
            _ct.RtDataStr = string.Empty;

            var gct = _ct as GradedCriterion;
            if (gct?.HappeningGrade > 0) {
//                _ct.RtDataDict.Add(nameof(gct.HappeningGrade), (int)gct.HappeningGrade);
                _ct.RtDataStr += $"Grade:{(int)gct.HappeningGrade}, ";
            }

            foreach (var d in _currentData) {
                var suffix = d.Key == _currentData.Last().Key ? string.Empty : Repo.Separator;
//                _ct.RtDataDict.Add(d.Key, d.Value);
                _ct.RtDataStr += $"{d.Key}:{Math.Round(d.Value, 6)}{suffix}";
            }
            
            //追加函数计算结果
            foreach (var funcParseResult in _funcParsedResults.Distinct()) {
                var prefix = funcParseResult.Equals(_funcParsedResults.First()) ? Repo.Separator : string.Empty;
                var suffix = funcParseResult.Equals(_funcParsedResults.Last()) ? string.Empty : Repo.Separator;
//                _ct.RtDataDict.Add(funcParseResult.FuncName, funcParseResult.Value);
                _ct.RtDataStr += $"{prefix}{funcParseResult.FuncName}:{funcParseResult.Value}{suffix}";
            }
        }

        private bool TryParseVariables(out string parsedExpression, double gradePercent = 1D)
        {
            var missingConsts = new List<string>();
            var missingAtVars = new List<string>();
            var missingSignals = new List<string>();
            var missingAtVarValues = new List<string>();
            _currentData.Clear();

            var exp = _ct.Expression;
            var vars = MatchVars(exp).Distinct().ToArray();
            foreach (var v in vars) {
                //替换判据中# 为 常量值
                if (v.StartsWith("#")) {
                    var gct = _ct as GradedCriterion;
                    //如果是分档变量并且是变量是分档变量
                    if (gct != null && v == gct.ServerityGradeField) {
                        var data = Repo.Consts[v];
                        data = data*gradePercent;
                        exp = exp.Replace(v, data.ToString());
//                        _currentData.Add(v, data);
                        _currentData.Add(v, Repo.Consts[v]);//加入原来的不分档的常量,
                    } else if (Repo.Consts.ContainsKey(v)) {
                        var data = Repo.Consts[v];
                        exp = exp.Replace(v, data.ToString());
                        _currentData.Add(v, data);
                    } else {
                        missingConsts.AddSingle(v);
                    }
                }

                //替换判据中@ 为 对应信号量的值
                else if (v.StartsWith("@")) {
                    if (_dict.ContainsKey(v)) {
                        var atValue = _dict[v];
                        if (!string.IsNullOrEmpty(atValue)) {
                            double n;
                            var isConst = atValue.StartsWith("#");
                            var isSignal = atValue.StartsWith("$");
                            var isGraph = atValue.StartsWith("{") && atValue.Contains("}_");


                            //@变量的值 是 常量
                            if (isConst) {
                                //替换 @变量的值 中的# 为 常量值
                                if (Repo.Consts.ContainsKey(atValue)) {
                                    var data = Repo.Consts[atValue];
                                    _currentData.Add(v, data);
                                    exp = exp.Replace(v, data.ToString());
                                } else {
                                    missingAtVarValues.AddSingle(v);
                                }
                            }

                            //@变量的值 是 信号量 or 图谱信号量
                            else if (isSignal || isGraph) {
                                var data = RuntimeRepo.RtData.FindSignalValue(atValue);
                                if (data != null) {
                                    if (isGraph) { //如果是图谱，记录图谱的编号
                                        _ct.GraphNumbers.Add((int) data);
                                    }
                                    _currentData.Add(v, data.Value);
                                    exp = exp.Replace(v, data.ToString());
                                } else {
                                    missingAtVarValues.AddSingle(_dict[v]);
                                }
                            }

                            //@变量的值 是 数值 
                            else if (double.TryParse(atValue, out n)) {
                                _currentData.Add(v, n);
                                exp = exp.Replace(v, atValue);
                            } else {
                                missingAtVarValues.AddSingle(_dict[v]);
                            }

                        } else {
                            missingSignals.AddSingle(v);
                        }
                    } else {
                        missingAtVars.AddSingle(v);
                    }
                }
            }

            if (missingSignals.Any() || missingAtVars.Any() || missingConsts.Any() || missingAtVarValues.Any()) {
                Log.Warn($"========= 判据错误: {_ct.LibId} =========");
                if (missingConsts.Any()) {
                    Log.Warn("以下【常量】无法解析：");
                    foreach (var missingConst in missingConsts) {
                        var suffix = missingConst == missingConsts.Last() ? "\r\n" : Repo.Separator;
                        Console.Write($"{missingConst}{suffix}");
                    }
                }
                if (missingAtVars.Any()) {
                    Log.Warn("以下【@变量】无法解析：");
                    foreach (var missingAtVar in missingAtVars) {
                        var suffix = missingAtVar == missingAtVars.Last() ? "\r\n" : Repo.Separator;
                        Console.Write($"{missingAtVar}{suffix}");
                    }
                }
                if (missingSignals.Any()) {
                    Log.Warn("以下【@变量】没有变量值：");
                    foreach (var missingSignal in missingSignals) {
                        var suffix = missingSignal == missingSignals.Last() ? "\r\n" : Repo.Separator;
                        Console.Write($"{missingSignal}{suffix}");
                    }
                }
                if (missingAtVarValues.Any()) {
                    Log.Warn("以下【@变量的变量值】无法解析：");
                    foreach (var msv in missingAtVarValues) {
                        var suffix = msv == missingAtVarValues.Last() ? "\r\n" : Repo.Separator;
                        Console.Write($"{msv}{suffix}");
                    }
                }
                Log.Inform();
                parsedExpression = string.Empty;
                return false;
            }
            parsedExpression = exp;
            return true;
        }
    }
}
