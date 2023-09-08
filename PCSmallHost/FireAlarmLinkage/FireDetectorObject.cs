namespace PCSmallHost.FireAlarmLinkage
{
    public enum FireAlarmEventType
    {
        /// <summary>
        /// 火警
        /// </summary>
        FireAlarm,
        /// <summary>
        /// 复位
        /// </summary>
        Reset,
        /// <summary>
        /// 可忽略事件
        /// </summary>
        IgnorableEvent
    }

    public enum FireDetectorType
    {
        /// <summary>
        /// 烟感报警器
        /// </summary>
        SmokeAlarm,
        /// <summary>
        /// 温感报警器
        /// </summary>
        TemperatureAlarm,
        /// <summary>
        /// 手动按钮
        /// </summary>
        ManualSwitch,
        /// <summary>
        /// 其他类型
        /// </summary>
        Other,
        /// <summary>
        /// 未知类型
        /// </summary>
        Unknown
    }

    public class FireDetectorObject
    {
        /// <summary>
        /// 主机网络地址
        /// </summary>
        public int NetworkAddress { get; set; }
        /// <summary>
        /// 回路号
        /// </summary>
        public int CircuitNumber { get; set; }
        /// <summary>
        /// 设备编号
        /// </summary>
        public int DeviceID { get; set; }
        /// <summary>
        /// 报警器事件
        /// </summary>
        public FireAlarmEventType EventType { get; set; }
            = FireAlarmEventType.IgnorableEvent;
        /// <summary>
        /// 火灾报警器类型
        /// </summary>
        public FireDetectorType DeviceType { get; set; }
            = FireDetectorType.Unknown;
    }
}
