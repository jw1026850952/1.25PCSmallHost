using PCSmallHost.Util;
using Sugar.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace PCSmallHost.FireAlarmLink
{
    /// <summary>
    /// 东君故障联动一体
    /// </summary>
    public class DJZMAL : AbsFireAlarmLink
    {
        /// <summary>
        /// 火灾联动发送数据长度
        /// </summary>
        private static int FireAlarmLinkSendDataLength = 5;
        /// <summary>
        /// 火灾联动接收数据长度
        /// </summary>
        private int FireAlarmLinkReceiveDataLength = 8;

        private static bool Issytran = false;

        public DJZMAL()
        {

        }

        /// <summary>
        /// 主机板和火灾联动交互
        /// </summary>
        public void FireAlarmLinkInter()
        {
            SendFireAlarmLinkData();
            ReceiveFireAlarmLinkData();
            
        }

        /// <summary>
        /// 发送火灾报警器数据
        /// </summary>
        private static void SendFireAlarmLinkData()
        {
            while (HostBoardSerialPort != null && HostBoardSerialPort.IsOpen && !IsSerialPortFree)
            {
                System.Windows.Forms.Application.DoEvents();
                Thread.Sleep(1000);
            }
            if (IsSerialPortFree)
            {
                lock (locker)
                {
                    if (IsSerialPortFree)
                    {
                        IsSerialPortFree = false;
                        if(!IsSyTrans)
                        {
                            try
                            {
                                IsSyTrans = true;
                                Issytran = true;
                                FireAlarmLinkSendData[0] = 0XAA;
                                FireAlarmLinkSendData[1] = 0X04;
                                FireAlarmLinkSendData[2] = FireAlarmLinkSendData[3] = 0X00;
                                FireAlarmLinkSendData[4] = 0X04;
                                HostBoardSerialPort.Write(FireAlarmLinkSendData, 0, FireAlarmLinkSendDataLength);
                                //Thread.Sleep(ExeInstructSleepTime);
                            }
                            catch
                            {
                                //CommonFunct.GetRemoteConnectRecord(ex.Message);
                            }
                        }
                    }
                }
            }
            #region 旧
            //if (HostBoardSerialPort != null && HostBoardSerialPort.IsOpen)
            //{
            //    serialPortIsOpen = true;
            //    if(!IsSyTrans)
            //    {
            //        try
            //        {
            //            IsSyTrans = true;
            //            Issytran = true;
            //            FireAlarmLinkSendData[0] = 0XAA;
            //            FireAlarmLinkSendData[1] = 0X04;
            //            FireAlarmLinkSendData[2] = FireAlarmLinkSendData[3] = 0x00;
            //            FireAlarmLinkSendData[4] = 0X04;
            //            while (!IsSerialPortFree)
            //            {
            //                System.Windows.Forms.Application.DoEvents();
            //                Thread.Sleep(100);
            //            }
            //            IsSerialPortFree = false;
            //            HostBoardSerialPort.Write(FireAlarmLinkSendData, 0, FireAlarmLinkSendDataLength);
            //            Thread.Sleep(ExeInstructSleepTime);
            //        }
            //        catch (Exception ex)
            //        {
            //            //CommonFunct.GetRemoteConnectRecord(ex.Message);
            //        }
            //    }
            //}
            //else
            //{
            //    if(serialPortIsOpen)
            //    {
            //        serialPortIsOpen = false;
            //        MessageBox.Show("主机通讯串口已关闭！！");
            //    }
            //}
            #endregion
        }

        /// <summary>
        /// 接收火灾报警器数据
        /// </summary>
        private void ReceiveFireAlarmLinkData()
        {
            try
            {
                Thread.Sleep(ExeInstructSleepTime);
                if (Issytran)
                {
                    lock (locker)
                    {
                        int bytesToRead = HostBoardSerialPort.BytesToRead;
                        if (bytesToRead > 0)
                        {
                            HostBoardSerialPort.Read(FireAlarmLinkReceiveData, 0, FireAlarmLinkReceiveDataLength);
                            if(FireAlarmLinkReceiveData[0] == 0XAA && FireAlarmLinkReceiveData[1] == 0X04 && FireAlarmLinkReceiveData[7] == GetChecksum())
                            {
                                if (FireAlarmLinkReceiveData[1] == 0X04)
                                {
                                    if (FireAlarmLinkReceiveData[2] == 0x01)
                                    {
                                        LstFireAlarmLinkZoneNumber.Add((int)EnumClass.PlanNumber.预案1);
                                    }

                                    if (FireAlarmLinkReceiveData[3] == 0x02)
                                    {
                                        LstFireAlarmLinkZoneNumber.Add((int)EnumClass.PlanNumber.预案2);
                                    }

                                    if (FireAlarmLinkReceiveData[4] == 0x03)
                                    {
                                        LstFireAlarmLinkZoneNumber.Add((int)EnumClass.PlanNumber.预案3);
                                    }

                                    if (FireAlarmLinkReceiveData[5] == 0x04)
                                    {
                                        LstFireAlarmLinkZoneNumber.Add((int)EnumClass.PlanNumber.预案4);

                                    }
                                    if (FireAlarmLinkReceiveData[6] == 0x05)
                                    {
                                        LstFireAlarmLinkZoneNumber.Add((int)EnumClass.PlanNumber.预案5);
                                    }
                                }
                            }
                        }
                        HostBoardSerialPort.DiscardInBuffer();
                    }
                    IsSerialPortFree = true;
                    IsSyTrans = false;
                    Issytran = false;
                }
                else if (HostBoardSerialPort == null || !HostBoardSerialPort.IsOpen)
                {
                    throw new InvalidOperationException("mainboard serial communication is excepiton");
                }
            }
            catch(InvalidOperationException ex)
            {
                IsSerialPortFree = true;
                IsSyTrans = false;
                Issytran = false;
                HostBoardReturnStatus = (byte)EnumClass.HostBoardStatus.通信故障;
                HostBoardReturnReset = 0;
                _ = LoggerManager.WriteDebug(ex.Message);
            }

            #region 旧
            //if (HostBoardSerialPort != null && HostBoardSerialPort.IsOpen)
            //{
            //    serialPortIsOpen = true;
            //    if(Issytran)
            //    {
            //        try
            //        {
            //            int bytesToRead = HostBoardSerialPort.BytesToRead;
            //            if (bytesToRead > 0)
            //            {
            //                HostBoardSerialPort.Read(FireAlarmLinkReceiveData, 0, FireAlarmLinkReceiveDataLength);
            //                if (FireAlarmLinkReceiveData[1] == 0X04)
            //                {
            //                    if (FireAlarmLinkReceiveData[2] == 0x01)
            //                    {
            //                        LstFireAlarmLinkZoneNumber.Add((int)EnumClass.PlanNumber.预案1);
            //                    }

            //                    if (FireAlarmLinkReceiveData[3] == 0x02)
            //                    {
            //                        LstFireAlarmLinkZoneNumber.Add((int)EnumClass.PlanNumber.预案2);
            //                    }

            //                    if (FireAlarmLinkReceiveData[4] == 0x03)
            //                    {
            //                        LstFireAlarmLinkZoneNumber.Add((int)EnumClass.PlanNumber.预案3);
            //                    }

            //                    if (FireAlarmLinkReceiveData[5] == 0x04)
            //                    {
            //                        LstFireAlarmLinkZoneNumber.Add((int)EnumClass.PlanNumber.预案4);
            //                    }
            //                }
            //            }
            //            HostBoardSerialPort.DiscardInBuffer();
            //            IsSerialPortFree = true;
            //            IsSyTrans = false;
            //            Issytran = false;
            //        }
            //        catch (Exception ex)
            //        {
            //            //CommonFunct.GetRemoteConnectRecord(ex.Message);
            //            IsSerialPortFree = true;
            //        }
            //    }
            //}
            //else
            //{
            //    if(serialPortIsOpen)
            //    {
            //        serialPortIsOpen = false;
            //        MessageBox.Show("主机通讯串口已关闭！！");
            //    }
            //}
            #endregion
        }

        private int GetChecksum()
        {
            int sum = 0;
            for (int i = 1; i < FireAlarmLinkReceiveDataLength - 1; i++)
            {
                sum += FireAlarmLinkReceiveData[i];
            }
            if(sum >= 255)
            {
                sum %= 255;
            }
            return sum;
        }
    }
}
