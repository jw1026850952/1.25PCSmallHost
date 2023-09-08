using Sugar.Log;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PCSmallHost.FireAlarmLinkage
{
    public sealed class Tanda : FireAlarmBase
    {
        public static FireAlarmBase Instance { get; } = new Tanda();

        private FireDetectorObject _fireDetector;
        public FireDetectorObject FireDetector => _fireDetector;

        private Tanda() { }

        public override void Enable(
            string portName,
            int baudRate,
            int queryPeriod = 1000,
            StopBits stopBits = StopBits.One,
            Parity parity = Parity.None,
            int readTimeout = -1,
            int writeTimeout = -1,
            int readDelayXms = 50,
            int writeDelayXms = 50,
            int maxUnusedReadCount = 1000,
            bool isDropOldMessage = true)
        {
            lock (this)
            {
                Disable();
                _ = serialMonitor.Connect(
                    portName, baudRate, stopBits, parity,
                    readTimeout, writeTimeout,
                    readDelayXms, writeDelayXms,
                    maxUnusedReadCount, isDropOldMessage);
                serialMonitor.OnEnable();
                HasAlarmed = false;
                OnMonitor(queryPeriod);
            }
        }

        public override void Disable()
        {
            lock (this)
            {
                serialMonitor.Disconnect();
                OffMonitor();
            }
        }

        public override bool OffMonitor()
        {
            try
            {
                _fireAlarmLinkageZone.Clear();
                serialMonitor.OnEnable();
                _cts?.Cancel();
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                _cts?.Dispose();
                _cts = new CancellationTokenSource();
            }
            return true;
        }

        private CancellationTokenSource _cts = new CancellationTokenSource();
        public override bool OnMonitor(int queryPeriod = 1000)
        {
            _monitorTokenSource = new CancellationTokenSource();
            try
            {
                if (!serialMonitor.IsConnected) return false;
                if (!serialMonitor.IsMonitoring)
                {
                    serialMonitor.OnEnable();
                }

                Task.Factory.StartNew(
                    () => ReadLoopAsync(queryPeriod >> 1, _cts.Token),
                    _cts.Token,
                    TaskCreationOptions.LongRunning,
                    TaskScheduler.Default);
            }
            catch (TaskCanceledException tce)
            {
                LoggerManager.WriteWarn(tce.Message);
            }
            catch (Exception ex)
            {
                LoggerManager.WriteError(ex.ToString());
            }
            return true;
        }

        protected override void UnpackMessage(byte[] message, out FireDetectorObject detector)
        {
            detector = new FireDetectorObject();
            try
            {
                int length = message.Length;
                //判断数据准确性
                if (message[0] != 0X40 || message[1] != 0X40 || message[length - 2] != 0X23 || message[length - 1] != 0X23)
                {
                    return;
                }
                else if ((message[25] * 256 + message[24]) != length - _basicData)//CompressBCD2Decimal(message.ElementAt(25), message.ElementAt(24))
                {
                    serialMonitor.DiscardInBuffer();
                    return;
                }

                //判断该数据是否是反馈火灾自动报警系统部件运行状态
                if (message[27] != 0X02) return;
                //判断是否反馈火警信号
                if (message[40] != 0X01 && message[40] != 0X08 && message[40] != 0X09 && message[40] != 0X0A && message[40] != 0X0B) return;

                detector.NetworkAddress = GetQuadraticCode(GetDesignateMessage(message, 32, 3));
                detector.CircuitNumber = GetQuadraticCode(GetDesignateMessage(message, 35, 2));
                detector.DeviceID = GetQuadraticCode(GetDesignateMessage(message, 37, 3));
                detector.EventType = MappingFireAlarmEventType(message.ElementAt(40));
            }
            catch (Exception ex)
            {
                LoggerManager.WriteError(ex.ToString());
            }
        }

        private async Task ReadLoopAsync(int delayXms, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            while (true)
            {
                if (token.IsCancellationRequested)
                {
                    token.ThrowIfCancellationRequested();
                }

                await Task.Delay(delayXms);
                byte[] message = serialMonitor.ReadMessage();
                if (message != null)
                {
                    UnpackMessage(message, out _fireDetector);
                    OnFireAlarm(_fireDetector);
                }
            }
        }

        private byte[] GetDesignateMessage(byte[] data, int start, int count)
        {
            byte[] newMessage = new byte[count];
            Array.Copy(data, start, newMessage, 0, count);
            return newMessage;
        }

        private char CompressASCII2Decimal(byte data)
        {
            //return (char)Convert.ToInt32(Convert.ToString(data, 10));
            return (char)data;
        }

        private int GetQuadraticCode(byte[] data)
        {
            string code = "";
            for (int i = 0; i < data.Length; i++)
            {
                code += CompressASCII2Decimal(data[i]);
            }
            return Convert.ToInt32(code);
        }

        private FireAlarmEventType MappingFireAlarmEventType(byte content)
        {
            //switch (content)
            //{
            //    case 0x00:
            //        return FireAlarmEventType.IgnorableEvent;
            //    case 0x01:
            //        return FireAlarmEventType.FireAlarm;
            //    case 0x0A:
            //        return FireAlarmEventType.Reset;
            //    default:
            //        return FireAlarmEventType.IgnorableEvent;
            //}
            if (content == 0X01 || content == 0X08 || content == 0X09 || content == 0X0A || content == 0X0B)
            {
                return FireAlarmEventType.FireAlarm;
            }
            else
            {
                return FireAlarmEventType.IgnorableEvent;
            }

        }

        /// <summary>
        /// 压缩BCD转十进制
        /// </summary>
        /// <param name="bcd"></param>
        /// <returns></returns>
        private int CompressBCD2Decimal(byte high, byte low)
        {
            var s = high >> 4;
            var c = low >> 4;
            var d = high & 0x0f;
            var b = low & 0x0f;
            string decimalStr = $"{high >> 4}{high & 0x0f}{low >> 4}{low & 0x0f}";
            int.TryParse(decimalStr, out int number);
            return number;
        }

        private CancellationTokenSource _monitorTokenSource;
        private const int _basicData = 30;//除应用数据单元外的字节数
    }
}
