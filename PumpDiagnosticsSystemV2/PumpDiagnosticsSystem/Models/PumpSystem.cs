using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using PumpDiagnosticsSystem.Datas;
using PumpDiagnosticsSystem.Dbs;
using PumpDiagnosticsSystem.Models.Enums;
using PumpDiagnosticsSystem.Util;
using ServiceStack.Common.Extensions;

namespace PumpDiagnosticsSystem.Models
{
    public class PumpSystem : List<BaseComponent>
    {
        /// <summary>
        /// 以泵的Guid作为自己的Guid
        /// <para>要获取Guid的字符，用Guid的扩展方法<see cref="GuidExt.ToFormatedString"/></para>
        /// </summary>
        public Guid Guid { get; }
        public Component Pump { get; }
        public Component Motor { get; }
        public Component Coupler { get; }
        public List<BaseTransducer> Transducers { get; } = new List<BaseTransducer>();

        public PumpSystem(Guid ppGuid)
        {
            Guid = ppGuid;

            Log.Inform();
            Log.Inform($"--------- 开始构建机泵系统{Guid.ToFormatedString()} ---------");
            Log.Inform();

            #region 构建机泵部件

            //部件部分
            Pump = new Component(Guid, CompType.Pump) {NameRemark = "水泵"};
            Motor = new Component(Guid, CompType.Motor) {NameRemark = "电机"};
            Coupler = new Component(Guid, CompType.Coupler) {NameRemark = "联轴器"};


            //振动传感器部分
            var vibrasensors = Repo.SensorList.Where(s => GuidExt.IsSameGuid(s.PPGUID, Guid)).ToList();
            foreach (var vibrasensor in vibrasensors) {
                var td = new VibraTransducer(vibrasensor.SSGUID) {
                    Signal = vibrasensor.ITEMCODE,
                    NameRemark = vibrasensor.SSNAME,
                    Position = PubFuncs.FindTdPosFromSignal(vibrasensor.ITEMCODE)
                };
                Transducers.Add(td);
                Log.Inform($"添加振动传感器({td.NameRemark}): 信号量：{td.Signal}  位置：{td.Position}");
            }

            //非振动部分
            foreach (var phydefnovibra in Repo.PhyDefNoVibra.Where(s => GuidExt.IsSameGuid(s.PPGUID, Guid))) {
                var type = PubFuncs.ParseSignalType(phydefnovibra.ITEMCODE);
                if (type.HasValue) {
                    var td = new NonVibraTransducer(phydefnovibra.PPGUID, type.Value, phydefnovibra.ID) {
                        Signal = phydefnovibra.ITEMCODE,
                        NameRemark = phydefnovibra.PDNVNAME,
                        Position = PubFuncs.FindTdPosFromSignal(phydefnovibra.ITEMCODE)
                    };
                    Transducers.Add(td);
                    Log.Inform($"添加非振动传感器({td.NameRemark}): 信号量：{td.Signal}  位置：{td.Position}");
                }
            }

            //还有一枚单独的转速变送器
            var speedTd = new SpeedTransducer(Guid);
            Transducers.Add(speedTd);
            Log.Inform($"添加转速传感器({speedTd.NameRemark}): 信号量：{speedTd.Signal}  位置：{speedTd.Position}");

            //            var ta = Repo.PhyDefNoVibra.Select(p => p.ITEMCODE).ToList();
            Add(Pump);
            Add(Motor);
            Add(Coupler);
            AddRange(Transducers);

            #endregion

            #region 构建部件间接属性

            foreach (DataRow row in PumpSysLib.TableIndirectProperty.Rows) {
                var prop = new Property {
                    Name = row["PropertyName"].ToString(),
                    Value = row["DefaultValue"].ToString(),
                    Variable = row["Variable"].ToString()
                };
                AllocateProperty(prop, row);
            }

            //连接点属性是间接属性的一部分,所以也加入
            foreach (DataRow row in PumpSysLib.TableConnectorPointProperty.Rows) {
                var prop = new Property {
                    Name = row["PropertyName"].ToString(),
//                    Value = row["DefaultValue"].ToString(),
                    Variable = row["Variable"].ToString()
                };
                AllocateProperty(prop, row);
            }

            #endregion

            #region 绑定传感器的自带信号量

            foreach (var transducer in Transducers) {
                transducer.BindSignal();
            }

            #endregion


            #region 设置振动传感器的图谱的信号量

            foreach (var tdv in Transducers.Where(t => t.Type == CompType.Td_V)) {
                foreach (var gType in Enum.GetNames(typeof (GraphType))) {
                    var prop = tdv.Properties.Find(p => p.Variable == "@" + gType);
                    prop.Value = PubFuncs.FormatGraphSignal(tdv.Code, (GraphType) Enum.Parse(typeof (GraphType), gType));
                }
            }

            #endregion


            #region 根据引用属性，将间接属性的值绑定到信号量

            foreach (DataRow row in PumpSysLib.TableRefProperty.Rows) {
                //找到要设置属性值的组件
                BaseComponent comp = null;
                var cpType = Repo.Map.TypeToEnum[row["TypeName"].ToString()];
                switch (cpType) {
                    case CompType.Pump:
                        comp = Pump;
                        break;
                    case CompType.Motor:
                        comp = Motor;
                        break;
                    case CompType.Coupler:
                        comp = Coupler;
                        break;
                }
                //找到要设置的属性值
                var prop = comp?.Properties.Find(p => p.Variable == row["IndirectVariable"].ToString());
                if (prop != null) {
                    //先找是哪个类型的传感器
                    var tdtype = Repo.Map.TypeToEnum[row["RefType"].ToString()];

                    //再根据位置找到该传感器
                    var tdpos = (TdPos) Enum.Parse(typeof (TdPos), row["Position"].ToString());

                    var td = Transducers.Find(t => t.Type == tdtype && t.Position == tdpos);

                    //再找到传感器上的属性
                    var rp = td?.Properties.Find(p => p.Variable == row["RefVariable"].ToString());
                    if (rp != null)
                        prop.Value = rp.Value;
                }
            }

            #endregion


            #region 根据水泵guid，设置水泵轴承和电机轴承的各自的4个缺陷频率

            foreach (var pumpBrInfo in DataDetailsOp.GetPumpBearingInfos(Guid)) {
                Pump.Properties.First(p => p.Variable == pumpBrInfo.Key).Value = pumpBrInfo.Value.ToString();
            }

            foreach (var motorBrInfo in DataDetailsOp.GetMotorBearingInfos(Guid)) {
                Motor.Properties.First(p => p.Variable == motorBrInfo.Key).Value = motorBrInfo.Value.ToString();
            }

            #endregion

            #region 根据水泵guid，设置水泵叶片数

            Pump.Properties.First(p => p.Variable == "@BladeNum").Value = DataDetailsOp.GetPumpFanCount(Guid).ToString();

            #endregion

            Log.Inform();
            Log.Inform($"--------- 机泵系统{ppGuid} 构建结束---------");
            Log.Inform();
        }

