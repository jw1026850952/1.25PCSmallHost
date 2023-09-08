using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;
using PCSmallHost.DB.Model;
using PCSmallHost.Util;
using PCSmallHost.DB.BLL;
using static PCSmallHost.Util.EnumClass;
using Sugar.Log;

namespace PCSmallHost.ConSysProtocol
{
    /// <summary>
    /// 控制系统协议
    /// </summary>
    public class Protocol
    {
        /// <summary>
        /// EPS使用串口
        /// </summary>
        private static SerialPort EPSSerialPort;
        /// <summary>
        /// 当前串口的通讯类型
        /// </summary>
        private static string SerialPortType;
        /// <summary>
        /// 记录串口是否空闲
        /// </summary>
        private static bool IsSerialPortFree = true;
        private static object locker = new object();
        /// <summary>
        /// EPS发送数据的数组
        /// </summary>
        private static byte[] EPSSendData = new byte[2048];
        //private static byte[] EPSSendData = new byte[8];
        /// <summary>
        /// 接收EPS数据的数组
        /// </summary>
        public static byte[] EPSReceiveData = new byte[2048];

        private static int CurrentExeInstructTime = 0;

        private static List<byte> LstEPSReceiveData = new List<byte>();
        /// <summary>
        /// 配电箱接收数据的数组长度
        /// </summary>
        private static int EPSReceiveDataLength;
        /// <summary>
        /// 灯状态字以及灯码前两位不属于相关数据
        /// </summary>
        private static int HeadDataLength = 2;
        /// <summary>
        /// 前面16位不属于灯码数据
        /// </summary>
        private static int LightCodeFrontDataLength = 16;
        /// <summary>
        /// 前面5位不属于EPS码数据
        /// </summary>
        private static int EPSCodeFrontDataLength = 5;
        /// <summary>
        /// 灯码组成的位数
        /// </summary>
        private static int LightCodeDataLength = 3;
        /// <summary>
        /// EPS每段数据长度(包括帧头)
        /// </summary>
        private static int EPSPerParaDataLength = 8;
        /// <summary>
        /// EPS发送数据的长度
        /// </summary>
        private static int EPSSendDataLength = 8;
        /// <summary>
        /// 指令休眠时间
        /// </summary>
        private static int ExeInstructSleepTime = 5;
        /// <summary>
        /// 一般定时器间隔
        /// </summary>
        private static int CommonTimeInterval = 300;
        /// <summary>
        /// 查询配电箱下灯码定时器间隔
        /// </summary>
        private static int QueryLightByEPSTimeInterval = 3500;
        /// <summary>
        /// 查询配电箱下灯状态定时器间隔
        /// </summary>
        private static int QueryLightStatusByEPSTimeInterval = 4000;
        /// <summary>
        /// 快速查询配电箱下灯码定时器间隔
        /// </summary>
        private static int TimerDisBoxReceiveDataFastlyInterval = 15000;
        /// <summary>
        /// 单灯添加或者替换定时器间隔
        /// </summary>
        private static int AddOrReplaceSignalLightTimeInterval = 1000;
        /// <summary>
        /// EPS信息百分比
        /// </summary>
        private static double EPSInfoPercent = 10;
        /// <summary>
        /// 获取配电箱数据的定时器
        /// </summary>
        private static System.Windows.Forms.Timer TimerDisBoxReceiveData;

        //private 
        /// <summary>
        /// 获取快速搜EPS的配电箱数据的定时器
        /// </summary>
        private static System.Windows.Forms.Timer TimerDisBoxReceiveDataFastly;
        /// <summary>
        /// 检测到接收到乱码时重新发送
        /// </summary>
        private static int SendAgainCount = 0;
        /// <summary>
        /// 检测串口是否打开
        /// </summary>
        public static bool SerialPortIsOpen;

        private static List<LightInfo> LstLight = new List<LightInfo>();

        private static List<DistributionBoxInfo> LstDistributionBox = new List<DistributionBoxInfo>();

        private static List<GblSettingInfo> LstGblSetting = new List<GblSettingInfo>();

        private static CGblSetting ObjGblSetting = new CGblSetting();

        private static CDistributionBox ObjDistributionBox = new CDistributionBox();

        private static CLight ObjLight = new CLight();

        /// <summary>
        /// 初始化EPS串口
        /// </summary>
        public static void InitEPSSeialPort(List<GblSettingInfo> lstGblSetting)
        {
            LstGblSetting = lstGblSetting;
            InitEPSSerialPortTimer();
            if (LstGblSetting.Find(x => x.Key == "DisBoxPort").SetValue != null)
            {
                lock (locker)
                {
                    InitDisBoxSerialPort(LstGblSetting.Find(x => x.Key == "DisBoxPort").SetValue);
                }
            }
            else
            {
                EPSSerialPort = null;
            }
        }

        /// <summary>
        /// 释放EPS和主机板串口
        /// </summary>
        public static void ReleaseEPSSerialPort()
        {
            if (EPSSerialPort != null && EPSSerialPort.IsOpen)
            {
                EPSSerialPort.Dispose();
            }
        }

        public static bool IsEPSSerialPort(string SerialPortName)
        {
            if (EPSSerialPort != null && EPSSerialPort.PortName == SerialPortName)
            {
                return true;
            }
            return false;
        }

        private static void GetSerialPortIsOpen()
        {
            if (EPSSerialPort != null && EPSSerialPort.IsOpen)
            {
                SerialPortIsOpen = true;
            }
            else
            {
                SerialPortIsOpen = false;
            }
        }

