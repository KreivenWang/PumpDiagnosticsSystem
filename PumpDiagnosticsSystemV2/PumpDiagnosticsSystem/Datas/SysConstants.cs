using System.Collections.Generic;

namespace PumpDiagnosticsSystem.Datas
{
    /// <summary>
    /// 常量类
    /// </summary>
    public static class SysConstants
    {


        public struct AQSPEED
        {
            /// <summary>
            /// 采速度
            /// </summary>
            public const int AQSPPED = 1;

            /// <summary>
            /// 不采速度
            /// </summary>
            public const int NOTAQSPPED = 0;

        }


        /// <summary>
        ///  振动类参数名称
        /// </summary>
        public struct VIBRADICT
        {
            /// <summary>
            /// OverAll
            /// </summary>
            public const string OverAll = "OverAll";

            /// <summary>
            /// Gap
            /// </summary>
            public const string Gap = "Gap";

            public const string Band1 = "Band1";

            public const string Band2 = "Band2";

            public const string Band3 = "Band3";

            public const string Band4 = "Band4";

            public const string Speed = "Speed";
        }


        public static readonly Dictionary<string, string> VIBRADICT2 = new Dictionary<string, string> {
                   {VIBRADICT.OverAll,"总振值"},
                   {VIBRADICT.Gap,"间隙电压"},
                   {VIBRADICT.Band1,"x(1/2)"},
                   {VIBRADICT.Band2,"x1"},
                   {VIBRADICT.Band3,"x2"},
                   {VIBRADICT.Band4,"x3"},
                   {VIBRADICT.Speed,"转速"}
                };

        public static readonly Dictionary<string, string> UNIT = new Dictionary<string, string> {
                   {VIBRADICT.OverAll,"mm/s"},
                   {VIBRADICT.Gap,"V"},
                   {VIBRADICT.Band1,""},
                   {VIBRADICT.Band2,""},
                   {VIBRADICT.Band3,""},
                   {VIBRADICT.Band4,""},
                   {VIBRADICT.Speed,"r/min"}
                };

        /// <summary>
        ///  权限类型
        /// </summary>
        public struct PTYPE
        {
            /// <summary>
            /// 非振动变量
            /// </summary>
            public const int PDNV = 2;

            /// <summary>
            /// 振动变量
            /// </summary>
            public const int PD = 1;


        }







        /// <summary>
        /// 传感器测方向
        /// </summary>
        public struct DIRECTION
        {
            /// <summary>
            /// 水平
            /// </summary>
            public const byte X = 1;

            /// <summary>
            /// 垂直
            /// </summary>
            public const byte Y = 2;

            /// <summary>
            /// 轴向
            /// </summary>
            public const byte Z = 3;

        }


        public static Dictionary<string, string> SENSORSETTING { get; } = new Dictionary<string, string> {

            {LOCATION.PUMPIN + "_" + DIRECTION.X, SENSORNAME.V_Pump_Drived_X},
            {LOCATION.PUMPIN + "_" + DIRECTION.Y, SENSORNAME.V_Pump_Drived_Y},
            {LOCATION.PUMPOUT + "_" + DIRECTION.X, SENSORNAME.V_Pump_NonDrived_X},
            {LOCATION.PUMPOUT + "_" + DIRECTION.Y, SENSORNAME.V_Pump_NonDrived_Y},
            {LOCATION.MOTORIN + "_" + DIRECTION.X, SENSORNAME.V_Motor_Drived_X},
            {LOCATION.MOTORIN + "_" + DIRECTION.Y, SENSORNAME.V_Motor_Drived_Y},
            {LOCATION.MOTOROUT + "_" + DIRECTION.X, SENSORNAME.V_Motor_NonDrived_X},
            {LOCATION.MOTOROUT + "_" + DIRECTION.Y, SENSORNAME.V_Motor_NonDrived_Y},
            {LOCATION.PUMPIN + "_" + DIRECTION.Z, SENSORNAME.V_Pump_Drived_Z},
            {LOCATION.PUMPOUT + "_" + DIRECTION.Z, SENSORNAME.V_Pump_NonDrived_Z},
            {LOCATION.MOTORIN + "_" + DIRECTION.Z, SENSORNAME.V_Motor_Drived_Z},
            {LOCATION.MOTOROUT + "_" + DIRECTION.Z, SENSORNAME.V_Motor_NonDrived_Z},
            {LOCATION.MOTORFOOT + "_" + DIRECTION.Y, SENSORNAME.V_Motor_Foot_Y},
        };

        public static string[] PHYDEF_NONVIBRA { get; } = {
            PHYDEFNAME.P_In,
            PHYDEFNAME.P_Out,
            PHYDEFNAME.F,
            PHYDEFNAME.Ia,
            PHYDEFNAME.Ib,
            PHYDEFNAME.Ic,
            PHYDEFNAME.Ua,
            PHYDEFNAME.Ub,
            PHYDEFNAME.Uc,
//            PHYDEFNAME.Speed, 转速单独
            PHYDEFNAME.Frequence,
            PHYDEFNAME.T_Pump_Drived,
            PHYDEFNAME.T_Pump_NonDrived,
            PHYDEFNAME.T_Motor_Drived,
            PHYDEFNAME.T_Motor_NonDrived,
            PHYDEFNAME.T_Motor_Coil_A,
            PHYDEFNAME.T_Motor_Coil_B,
            PHYDEFNAME.T_Motor_Coil_C,
        };

