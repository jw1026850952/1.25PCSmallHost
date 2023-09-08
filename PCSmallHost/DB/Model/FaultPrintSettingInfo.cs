namespace PCSmallHost.DB.Model
{
    public class FaultPrintSettingInfo
    {
        public int Id { get; set; }
        /// <summary>
        /// 故障类型（0：主机 1：Eps 2：灯具）
        /// </summary>
        public int FaultType { get; set; }
        /// <summary>
        /// 是否打印（0：否 1：是）
        /// </summary>
        public int IsPrint { get; set; }
    }
}
