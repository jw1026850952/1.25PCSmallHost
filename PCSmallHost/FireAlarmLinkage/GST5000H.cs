using PCSmallHost.DB.Model;
using Sugar.Log;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PCSmallHost.FireAlarmLinkage
{
    public sealed class GST5000H : FireAlarmBase
    {
        public static FireAlarmBase Instance { get; } = new GST5000H();

        private FireDetectorObject _fireDetector;
        public FireDetectorObject FireDetector => _fireDetector;

        private GST5000H() { }

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
                serialMonitor.OnDisable();
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
            try
            {
                if (!serialMonitor.IsConnected)
                {
                    return false;
                }

                if (!serialMonitor.IsMonitoring)
                {
                    serialMonitor.OnEnable();
                }

                Task.Factory.StartNew(
                    () => WriteLoopAsync(queryPeriod, _cts.Token),
                    _cts.Token,
                    TaskCreationOptions.LongRunning,
                    TaskScheduler.Default);

                Task.Factory.StartNew(
                    () => ReadLoopAsync(queryPeriod >> 1, _cts.Token),
                    _cts.Token,
                    TaskCreationOptions.LongRunning,
                    TaskScheduler.Default);
            }
            catch (OperationCanceledException oce)
            {
                LoggerManager.WriteError(oce.Message);
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
                if (length < _readBufferSize)
                {
                    return;
                }
                else if (length > _readBufferSize)
                {
                    serialMonitor.DiscardInBuffer();
                }

                ushort crc16 =
                    (ushort)((message.ElementAt(length - 1) << 8)
                    | message.ElementAt(length - 2));
                IEnumerable<byte> data = message.Take(length - 2);
                ushort checkCode = ToModbusCRC16(data);
                if (crc16 != checkCode)
                {
                    return;
                }

                detector.CircuitNumber = CompressBCD2Decimal(0, message.ElementAt(8));
                detector.DeviceID = CompressBCD2Decimal(message.ElementAt(9), message.ElementAt(10));
                detector.EventType = MappingFireAlarmEventType(message.ElementAt(3));

            }
            catch (Exception ex)
            {
                LoggerManager.WriteError(ex.ToString());
            }
        }

        private async Task WriteLoopAsync(int delayXms, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            while (true)
            {
                if (token.IsCancellationRequested)
                {
                    token.ThrowIfCancellationRequested();
                }

                await Task.Delay(delayXms);
                serialMonitor.SendMessage(_firstQueryCommand);
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

        private FireAlarmEventType MappingFireAlarmEventType(byte content)
        {
            switch (content)
            {
                case 0x00:
                    return FireAlarmEventType.IgnorableEvent;
                case 0x01:
                    return FireAlarmEventType.FireAlarm;
                case 0x0A:
                    return FireAlarmEventType.Reset;
                default:
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
            string decimalStr = $"{high >> 4}{high & 0x0f}{low >> 4}{low & 0x0f}";
            int.TryParse(decimalStr, out int number);
            return number;
        }

        private ushort ToModbusCRC16(IEnumerable<byte> buffer)
        {
            const ushort ploy = 0xA001; // 反转后的生成多项式，原生成多项式0x8005
            const ushort xorout = 0x0000;
            uint init = 0xFFFF;
            foreach (byte data in buffer)
            {
                init ^= data;
                for (int i = 0; i < 8; ++i)
                {
                    init = (init & 0x01) != 0 ? (init >> 1) ^ ploy : (init >> 1);
                }
            }
            return (ushort)(init ^ xorout);
        }

        #region GST5000H协议
        private readonly byte[] _firstQueryCommand
            = new byte[] { 0x01, 0x03, 0x00, 0x00, 0x00, 0x04, 0x09, 0x44 };
        private readonly byte[] _requeryCommand
            = new byte[] { 0x01, 0x03, 0x00, 0x04, 0x00, 0x04, 0xC8, 0x05 };
        private const int _readBufferSize = 13;
        #endregion
    }
}