        /// <summary>
        /// 传感器对应表
        /// </summary>
        public struct SENSORNAME
        {
            public const string V_Pump_Drived_X = "V_Pump_Drived_X";
            public const string V_Pump_Drived_Y = "V_Pump_Drived_Y";
            public const string V_Pump_NonDrived_X = "V_Pump_NonDrived_X";
            public const string V_Pump_NonDrived_Y = "V_Pump_NonDrived_Y";
            public const string V_Motor_Drived_X = "V_Motor_Drived_X";
            public const string V_Motor_Drived_Y = "V_Motor_Drived_Y";
            public const string V_Motor_NonDrived_X = "V_Motor_NonDrived_X";
            public const string V_Motor_NonDrived_Y = "V_Motor_NonDrived_Y";

            public const string V_Pump_Drived_Z = "V_Pump_Drived_Z";
            public const string V_Pump_NonDrived_Z = "V_Pump_NonDrived_Z";
            public const string V_Motor_Drived_Z = "V_Motor_Drived_Z";
            public const string V_Motor_NonDrived_Z = "V_Motor_NonDrived_Z";

            public const string V_Motor_Foot_Y = "V_Motor_Foot_Y";

        };

        /// <summary>
        /// 信号量对应表
        /// </summary>
        public struct PHYDEFNAME
        {
            public const string P_In = "P_In";
            public const string P_Out = "P_Out";
            public const string F = "F";
            public const string Ia = "Ia";
            public const string Ib = "Ib";
            public const string Ic = "Ic";
            public const string Ua = "Ua";
            public const string Ub = "Ub";
            public const string Uc = "Uc";
//            public const string Speed = "Speed"; // 转速单独
            public const string Frequence = "Frequence";
            public const string T_Pump_Drived = "T_Pump_Drived";
            public const string T_Pump_NonDrived = "T_Pump_NonDrived";
            public const string T_Motor_Drived = "T_Motor_Drived";
            public const string T_Motor_NonDrived = "T_Motor_NonDrived";
            public const string T_Motor_Coil_A = "T_Motor_Coil_A";
            public const string T_Motor_Coil_B = "T_Motor_Coil_B";
            public const string T_Motor_Coil_C = "T_Motor_Coil_C";
        };

//        public static readonly Dictionary<string, string> tranttserType = new Dictionary<string, string> {
//                   {PHYDEFNAME。P_In,“Ptrass”},
//                   {"2","水泵非驱动端"},
//                   {"3","电机驱动端"},
//                   {"4","电机非驱动端"},
//                   {"5","底脚"}
//                };


        /// <summary>
        /// 传感器安装位置
        /// </summary>
        public struct LOCATION
        {
            /// <summary>
            /// 水泵驱动端
            /// </summary>
            public const byte PUMPIN = 1;

            /// <summary>
            /// 水泵非驱动端
            /// </summary>
            public const byte PUMPOUT = 2;

            /// <summary>
            /// 电机驱动端
            /// </summary>
            public const byte MOTORIN = 3;

            /// <summary>
            /// 电机非驱动端
            /// </summary>
            public const byte MOTOROUT = 4;

            /// <summary>
            /// 底脚
            /// </summary>
            public const byte MOTORFOOT = 5;
        }


        public static readonly Dictionary<string, string> SENSORLOCATION = new Dictionary<string, string> {
                   {"1","水泵驱动端"},
                   {"2","水泵非驱动端"},
                   {"3","电机驱动端"},
                   {"4","电机非驱动端"},
                   {"5","底脚"}
                };


        public static readonly Dictionary<string, string> PHYDEFNOVIBRATYPE = new Dictionary<string, string> {
                   {"1","泵运行状态"},
                   {"WALT","电度"},
                   {"FREQUENCE","功率"},
                   {"CURRENT","电流"},
                   {"YINSHU","因数"},
                   {"MOTORBERGINGTEMP","电机轴承温度"},
                   {"PUMPBERGINGTEMP","水泵轴承温度"},
                   {"MOTORTEMP","电机绕组温度"},
                   {"UA","电压"},
                   {"PRE","压力"},
                   {"PUMP","流量"},
                   {"TULT","浊度"},
                   {"CL","余氯"},
                   {"LEVEL","液位"}
                };

        public struct PHYDEFNOVIBRATYPE2
        {
            public const string RUNSTATS = "1";
            public const string WALT = "WALT";
            public const string FREQUENCE = "FREQUENCE";
            public const string CURRENT = "CURRENT";
            public const string YINSHU = "YINSHU";
            public const string MOTORBERGINGTEMP = "MOTORBERGINGTEMP";
            public const string PUMPBERGINGTEMP = "PUMPBERGINGTEMP";
            public const string MOTORTEMP = "MOTORTEMP";
            public const string UA = "UA";
            public const string PRE = "PRE";
            public const string PUMP = "PUMP";
            public const string TULT = "TULT";
            public const string CL = "CL";
            public const string LEVEL = "LEVEL";
        };

        public static readonly Dictionary<string, string> PHYDEFNOVIBRAUNIT = new Dictionary<string, string> {
                   {"RUNSTATS",""},
                   {"WALT","0.00"},
                   {"FREQUENCE","0.00"},
                   {"CURRENT","0.00"},
                   {"YINSHU","0.00"},
                   {"MOTORBERGINGTEMP","0.0"},
                   {"PUMPBERGINGTEMP","0.0"},
                   {"MOTORTEMP","0.0"},
                   {"UA","0.0"},
                   {"PRE","0.00"},
                   {"PUMP","0.00"},
                   {"TULT","0.00"},
                   {"CL","0.00"},
                   {"LEVEL","0.00"}
                };



        public struct GONGSHIENABLE
        {
            public const int DISABLES = 1;
            public const int ENABLES = 0;
        }


        public struct ISLOGICDELETE
        {
            public const int ISNOTDEL = 1;
            public const int ISDEL = 0;
        }

        public struct PUMPSTATIONCODE
        {
            public const string ZBSC = "zbsc";
            public const string THSC = "thsc";
        }
    }
}

