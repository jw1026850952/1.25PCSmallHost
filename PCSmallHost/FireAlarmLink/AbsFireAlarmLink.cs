using PCSmallHost.Util;
using PCSmallHost.DB.Model;
using PCSmallHost.DB.BLL;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Sugar.Log;

namespace PCSmallHost.FireAlarmLink
{
    /// <summary>
    /// 火灾报警器父类
    /// </summary>
    public class AbsFireAlarmLink
    {
        static MainWindow _context;
        /// <summary>
        /// 火灾联动时间间隔
        /// </summary>
        //private static int FireAlarmLinkTimerInterval = 4000; 
        /// <summary>
        /// 执行指令休眠时间
        /// </summary>
        //protected static int ExeInstructSleepTime = 500;
        protected const int ExeInstructSleepTime = 500;
        /// <summary>
        /// 主机板接收数据长度
        /// </summary>
        //protected static int HostBoardReceiveDataLength = 9;
        protected const int HostBoardReceiveDataLength = 9;
        /// <summary>
        /// 主机板发送数据长度
        /// </summary>
        //private static int HostBoardSendDataLength = 5;
        private const int HostBoardSendDataLength = 5;
        /// <summary>
        /// 返回主机板状态字
        /// </summary>
        public static byte HostBoardReturnStatus;
        /// <summary>
        /// 返回主机板复位按钮命令
        /// </summary>
        public static byte HostBoardReturnReset;
        /// <summary>
        /// 发送主机板状态字
        /// </summary>
        public static byte HostBoardSendStatus;
        /// <summary>
        /// 主控板无数据返回的次数
        /// </summary>
        public static int DroppedCount = 0;
        /// <summary>
        /// 主机板电池电压
        /// </summary>
        public static double HostBatVoltage;
        /// <summary>
        /// 发送主机板数据的数组
        /// </summary>
        public static byte[] HostBoardSendData = new byte[HostBoardSendDataLength];
        /// <summary>
        /// 接收主机板数据的数组
        /// </summary>
        public static byte[] HostBoardReceiveData = new byte[HostBoardReceiveDataLength];
        /// <summary>
        /// 发送火灾报警器数据的数组
        /// </summary>
        protected static byte[] FireAlarmLinkSendData = new byte[5];
        /// <summary>
        /// 接收火灾报警器数据的数组
        /// </summary>
        protected static byte[] FireAlarmLinkReceiveData = new byte[64];
        /// <summary>
        /// 火灾号列表
        /// </summary>
        protected static List<int> LstFireAlarmLinkZoneNumber = new List<int>();
        /// <summary>
        /// 已经处于火灾联动的火灾号列表
        /// </summary>
        public static List<int> LstOldZoneNumber = new List<int>();
        private static List<int> LstNewZoneNumber = new List<int>();
        /// <summary>
        /// 主机板串口
        /// </summary>
        protected static SerialPort HostBoardSerialPort;
        /// <summary>
        /// 当前串口
        /// </summary>
        public static string SerialPortType;
        /// <summary>
        /// 主机串口是否空闲
        /// </summary>
        // MODIFY:
        //    @1 2021/10/05 初始值改为假
        protected static bool IsSerialPortFree = false;

        private static Thread HostBoardAndFireAlarmLinkTimer;
        /// <summary>
        /// 选中的火灾报警器
        /// </summary>
        private static object SelectFireAlarmType;
        /// <summary>
        /// 所有的分区设置
        /// </summary>
        protected static List<FireAlarmPartitionSetInfo> LstFireAlarmPartitionSet;

        private static List<FireAlarmTypeInfo> LstFireAlarmType;
        /// <summary>
        /// 标志主机串口是否已打开
        /// </summary>
        public static bool serialPortIsOpen;
        /// <summary>
        /// 是否同步收发
        /// </summary>
        public static bool IsSyTrans;

        public static bool IsHostThread = true;

        public static object locker = new object();

        public static object Locker = new object();

        private static List<GblSettingInfo> LstGblSetting = new List<GblSettingInfo>();

        private static CGblSetting ObjGblSetting = new CGblSetting();

