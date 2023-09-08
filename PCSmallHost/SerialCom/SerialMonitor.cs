using System;
using System.IO.Ports;
using System.Collections;
using System.Threading.Tasks;
using System.Threading;
using System.IO;

namespace PCSmallHost
{
    public sealed class SerialMonitor
    {
        public bool IsConnected => _serialPort.IsOpen;

        public bool IsMonitoring { get; private set; } = false;

        public string PortName => _serialPort?.PortName;

        public SerialMonitor()
        {

        }

        public bool Connect(
            string portName,
            int baudRate,
            StopBits stopBits = StopBits.One,
            Parity parity = Parity.None,
            int readTimeout = -1,
            int writeTimeout = -1,
            int readDelayXms = 50,
            int writeDelayXms = 50,
            int maxUnusedReadCount = 1000,
            bool isDropOldMessage = true)
        {
            if (_serialPort.IsOpen)
            {
                return true;
            }
            lock (this)
            {
                try
                {
                    _serialPort.PortName = portName;
                    _serialPort.BaudRate = baudRate;
                    _serialPort.Parity = parity;
                    _serialPort.StopBits = stopBits;
                    _serialPort.ReadTimeout = readTimeout;
                    _serialPort.WriteTimeout = writeTimeout;
                    _readDelayXms = readDelayXms;
                    _writeDelayXms = writeDelayXms;
                    _maxUnusedReadCount = maxUnusedReadCount;
                    _isDropOldMessage = isDropOldMessage;
                    _serialPort.Open();
                }
                catch (IOException)
                {

                }
            }
            return _serialPort.IsOpen;
        }

        public bool Disconnect()
        {
            if (!_serialPort.IsOpen)
            {
                return true;
            }
            lock (this)
            {
                if (_serialPort.IsOpen)
                {
                    _serialPort.Close();
                }
            }
            return !_serialPort.IsOpen;
        }

        public void OnEnable()
        {
            lock (this)
            {
                if (!IsMonitoring)
                {
                    IsMonitoring = true;
                    _wcts = new CancellationTokenSource();
                    _rcts = new CancellationTokenSource();
                }
                else return;
            }

            Task.Factory.StartNew(
                WriteAsyncLoop, _wcts.Token,
                TaskCreationOptions.LongRunning, TaskScheduler.Default)
                .ConfigureAwait(false);
            Task.Factory.StartNew(
                ReadAsyncLoop, _rcts.Token,
                TaskCreationOptions.LongRunning, TaskScheduler.Default)
                .ConfigureAwait(false);
        }

        public void OnDisable()
        {
            try
            {
                lock (this)
                {
                    IsMonitoring = false;
                    _wcts?.Cancel();
                    _rcts?.Cancel();
                    _wcts?.Dispose();
                    _rcts?.Dispose();
                    ClearCahceMessage();
                }
            }
            catch
            {

            }
            //_wcts = new CancellationTokenSource();
            //_rcts = new CancellationTokenSource();
        }

        public void DiscardInBuffer()
        {
            try
            {
                _serialPort.DiscardInBuffer();

            }
            catch
            {

            }
        }

        public void ClearCahceMessage()
        {
            _outputQueue.Clear();
            _inputQueue.Clear();
        }

        public void SendMessage(byte[] message)
        {
            _outputQueue.Enqueue(message);
        }

        public byte[] ReadMessage()
        {
            if (_inputQueue?.Count > 0)
                return (byte[])_inputQueue?.Dequeue();
            return null;
        }

        private async Task WriteAsyncLoop()
        {
            while (!_wcts.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(_writeDelayXms);
                    if (_outputQueue?.Count > 0)
                    {
                        SendToWire((byte[])_outputQueue.Dequeue());
                    }
                }
                catch (TaskCanceledException)
                {

                }
                catch (TimeoutException)
                {

                }
            }
        }

        private async Task ReadAsyncLoop()
        {
            while (!_rcts.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(_readDelayXms);
                    byte[] message = ReadFromWire();
                    if (message != null)
                    {
                        if (_inputQueue.Count < _maxUnusedReadCount)
                        {
                            _inputQueue.Enqueue(message);
                        }
                        else
                        {
                            if (_isDropOldMessage)
                            {
                                _inputQueue.Dequeue();
                                _inputQueue.Enqueue(message);
                            }
                        }
                    }
                }
                catch (TaskCanceledException)
                {

                }
                catch (TimeoutException)
                {

                }
            }
        }

        private void SendToWire(byte[] message)
        {
            _serialPort.Write(message, 0, message.Length);
        }

        private byte[] ReadFromWire()
        {
            try
            {
                int dataLength = _serialPort.BytesToRead;
                if (dataLength > 0)
                {
                    byte[] inputBuffer = new byte[dataLength];
                    _ = _serialPort.Read(inputBuffer, 0, inputBuffer.Length);
                    _serialPort.DiscardInBuffer();
                    return inputBuffer;
                }
            }
            catch (Exception)
            {

            }
            finally
            {
            }
            return null;
        }

        private CancellationTokenSource _wcts;
        private CancellationTokenSource _rcts;

        private readonly Queue _inputQueue = Queue.Synchronized(new Queue());
        private readonly Queue _outputQueue = Queue.Synchronized(new Queue());
        private readonly SerialPort _serialPort = new SerialPort();
        private int _readDelayXms;
        private int _writeDelayXms;
        private int _maxUnusedReadCount;
        private bool _isDropOldMessage;
    }
}