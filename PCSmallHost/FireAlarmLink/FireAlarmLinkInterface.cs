using FireLinkage;
using PCSmallHost.DB.BLL;
using PCSmallHost.DB.Model;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PCSmallHost.FireAlarmLink
{
    public class FireAlarmLinkInterface
    {
        /// <summary>
        /// 火灾报警器串口
        /// </summary>
        protected static SerialPort FireAlarmSerialPort;
        /// <summary>
        /// 发送火灾报警器数据的数组
        /// </summary>
        protected static byte[] FireAlarmLinkSendData = new byte[5];
        /// <summary>
        /// 执行指令休眠时间
        /// </summary>
        protected static int ExeInstructSleepTime = 500;
        /// <summary>
        /// 选中的火灾报警器
        /// </summary>
        private static object FireAlarmType;
        /// <summary>
        /// 定时处理火灾报警器数据
        /// </summary>
        private static System.Windows.Forms.Timer TimerDealFireAlarmData;

        private static Thread ThreadDealFireAlarmData;
        /// <summary>
        /// 用于接收火灾报警器数据的数组
        /// </summary>
        protected static byte[] FireAlarmReceiveData = new byte[65600];
        /// <summary>
        /// 用于记录上海松江的控制器号
        /// </summary>
        protected static byte ControllerNum = 0;

        private static List<FireAlarmPartitionSetInfo> LstFireAlarmPartitionSet;
        /// <summary>
        /// 火灾报警器类型
        /// </summary>
        public static List<FireAlarmTypeInfo> LstFireAlarmType;

        private static List<GblSettingInfo> LstGblSetting;

        protected static List<int> LstFireAlarmLinkZoneNumber = new List<int>();
        public static List<int> LstOldNumber = new List<int>();
        public static List<int> LstNewNumber = new List<int>();

        private static CFireAlarmPartitionSet ObjFireAlarmPartitionSet = new CFireAlarmPartitionSet();
        private static CFireAlarmType ObjFireAlarmType = new CFireAlarmType();
        private static CGblSetting ObjGblSetting = new CGblSetting();
        public static void InitFireAlarmSerialPort()
        {
            InitDataSource();
            CloseFireAlarmSerialPort();
            OpenFireAlarmSerialPort();
            //StopTimer();
            InitTimerDealFireAlarmData();
        }

        /// <summary>
        /// 初始化数据源
        /// </summary>
        public static void InitDataSource()
        {
            LstFireAlarmPartitionSet = ObjFireAlarmPartitionSet.GetAll();
            LstFireAlarmType = ObjFireAlarmType.GetAll();
            LstGblSetting = ObjGblSetting.GetAll();
        }

        /// <summary>
        /// 关闭火灾报警器串口以及定时器
        /// </summary>
        public static void CloseFireAlarmSerialPort()
        {
            if (FireAlarmSerialPort != null && FireAlarmSerialPort.IsOpen)
            {
                //TimerDealFireAlarmData.Enabled = false;
                //StopTimer();
                FireAlarmSerialPort.Close();
            }
            if(ThreadDealFireAlarmData != null)
            {
                ThreadDealFireAlarmData.Abort();
            }
        }

        public static bool IsFireAlarmSerialPort(string SerialPortName)
        {
            if (FireAlarmSerialPort != null && FireAlarmSerialPort.PortName == SerialPortName)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 开启火灾报警器串口
        /// </summary>
        private static void OpenFireAlarmSerialPort()
        {
            try
            {
                if (LstGblSetting.Find(x => x.Key == "FireAlarmPort").SetValue != string.Empty)//判断火灾报警器是否设定COM
                {
                    //根据系统参数维护的火灾报警器进行初始实例化   
                    FireAlarmSerialPort = new SerialPort();
                    FireAlarmSerialPort.PortName = LstGblSetting.Find(x => x.Key == "FireAlarmPort").SetValue;
                    FireAlarmSerialPort.BaudRate = Convert.ToInt32(LstGblSetting.Find(x => x.Key == "FireAlarmBaudRate").SetValue);
                    FireAlarmSerialPort.DataReceived += FireAlarmSerialPort_DataReceived;
                    FireAlarmSerialPort.Open();

                    string strFireAlarmCode = LstFireAlarmType.Find(x => x.IsCurrentFireAlarm == 1).FireAlarmCode;//获取当前选择的火灾报警器
                    Type type = Type.GetType(strFireAlarmCode);
                    FireAlarmType = Activator.CreateInstance(type);
                }
            }
            catch
            {
                FireAlarmType = null;
            }
        }

        /// <summary>
        /// 火灾报警器串口触发数据返回事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void FireAlarmSerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {

        }

        /// <summary>
        /// 清空火灾报警器数据
        /// </summary>
        public static void ClearFireAlarmReceiveData()
        {
            FireAlarmLinkSendData = Enumerable.Repeat((byte)0, FireAlarmLinkSendData.Length).ToArray();
            FireAlarmReceiveData = Enumerable.Repeat((byte)0, FireAlarmReceiveData.Length).ToArray();
        }

        /// <summary>
        /// 停止定时器
        /// </summary>
        private static void StopTimer()
        {
            if (TimerDealFireAlarmData != null)
            {
                TimerDealFireAlarmData.Enabled = false;
            }
        }

        /// <summary>
        /// 初始化定时处理火灾报警器数据
        /// </summary>
        private static void InitTimerDealFireAlarmData()
        {
            ThreadDealFireAlarmData = new Thread(() =>
             {
                 while(true)
                 {
                     if(FireAlarmSerialPort != null && FireAlarmSerialPort.IsOpen)
                     {
                         if(FireAlarmType != null)
                         {
                             TimerDealFireAlarmData_Tick();
                         }
                     }
                     Thread.Sleep(2000);
                 }
             });
            ThreadDealFireAlarmData.Start();
            ThreadDealFireAlarmData.IsBackground = true;

            //if (FireAlarmType != null)
            //{
            //    TimerDealFireAlarmData = new System.Windows.Forms.Timer();
            //    TimerDealFireAlarmData.Interval = 2000;
            //    TimerDealFireAlarmData.Tick += TimerDealFireAlarmData_Tick;
            //    TimerDealFireAlarmData.Enabled = true;
            //}
        }

        private static void TimerDealFireAlarmData_Tick()
        {
            try
            {
                LstFireAlarmLinkZoneNumber.Clear();
                //TimerDealFireAlarmData.Enabled = false;

                if (FireAlarmType.ToString() != "PCSmallHost.FireAlarmLink.DJZMAL")
                {
                    if (FireAlarmType.ToString() == "PCSmallHost.FireAlarmLink.DJZMSL")
                    {
                        FireAlarmType.GetType().GetMethod("FireAlarmLinkInter").Invoke(FireAlarmType, new object[] { });
                    }
                    else
                    {
                        int bytesToRead = FireAlarmSerialPort.BytesToRead;
                        if (bytesToRead == 0)
                        {
                            TimerDealFireAlarmData.Enabled = true;
                            return;
                        }
                        FireAlarmSerialPort.Read(FireAlarmReceiveData, 0, bytesToRead);

                        if (FireAlarmType.ToString() == "PCSmallHost.FireAlarmLink.JB3208")
                        {
                            byte[] data =
                            {
                                FireAlarmReceiveData[1],
                                FireAlarmReceiveData[2],
                                FireAlarmReceiveData[3],
                                FireAlarmReceiveData[4],
                                FireAlarmReceiveData[5]
                            };
                            byte[] crc16 =
                            {
                                FireAlarmReceiveData[6],
                                FireAlarmReceiveData[7]
                            };
                            if (CRC.CheckWithCRC16(data, crc16, true))
                            {
                                if (FireAlarmReceiveData[2] == 100 && FireAlarmReceiveData[5] == 100)
                                {
                                    ControllerNum = FireAlarmReceiveData[1];//记录上海松江的控制器号
                                    FireAlarmType.GetType().GetMethod("FireAlarmLinkInter").Invoke(FireAlarmType, new object[] { });
                                }
                            }
                        }
                        else
                        {
                            int fireAlarmZoneNumber = Convert.ToInt32(FireAlarmType
                                .GetType()
                                .GetMethod("DealFireAlarmData")
                                .Invoke(FireAlarmType, new object[] { FireAlarmReceiveData, LstFireAlarmPartitionSet }));//跳转到北大青鸟等的火灾报警器类里面的DealFireAlarmData函数去，获取该数据对应系统中的预案分区号

                            //fireAlarmZoneNumber = 1;
                            if (fireAlarmZoneNumber != 0 && !LstFireAlarmLinkZoneNumber.Contains(fireAlarmZoneNumber))
                            {
                                LstFireAlarmLinkZoneNumber.Add(fireAlarmZoneNumber);
                            }
                        }
                    }
                }
                bool same = false;
                LstNewNumber= LstFireAlarmLinkZoneNumber.ToList();
                if (LstOldNumber.Count == 0)
                {
                    if (LstNewNumber.Count > 0)
                    {
                        LstOldNumber = LstNewNumber.ToList();
                        same = true;
                    }
                }
                else
                {
                    for (int i = 0; i < LstNewNumber.Count; i++)
                    {
                        if (!LstOldNumber.Contains(LstNewNumber[i]))
                        {
                            same = true;
                            LstOldNumber.Add(LstNewNumber[i]);
                        }
                    }
                }

                if (same)
                {
                    MainWindow.OnStartRealFireAlarmLink(LstNewNumber);
                }
                ClearFireAlarmReceiveData();
                //TimerDealFireAlarmData.Enabled = true;
            }
            catch
            {
                //GetRemoteConnectRecord(ex.StackTrace);
            }
        }

    }
}
