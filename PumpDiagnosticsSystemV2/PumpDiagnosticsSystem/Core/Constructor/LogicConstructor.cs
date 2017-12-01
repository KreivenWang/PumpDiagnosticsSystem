using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PumpDiagnosticsSystem.Dbs;
using PumpDiagnosticsSystem.Models;
using PumpDiagnosticsSystem.Util;

namespace PumpDiagnosticsSystem.Core.Constructor
{
    public class LogicConstructor
    {
        public static void ConstructRepo()
        {
            #region 读取故障项（组件和故障模式）

            Repo.FaultItems.Clear();
            foreach (DataRow row in PumpSysLib.TableFaultItem.Rows) {
                Repo.FaultItems.Add(new FaultItem {
                    CompType = Repo.Map.TypeToEnum[row["TypeName"].ToString()],
                    EventMode = row["EventMode"].ToString(),
                    Description = row["Description"].ToString(),
                    Advise = row["Advise"].ToString()
                });
            }

            #endregion


            #region 构建判据

            //读取判据模板
            var ctTemplates = new List<CriterionTemplate>();
            foreach (DataRow row in PumpSysLib.TableCriterionTemplate.Rows) {
                ctTemplates.Add(new CriterionTemplate {
                    Id = int.Parse(row["ID"].ToString()),
                    Desciption = row["Description"].ToString(),
                    FuncName = row["FuncName"].ToString(),
                    ExpressionTemplate = row["ExpressionTemplate"].ToString(),
                    GradeVar = row["GradeVar"].ToString(),
                    FilteCount = int.Parse(row["FilteCount"].ToString()),
                    ThresholdField = row["ThresholdField"].ToString(),
                    AsReportResult = (bool)row["AsReportResult"]

                });
            }

            //区分模板中带函数的和不带函数的, 并替换 函数名 为 表达式
            var ctTemplates_Func = ctTemplates.Where(ct => !string.IsNullOrEmpty(ct.FuncName)).OrderByDescending(ct=>ct.FuncName.Length).ToArray();
            Debug.Assert(ctTemplates_Func.Distinct().Count() == ctTemplates_Func.Length,"判据模板函数必须没有重复");
            Debug.Assert(ctTemplates_Func[0].FuncName.Length >= ctTemplates_Func[1].FuncName.Length, "判据模板函数名必须按函数名长度倒序");
            var ctTemplates_all = ctTemplates;
            foreach (var cttemplate in ctTemplates_all) {
                foreach (var cttFunc in ctTemplates_Func) {
                    if (cttemplate != cttFunc) {
                        if (cttemplate.ExpressionTemplate.Contains(cttFunc.FuncName)) {
                            cttemplate.ExpressionTemplate = cttemplate.ExpressionTemplate.Replace(cttFunc.FuncName, cttFunc.ExpressionTemplate);
                        }
                    }
                }
            }


            //读取要构建的判据表
            var ctToBuildList = new List<CriterionToBuild>();
            foreach (DataRow row in PumpSysLib.TableCriterionToBuild.Rows) {
                //                var splitIndex = evMode.IndexOf("_", StringComparison.Ordinal);
                //                var compStr = evMode.Substring(0, splitIndex);
                //                var evModeStr = evMode.Substring()
                var fitem = Repo.FindFaultItem(row["EventMode"].ToString());
                if (fitem != null) {
                    var cttb = new CriterionToBuild {
                        CompType = fitem.CompType,
                        EventMode = fitem.EventMode,
                        LibId = int.Parse(row["LibID"].ToString()),
                        TemplateId = int.Parse(row["TemplateID"].ToString()),
                        PosRemark = row["PosRemark"].ToString(),
                        _VAR_A = row["_VAR_A"].ToString(),
                        _VAR_B = row["_VAR_B"].ToString(),
                        _VAR_C = row["_VAR_C"].ToString(),
                        _VAR_D = row["_VAR_D"].ToString(),
                        _VAR_E = row["_VAR_E"].ToString(),
                        _VAR_F = row["_VAR_F"].ToString(),
                    };
                    ctToBuildList.Add(cttb);
                } else {
                    Log.Error(
                        $"构建判据错误：Access中{nameof(PumpSysLib.TableCriterionToBuild)}故障模式无法读取： {row["EventMode"]}");
                }
            }

            //设置判据
            Repo.Criteria.Clear();
            foreach (var ctToBuild in ctToBuildList) {
                var template = ctTemplates_all.FirstOrDefault(tpl => tpl.Id == ctToBuild.TemplateId);
                var fitem = Repo.FaultItems.FirstOrDefault(fi => fi.IsSameFaultItem(ctToBuild));

                if (template == null) {
                    Log.Error($"构建判据错误：ID为{ctToBuild.LibId}的判据模板未找到");
                } else if (template.ExpressionTemplate.Contains("PREV")) {
                    continue; //不加载带有PREV函数的判据) 
                } else if (fitem == null) {
                    Log.Error($"构建判据错误：{ctToBuild.CompType} AND {ctToBuild.EventMode}的故障项未找到");
                } else {
                    Criterion ct;
                    if (string.IsNullOrEmpty(template.GradeVar)) {
                        ct = new Criterion();
                    } else {
                        ct = new GradedCriterion {
                            ServerityGradeField = template.GradeVar
                        };
                    }
                    ct.LibId = ctToBuild.LibId;
                    ct.TemplateId = template.Id;
                    ct.CompType = ctToBuild.CompType;
                    ct.EventMode = ctToBuild.EventMode;
                    ct.Expression = template.ExpressionTemplate;

                    #region 替换表达式中的_VAR_变量

                    ct.Expression = ct.Expression.Replace(nameof(ctToBuild._VAR_A), ctToBuild._VAR_A);
                    ct.Expression = ct.Expression.Replace(nameof(ctToBuild._VAR_B), ctToBuild._VAR_B);
                    ct.Expression = ct.Expression.Replace(nameof(ctToBuild._VAR_C), ctToBuild._VAR_C);
                    ct.Expression = ct.Expression.Replace(nameof(ctToBuild._VAR_D), ctToBuild._VAR_D);
                    ct.Expression = ct.Expression.Replace(nameof(ctToBuild._VAR_E), ctToBuild._VAR_E);
                    ct.Expression = ct.Expression.Replace(nameof(ctToBuild._VAR_F), ctToBuild._VAR_F);

                    #endregion

                    ct.Description = $"{fitem.Description}({template.Desciption})";
                    ct.Advise = fitem.Advise;

                    ct.ThresholdField = template.ThresholdField;
                    ct.AsReportResult = template.AsReportResult;

                    //加入到FaultItem中
                    fitem.Criteria.Add(ct);

                    //设置faultitem的阈值字段
                    fitem.ThresholdField = ct.ThresholdField;

                    ct.PosRemark = ctToBuild.PosRemark;

                    Repo.Criteria.Add(ct);
                }
            }

            #endregion


            #region 构建推断组合项列表

            Repo.InferCombos.Clear();
            foreach (DataRow row in PumpSysLib.TableInferCombo.Rows) {

                var fItem = Repo.FindFaultItem(row["EventMode"].ToString());
                
                var icitem = new InferComboItem {
                    EventMode = fItem.EventMode,
                    CompType = fItem.CompType,
                    Id = int.Parse(row["ID"].ToString()),
                    Expression = row["Expression"].ToString(),
                    FaultResult = row["FaultResult"].ToString(),
                    Advice = row["Advice"].ToString(),
                };
                var prevIdsStr = row["PrevIds"].ToString();
                if (!string.IsNullOrEmpty(prevIdsStr)) {
                    icitem.PrevIds = prevIdsStr.Split(',').Select(int.Parse).ToArray();
                } else {
                    icitem.PrevIds = new int[] {};
                }
                Repo.InferCombos.Add(icitem);
            }

            #endregion

        }
    }
}