        /// <summary>
        /// 打开串口
        /// </summary>
        /// <returns></returns>
        private static bool OpenSerialPort()
        {
            try
            {
                if (!EPSSerialPort.IsOpen)
                {
                    EPSSerialPort.Open();
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 初始化配电箱串口
        /// </summary>
        public static void InitDisBoxSerialPort(string PortName)
        {
            try
            {
                if (PortName != null)
                {
                    ReleaseEPSSerialPort();

                    EPSSerialPort = new SerialPort();
                    EPSSerialPort.PortName = PortName;
                    EPSSerialPort.BaudRate = (int)EnumClass.BaudRateClass.配电箱和主机板;
                    EPSSerialPort.DataReceived += DisBoxSerialPort_DataReceived;
                    EPSSerialPort.Open();

                    if (CheckEpsSeialPort() != "DisBoxPort")
                    {
                        ReleaseEPSSerialPort();
                    }
                }
                else
                {
                    ReleaseEPSSerialPort();     
                }
            }
            catch
            {
                ReleaseEPSSerialPort();
            }
        }

        /// <summary>
        /// 串口掉线处理
        /// </summary>
        private static void OfflineHandling(byte[] DisBoxData, int WaitTime)
        {
            LstGblSetting = ObjGblSetting.GetAll();
            if (EPSSerialPort != null)
            {
                if (!EPSSerialPort.IsOpen)
                {
                    bool IsOpen = false;
                    for (int i = 0; i < 5; i++)
                    {
                        if (OpenSerialPort())
                        {
                            IsOpen = OpenSerialPort();
                        }
                        if (IsOpen)
                        {
                            break;
                        }
                    }
                    if (IsOpen)
                    {
                        if (CheckEpsSeialPort() == "DisBoxPort")
                        {
                            SendDataAgain(DisBoxData, WaitTime);
                        }
                        else
                        {
                            InitDisBoxSerialPort(LstGblSetting.Find(x => x.Key == "DisBoxPort").SetValue);
                            if (EPSSerialPort.IsOpen)
                            {
                                SendDataAgain(DisBoxData, WaitTime);
                            }
                        }
                    }
                    else
                    {
                        InitDisBoxSerialPort(LstGblSetting.Find(x => x.Key == "DisBoxPort").SetValue);//串口掉线后，打开不了串口，需要重新初始化串口
                        if (EPSSerialPort.IsOpen)
                        {
                            SendDataAgain(DisBoxData, WaitTime);
                        }
                    }
                }
            }
            else
            {
                InitDisBoxSerialPort(LstGblSetting.Find(x => x.Key == "DisBoxPort").SetValue);//串口掉线后，打开不了串口，需要重新初始化串口
                if (EPSSerialPort.IsOpen)
                {
                    SendDataAgain(DisBoxData, WaitTime);
                }
            }
            IsSerialPortFree = true;
        }

        private static void SendDataAgain(byte[] DisBoxData, int WaitTime)
        {
            EPSSerialPort.DiscardOutBuffer();//情空串口发送缓冲区的数据
            EPSSerialPort.DiscardInBuffer();//清空串口接收缓冲区的数据
            if (DisBoxData[0] == 0XAB || DisBoxData[0] == 0XA0 || DisBoxData[0] == 0XA2 || DisBoxData[0] == 0XA4)
            {
                EPSSerialPort.Write(DisBoxData, 0, EPSSendDataLength);
            }
            else
            {
                if (DisBoxData[1] == 0X55)
                {
                    EPSSerialPort.Write(DisBoxData, 0, 5);
                }
                else
                {
                    EPSSerialPort.Write(DisBoxData, 0, EPSPerParaDataLength);
                }
            }

            while (CurrentExeInstructTime < WaitTime)
            {
                System.Windows.Forms.Application.DoEvents();
                Thread.Sleep(10);
                CurrentExeInstructTime += 10;
            }
            CurrentExeInstructTime = 0;
            try
            {
                GetSerialPortIsOpen();
                ClearEPSSendAndReceiveData();
                EPSReceiveDataLength = EPSSerialPort.BytesToRead;
                if (EPSReceiveDataLength > 0)
                {
                    EPSSerialPort.Read(EPSReceiveData, 0, EPSReceiveDataLength);
                }

                if (EPSReceiveData[0] == 0XAA && EPSReceiveData[1] == 0X55)
                {
                    if (EPSReceiveData[2] == 0X01 && EPSReceiveData[4] == 0X56)
                    {
                        SerialPortType = "DisBoxPort";
                    }
                    if (EPSReceiveData[2] == 0X02 && EPSReceiveData[4] == 0X57)
                    {
                        SerialPortType = "HostBoardPort";
                    }
                }
                else
                {
                    LstEPSReceiveData.Clear();

                    for (int i = 0; i < EPSReceiveDataLength; i++)
                    {
                        LstEPSReceiveData.Add(EPSReceiveData[i]);
                    }
                }

                EPSSerialPort.DiscardInBuffer();
                IsSerialPortFree = true;
            }
            catch
            {

            }
            IsSerialPortFree = true;
            TimerDisBoxReceiveData.Enabled = false;
        }

        /// <summary>
        /// 初始化EPS定时器
        /// </summary>
        private static void InitEPSSerialPortTimer()
        {
            TimerDisBoxReceiveData = new System.Windows.Forms.Timer();
            TimerDisBoxReceiveData.Tick += TimerDisBoxReceiveData_Tick;
            TimerDisBoxReceiveData.Enabled = false;

            TimerDisBoxReceiveDataFastly = new System.Windows.Forms.Timer();
            TimerDisBoxReceiveDataFastly.Tick += TimerDisBoxReceiveDataFastly_Tick;
            TimerDisBoxReceiveDataFastly.Enabled = false;
        }

        /// <summary>
        /// 配电箱串口触发数据返回事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void DisBoxSerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {

        }

        /// <summary>
        /// 发送指令
        /// </summary>
        /// <param name="DisBoxData">需要发送的数据</param>
        /// <param name="WaitTime">等待接收</param>
        private static void DisBoxSendData(byte[] DisBoxData, int SendDataCount, int WaitTime)
        {
            try
            {
                while (!IsSerialPortFree)
                {
                    System.Windows.Forms.Application.DoEvents();
                    Thread.Sleep(100);
                }
                IsSerialPortFree = false;

                lock (locker)
                {
                    if (SendDataCount == 0)
                    {
                        if (DisBoxData[1] == 0X55)
                        {
                            EPSSerialPort.DiscardInBuffer();
                            EPSSerialPort.Write(DisBoxData, 0, 5);
                        }
                        else
                        {
                            if(DisBoxData[0] == 0XAA && DisBoxData[1] != 0X00 && DisBoxData[1] != 0X01 && DisBoxData[1] != 0X02 && DisBoxData[1] != 0X04)
                            {
                                EPSSerialPort.DiscardInBuffer();
                            }
                            EPSSerialPort.Write(DisBoxData, 0, EPSPerParaDataLength);
                        }
                    }
                    else
                    {
                        EPSSerialPort.DiscardInBuffer();
                        EPSSerialPort.Write(DisBoxData, 0, SendDataCount);
                    }
                }

                if (DisBoxData[0] == 0XAA && DisBoxData[1] == 0X23)
                {
                    int CurrentTime = 0;
                    while (CurrentTime < WaitTime)
                    {
                        int count = EPSSerialPort.BytesToRead;
                        if (count == (LstDistributionBox.Count * 8))
                        {
                            EPSSerialPort.Read(EPSReceiveData, 0, count);
                            //TimerDisBoxReceiveData.Enabled = true;//开启计时器，跳转到计时器函
                            IsSerialPortFree = true;
                            break;
                            //CurrentTime = WaitTime;
                        }
                        System.Windows.Forms.Application.DoEvents();
                        Thread.Sleep(ExeInstructSleepTime);
                        CurrentTime += ExeInstructSleepTime;
                    }
                }
                else
                {
                    #region 旧版问灯状态功能
                    //if (DisBoxData[0] == 0XAA && DisBoxData[1] == 0X83)
                    //{
                    //    string EPSCode = Convert.ToString((DisBoxData[5] * 256 + DisBoxData[6]) * 256 + DisBoxData[7]);
                    //    int LampCount = LstLight.FindAll(x => x.DisBoxID == Convert.ToInt32(EPSCode)).Count;
                    //    int CurrentTime = 0;
                    //    while(CurrentTime < WaitTime)
                    //    {
                    //        int count = EPSSerialPort.BytesToRead;
                    //        if (LampCount % 6 == 0)
                    //        {
                    //            if(count == (LampCount / 6) * 8)
                    //            {
                    //                EPSSerialPort.Read(EPSReceiveData, 0, count);
                    //                IsSerialPortFree = true;
                    //                EPSReceiveDataLength = count;
                    //                break;
                    //            }
                    //        }
                    //        else
                    //        {
                    //            if (count == (LampCount / 6 + 1) * 8)
                    //            {
                    //                EPSSerialPort.Read(EPSReceiveData, 0, count);
                    //                IsSerialPortFree = true;
                    //                EPSReceiveDataLength = count;
                    //                break;
                    //            }
                    //        }
                    //        //System.Windows.Forms.Application.DoEvents();
                    //        Thread.Sleep(ExeInstructSleepTime);
                    //        CurrentTime += ExeInstructSleepTime;
                    //    }
                    //}
                    //else
                    //{
                    #endregion
                    if (DisBoxData[0] == 0XAA && (DisBoxData[1] == 0X01 || DisBoxData[1] == 0X02 || DisBoxData[1] == 0X04))
                    {
                        Thread.Sleep(WaitTime);
                        IsSerialPortFree = true;
                    }
                    else
                    {
                        TimerDisBoxReceiveData.Interval = WaitTime;
                        TimerDisBoxReceiveData.Enabled = true;//开启计时器，跳转到计时器函
                    }
                    //}
                }

                while (TimerDisBoxReceiveData.Enabled)
                {
                    System.Windows.Forms.Application.DoEvents();
                    Thread.Sleep(ExeInstructSleepTime);
                }
                IsSerialPortFree = true;
            }
            catch (Exception ex)
            {
                LoggerManager.WriteDebug(ex.ToString());
                IsSerialPortFree = true;
                // TODO 串口掉线处理
                if(DisBoxData[1] != 0X00)
                {
                    OfflineHandling(DisBoxData, WaitTime);
                    if (EPSSerialPort.IsOpen)
                    {
                        if (SerialPortIsOpen)
                        {
                            SerialPortIsOpen = false;
                            //MessageBox.Show("配电箱通讯串口已关闭！！");
                            TimerDisBoxReceiveData.Enabled = false;
                        }
                    }
                    else
                    {
                        IsSerialPortFree = true;
                    }
                }
            }
        }

        private static void TimerDisBoxReceiveData_Tick(object sender, EventArgs e)
        {
            try
            {
                if (EPSSendData[0] == 0XAA && EPSSendData[1] == 0X83)
                {
                    string EPSCode = Convert.ToString((EPSSendData[5] * 256 + EPSSendData[6]) * 256 + EPSSendData[7]);
                    int LampCount = LstLight.FindAll(x => x.DisBoxID == Convert.ToInt32(EPSCode)).Count;
                    int CurrentTime = 0;
                    while (CurrentTime < 4000)
                    {
                        int count = EPSSerialPort.BytesToRead;
                        if (LampCount % 6 == 0)
                        {
                            if (count == (LampCount / 6) * 8)
                            {
                                EPSSerialPort.Read(EPSReceiveData, 0, count);
                                IsSerialPortFree = true;
                                EPSReceiveDataLength = count;
                                break;
                            }
                        }
                        else
                        {
                            if (count == (LampCount / 6 + 1) * 8)
                            {
                                EPSSerialPort.Read(EPSReceiveData, 0, count);
                                IsSerialPortFree = true;
                                EPSReceiveDataLength = count;
                                break;
                            }
                        }
                        //System.Windows.Forms.Application.DoEvents();
                        Thread.Sleep(ExeInstructSleepTime);
                        CurrentTime += ExeInstructSleepTime;
                        //LoggerManager.WriteDebug(CurrentTime.ToString());
                    }
                }
                else
                {
                    GetSerialPortIsOpen();
                    ClearEPSSendAndReceiveData();
                    EPSReceiveDataLength = EPSSerialPort.BytesToRead;
                    if (EPSReceiveDataLength > 0)
                    {
                        EPSSerialPort.Read(EPSReceiveData, 0, EPSReceiveDataLength);
                    }

                    if (EPSReceiveData[0] == 0XAA && EPSReceiveData[1] == 0X55)
                    {
                        if (EPSReceiveData[2] == 0X01 && EPSReceiveData[4] == 0X56)
                        {
                            SerialPortType = "DisBoxPort";
                        }
                        if (EPSReceiveData[2] == 0X02 && EPSReceiveData[4] == 0X57)
                        {
                            SerialPortType = "HostBoardPort";
                        }
                    }
                    else
                    {
                        LstEPSReceiveData.Clear();

                        for (int i = 0; i < EPSReceiveDataLength; i++)
                        {
                            LstEPSReceiveData.Add(EPSReceiveData[i]);
                        }
                    }
                }

                EPSSerialPort.DiscardInBuffer();
                IsSerialPortFree = true;
            }
            catch
            {
                OfflineHandling(EPSSendData, TimerDisBoxReceiveData.Interval);
                TimerDisBoxReceiveData.Enabled = false;

            }
            IsSerialPortFree = true;
            TimerDisBoxReceiveData.Enabled = false;
        }

        private static void TimerDisBoxReceiveDataFastly_Tick(object sender, EventArgs e)
        {
            try
            {
                if (EPSSerialPort != null && EPSSerialPort.IsOpen)
                {
                    SerialPortIsOpen = true;
                    EPSReceiveDataLength = EPSSerialPort.BytesToRead;
                    if (EPSReceiveDataLength > 0)
                    {
                        EPSSerialPort.Read(EPSReceiveData, 0, EPSReceiveDataLength);
                    }
                    EPSSerialPort.DiscardInBuffer();
                    IsSerialPortFree = true;
                }
                else
                {
                    if (SerialPortIsOpen)
                    {
                        SerialPortIsOpen = false;
                        //MessageBox.Show("配电箱串口已关闭！！");
                    }
                }
                TimerDisBoxReceiveDataFastly.Enabled = false;
            }
            catch
            {
                TimerDisBoxReceiveDataFastly.Enabled = false;
            }
        }

        public static string CheckEpsSeialPort()
        {
            lock (locker)
            {
                ClearEPSSendAndReceiveData();
                return CheckEPSSerialPort();
            }
        }

        /// <summary>
        /// 心跳
        /// </summary>
        public static void Heartbeat()
        {
            Task task = Task.Run(() =>
            {
                lock (locker)
                {
                    EPSSendData = Enumerable.Repeat((byte)0, EPSSendData.Length).ToArray();
                    SendDataHeartbeat();
                }
            });
        }

        /// <summary>
        /// 全体主电
        /// </summary>
        public static void AllMainEle()
        {
            lock (locker)
            {
                EPSSendData = Enumerable.Repeat((byte)0, EPSSendData.Length).ToArray();
                SendDataAllMainEle();
            }
        }

        /// <summary>
        /// 全体应急
        /// </summary>
        public static void AllEmergency()
        {
            lock (locker)
            {
                EPSSendData = Enumerable.Repeat((byte)0, EPSSendData.Length).ToArray();
                SendDataAllEmergency();
            }
        }

        /// <summary>
        /// 修改配电箱预案
        /// </summary>
        /// <param name="strEPSCode"></param>
        public static void UpdateEPSPlan(DistributionBoxInfo infoDistributionBox)
        {
            Task task = Task.Run(() =>
            {
                lock (locker)
                {
                    ClearEPSSendAndReceiveData();
                    SendDataUpdateEPSPlan(infoDistributionBox);
                }
            });
        }

        /// <summary>
        /// 寻EPS
        /// </summary>
        /// <returns></returns>
        public static bool FindEPS(string strEPSCode)
        {
            lock (locker)
            {
                ClearEPSSendAndReceiveData();
                return SendDataFindEPS(strEPSCode);
            }
        }


        /// <summary>
        /// 快速寻EPS
        /// </summary>
        /// <returns></returns>
        public static string[] FindEPSFastly()
        {
            lock (locker)
            {
                ClearEPSSendAndReceiveData();
                return SendDataFindEPSFastly();
            }
        }

        public static void HostDataUpload(LoadProgressBar progressBar)
        {
            lock (locker)
            {
                LstDistributionBox = ObjDistributionBox.GetAll();
                LstLight = ObjLight.GetAll();
                ClearEPSSendAndReceiveData();
                SendHostData(progressBar);
            }
        }
        /// <summary>
        /// 所有EPS寻灯码
        /// </summary>
        /// <param name="strLightCode"></param>
        /// <returns></returns>
        public static void FindLightByAllEPS()
        {
            lock (locker)
            {
                ClearEPSSendAndReceiveData();
                SendDataFindLightByAllEPS();
            }
        }

        public static string[] FindLightByAllEPSFastly()
        {
            lock (locker)
            {
                ClearEPSSendAndReceiveData();
                return SendDataFindLightByAllEPSFastly();
            }
        }

        /// <summary>
        /// 查询配电箱下所有灯
        /// </summary>
        /// <param name="strEPSCode"></param>
        /// <returns></returns>
        public static string[] QueryLightByEPS(string strEPSCode)
        {
            lock (locker)
            {
                ClearEPSSendAndReceiveData();
                return SendDataQueryLightByEPS(strEPSCode);
            }
        }

        public static void SearchLampFastly()
        {
            LstDistributionBox = ObjDistributionBox.GetAll();
            lock (locker)
            {
                ClearEPSSendAndReceiveData();
                SendDataQueryLightFastly();
            }
        }

        /// <summary>
        /// 问指定EPS下的灯码
        /// </summary>
        /// <param name="strEPSCode"></param>
        /// <returns></returns>
        public static string[] QueryLightByEPSFastly(string strEPSCode)
        {
            lock (locker)
            {
                ClearEPSSendAndReceiveData();
                return SendDataQueryLightByEPSFastly(strEPSCode);
            }
        }

        /// <summary>
        /// 指定EPS快速搜灯
        /// </summary>
        /// <param name="strDisCode">指定EPS</param>
        /// <returns></returns>
        public static void ADDEPSLamp(string strDisCode)
        {
            lock (locker)
            {
                ClearEPSSendAndReceiveData();
                SendDataADDEPSLamp(strDisCode);
            }
        }

        /// <summary>
        /// 查询配电箱下所有灯状态
        /// </summary>
        /// <param name="strEPSCode"></param>
        /// <returns></returns>
        public static int[] QueryLightStatusByEPS(string strEPSCode)
        {
            lock (locker)
            {
                LstLight = ObjLight.GetAll();
                ClearEPSSendAndReceiveData();
                return SendDataQueryLightStatusByEPS(strEPSCode);
            }
        }

        /// <summary>
        /// 指定EPS应急或者主电
        /// </summary>
        /// <param name="isEmergency"></param>
        /// <param name="strEPSCode"></param>
        public static void EmergencyOrMainEleByEPS(bool isEmergency, string strEPSCode)
        {
            Task task = Task.Run(() =>
            {
                lock (locker)
                {
                    ClearEPSSendAndReceiveData();
                    SendDataEmergencyOrMainEleByEPS(isEmergency, strEPSCode);
                }
            });
        }

        /// <summary>
        /// 设置灯具左预案
        /// </summary>
        /// <param name="strLightCode"></param>
        /// <returns></returns>
        public static void SetLightLeftPlan(LightInfo infoLight)
        {
            lock (locker)
            {
                ClearEPSSendAndReceiveData();
                SendDataSetLightLeftPlan(infoLight);
            }
        }

        /// <summary>
        /// 设置灯具右预案
        /// </summary>
        /// <param name="infoLight"></param>
        public static void SetLightRightPlan(LightInfo infoLight)
        {
            lock (locker)
            {
                ClearEPSSendAndReceiveData();
                SendDataSetLightRightPlan(infoLight);
            }
        }

        /// <summary>
        /// 设定单灯
        /// </summary>
        /// <param name="strLightCode"></param>
        public static Task SetSignalLight(int keyWord, string strLightCode, string strEPSCode)
        {
            Task task = Task.Run(() =>
            {
                lock (locker)
                {
                    ClearEPSSendAndReceiveData();
                    SendDataSetSignalLight(keyWord, strLightCode, strEPSCode);
                }
            });
            return task;
        }

        /// <summary>
        /// 设定灯具初始状态
        /// </summary>
        /// <param name="strLightCode"></param>
        /// <param name="lightStatus"></param>
        public static void SetLightBeginStatus(string strLightCode, int lightStatus, string strEPSCode)
        {
            lock (locker)
            {
                ClearEPSSendAndReceiveData();
                SendDataSetLightBeginStatus(strLightCode, lightStatus, strEPSCode);
            }
        }

        /// <summary>
        /// 开启断通讯应急功能
        /// </summary>
        /// <returns></returns>
        public static bool OpenEmergencySwitch()
        {
            lock(locker)
            {
                ClearEPSSendAndReceiveData();
                return SendDataEmergencyOn();
            }
        }

        public static bool CloseEmergencySwitch()
        {
            lock(locker)
            {
                ClearEPSSendAndReceiveData();
                return SendDataEmergencyOff();
            }
        }

        /// <summary>
        /// 全部灯执行预案
        /// </summary>      
        public static void ExecutePlanByAllLight(int planNumber)
        {
            lock (locker)
            {
                ClearEPSSendAndReceiveData();
                SendDataExecutePlanByAllLight(planNumber);
            }
        }

        /// <summary>
        /// 旧单灯替换
        /// </summary>      
        public static bool ReplaceSingleOldLight(string strOldLightCode, string strEPSCode)
        {
            lock (locker)
            {
                ClearEPSSendAndReceiveData();
                return SendDataReplaceSingleOldLight(strOldLightCode, strEPSCode);
            }
        }

        /// <summary>
        /// 新单灯替换
        /// </summary>       
        public static bool ReplaceSingleNewLight(string strNewLightCode, string strEPSCode)
        {
            lock (locker)
            {
                ClearEPSSendAndReceiveData();
                return SendDataReplaceSingleNewLight(strNewLightCode, strEPSCode);
            }
        }

        /// <summary>
        /// 单灯查询
        /// </summary>       
        public static bool QuerySingleLight(string strLightCode, string strEPSCode)
        {
            lock (locker)
            {
                ClearEPSSendAndReceiveData();
                return SendDataQuerySingleLight(strLightCode, strEPSCode);
            }
        }

        /// <summary>
        /// 单灯添加
        /// </summary>
        /// <param name="strLightCode"></param>
        /// <param name="strEPSCode"></param>
        public static void AddSingleLight(string strLightCode, string strEPSCode)
        {
            lock (locker)
            {
                ClearEPSSendAndReceiveData();
                SendDataAddSingleLight(strLightCode, strEPSCode);
            }
        }

        /// <summary>
        /// 单灯删除
        /// </summary>       
        public static bool DeleteSingleLight(string strLightCode, string strEPSCode)
        {
            lock (locker)
            {
                ClearEPSSendAndReceiveData();
                return SendDataDeleteSingleLight(strLightCode, strEPSCode);
            }
        }

        /// <summary>
        /// 进入EPS搜索
        /// </summary>
        public static void EnterSearchEPS()
        {
            lock (locker)
            {
                ClearEPSSendAndReceiveData();
                SendDataEnterSearchEPS();
            }
        }

        /// <summary>
        /// 退出EPS搜索
        /// </summary>
        /// <param name="isEnterSearchEPS"></param>
        public static void ExitSearchEPS()
        {
            lock (locker)
            {
                ClearEPSSendAndReceiveData();
                SendDataExitSearchEPS();
            }
        }

        /// <summary>
        /// 获取EPS信息
        /// </summary>
        /// <param name="strDisBoxCode"></param>
        public static Task<double[]> GetEPSInfo(string strDisBoxCode)
        {
            var task = Task.Run<double[]>(() =>
            {
                lock (locker)
                {
                    ClearEPSSendAndReceiveData();
                    return SendDataGetEPSInfo(strDisBoxCode);
                }
            });
            return task;
        }
        /// <summary>
        /// 送EPS灯码信息
        /// </summary>
        /// <param name="LstLightByEPS"></param>
        /// <param name="strEPSCode"></param>
        public static void TransAllLightCodeByEPS(List<LightInfo> LstLightByEPS, string strEPSCode)
        {
            lock (locker)
            {
                ClearEPSSendAndReceiveData();
                SendDataTransLightCountByEPS(LstLightByEPS, strEPSCode);
                SendDataTransAllLightCodeByEPS(LstLightByEPS, strEPSCode);
            }
        }

        /// <summary>
        /// 送灯左预案
        /// </summary>
        /// <param name="LstLightByEPS"></param>
        /// <param name="strEPSCode"></param>
        public static void TransLightLeftPlanByEPS(List<LightInfo> LstLightByEPS, string strEPSCode)
        {
            lock (locker)
            {
                ClearEPSSendAndReceiveData();
                SendDataTransLightLeftPlanByEPS(LstLightByEPS, strEPSCode);
            }
        }

        /// <summary>
        /// 送灯右预案
        /// </summary>
        /// <param name="LstLightByEPS"></param>
        /// <param name="strEPSCode"></param>
        public static void TransLightRightPlanByEPS(List<LightInfo> LstLightByEPS, string strEPSCode)
        {
            lock (locker)
            {
                ClearEPSSendAndReceiveData();
                SendDataTransLightRightPlanByEPS(LstLightByEPS, strEPSCode);
            }
        }

        /// <summary>
        /// 送灯状态
        /// </summary>
        /// <param name="LstLightByEPS"></param>
        /// <param name="strEPSCode"></param>
        public static void TransLightStateByEPS(List<LightInfo> LstLightByEPS, string strEPSCode)
        {
            lock (locker)
            {
                ClearEPSSendAndReceiveData();
                SendDataTransLightStateByEPS(LstLightByEPS, strEPSCode);
            }
        }

        /// <summary>
        /// 传送EPS预案
        /// </summary>
        /// <param name="infoDistributionBox"></param>
        public static void TransEPSPlan(DistributionBoxInfo infoDistributionBox,string strDisCode)
        {
            lock (locker)
            {
                ClearEPSSendAndReceiveData();
                SendDataTransEPSPlan(infoDistributionBox,strDisCode);
            }
        }

        /// <summary>
        /// 检查相应串口是否设置正常
        /// </summary>
        private static string CheckEPSSerialPort()
        {
            try
            {
                GetSerialPortIsOpen();
                EPSSendData[0] = 0xAA;
                EPSSendData[1] = 0x55;
                EPSSendData[2] = EPSSendData[3] = 0X00;
                EPSSendData[4] = 0x55;

                DisBoxSendData(EPSSendData, 0, 300);
            }
            catch
            {
                TimerDisBoxReceiveData.Enabled = false;
            }
            IsSerialPortFree = true;
            //跳过校验
            SerialPortType = "DisBoxPort";
            return SerialPortType;
        }

        /// <summary>
        /// 开启断通讯自动应急功能
        /// </summary>
        public static bool SendDataEmergencyOn()
        {
            try
            {
                GetSerialPortIsOpen();

                EPSSendData[0] = 0XAA;
                EPSSendData[1] = 0X1C;
                EPSSendData[2] = 0X24;
                EPSSendData[3] = EPSSendData[4] = EPSSendData[5] = EPSSendData[6] = 0X00;
                EPSSendData[7] = GetCheckSum();

                DisBoxSendData(EPSSendData, 0, CommonTimeInterval);

                if(EPSReceiveData[0] == 0X55 && EPSReceiveData[1] == 0X24 && EPSReceiveData[7] == 0X24)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                TimerDisBoxReceiveData.Enabled = false;
                return false;
            }
        }

        public static bool SendDataEmergencyOff()
        {
            try
            {
                GetSerialPortIsOpen();

                EPSSendData[0] = 0XAA;
                EPSSendData[1] = 0X1C;
                EPSSendData[2] = 0X25;
                EPSSendData[3] = EPSSendData[4] = EPSSendData[5] = EPSSendData[6] = 0X00;
                EPSSendData[7] = GetCheckSum();

                DisBoxSendData(EPSSendData, 0, CommonTimeInterval);

                if (EPSReceiveData[0] == 0X55 && EPSReceiveData[1] == 0X25 && EPSReceiveData[7] == 0X25)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                TimerDisBoxReceiveData.Enabled = false;
                return false;
            }
        }

        /// <summary>
        /// 发送全部灯执行预案指令
        /// </summary>
        /// <param name="planNumber"></param>
        /// <returns></returns>
        public static void SendDataExecutePlanByAllLight(int planNumber)
        {
            try
            {
                GetSerialPortIsOpen();

                EPSSendData[0] = 0xAA;
                EPSSendData[1] = 0x04;
                EPSSendData[2] = (byte)planNumber;
                EPSSendData[3] = EPSSendData[4] = EPSSendData[5]
                = EPSSendData[6] = EPSSendData[7] = 0x00;
                DisBoxSendData(EPSSendData, 0, CommonTimeInterval);
            }
            catch
            {
                TimerDisBoxReceiveData.Enabled = false;
                //CommonFunct.GetRemoteConnectRecord(ex.StackTrace);
            }
        }

        /// <summary>
        /// 发送设置灯具左预案指令
        /// </summary>
        /// <returns></returns>
        private static void SendDataSetLightLeftPlan(LightInfo infoLight)
        {
            try
            {
                GetSerialPortIsOpen();
                EPSSendData[0] = 0xAA;
                EPSSendData[1] = 0x86;
                EPSSendData[2] = (byte)(Convert.ToInt32(infoLight.Code) / Math.Pow(256, 2) % 256);
                EPSSendData[3] = (byte)(Convert.ToInt32(infoLight.Code) / 256 % 256);
                EPSSendData[4] = (byte)(Convert.ToInt32(infoLight.Code) % 256);
                EPSSendData[5] = (byte)infoLight.PlanLeft1;
                EPSSendData[6] = (byte)infoLight.PlanLeft2;
                EPSSendData[7] = (byte)infoLight.PlanLeft3;

                DisBoxSendData(EPSSendData, 0, CommonTimeInterval);
            }
            catch
            {
                TimerDisBoxReceiveData.Enabled = false;
            }
        }

        /// <summary>
        /// 发送设置灯具右预案指令
        /// </summary>
        /// <param name="infoLight"></param>
        private static void SendDataSetLightRightPlan(LightInfo infoLight)
        {
            try
            {
                GetSerialPortIsOpen();
                EPSSendData[0] = 0xAA;
                EPSSendData[1] = 0x87;
                EPSSendData[2] = (byte)(Convert.ToInt32(infoLight.Code) / Math.Pow(256, 2) % 256);
                EPSSendData[3] = (byte)(Convert.ToInt32(infoLight.Code) / 256 % 256);
                EPSSendData[4] = (byte)(Convert.ToInt32(infoLight.Code) % 256);
                EPSSendData[5] = (byte)infoLight.PlanRight1;
                EPSSendData[6] = (byte)infoLight.PlanRight2;
                EPSSendData[7] = (byte)infoLight.PlanRight3;

                DisBoxSendData(EPSSendData, 0, CommonTimeInterval);
            }
            catch
            {
                TimerDisBoxReceiveData.Enabled = false;
            }
        }

        /// <summary>
        /// 发送指定EPS应急或者主电指令
        /// </summary>
        /// <param name="strEPSCode"></param>
        private static void SendDataEmergencyOrMainEleByEPS(bool isEmergency, string strEPSCode)
        {
            try
            {
                GetSerialPortIsOpen();
                EPSSendData[0] = 0xAA;
                if (isEmergency)
                {
                    EPSSendData[1] = 0x0E;
                }
                else
                {
                    EPSSendData[1] = 0x0F;
                }
                EPSSendData[2] = EPSSendData[3] = EPSSendData[4] = 0;
                EPSSendData[5] = (byte)(Convert.ToInt32(strEPSCode) / Math.Pow(256, 2) % 256);
                EPSSendData[6] = (byte)(Convert.ToInt32(strEPSCode) / 256 % 256);
                EPSSendData[7] = (byte)(Convert.ToInt32(strEPSCode) % 256);

                DisBoxSendData(EPSSendData, 0, CommonTimeInterval);
            }
            catch
            {
                TimerDisBoxReceiveData.Enabled = false;
            }
        }

        /// <summary>
        /// 发送查询配电箱下所有灯状态指令
        /// </summary>
        /// <returns></returns>
        private static int[] SendDataQueryLightStatusByEPS(string strEPSCode)
        {
            try
            {
                GetSerialPortIsOpen();
                EPSSendData[0] = 0xAA;
                EPSSendData[1] = 0x83;
                EPSSendData[2] = EPSSendData[3] = EPSSendData[4] = 0;
                EPSSendData[5] = (byte)(Convert.ToInt32(strEPSCode) / Math.Pow(256, 2) % 256);
                EPSSendData[6] = (byte)(Convert.ToInt32(strEPSCode) / 256 % 256);
                EPSSendData[7] = (byte)(Convert.ToInt32(strEPSCode) % 256);

                DisBoxSendData(EPSSendData, 0, 10);// QueryLightStatusByEPSTimeInterval);

                if (EPSReceiveData[0] == 0x61 && EPSReceiveData[1] == 0x01)
                {
                    return GetQueryLightStatusByEPS();
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                TimerDisBoxReceiveData.Enabled = false;
                return null;
            }
        }

        /// <summary>
        /// 发送查询配电箱下所有灯指令
        /// </summary>
        /// <param name="strEPSCode"></param>
        /// <returns></returns>
        private static string[] SendDataQueryLightByEPS(string strEPSCode)
        {
            try
            {
                GetSerialPortIsOpen();
                EPSSendData[0] = 0xAA;
                EPSSendData[1] = 0x82;
                EPSSendData[2] = EPSSendData[3] = EPSSendData[4] = 0;
                EPSSendData[5] = (byte)(Convert.ToInt32(strEPSCode) / Math.Pow(256, 2) % 256);
                EPSSendData[6] = (byte)(Convert.ToInt32(strEPSCode) / 256 % 256);
                EPSSendData[7] = (byte)(Convert.ToInt32(strEPSCode) % 256);

                DisBoxSendData(EPSSendData, 0, QueryLightByEPSTimeInterval);

                if (EPSReceiveData[0] == 0x60 && EPSReceiveData[1] == 0x01)
                {
                    return GetQueryLightCodeByEPS();
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                TimerDisBoxReceiveData.Enabled = false;
                return null;
            }
        }

        private static string[] SendDataQueryLightByEPSFastly(string strEPSCode)
        {
            try
            {
                GetSerialPortIsOpen();
                EPSSendData[0] = 0xAA;
                EPSSendData[1] = 0x82;
                EPSSendData[2] = EPSSendData[3] = EPSSendData[4] = 0;
                EPSSendData[5] = (byte)(Convert.ToInt32(strEPSCode) / Math.Pow(256, 2) % 256);
                EPSSendData[6] = (byte)(Convert.ToInt32(strEPSCode) / 256 % 256);
                EPSSendData[7] = (byte)(Convert.ToInt32(strEPSCode) % 256);

                DisBoxSendData(EPSSendData, 0, QueryLightByEPSTimeInterval);

                for (int i = 0; i < LstEPSReceiveData.Count; i++)
                {
                    if (LstEPSReceiveData[i] == 0X60 && LstEPSReceiveData[i + 1] == 0X01)
                    {
                        break;
                    }
                    else
                    {
                        if (LstEPSReceiveData[i] != 0X60)
                        {
                            LstEPSReceiveData.RemoveAt(i);
                            i = 0;
                        }
                        else
                        {
                            if (LstEPSReceiveData[i + 1] != 0X01)
                            {
                                LstEPSReceiveData.RemoveAt(i);
                                i = 0;
                            }
                        }
                    }
                }

                if (LstEPSReceiveData.Count != 0)
                {
                    if (LstEPSReceiveData.Count != 0 && LstEPSReceiveData[0] == 0x60 && LstEPSReceiveData[1] == 0x01)
                    {
                        return GetQueryLightCodeByEPS();
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    SendAgainCount++;
                    if (SendAgainCount < 4)
                    {
                        return SendDataQueryLightByEPSFastly(strEPSCode);
                    }
                    else
                    {
                        SendAgainCount = 0;
                        return null;
                    }
                }
            }
            catch
            {
                TimerDisBoxReceiveData.Enabled = false;
                return null;
            }
        }

        private static void SendDataADDEPSLamp(string strEPSCode)
        {
            try
            {
                GetSerialPortIsOpen();
                EPSSendData[0] = 0XAA;
                EPSSendData[1] = 0X25;
                EPSSendData[2] = 0X00;
                EPSSendData[3] = 0X27;
                EPSSendData[4] = 0X10;
                EPSSendData[5] = (byte)(Convert.ToInt32(strEPSCode) / Math.Pow(256, 2) % 256);
                EPSSendData[6] = (byte)(Convert.ToInt32(strEPSCode) / 256 % 256);
                EPSSendData[7] = (byte)(Convert.ToInt32(strEPSCode) % 256);

                DisBoxSendData(EPSSendData, 0, 480000);
            }
            catch
            {
                TimerDisBoxReceiveData.Enabled = false;
            }
        }

        private static void SendDataQueryLightFastly()
        {
            try
            {
                GetSerialPortIsOpen();
                EPSSendData[0] = 0xAA;
                EPSSendData[1] = 0x23;
                EPSSendData[2] = EPSSendData[3] = EPSSendData[4] = 0;
                EPSSendData[5] = 0;
                EPSSendData[6] = 0X20;
                EPSSendData[7] = 0X80;

                DisBoxSendData(EPSSendData, 0, 480000);
            }
            catch
            {
                TimerDisBoxReceiveData.Enabled = false;
            }
        }

        /// <summary>
        /// 发送设定单灯指令
        /// </summary>
        /// <param name="strLightCode"></param>
        private static void SendDataSetSignalLight(int keyWord, string strLightCode, string strEPSCode)
        {
            try
            {
                GetSerialPortIsOpen();
                EPSSendData[0] = 0xAA;
                EPSSendData[1] = (byte)keyWord;
                EPSSendData[2] = (byte)(Convert.ToInt32(strLightCode) / Math.Pow(256, 2) % 256);
                EPSSendData[3] = (byte)(Convert.ToInt32(strLightCode) / 256 % 256);
                EPSSendData[4] = (byte)(Convert.ToInt32(strLightCode) % 256);
                EPSSendData[5] = (byte)(Convert.ToInt32(strEPSCode) / Math.Pow(256, 2) % 256);
                EPSSendData[6] = (byte)(Convert.ToInt32(strEPSCode) / 256 % 256);
                EPSSendData[7] = (byte)(Convert.ToInt32(strEPSCode) % 256);

                DisBoxSendData(EPSSendData, 0, CommonTimeInterval);
            }
            catch
            {
                TimerDisBoxReceiveData.Enabled = false;
            }
        }

        #region 批量发送单灯控制指令
        public struct SingleLightCommandStructure
        {
            public byte CommandType { get; set; }
            public int LightCode { get; set; }
        }

        // TODO 批量发送单灯控制指令
        private static void SendBatchDataSetSingleLight(
            Dictionary<uint/*eps code*/, Queue<SingleLightCommandStructure>> commands)
        {
            lock (locker)
            {
                //Task.Run(async () =>
                //{
                int remainingCommandCount = commands.Count;
                while (remainingCommandCount > 0)
                {
                    foreach (var command in commands)
                    {
                        if (command.Value.Count == 0)
                        {
                            remainingCommandCount--;
                            continue;
                        }
                        SingleLightCommandStructure lightCommand = commands[command.Key].Dequeue();
                        EPSSendData[0] = 0xAA;
                        EPSSendData[1] = lightCommand.CommandType;
                        EPSSendData[2] = (byte)(lightCommand.LightCode >> 16);
                        EPSSendData[3] = (byte)(lightCommand.LightCode >> 8 & 0xFF);
                        EPSSendData[4] = (byte)(lightCommand.LightCode & 0xFF);
                        EPSSendData[5] = (byte)(command.Key >> 16);
                        EPSSendData[6] = (byte)(command.Key >> 8 & 0xFF);
                        EPSSendData[7] = (byte)(command.Key & 0xFF);
                        //var msg = string.Join(" ", EPSSendData);
                        //LoggerManager.WriteDebug(msg);
                        DisBoxSendData(EPSSendData, EPSSendData.Length, 150);
                    }
                    //await Task.Delay(300);
                    Thread.Sleep(50);
                    System.Windows.Forms.Application.DoEvents();
                }

                //}).ConfigureAwait(false);
            }

        }

        private static uint GetDisBoxCode(int disboxID)
        {
            var codes = ObjDistributionBox.GetAll();
            var codeStr = codes.Find(it => it.ID == disboxID)?.Code;
            uint.TryParse(codeStr, out uint code);
            return code;
        }

        private static void MergeList(
            List<LightInfo> lights, byte commandType,
            ref Dictionary<uint, Queue<SingleLightCommandStructure>> commands)
        {
            try
            {
                foreach (LightInfo light in lights)
                {
                    uint epsCode = (uint)light.DisBoxID;
                    int.TryParse(light.Code, out int lightCode);
                    if (!commands.ContainsKey(epsCode))
                    {
                        commands[epsCode] = new Queue<SingleLightCommandStructure>();
                    }
                    commands[epsCode].Enqueue(new SingleLightCommandStructure
                    {
                        CommandType = commandType,
                        LightCode = lightCode
                    });
                }
            }
            catch (Exception ex)
            {
                LoggerManager.WriteDebug(ex.ToString());
            }

        }

        private static Dictionary<uint, Queue<SingleLightCommandStructure>> MergeLightCommand(
            List<LightInfo> twoBrightLights,
            List<LightInfo> leftBrightLights,
            List<LightInfo> rightBrightLights)
        {
            Dictionary<uint, Queue<SingleLightCommandStructure>> commands
                = new Dictionary<uint, Queue<SingleLightCommandStructure>>();
            MergeList(twoBrightLights, (byte)SingleLightControlClass.全亮, ref commands);
            MergeList(leftBrightLights, (byte)SingleLightControlClass.左亮, ref commands);
            MergeList(rightBrightLights, (byte)SingleLightControlClass.右亮, ref commands);
            return commands;

            //MergeList(twoBrightLights, (byte)SingleLightControlClass.右亮, ref commands);
            //MergeList(leftBrightLights, (byte)SingleLightControlClass.右亮, ref commands);
            //MergeList(rightBrightLights, (byte)SingleLightControlClass.右亮, ref commands);
            //return commands;
        }

        public static void SendBatchDatSetSingleLight(
            List<LightInfo> twoBrightLights,
            List<LightInfo> leftBrightLights,
            List<LightInfo> rightBrightLights)
        {
            var commands = MergeLightCommand(twoBrightLights, leftBrightLights, rightBrightLights);
            SendBatchDataSetSingleLight(commands);
        }
        #endregion

        /// <summary>
        /// 发送设定灯具初始状态指令
        /// </summary>
        /// <param name="strLightCode"></param>
        /// <param name="lightStatus"></param>
        private static void SendDataSetLightBeginStatus(string strLightCode, int lightStatus, string strEPSCode)
        {
            try
            {
                GetSerialPortIsOpen();
                EPSSendData[0] = 0xAA;
                EPSSendData[1] = 0x85;
                EPSSendData[2] = (byte)(Convert.ToInt32(strLightCode) / Math.Pow(256, 2) % 256);
                EPSSendData[3] = (byte)(Convert.ToInt32(strLightCode) / 256 % 256);
                EPSSendData[4] = (byte)(Convert.ToInt32(strLightCode) % 256);
                EPSSendData[5] = (byte)lightStatus;
                EPSSendData[6] = (byte)(Convert.ToInt32(strEPSCode) / 256 % 256);
                EPSSendData[7] = (byte)(Convert.ToInt32(strEPSCode) % 256);

                DisBoxSendData(EPSSendData, 0, CommonTimeInterval);
            }
            catch
            {
                TimerDisBoxReceiveData.Enabled = false;
            }
        }

        /// <summary>
        /// 发送修改配电箱预案
        /// </summary>
        private static void SendDataUpdateEPSPlan(DistributionBoxInfo infoDistributionBox)
        {
            try
            {
                GetSerialPortIsOpen();
                EPSSendData[0] = 0xA6;
                EPSSendData[1] = (byte)infoDistributionBox.Plan1;
                EPSSendData[2] = (byte)infoDistributionBox.Plan2;
                EPSSendData[3] = (byte)infoDistributionBox.Plan3;
                EPSSendData[4] = (byte)infoDistributionBox.Plan4;
                EPSSendData[5] = (byte)infoDistributionBox.Plan5;
                EPSSendData[6] = (byte)(Convert.ToInt32(infoDistributionBox.Code) / 256 % 256);
                EPSSendData[7] = (byte)(Convert.ToInt32(infoDistributionBox.Code) % 256);

                DisBoxSendData(EPSSendData, 0, CommonTimeInterval);
            }
            catch
            {
                TimerDisBoxReceiveData.Enabled = false;
            }
        }

        /// <summary>
        /// 发送主机数据下传指令
        /// </summary>
        private static void SendHostData(LoadProgressBar progressBar)
        {
            try
            {
                if (LstDistributionBox != null)
                {
                    GetSerialPortIsOpen();
                    byte[] HostUploadData = new byte[8];
                    for (int i = 0; i < LstDistributionBox.Count; i++)
                    {
                        List<LightInfo> CurrentDisbox = LstLight.FindAll(x => x.DisBoxID == Convert.ToInt32(LstDistributionBox[i].Code));
                        int AllLight_num = CurrentDisbox == null ? 0 : CurrentDisbox.Count;
                        HostUploadData[0] = 0XAB;
                        HostUploadData[1] = 0X00;
                        HostUploadData[2] = (byte)(AllLight_num / 256);
                        HostUploadData[3] = (byte)(AllLight_num % 256);
                        HostUploadData[4] = 0X00;
                        HostUploadData[5] = (byte)(Convert.ToInt32(LstDistributionBox[i].Code) >> 16);
                        HostUploadData[6] = (byte)(Convert.ToInt32(LstDistributionBox[i].Code) >> 8);
                        HostUploadData[7] = (byte)(Convert.ToInt32(LstDistributionBox[i].Code) & 0x00ff);
                        DisBoxSendData(HostUploadData, 8, CommonTimeInterval);
                        for(int j = 0;j < AllLight_num; j++)
                        {
                            HostUploadData[0] = 0XAB;
                            HostUploadData[1] = (byte)j;
                            HostUploadData[2] = (byte)(Convert.ToInt32(CurrentDisbox[j].Code) >> 16);
                            HostUploadData[3] = (byte)(Convert.ToInt32(CurrentDisbox[j].Code) >> 8);
                            HostUploadData[4] = (byte)(Convert.ToInt32(CurrentDisbox[j].Code) & 0X00ff);
                            HostUploadData[5] = (byte)(Convert.ToInt32(LstDistributionBox[i].Code) >> 16);
                            HostUploadData[6] = (byte)(Convert.ToInt32(LstDistributionBox[i].Code) >> 8);
                            HostUploadData[7] = (byte)(Convert.ToInt32(LstDistributionBox[i].Code) & 0x00ff);
                            Thread.Sleep(51);
                            progressBar.ImportedFilesCount++;
                            DisBoxSendData(HostUploadData, 8, CommonTimeInterval);
                        }
                    }
                    CommonFunct.PopupWindow("发送完成");
                }
            }
            catch
            {
                TimerDisBoxReceiveData.Enabled = false;
            }
            
        }

        /// <summary>
        /// 发送所有配电箱寻灯码指令(每个范围搜10000个灯)
        /// </summary>
        /// <param name="strLightCode"></param>
        /// <returns></returns>
        private static void SendDataFindLightByAllEPS()
        {
            try
            {
                GetSerialPortIsOpen();
                EPSSendData[0] = 0xAA;
                EPSSendData[1] = 0x03;
                EPSSendData[2] = EPSSendData[3] = EPSSendData[4] = 0x00;
                EPSSendData[5] = 0x00;
                EPSSendData[6] = 0x27;
                EPSSendData[7] = 0x10;
                DisBoxSendData(EPSSendData, 0, CommonTimeInterval);
            }
            catch
            {
                TimerDisBoxReceiveData.Enabled = false;
            }
        }

        /// <summary>
        /// 发送所有配电箱快速寻灯码指令
        /// </summary>
        private static string[] SendDataFindLightByAllEPSFastly()
        {
            try
            {
                GetSerialPortIsOpen();
                EPSSendData[0] = 0xAA;
                EPSSendData[1] = 0x23;
                EPSSendData[2] = EPSSendData[3] = EPSSendData[4] = EPSSendData[5] = 0x00;
                EPSSendData[6] = 0x1F;
                EPSSendData[7] = 0xB7;

                DisBoxSendData(EPSSendData, 0, TimerDisBoxReceiveDataFastlyInterval);

                if (EPSReceiveData[0] == 0x67 && EPSReceiveData[1] == 0x8D)
                {
                    return GetEPSCodeFastly();
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                TimerDisBoxReceiveData.Enabled = false;
                return null;
            }
        }

        /// <summary>
        /// 发送寻配电箱指令
        /// </summary>
        /// <param name="strDisBoxCode"></param>
        /// <returns></returns>
        private static bool SendDataFindEPS(string strDisBoxCode)
        {
            try
            {
                GetSerialPortIsOpen();
                //例如600001 即为0927C1 => EPSSendData[5]=09;EPSSendData[6]=27;EPSSendData[7]=C1
                EPSSendData[0] = 0xAA;
                EPSSendData[1] = 0x81;
                EPSSendData[2] = EPSSendData[3] = EPSSendData[4] = 0;
                EPSSendData[5] = (byte)(Convert.ToInt32(strDisBoxCode) / Math.Pow(256, 2) % 256);
                EPSSendData[6] = (byte)(Convert.ToInt32(strDisBoxCode) / 256 % 256);
                EPSSendData[7] = (byte)(Convert.ToInt32(strDisBoxCode) % 256);

                DisBoxSendData(EPSSendData, 0, CommonTimeInterval);

                if (EPSReceiveData[0] == 0x55 && EPSReceiveData[1] == 0x81)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                TimerDisBoxReceiveData.Enabled = false;
                return false;
            }
        }

        /// <summary>
        /// 发送快速寻配电箱指令
        /// </summary>
        /// <param name="strDisBoxCode"></param>
        /// <returns></returns>
        private static string[] SendDataFindEPSFastly()
        {
            try
            {
                GetSerialPortIsOpen();
                EPSSendData[0] = 0xAA;
                EPSSendData[1] = 0x90;
                EPSSendData[2] = EPSSendData[3] = EPSSendData[4] = EPSSendData[5] = EPSSendData[6] = EPSSendData[7] = 0;

                DisBoxSendData(EPSSendData, 0, 2000);

                if (EPSReceiveData[0] == 0x68 && EPSReceiveData[1] == 0x8D)
                {
                    return GetEPSCodeFastly();
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                TimerDisBoxReceiveData.Enabled = false;
                return null;
            }
        }

        /// <summary>
        /// 发送心跳指令
        /// </summary>
        private static void SendDataHeartbeat()
        {
            try
            {
                GetSerialPortIsOpen();
                EPSSendData[0] = 0xAA;
                EPSSendData[1] = EPSSendData[2] = EPSSendData[3] = EPSSendData[4] = EPSSendData[5]
                = EPSSendData[6] = EPSSendData[7] = 0x00;

                DisBoxSendData(EPSSendData, 0, 100);
            }
            catch
            {
                TimerDisBoxReceiveData.Enabled = false;
            }
        }

        /// <summary>
        /// 发送全体应急指令
        /// </summary>
        private static void SendDataAllEmergency()
        {
            try
            {
                GetSerialPortIsOpen();
                EPSSendData[0] = 0xAA;
                EPSSendData[1] = 0x01;
                EPSSendData[2] = EPSSendData[3] = EPSSendData[4] = EPSSendData[5]
                = EPSSendData[6] = EPSSendData[7] = 0x00;

                DisBoxSendData(EPSSendData, 0, 100);
            }
            catch
            {
                TimerDisBoxReceiveData.Enabled = false;
            }
        }

        /// <summary>
        /// 发送全体主电指令
        /// </summary>
        private static void SendDataAllMainEle()
        {
            try
            {
                GetSerialPortIsOpen();
                EPSSendData[0] = 0xAA;
                EPSSendData[1] = 0x02;
                EPSSendData[2] = EPSSendData[3] = EPSSendData[4] = EPSSendData[5]
                = EPSSendData[6] = EPSSendData[7] = 0x00;

                DisBoxSendData(EPSSendData, 0, 100);
            }
            catch
            {
                TimerDisBoxReceiveData.Enabled = false;
            }
        }

        /// <summary>
        /// 发送EPS下灯数量指令
        /// </summary>
        /// <param name="LstLightByEPS"></param>
        /// <param name="strEPSCode"></param>
        private static void SendDataTransLightCountByEPS(List<LightInfo> LstLightByEPS, string strEPSCode)
        {
            EPSSendData[0] = 0xAB;
            EPSSendData[1] = EPSSendData[2] = 0x00;
            EPSSendData[3] = (byte)LstLightByEPS.Count;
            EPSSendData[4] = 0x00;
            EPSSendData[5] = (byte)(Convert.ToInt32(strEPSCode) / Math.Pow(256, 2) % 256);
            EPSSendData[6] = (byte)(Convert.ToInt32(strEPSCode) / 256 % 256);
            EPSSendData[7] = (byte)(Convert.ToInt32(strEPSCode) % 256);
        }

        /// <summary>
        /// 发送送EPS灯信息指令
        /// </summary>
        private static void SendDataTransAllLightCodeByEPS(List<LightInfo> LstLightByEPS, string strEPSCode)
        {
            try
            {
                GetSerialPortIsOpen();
                foreach (LightInfo infoLight in LstLightByEPS)
                {
                    EPSSendData[EPSPerParaDataLength * (infoLight.LightIndex + 1)] = 0xAB;
                    EPSSendData[EPSPerParaDataLength * (infoLight.LightIndex + 1) + 1] = (byte)(infoLight.LightIndex + 1);
                    EPSSendData[EPSPerParaDataLength * (infoLight.LightIndex + 1) + 2] = (byte)(Convert.ToInt32(infoLight.Code)
                    / Math.Pow(256, 2) % 256);
                    EPSSendData[EPSPerParaDataLength * (infoLight.LightIndex + 1) + 3] = (byte)(Convert.ToInt32(infoLight.Code)
                    / 256 % 256);
                    EPSSendData[EPSPerParaDataLength * (infoLight.LightIndex + 1) + 4] = (byte)(Convert.ToInt32(infoLight.Code)
                    % 256);
                    EPSSendData[EPSPerParaDataLength * (infoLight.LightIndex + 1) + 5] = (byte)(Convert.ToInt32(strEPSCode)
                    / Math.Pow(256, 2) % 256);
                    EPSSendData[EPSPerParaDataLength * (infoLight.LightIndex + 1) + 6] = (byte)(Convert.ToInt32(strEPSCode)
                    / 256 % 256);
                    EPSSendData[EPSPerParaDataLength * (infoLight.LightIndex + 1) + 7] = (byte)(Convert.ToInt32(strEPSCode)
                    % 256);
                }
                EPSSendDataLength = EPSPerParaDataLength * (LstLightByEPS.Count + 1);
                DisBoxSendData(EPSSendData, EPSSendDataLength, QueryLightByEPSTimeInterval);
            }
            catch
            {
                TimerDisBoxReceiveData.Enabled = false;
            }
        }

        /// <summary>
        /// 发送送灯左预案指令
        /// </summary>
        /// <param name="LstLightByEPS"></param>
        /// <param name="strEPSCode"></param>
        private static void SendDataTransLightLeftPlanByEPS(List<LightInfo> LstLightByEPS, string strEPSCode)
        {
            try
            {
                GetSerialPortIsOpen();
                foreach (LightInfo infoLight in LstLightByEPS)
                {
                    EPSSendData[EPSPerParaDataLength * infoLight.LightIndex] = 0xA0;
                    EPSSendData[EPSPerParaDataLength * infoLight.LightIndex + 1] = (byte)(infoLight.LightIndex + 1);
                    EPSSendData[EPSPerParaDataLength * infoLight.LightIndex + 2] = (byte)infoLight.PlanLeft1;
                    EPSSendData[EPSPerParaDataLength * infoLight.LightIndex + 3] = (byte)infoLight.PlanLeft2;
                    EPSSendData[EPSPerParaDataLength * infoLight.LightIndex + 4] = (byte)infoLight.PlanLeft3;
                    EPSSendData[EPSPerParaDataLength * infoLight.LightIndex + 5] = (byte)(Convert.ToInt32(strEPSCode)
                    / Math.Pow(256, 2) % 256);
                    EPSSendData[EPSPerParaDataLength * infoLight.LightIndex + 6] = (byte)(Convert.ToInt32(strEPSCode)
                    / 256 % 256);
                    EPSSendData[EPSPerParaDataLength * infoLight.LightIndex + 7] = (byte)(Convert.ToInt32(strEPSCode)
                    % 256);
                }
                EPSSendDataLength = EPSPerParaDataLength * (LstLightByEPS.Count + 1);
                DisBoxSendData(EPSSendData, EPSSendDataLength, QueryLightByEPSTimeInterval);

            }
            catch
            {
                TimerDisBoxReceiveData.Enabled = false;
            }
        }

        /// <summary>
        /// 发送送灯右预案指令
        /// </summary>
        /// <param name="LstLightByEPS"></param>
        /// <param name="strEPSCode"></param>
        private static void SendDataTransLightRightPlanByEPS(List<LightInfo> LstLightByEPS, string strEPSCode)
        {
            try
            {
                GetSerialPortIsOpen();
                foreach (LightInfo infoLight in LstLightByEPS)
                {
                    EPSSendData[EPSPerParaDataLength * infoLight.LightIndex] = 0xA2;
                    EPSSendData[EPSPerParaDataLength * infoLight.LightIndex + 1] = (byte)(infoLight.LightIndex + 1);
                    EPSSendData[EPSPerParaDataLength * infoLight.LightIndex + 2] = (byte)infoLight.PlanRight1;
                    EPSSendData[EPSPerParaDataLength * infoLight.LightIndex + 3] = (byte)infoLight.PlanRight2;
                    EPSSendData[EPSPerParaDataLength * infoLight.LightIndex + 4] = (byte)infoLight.PlanRight3;
                    EPSSendData[EPSPerParaDataLength * infoLight.LightIndex + 5] = (byte)(Convert.ToInt32(strEPSCode)
                    / Math.Pow(256, 2) % 256);
                    EPSSendData[EPSPerParaDataLength * infoLight.LightIndex + 6] = (byte)(Convert.ToInt32(strEPSCode)
                    / 256 % 256);
                    EPSSendData[EPSPerParaDataLength * infoLight.LightIndex + 7] = (byte)(Convert.ToInt32(strEPSCode)
                    % 256);
                }
                EPSSendDataLength = EPSPerParaDataLength * (LstLightByEPS.Count + 1);
                DisBoxSendData(EPSSendData, EPSSendDataLength, QueryLightByEPSTimeInterval);
            }
            catch
            {
                TimerDisBoxReceiveData.Enabled = false;
            }
        }

        /// <summary>
        /// 发送送灯状态指令
        /// </summary>
        /// <param name="LstLightByEPS"></param>
        /// <param name="strEPSCode"></param>
        private static void SendDataTransLightStateByEPS(List<LightInfo> LstLightByEPS, string strEPSCode)
        {
            try
            {
                GetSerialPortIsOpen();
                foreach (LightInfo infoLight in LstLightByEPS)
                {
                    EPSSendData[EPSPerParaDataLength * infoLight.LightIndex] = 0xA4;
                    EPSSendData[EPSPerParaDataLength * infoLight.LightIndex + 1] = (byte)(infoLight.LightIndex + 1);
                    EPSSendData[EPSPerParaDataLength * infoLight.LightIndex + 2] = (byte)infoLight.BeginStatus;
                    EPSSendData[EPSPerParaDataLength * infoLight.LightIndex + 3] =
                    EPSSendData[EPSPerParaDataLength * infoLight.LightIndex + 4] = 0;
                    EPSSendData[EPSPerParaDataLength * infoLight.LightIndex + 5] = (byte)(Convert.ToInt32(strEPSCode)
                    / Math.Pow(256, 2) % 256);
                    EPSSendData[EPSPerParaDataLength * infoLight.LightIndex + 6] = (byte)(Convert.ToInt32(strEPSCode)
                    / 256 % 256);
                    EPSSendData[EPSPerParaDataLength * infoLight.LightIndex + 7] = (byte)(Convert.ToInt32(strEPSCode)
                    % 256);
                }
                EPSSendDataLength = EPSPerParaDataLength * (LstLightByEPS.Count + 1);
                DisBoxSendData(EPSSendData, EPSSendDataLength, QueryLightByEPSTimeInterval);

            }
            catch
            {
                TimerDisBoxReceiveData.Enabled = false;
            }
        }

        /// <summary>
        /// 发送传送EPS预案
        /// </summary>
        private static void SendDataTransEPSPlan(DistributionBoxInfo infoDistributionBox,string strDisCode)
        {
            try
            {
                GetSerialPortIsOpen();
                EPSSendData[0] = 0xA6;
                EPSSendData[1] = (byte)infoDistributionBox.Plan1;
                EPSSendData[2] = (byte)infoDistributionBox.Plan2;
                EPSSendData[3] = (byte)infoDistributionBox.Plan3;
                EPSSendData[4] = (byte)infoDistributionBox.Plan4;
                EPSSendData[5] = (byte)infoDistributionBox.Plan5;
                EPSSendData[6] = (byte)(Convert.ToInt32(strDisCode) / 256 % 256);
                EPSSendData[7] = (byte)(Convert.ToInt32(strDisCode) % 256);

                DisBoxSendData(EPSSendData, 0, QueryLightByEPSTimeInterval);
            }
            catch
            {
                TimerDisBoxReceiveData.Enabled = false;
            }
        }

        /// <summary>
        /// 发送旧单灯替换指令
        /// </summary>
        /// <param name="strOldLightCode"></param>
        /// <param name="strDisBoxCode"></param>
        /// <returns></returns>
        private static bool SendDataReplaceSingleOldLight(string strOldLightCode, string strDisBoxCode)
        {
            try
            {
                GetSerialPortIsOpen();
                EPSSendData[0] = 0xAA;
                EPSSendData[1] = 0x8B;
                EPSSendData[2] = (byte)(Convert.ToInt32(strOldLightCode) / Math.Pow(256, 2) % 256);
                EPSSendData[3] = (byte)(Convert.ToInt32(strOldLightCode) / 256 % 256);
                EPSSendData[4] = (byte)(Convert.ToInt32(strOldLightCode) % 256);
                EPSSendData[5] = (byte)(Convert.ToInt32(strDisBoxCode) / Math.Pow(256, 2) % 256);
                EPSSendData[6] = (byte)(Convert.ToInt32(strDisBoxCode) / 256 % 256);
                EPSSendData[7] = (byte)(Convert.ToInt32(strDisBoxCode) % 256);

                DisBoxSendData(EPSSendData, 0, AddOrReplaceSignalLightTimeInterval);

                if (EPSReceiveData[0] == 0X66 && EPSReceiveData[1] == 0X8B && EPSReceiveData[7] == 0x11)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                TimerDisBoxReceiveData.Enabled = false;
                return false;
            }
        }

        /// <summary>
        /// 发送新单灯替换指令
        /// </summary>
        /// <param name="strNewLightCode"></param>
        /// <param name="strDisBoxCode"></param>
        /// <returns></returns>
        private static bool SendDataReplaceSingleNewLight(string strNewLightCode, string strDisBoxCode)
        {
            try
            {
                GetSerialPortIsOpen();
                EPSSendData[0] = 0xAA;
                EPSSendData[1] = 0x8C;
                EPSSendData[2] = (byte)(Convert.ToInt32(strNewLightCode) / Math.Pow(256, 2) % 256);
                EPSSendData[3] = (byte)(Convert.ToInt32(strNewLightCode) / 256 % 256);
                EPSSendData[4] = (byte)(Convert.ToInt32(strNewLightCode) % 256);
                EPSSendData[5] = (byte)(Convert.ToInt32(strDisBoxCode) / Math.Pow(256, 2) % 256);
                EPSSendData[6] = (byte)(Convert.ToInt32(strDisBoxCode) / 256 % 256);
                EPSSendData[7] = (byte)(Convert.ToInt32(strDisBoxCode) % 256);

                DisBoxSendData(EPSSendData, 0, AddOrReplaceSignalLightTimeInterval);

                if (EPSReceiveData[0] == 0x66 && EPSReceiveData[1] == 0x8C && EPSReceiveData[7] == 0x22)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                TimerDisBoxReceiveData.Enabled = false;
                return false;
            }
        }

        /// <summary>
        /// 发送单灯查询指令
        /// </summary>
        private static bool SendDataQuerySingleLight(string strLightCode, string strDisBoxCode)
        {
            try
            {
                GetSerialPortIsOpen();
                EPSSendData[0] = 0xAA;
                EPSSendData[1] = 0x89;
                EPSSendData[2] = (byte)(Convert.ToInt32(strLightCode) / Math.Pow(256, 2) % 256);
                EPSSendData[3] = (byte)(Convert.ToInt32(strLightCode) / 256 % 256);
                EPSSendData[4] = (byte)(Convert.ToInt32(strLightCode) % 256);
                EPSSendData[5] = (byte)(Convert.ToInt32(strDisBoxCode) / Math.Pow(256, 2) % 256);
                EPSSendData[6] = (byte)(Convert.ToInt32(strDisBoxCode) / 256 % 256);
                EPSSendData[7] = (byte)(Convert.ToInt32(strDisBoxCode) % 256);

                DisBoxSendData(EPSSendData, 0, AddOrReplaceSignalLightTimeInterval);

                if (EPSReceiveData[0] == 0x64 && EPSReceiveData[1] == 0x89 && EPSReceiveData[7] == 0x11)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                TimerDisBoxReceiveData.Enabled = false;
                return false;
            }
        }

        /// <summary>
        /// 发送单灯添加指令
        /// </summary>
        /// <param name="strLightCode"></param>
        /// <param name="strDisBoxCode"></param>
        private static void SendDataAddSingleLight(string strLightCode, string strDisBoxCode)
        {
            try
            {
                GetSerialPortIsOpen();
                EPSSendData[0] = 0xAA;
                EPSSendData[1] = 0X0D;
                EPSSendData[2] = (byte)(Convert.ToInt32(strLightCode) / Math.Pow(256, 2) % 256);
                EPSSendData[3] = (byte)(Convert.ToInt32(strLightCode) / 256 % 256);
                EPSSendData[4] = (byte)(Convert.ToInt32(strLightCode) % 256);
                EPSSendData[5] = (byte)(Convert.ToInt32(strDisBoxCode) / Math.Pow(256, 2) % 256);
                EPSSendData[6] = (byte)(Convert.ToInt32(strDisBoxCode) / 256 % 256);
                EPSSendData[7] = (byte)(Convert.ToInt32(strDisBoxCode) % 256);

                DisBoxSendData(EPSSendData, 0, AddOrReplaceSignalLightTimeInterval);
            }
            catch
            {
                TimerDisBoxReceiveData.Enabled = false;
            }
        }

        /// <summary>
        /// 发送单灯删除指令
        /// </summary>
        private static bool SendDataDeleteSingleLight(string strLightCode, string strEPSCode)
        {
            try
            {
                GetSerialPortIsOpen();
                EPSSendData[0] = 0xAA;
                EPSSendData[1] = 0X10; //0x89;
                EPSSendData[2] = (byte)(Convert.ToInt32(strLightCode) / Math.Pow(256, 2) % 256);
                EPSSendData[3] = (byte)(Convert.ToInt32(strLightCode) / 256 % 256);
                EPSSendData[4] = (byte)(Convert.ToInt32(strLightCode) % 256);
                EPSSendData[5] = (byte)(Convert.ToInt32(strEPSCode) / Math.Pow(256, 2) % 256);
                EPSSendData[6] = (byte)(Convert.ToInt32(strEPSCode) / 256 % 256);
                EPSSendData[7] = (byte)(Convert.ToInt32(strEPSCode) % 256);

                DisBoxSendData(EPSSendData, 0, AddOrReplaceSignalLightTimeInterval);

                if (EPSReceiveData[0] == 0x64 && EPSReceiveData[1] == 0x89 && EPSReceiveData[7] == 0x11)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                TimerDisBoxReceiveData.Enabled = false;
                return false;
            }
        }

        /// <summary>
        /// 发送进入EPS搜索指令
        /// </summary>
        private static void SendDataEnterSearchEPS()
        {
            try
            {
                GetSerialPortIsOpen();
                EPSSendData[0] = 0xAA;
                EPSSendData[1] = 0x20;
                EPSSendData[2] = EPSSendData[3] = EPSSendData[3] = EPSSendData[4]
                = EPSSendData[5] = EPSSendData[6] = 0x00;

                DisBoxSendData(EPSSendData, 0, CommonTimeInterval);
            }
            catch
            {
                TimerDisBoxReceiveData.Enabled = false;
            }
        }

        /// <summary>
        /// 发送退出EPS搜索指令
        /// </summary>
        private static void SendDataExitSearchEPS()
        {
            try
            {
                GetSerialPortIsOpen();
                EPSSendData[0] = 0xAA;
                EPSSendData[1] = 0x21;
                EPSSendData[2] = EPSSendData[3] = EPSSendData[4] = EPSSendData[5]
                = EPSSendData[6] = EPSSendData[7] = 0x00;

                DisBoxSendData(EPSSendData, 0, CommonTimeInterval);
            }
            catch
            {
                TimerDisBoxReceiveData.Enabled = false;
            }
        }

        /// <summary>
        /// 发送获取配电箱信息指令
        /// </summary>
        /// <returns></returns>
        private static double[] SendDataGetEPSInfo(string strEPSCode)
        {
            try
            {
                GetSerialPortIsOpen();
                EPSSendData[0] = 0xAA;
                EPSSendData[1] = 0x84;
                EPSSendData[2] = EPSSendData[3] = EPSSendData[4] = 0x00;
                EPSSendData[5] = (byte)(Convert.ToInt32(strEPSCode) / Math.Pow(256, 2) % 256);
                EPSSendData[6] = (byte)(Convert.ToInt32(strEPSCode) / 256 % 256);
                EPSSendData[7] = (byte)(Convert.ToInt32(strEPSCode) % 256);

                DisBoxSendData(EPSSendData, 0, 1000);

                if (EPSReceiveData[0] == 0x62 && EPSReceiveData[1] == 0x01)
                {
                    double[] EPSInfo = new double[5];
                    EPSInfo[0] = Convert.ToDouble(EPSReceiveData[2] * 256 + EPSReceiveData[3]) / EPSInfoPercent;
                    EPSInfo[1] = Convert.ToDouble(EPSReceiveData[4] * 256 + EPSReceiveData[5]) / EPSInfoPercent;
                    EPSInfo[2] = Convert.ToDouble(EPSReceiveData[6] * 256 + EPSReceiveData[7]) / EPSInfoPercent;
                    EPSInfo[3] = Convert.ToDouble(EPSReceiveData[10] * 256 + EPSReceiveData[11]) / EPSInfoPercent;
                    EPSInfo[4] = Convert.ToDouble(EPSReceiveData[12] * 256 + EPSReceiveData[13]);
                    return EPSInfo;
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                TimerDisBoxReceiveData.Enabled = false;
            }
            return null;
        }

        /// <summary>
        /// 获取配电箱下所有灯码
        /// </summary>
        /// <returns></returns>
        private static string[] GetQueryLightCodeByEPS()
        {
            string[] LightCode = new string[LstEPSReceiveData[2] * 256 + LstEPSReceiveData[3]];
            for (int i = 0; i < LightCode.Length; i++)
            {
                //LightCodeFrontDataLength=16  HeadDataLength=2  EPSPerParaDataLength=8  LightCodeDataLength=3
                if(LightCodeFrontDataLength + HeadDataLength + (i / 2) * EPSPerParaDataLength + (i % 2) * LightCodeDataLength + 2 < LstEPSReceiveData.Count)
                {
                    LightCode[i] = Convert.ToString((LstEPSReceiveData[LightCodeFrontDataLength + HeadDataLength + (i / 2)
                    * EPSPerParaDataLength + (i % 2) * LightCodeDataLength] * 256 + LstEPSReceiveData[LightCodeFrontDataLength
                    + HeadDataLength + (i / 2) * EPSPerParaDataLength + (i % 2) * LightCodeDataLength + 1]) * 256
                    + LstEPSReceiveData[LightCodeFrontDataLength + HeadDataLength + (i / 2) * EPSPerParaDataLength + (i % 2)
                    * LightCodeDataLength + 2]);
                }
                else
                {
                    break;
                }
            }
            return LightCode;
        }
        /// <summary>
        /// 获取配电箱码
        /// </summary>
        /// <returns></returns>
        private static string[] GetEPSCodeFastly()
        {
            string[] epscode = new string[EPSReceiveDataLength / 8];
            for (int i = 0; i < epscode.Length; i++)
            {
                epscode[i] = Convert.ToString((EPSReceiveData[EPSCodeFrontDataLength + 8 * i] * 256 + EPSReceiveData[EPSCodeFrontDataLength + 8 * i + 1]) * 256 + EPSReceiveData[EPSCodeFrontDataLength + 8 * i + 2]);
            }
            return epscode;
        }

        /// <summary>
        /// 获取配电箱下所有灯状态
        /// </summary>
        /// <returns></returns>
        private static int[] GetQueryLightStatusByEPS()
        {
            int[] LightStatusByEPS = new int[EPSReceiveDataLength - HeadDataLength * (EPSReceiveDataLength / EPSPerParaDataLength)];
            for (int i = 0; i < LightStatusByEPS.Length; i++)
            {
                LightStatusByEPS[i] = EPSReceiveData[EPSPerParaDataLength * (i / (EPSPerParaDataLength - HeadDataLength))
                + HeadDataLength + i % (EPSPerParaDataLength - HeadDataLength)];
            }
            return LightStatusByEPS;
        }

        private static byte GetCheckSum()
        {
            int CheckSum = 0;
            for (int i = 1; i < EPSSendData.Length - 1; i++)
            {
                CheckSum = (CheckSum + (int)EPSSendData[i]) % 256;
            }
            return (byte)CheckSum;
        }

        /// <summary>
        /// 清空EPS发送以及接收的数据
        /// </summary>
        private static void ClearEPSSendAndReceiveData()
        {
            EPSReceiveDataLength = 0;
            EPSReceiveData = Enumerable.Repeat((byte)0, EPSReceiveData.Length).ToArray();
            EPSSendData = Enumerable.Repeat((byte)0, EPSSendData.Length).ToArray();
        }
    }
}