        /// <summary>
        /// 初始化主机板和火灾报警器串口
        /// </summary>
        public static void InitHostBoardAndFireAlarmLinkSerialPort(
          List<GblSettingInfo> lstGblSetting, List<FireAlarmTypeInfo> lstFireAlarmType,
          List<FireAlarmPartitionSetInfo> LstFireAlarmPartitionSetInput,
          MainWindow context)
        {
            _context = context;
            LstGblSetting = lstGblSetting;
            LstFireAlarmType = lstFireAlarmType;
            InitDataSource(LstFireAlarmPartitionSetInput);
            ReleaseHostBoardSerialPort();
            if (LstGblSetting.Find(x => x.Key == "HostBoardPort").SetValue != null)
            {
                OpenHostBoardSerialPort(LstGblSetting.Find(x => x.Key == "HostBoardPort").SetValue);
            }
            else
            {
                throw new ArgumentNullException("The communication serial port of mainboard is empty");
            }
            InitTimer();
            ReStartTimer();
        }

        /// <summary>
        /// 初始化数据源
        /// </summary>
        private static void InitDataSource(List<FireAlarmPartitionSetInfo> LstFireAlarmPartitionSetInput)
        {
            LstFireAlarmPartitionSet = LstFireAlarmPartitionSetInput;
            LstGblSetting = ObjGblSetting.GetAll();
        }

        /// <summary>
        /// 初始化定时处理主机板和火灾报警器数据
        /// </summary>
        private static void InitTimer()
        {
            HostBoardAndFireAlarmLinkTimer = new Thread(() =>
            {
                while (true)
                {
                    if (IsHostThread)
                    {
                        if (_context.IsLinkageTiming)//|| _context.IsComEmergency
                        {
                            _context.IndicatorLight();
                        }
                        HostBoardAndFireAlarmLinkTimer_Tick(null, null);
                    }
                    Thread.Sleep(500);
                }
            });
            HostBoardAndFireAlarmLinkTimer.Start();
            HostBoardAndFireAlarmLinkTimer.IsBackground = true;
        }

        /// <summary>
        /// 释放主机板串口
        /// </summary>
        public static void ReleaseHostBoardSerialPort()
        {
            lock (locker)
            {
                IsSerialPortFree = false;
                IsSyTrans = false;
                if (HostBoardSerialPort != null && HostBoardSerialPort.IsOpen)
                {
                    HostBoardSerialPort.Dispose();
                }
            }
        }