        /// <summary>
        /// 分配属性到对应的组件上
        /// </summary>
        private void AllocateProperty(Property prop, DataRow dbRow)
        {
            var cpType = Repo.Map.TypeToEnum[dbRow["TypeName"].ToString()];
            switch (cpType) {
                case CompType.Pump:
                    Pump.Properties.Add(prop);
                    break;
                case CompType.Motor:
                    Motor.Properties.Add(prop);
                    break;
                case CompType.Coupler:
                    Coupler.Properties.Add(prop);
                    break;
                default:
                    Transducers.Where(t => t.Type == cpType).ForEach(t => t.Properties.Add(prop.DeepClone()));
                    break;
            }
        }

        #region 生成构建结果的报告

        public class ReportItem
        {
            public string PumpCode { get; set; }
            public string CompCode { get; set; }
            public string CompName { get; set; }
            public string PropName { get; set; }
            public string Variable { get; set; }
            public string Value { get; set; }
            public CompType CompType { get; set; }
            public TdPos? TdPos { get; set; }
        }

        public List<ReportItem> GetReport()
        {
            return (from comp in this
                from prop in comp.Properties
                select new ReportItem {
                    PumpCode = Guid.ToFormatedString(),
                    CompName = comp.NameRemark,
                    CompCode = comp.Code,
                    CompType = comp.Type,
                    TdPos = (comp as BaseTransducer)?.Position,
                    PropName = prop.Name,
                    Variable = prop.Variable,
                    Value = prop.Value,
                })
                .OrderBy(r=>r.CompCode)
                .ThenBy(r=>r.Variable)
                .ToList();
        }

        #endregion

        #region 判断当前数据是否最新

        private readonly Dictionary<Guid, DateTime?> _tdLatestTimes = new Dictionary<Guid, DateTime?>();

        public void InitializeTdTimes()
        {
            _tdLatestTimes.Clear();
            _tdLatestTimes.Add(Guid, null);
            foreach (var td in this.Where(c => c.IsDataIsolateSample)) {
                _tdLatestTimes.Add(td.Guid, null);

            }
        }

        public void Check()
        {
            foreach (var ppsys in RuntimeRepo.PumpSysList) {
                
            }
        }

        #endregion

    }
}
