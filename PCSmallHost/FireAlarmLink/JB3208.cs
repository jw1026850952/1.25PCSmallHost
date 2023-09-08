using FireLinkage;
using PCSmallHost.DB.BLL;
using PCSmallHost.DB.Model;
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
    public class JB3208 : FireAlarmLinkInterface
    {
        /// <summary>
        /// 火灾联动发送数据长度
        /// </summary>
        private int FireAlarmLinkSendDataLength = 9;
        /// <summary>
        /// 发送火灾报警器数据的数组
        /// </summary>
        private byte[] FireAlarmSendData = new byte[9];
        /// <summary>
        /// 串口缓存的数据字节
        /// </summary>
        private int bytesToRead = 0;
        /// <summary>
        /// 串口中数据中有多少组数
        /// </summary>
        private int SumData = 0;

        private List<FireAlarmPartitionSetInfo> LstFireAlarmPartitionSet;

        private CFireAlarmPartitionSet ObjFireAlarmPartitionSet = new CFireAlarmPartitionSet();


        public JB3208()
        { }

        public void FireAlarmLinkInter()
        {
            LstFireAlarmPartitionSet = ObjFireAlarmPartitionSet.GetAll();
            LstFireAlarmLinkZoneNumber.Clear();
            ClearFireAlarmReceiveData();
            if(ControllerNum != 0)
            {
                SendDataFireAlarmLink();
                ReceiveDataFireAlarmLink();
            }
        }

        private byte[] GetByteData(byte[] data)
        {
            List<byte> LstData = new List<byte>();
            LstData = data.ToList<byte>();
            LstData.RemoveAt(0);
            LstData.RemoveRange(bytesToRead - 3, (data.Length-1) - (bytesToRead - 3) + 1);
            byte[] NewData = LstData.ToArray<byte>();
            return NewData;
        }

        /// <summary>
        /// 应答上海松江控制器
        /// </summary>
        private void SendDataFireAlarmLink()
        {
            if(FireAlarmSerialPort != null && FireAlarmSerialPort.IsOpen)
            {
                
                try
                {
                    FireAlarmSendData[0] = 0XFE;
                    FireAlarmSendData[1] = ControllerNum;
                    FireAlarmSendData[2] = FireAlarmSendData[5] = 0X6E;
                    FireAlarmSendData[3] = 0X01;
                    FireAlarmSendData[4] = 0X00;

                    byte[] data = { ControllerNum, 0X6E, 0X01, 0X00, 0X6E };
                    byte[] crc = CRC.ToCRC16Bytes(data, true);

                    FireAlarmSendData[6] = crc[0];
                    FireAlarmSendData[7] = crc[1];
                    FireAlarmSendData[8] = 0XFF;
                    FireAlarmSerialPort.Write(FireAlarmSendData, 0, FireAlarmLinkSendDataLength);
                    Thread.Sleep(ExeInstructSleepTime);
                }
                catch
                {
                    //CommonFunct.GetRemoteConnectRecord(ex.Message);
                }

            }
            else
            {
                MessageBox.Show("火警通讯串口已关闭!!");
            }
        }

        private void ReceiveDataFireAlarmLink()
        {
            if(FireAlarmSerialPort != null && FireAlarmSerialPort.IsOpen)
            {
                try
                {
                    int bytesToRead = FireAlarmSerialPort.BytesToRead;//获取串口接收缓冲去数据的字节数
                    if(bytesToRead > 0)
                    {
                        FireAlarmSerialPort.Read(FireAlarmReceiveData, 0, bytesToRead);

                        byte[] crc16 =
                        {
                            FireAlarmReceiveData[bytesToRead - 3],
                            FireAlarmReceiveData[bytesToRead - 2]
                        };

                        if(CRC.CheckWithCRC16(GetByteData(FireAlarmReceiveData),crc16,true))
                        {
                            if (FireAlarmReceiveData[1] == ControllerNum && FireAlarmReceiveData[2] == 110)
                            {
                                SumData = FireAlarmReceiveData[5] * 256 + FireAlarmReceiveData[6];
                                int fireAlarmZoneNumber = 0;//记录符合条件的预案分区
                                int mainBoardCircuit = 0;//主板回路
                                int deviceValue = 0;//设备地址

                                for (int i=0;i<SumData;i++)
                                {
                                    if(FireAlarmReceiveData[9 + 32 * (i - 1)] > 0 && FireAlarmReceiveData[9 + 32 * (i - 1)] < 73)//判断返回数据是探测点或者模块号的数据
                                    {
                                        if(FireAlarmReceiveData[13 + 32 * (i - 1)] == 1)//判断该点号是否是火警状态
                                        {
                                            mainBoardCircuit = FireAlarmReceiveData[8 + 32 * (i - 1)];
                                            deviceValue = FireAlarmReceiveData[9 + 32 * (i - 1)];

                                            foreach(FireAlarmPartitionSetInfo infoFireAlarmPartitionSet in LstFireAlarmPartitionSet)
                                            {
                                                if(infoFireAlarmPartitionSet.MainBoardCircuit == mainBoardCircuit && infoFireAlarmPartitionSet.LowDeviceRange >= deviceValue && infoFireAlarmPartitionSet.HighDeviceRange <= deviceValue)
                                                {
                                                    fireAlarmZoneNumber = infoFireAlarmPartitionSet.PlanPartition;
                                                    LstFireAlarmLinkZoneNumber.Add(fireAlarmZoneNumber);
                                                    break;
                                                }
                                            }
                                        }
                                    }
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
        }
    }
}