        public static bool IsHostSerialPort(string SerialPortName)
        {
            if (HostBoardSerialPort?.PortName == SerialPortName)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 开启主机板和火灾报警器串口 V2.0
        /// </summary>
        public static void OpenHostBoardSerialPort(string PortName)
        {
            if (PortName != null)
            {
                while (HostBoardSerialPort != null && HostBoardSerialPort.IsOpen && !IsSerialPortFree)
                {
                    System.Windows.Forms.Application.DoEvents();
                    Thread.Sleep(1000);
                }
                ReleaseHostBoardSerialPort();
                IsSerialPortFree = false;
                try
                {
                    if (!OpenSerialPort(PortName))
                    {
                        bool IsOpen = false;
                        IsSerialPortFree = false;
                        for (int i = 0; i < 5; i++)
                        {
                            if (LstGblSetting.Find(x => x.Key == "HostBoardPort").SetValue != null)
                            {
                                ReleaseHostBoardSerialPort();
                                IsOpen = OpenSerialPort(PortName);
                                //System.Windows.Forms.Application.DoEvents();
                            }
                            if (IsOpen)
                            {
                                break;
                            }
                            else
                            {
                                ReleaseHostBoardSerialPort();
                                LstGblSetting.Find(x => x.Key == "HostBoardPort").SetValue = null;
                                ObjGblSetting.Save(LstGblSetting);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    //MessageBox.Show(ex.ToString());
                    _ = LoggerManager.WriteInfo(ex.Message);

                    //IsSerialPortFree = false;
                    //HostBoardSerialPort = null;
                    ReleaseHostBoardSerialPort();
                    var infoGblSetting = LstGblSetting.Find(x => x.Key == "HostBatVoltage");
                    if (infoGblSetting != null)
                    {
                        infoGblSetting.SetValue = string.Empty;
                        _ = ObjGblSetting.Update(infoGblSetting);
                    }

                }
            }
            else
            {
                ReleaseHostBoardSerialPort();
                var infoGblSetting = LstGblSetting.Find(x => x.Key == "HostBatVoltage");
                if (infoGblSetting != null)
                {
                    infoGblSetting.SetValue = string.Empty;
                    _ = ObjGblSetting.Update(infoGblSetting);
                }
            }
        }


        private static bool OpenSerialPort(string PortName)
        {
            lock (locker)
            {
                try
                {
                    HostBoardSerialPort = new SerialPort
                    {
                        PortName = PortName,
                        BaudRate = (int)EnumClass.BaudRateClass.配电箱和主机板
                    };
                    HostBoardSerialPort.DataReceived += HostBoardSerialPort_DataReceived;
                    HostBoardSerialPort.Open();
                    IsSerialPortFree = true;

                    CheckHostSerialPort();
                    if (SerialPortType != "HostBoardPort")
                    {
                        return false;
                    }
                    return true;
                }
                catch
                {
                    ReleaseHostBoardSerialPort();
                    var infoGblSetting = LstGblSetting.Find(x => x.Key == "HostBatVoltage");
                    if (infoGblSetting != null)
                    {
                        infoGblSetting.SetValue = string.Empty;
                        _ = ObjGblSetting.Update(infoGblSetting);
                    }
                    return false;
                }
            }
        }

        /// <summary>
        /// 重启定时器
        /// </summary>
        private static void ReStartTimer()
        {
            //HostBoardAndFireAlarmLinkTimer.Enabled = true;
        }

        /// <summary>
        /// 清空主机板和火灾报警器数据
        /// </summary>
        private static void ClearHostBoardAndFireAlarmLinkData()
        {
            LstFireAlarmLinkZoneNumber.Clear();
            HostBoardSendData = Enumerable.Repeat((byte)0, HostBoardSendData.Length).ToArray();
            HostBoardReceiveData = Enumerable.Repeat((byte)0, HostBoardReceiveData.Length).ToArray();
            FireAlarmLinkSendData = Enumerable.Repeat((byte)0, FireAlarmLinkSendData.Length).ToArray();
            FireAlarmLinkReceiveData = Enumerable.Repeat((byte)0, FireAlarmLinkReceiveData.Length).ToArray();
        }

        private void IsLinkage() { }

        // MODIFY: 
        //    @1 2021/10/05 移除串口通信失败处理，修改锁的作用范围
        public static void SendHostBoardData(byte HostBoardStatus)
        {
            if (IsSerialPortFree)
            {
                lock (locker)
                {
                    if (IsSerialPortFree)
                    {
                        IsSerialPortFree = false;
                        //while(IsSyTrans)
                        //{
                        //    System.Windows.Forms.Application.DoEvents();
                        //    Thread.Sleep(50);
                        //}
                        if (!IsSyTrans && HostBoardSerialPort != null)
                        {
                            try
                            {
                                IsSyTrans = true;
                                HostBoardSendData[0] = 0xAA;
                                HostBoardSendData[1] = 0x01;
                                HostBoardSendData[2] = HostBoardStatus;
                                HostBoardSendData[3] = 0x00;
                                HostBoardSendData[4] = GetCheckSum();

                                HostBoardSerialPort.Write(HostBoardSendData, 0, HostBoardSendDataLength);
                                //IsSyTrans = false;
                            }
                            catch
                            {
                                _ = LoggerManager.WriteError("Serial data transmission failed");
                                //CommonFunct.GetRemoteConnectRecord(ex.Message);
                            }
                        }
                        IsSerialPortFree = true;
                    }
                }
                Thread.Sleep(300);
                ReceiveHostBoardData();
            }
        }

        /// <summary>
        /// 发送主机板数据
        /// </summary>
        // MODIFY:
        // @1 2021/10/05 修改锁的作用范围；修改调用线程进入挂起态的代码位置
        private static void SendHostBoardData()
        {
            if (IsSerialPortFree)
            {
                lock (locker)
                {
                    if (IsSerialPortFree)
                    {
                        IsSerialPortFree = false;
                        if (!IsSyTrans && HostBoardSerialPort!=null)
                        {
                            try
                            {
                                IsSyTrans = true;
                                HostBoardSendData[0] = 0xAA;
                                HostBoardSendData[1] = 0x01;
                                HostBoardSendData[2] = HostBoardSendStatus;
                                HostBoardSendData[3] = 0x00;
                                HostBoardSendData[4] = GetCheckSum();

                                HostBoardSerialPort.Write(HostBoardSendData, 0, HostBoardSendDataLength);
                            }
                            catch
                            {
                                _ = LoggerManager.WriteWarn("Serial data transmission failed");
                                //CommonFunct.GetRemoteConnectRecord(ex.Message);
                            }
                        }
                        IsSerialPortFree = true;
                    }
                }
            }
        }

        public static void CheckHostSerialPort()
        {
            SerialPortType = string.Empty;
            while (HostBoardSerialPort != null && HostBoardSerialPort.IsOpen && !IsSerialPortFree)
            {
                System.Windows.Forms.Application.DoEvents();
                Thread.Sleep(1000);
            }
            if (IsSerialPortFree)
            {
                while (IsSyTrans)
                {
                    System.Windows.Forms.Application.DoEvents();
                    Thread.Sleep(1000);
                }
                lock (locker)
                {
                    if (IsSerialPortFree)
                    {
                        IsSerialPortFree = false;
                        if (!IsSyTrans)
                        {
                            try
                            {
                                IsSyTrans = true;
                                HostBoardSendData[0] = 0xAA;
                                HostBoardSendData[1] = 0x55;
                                HostBoardSendData[2] = HostBoardSendData[3] = 0x00;
                                HostBoardSendData[4] = GetCheckSum();
                                HostBoardSerialPort.Write(HostBoardSendData, 0, HostBoardSendDataLength);
                            }
                            catch
                            {
                                _ = LoggerManager.WriteWarn("Serial data transmission failed");
                                //CommonFunct.GetRemoteConnectRecord(ex.Message);
                            }
                        }
                        IsSerialPortFree = true;
                    }
                }
                Thread.Sleep(300);
                ReceiveHostBoardData();
                //跳过校验
                SerialPortType = "HostBoardPort";
            }
        }

        /// <summary>
        /// 接收主机板数
        /// </summary>
        // MODIFY: 
        //    @1 2021/10/05 修改锁的作用范围
        private static void ReceiveHostBoardData()
        {
            try
            {
                if (IsSyTrans)//IsSerialPortFree
                {
                    Thread.Sleep(150);
                    lock (locker)
                    {
                        if (IsSyTrans)
                        {
                            IsSerialPortFree = false;
                            var bytesToRead = HostBoardSerialPort.BytesToRead;
                            ClearHostBoardAndFireAlarmLinkData();
                            if (bytesToRead > 0)
                            {
                                DroppedCount = 0;
                                _ = HostBoardSerialPort.Read(HostBoardReceiveData, 0, HostBoardReceiveDataLength);

                                if (HostBoardReceiveData[0] == 0xAA && HostBoardReceiveData[1] == 0x02)
                                {
                                    HostBoardReturnStatus = HostBoardReceiveData[5];
                                    HostBoardReturnReset = HostBoardReceiveData[6];
                                    HostBatVoltage = ((HostBoardReceiveData[3] << 8) | HostBoardReceiveData[4]) / 100.0;

                                    List<GblSettingInfo> LstGblSetting = ObjGblSetting.GetAll();
                                    GblSettingInfo infoGblSetting = LstGblSetting.Find(x => x.Key == "HostBatVoltage");
                                    infoGblSetting.SetValue = HostBatVoltage.ToString();
                                    _ = ObjGblSetting.Update(infoGblSetting);
                                    //LoggerManager.WriteDebug((infoGblSetting is null).ToString() + "/" + HostBatVoltage.ToString());
                                }

                                if (HostBoardReceiveData[0] == 0xAA && HostBoardReceiveData[1] == 0x55)
                                {
                                    if (HostBoardReceiveData[2] == 0x01 && HostBoardReceiveData[4] == 0x56)
                                    {
                                        SerialPortType = "DisBoxPort";
                                    }
                                    if (HostBoardReceiveData[2] == 0x02 && HostBoardReceiveData[4] == 0x57)
                                    {
                                        SerialPortType = "HostBoardPort";
                                    }
                                }

                                HostBoardSerialPort.DiscardInBuffer();
                            }
                            else
                            {
                                DroppedCount++;
                                if (DroppedCount == 10)
                                {
                                    HostBoardReturnStatus = (byte)EnumClass.HostBoardStatus.通信故障;
                                    HostBoardReturnReset = 0;
                                }
                                IsSerialPortFree = true;
                                IsSyTrans = false;
                            }
                        }
                        else if (HostBoardSerialPort == null || !HostBoardSerialPort.IsOpen)
                        {
                            throw new InvalidOperationException("mainboard serial communication is exception");
                        }
                    }
                    IsSerialPortFree = true;
                    IsSyTrans = false;
                }
                else if (HostBoardSerialPort == null || !HostBoardSerialPort.IsOpen)
                {
                    throw new InvalidOperationException("mainboard serial communication is exception");
                }
            }
            catch
            {
                HostBoardReturnStatus = (byte)EnumClass.HostBoardStatus.通信故障;
                HostBoardReturnReset = 0;
                IsSerialPortFree = true;
                IsSyTrans = false;
                //_ = LoggerManager.WriteDebug(ex.Message);
                ReleaseHostBoardSerialPort();
                if (LstGblSetting.Find(x => x.Key == "HostBoardPort").SetValue != null)
                {
                    OpenHostBoardSerialPort(LstGblSetting.Find(x => x.Key == "HostBoardPort").SetValue);
                }
            }
        }

        /// <summary>
        /// 获取校验和
        /// </summary>
        /// <returns></returns>
        private static byte GetCheckSum()
        {
            int CheckSum = 0;
            for (int i = 1; i < HostBoardSendData.Length - 1; i++)
            {
                CheckSum = (CheckSum + (int)HostBoardSendData[i]) % 256;
            }
            return (byte)CheckSum;
        }

        /// <summary>
        /// 开始火灾联动交互
        /// </summary>
        private static void OnStartFireAlarmLinkInter()
        {
            FireAlarmTypeInfo infoFireAlarmType = LstFireAlarmType.Find(x => x.IsCurrentFireAlarm == 1);
            if (infoFireAlarmType.FireAlarmCode != "GST5000H")
            {
                Type type = Type.GetType(infoFireAlarmType.FireAlarmCode);
                SelectFireAlarmType = Activator.CreateInstance(type);
                if (SelectFireAlarmType.ToString() == "PCSmallHost.FireAlarmLink.DJZMAL")
                {
                    SelectFireAlarmType.GetType().GetMethod("FireAlarmLinkInter").Invoke(SelectFireAlarmType, new object[] { });
                }

                bool same = false;
                LstNewZoneNumber = LstFireAlarmLinkZoneNumber.ToList();
                if (LstOldZoneNumber.Count == 0)
                {
                    if (LstNewZoneNumber.Count > 0)
                    {
                        LstOldZoneNumber = LstNewZoneNumber.ToList();
                        same = true;
                    }
                }
                else
                {
                    for (int i = 0; i < LstNewZoneNumber.Count; i++)
                    {
                        if (!LstOldZoneNumber.Contains(LstNewZoneNumber[i]))
                        {
                            same = true;
                            LstOldZoneNumber.Add(LstNewZoneNumber[i]);
                        }
                    }
                }

                if (same)
                {
                    MainWindow.OnStartRealFireAlarmLink(LstNewZoneNumber);
                }
            }
        }

        public static void HostBoardAndFireAlarmLinkTimer_Tick(object sender, EventArgs e)
        {
            while (HostBoardSerialPort != null && HostBoardSerialPort.IsOpen && !IsSerialPortFree)
            {
                System.Windows.Forms.Application.DoEvents();
                Thread.Sleep(1000);
            }
            lock (Locker)
            {
                ClearHostBoardAndFireAlarmLinkData();
                SendHostBoardData();
                ReceiveHostBoardData();
                OnStartFireAlarmLinkInter();
            }
        }

        private static void FireAlarmSerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {

        }

        private static void HostBoardSerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {

        }
    }
}
