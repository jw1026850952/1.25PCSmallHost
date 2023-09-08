using PCSmallHost.Util;
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
    /// 东君联动
    /// </summary>
    public class DJZMSL : FireAlarmLinkInterface
    {
        /// <summary>
        /// 火灾联动发送数据长度
        /// </summary>
        private int FireAlarmLinkSendDataLength = 5;
        /// <summary>
        /// 火灾联动接收数据长度
        /// </summary>
        private int FireAlarmLinkReceiveDataLength = 6;

        /// <summary>
        /// 检测串口是否已打开
        /// </summary>
        public static bool SerialPortIsOpen;

        public DJZMSL()
        {

        }

        /// <summary>
        /// 主机板和火灾联动交互
        /// </summary>
        public void FireAlarmLinkInter()
        {
            SendDataFireAlarmLink();
            ReceiveDataFireAlarmLink();
        }

        /// <summary>
        /// 发送火灾报警器数据
        /// </summary>
        private void SendDataFireAlarmLink()
        {
            if (FireAlarmSerialPort != null && FireAlarmSerialPort.IsOpen)
            {
                SerialPortIsOpen = true;
                try
                {
                    FireAlarmLinkSendData[0] = 0xAA;
                    FireAlarmLinkSendData[1] = 0x03;
                    FireAlarmLinkSendData[2] = 0x00;
                    FireAlarmLinkSendData[3] = 0x00;
                    FireAlarmLinkSendData[4] = 0X03;
                    FireAlarmSerialPort.Write(FireAlarmLinkSendData, 0, FireAlarmLinkSendDataLength);
                    Thread.Sleep(ExeInstructSleepTime);
                }
                catch
                {
                    //CommonFunct.GetRemoteConnectRecord(ex.Message);
                }
            }
            else
            {
                if(SerialPortIsOpen)
                {
                    SerialPortIsOpen = false;
                    MessageBox.Show("火警通讯串口已关闭！！");
                }
            }
        }

        private byte GetSum()
        {
            int sum = 0;
            for(int i =1;i< FireAlarmLinkReceiveDataLength - 1; i++)
            {
                sum += FireAlarmReceiveData[i];
            }
            if(sum >= 255)
            {
                sum %= 255;
            }
            return (byte)sum;
        }

        /// <summary>
        /// 接收火灾报警器数据
        /// </summary>
        private void ReceiveDataFireAlarmLink()
        {
            if (FireAlarmSerialPort != null && FireAlarmSerialPort.IsOpen)
            {
                SerialPortIsOpen = true;
                try
                {
                    int bytesToRead = FireAlarmSerialPort.BytesToRead;//获取串口接收缓冲区数据的字节数
                    if (bytesToRead > 0)
                    {
                        //接收到0X09，即为00001001，对应的是预案一和预案四
                        FireAlarmSerialPort.Read(FireAlarmReceiveData, 0, FireAlarmLinkReceiveDataLength);
                        if(FireAlarmReceiveData[0] == 0XAA && FireAlarmReceiveData[1] ==0X03 && FireAlarmReceiveData[5] == GetSum())
                        {
                            if (FireAlarmReceiveData[2] != 0x00)
                            {
                                if ((FireAlarmReceiveData[2] & 0X80) != 0)
                                {
                                    LstFireAlarmLinkZoneNumber.Add((int)EnumClass.PlanNumber.预案8);
                                }

                                if ((FireAlarmReceiveData[2] & 0X40) != 0)
                                {
                                    LstFireAlarmLinkZoneNumber.Add((int)EnumClass.PlanNumber.预案7);
                                }

                                if ((FireAlarmReceiveData[2] & 0X20) != 0)
                                {
                                    LstFireAlarmLinkZoneNumber.Add((int)EnumClass.PlanNumber.预案6);
                                }

                                if ((FireAlarmReceiveData[2] & 0X10) != 0)
                                {
                                    LstFireAlarmLinkZoneNumber.Add((int)EnumClass.PlanNumber.预案5);
                                }

                                if ((FireAlarmReceiveData[2] & 0X08) != 0)
                                {
                                    LstFireAlarmLinkZoneNumber.Add((int)EnumClass.PlanNumber.预案4);
                                }

                                if ((FireAlarmReceiveData[2] & 0X04) != 0)
                                {
                                    LstFireAlarmLinkZoneNumber.Add((int)EnumClass.PlanNumber.预案3);
                                }

                                if ((FireAlarmReceiveData[2] & 0X02) != 0)
                                {
                                    LstFireAlarmLinkZoneNumber.Add((int)EnumClass.PlanNumber.预案2);
                                }

                                if ((FireAlarmReceiveData[2] & 0X01) != 0)
                                {
                                    LstFireAlarmLinkZoneNumber.Add((int)EnumClass.PlanNumber.预案1);
                                }
                            }

                            if (FireAlarmReceiveData[3] != 0X00)
                            {
                                if ((FireAlarmReceiveData[3] & 0X40) != 0)
                                {
                                    LstFireAlarmLinkZoneNumber.Add((int)EnumClass.PlanNumber.预案15);
                                }

                                if ((FireAlarmReceiveData[3] & 0X20) != 0)
                                {
                                    LstFireAlarmLinkZoneNumber.Add((int)EnumClass.PlanNumber.预案14);
                                }

                                if ((FireAlarmReceiveData[3] & 0X10) != 0)
                                {
                                    LstFireAlarmLinkZoneNumber.Add((int)EnumClass.PlanNumber.预案13);
                                }

                                if ((FireAlarmReceiveData[3] & 0X08) != 0)
                                {
                                    LstFireAlarmLinkZoneNumber.Add((int)EnumClass.PlanNumber.预案12);
                                }

                                if ((FireAlarmReceiveData[3] & 0X04) != 0)
                                {
                                    LstFireAlarmLinkZoneNumber.Add((int)EnumClass.PlanNumber.预案11);
                                }

                                if ((FireAlarmReceiveData[3] & 0X02) != 0)
                                {
                                    LstFireAlarmLinkZoneNumber.Add((int)EnumClass.PlanNumber.预案10);
                                }

                                if ((FireAlarmReceiveData[3] & 0X01) != 0)
                                {
                                    LstFireAlarmLinkZoneNumber.Add((int)EnumClass.PlanNumber.预案9);
                                }
                            }
                        }
                    }
                }
                catch
                {
                    //CommonFunct.GetRemoteConnectRecord(ex.Message);
                }
            }
            else
            {
                if(SerialPortIsOpen)
                {
                    SerialPortIsOpen = false;
                    MessageBox.Show("火警通讯串口已关闭！！");
                }
            }
        }
    }
}
