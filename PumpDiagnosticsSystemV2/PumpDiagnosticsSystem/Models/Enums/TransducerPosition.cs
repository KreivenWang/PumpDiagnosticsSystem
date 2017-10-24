namespace PumpDiagnosticsSystem.Models
{
    /// <summary>
    /// 传感器位置，用于判断引用属性中需要的传感器, 注意不要替换顺序，因为是按信号量的组成来判断的，温度的字符串比较短，放前面会影响振动的判断
    /// </summary>
    public enum TdPos
    {
        #region 振动

        V_Pump_Drived_X,
        V_Pump_Drived_Y,
        V_Pump_Drived_Z,

        V_Pump_NonDrived_X,
        V_Pump_NonDrived_Y,
        V_Pump_NonDrived_Z,

        V_Motor_Drived_X,
        V_Motor_Drived_Y,
        V_Motor_Drived_Z,

        V_Motor_NonDrived_X,
        V_Motor_NonDrived_Y,
        V_Motor_NonDrived_Z,

        /// <summary>
        /// 基础
        /// </summary>
        V_Base,

        /// <summary>
        /// 底部
        /// </summary>
        V_Bottom,
        V_Motor_Foot_Y,

        #endregion

        #region 温度

        T_Pump_Drived,
        T_Pump_NonDrived,
        T_Motor_Drived,
        T_Motor_NonDrived,
        T_Motor_Coil_A,
        T_Motor_Coil_B,
        T_Motor_Coil_C,

        #endregion

        #region 压力、流量

        P_In,
        P_Out,

        #endregion

        #region 转速

        Speed,

        #endregion

        #region 电气

        Ia,
        Ib,
        Ic,
        Ua,
        Ub,
        Uc,
        Frequence

        #endregion
    }
}
