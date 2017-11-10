using System;
using System.Collections.Generic;
using System.Linq;

namespace PumpDiagnosticsSystem.Core
{
    public class SpectrumAnalyser
    {
        public List<Spectrum> Specs { get; } = new List<Spectrum>();

        public Dictionary<Guid, Spectrum[][]>  BrPoses_Specs_Dict { get; private set; } = new Dictionary<Guid, Spectrum[][]>();

        public SpectrumAnalyser()
        {
        }

        public void UpdateSpecs(List<Guid> runningPPGuids, List<Spectrum> specs)
        {
            Specs.Clear();
            Specs.AddRange(specs);
            foreach (var ppGuid in runningPPGuids) {
                ClassifySpecsBs(ppGuid);
            }
        }

        /// <summary>
        /// 从左到右:水泵非驱动端到电机非驱动端为0,1,2,3, 不能用字符串解析来代替，因为判据中的函数只能用double类型
        /// </summary>
        private void ClassifySpecsBs(Guid ppGuid)
        {
            BrPoses_Specs_Dict.Clear();
            
            var value_BrPoses_Specs = new Spectrum[4][];
            for (int i = 0; i < 4; i++) {
                value_BrPoses_Specs[i] = new Spectrum[3];
            }

            value_BrPoses_Specs[0][0] = Specs.FirstOrDefault(s => s.Pos.IsPumpOutX && s.PPGuid == ppGuid);
            value_BrPoses_Specs[0][1] = Specs.FirstOrDefault(s => s.Pos.IsPumpOutY && s.PPGuid == ppGuid);
            value_BrPoses_Specs[0][2] = Specs.FirstOrDefault(s => s.Pos.IsPumpOutZ && s.PPGuid == ppGuid);

            value_BrPoses_Specs[1][0] = Specs.FirstOrDefault(s => s.Pos.IsPumpInX && s.PPGuid == ppGuid);
            value_BrPoses_Specs[1][1] = Specs.FirstOrDefault(s => s.Pos.IsPumpInY && s.PPGuid == ppGuid);
            value_BrPoses_Specs[1][2] = Specs.FirstOrDefault(s => s.Pos.IsPumpInZ && s.PPGuid == ppGuid);

            value_BrPoses_Specs[2][0] = Specs.FirstOrDefault(s => s.Pos.IsMotorInX && s.PPGuid == ppGuid);
            value_BrPoses_Specs[2][1] = Specs.FirstOrDefault(s => s.Pos.IsMotorInY && s.PPGuid == ppGuid);
            value_BrPoses_Specs[2][2] = Specs.FirstOrDefault(s => s.Pos.IsMotorInZ && s.PPGuid == ppGuid);

            value_BrPoses_Specs[3][0] = Specs.FirstOrDefault(s => s.Pos.IsMotorOutX && s.PPGuid == ppGuid);
            value_BrPoses_Specs[3][1] = Specs.FirstOrDefault(s => s.Pos.IsMotorOutY && s.PPGuid == ppGuid);
            value_BrPoses_Specs[3][2] = Specs.FirstOrDefault(s => s.Pos.IsMotorOutZ && s.PPGuid == ppGuid);

            BrPoses_Specs_Dict.Add(ppGuid, value_BrPoses_Specs);
        }

        private bool IsNearbySpec(Spectrum.Position pos1, Spectrum.Position pos2)
        {
            //电机2端
            if (pos1.CompPos == Spectrum.Position.Component.Motor &&
                pos2.CompPos == Spectrum.Position.Component.Motor &&
                pos1.DriverPos != pos2.DriverPos && //2端
                pos1.DirectionPos == pos2.DirectionPos ) { //xyz 相同
                return true;
            }

            //联轴器2端
            if (pos1.CompPos != pos2.CompPos && //水泵和电机 或者 电机和水泵
                pos1.DriverPos == Spectrum.Position.Driver.In && //都是驱动端
                pos2.DriverPos == Spectrum.Position.Driver.In &&
                pos1.DirectionPos == pos2.DirectionPos) { //xyz 相同
                return true;
            }

            //水泵2端
            if (pos1.CompPos == Spectrum.Position.Component.Pump &&
                pos2.CompPos == Spectrum.Position.Component.Pump &&
                pos1.DriverPos != pos2.DriverPos &&
                pos1.DirectionPos == pos2.DirectionPos) { //xyz 相同
                return true;
            }

            return false;
        }
    }
}
