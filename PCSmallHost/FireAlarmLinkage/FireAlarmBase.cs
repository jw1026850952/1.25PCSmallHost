using PCSmallHost.DB.BLL;
using PCSmallHost.DB.Model;
using Sugar.Log;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCSmallHost.FireAlarmLinkage
{
    public abstract class FireAlarmBase
    {
        public string FireAlarmPortName => serialMonitor?.PortName;

        public abstract void Enable(
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
            bool isDropOldMessage = true);

        public abstract void Disable();

        public abstract bool OnMonitor(int queryPeriod = 1000);

        //public virtual bool OffMonitor()
        //{
        //    serialMonitor.OnDisable();
        //    _fireAlarmLinkageZone.Clear();
        //    _hasAlarmed = false;
        //    return true;
        //}

        public abstract bool OffMonitor();

        protected virtual void OnFireAlarm(FireDetectorObject detector)
        {
            try
            {
                int zone = GetFireAlarmZone(detector);
                if (zone > 0)
                {
                    if (!_fireAlarmLinkageZone.Contains(zone))
                    {
                        _fireAlarmLinkageZone.Add(zone);
                        _hasAlarmed = false;
                    }
                    else
                    {
                        _hasAlarmed = true;
                    }
                }
                if (_fireAlarmLinkageZone.Count > 0)
                {
                    MainWindow.OnStartRealFireAlarmLink(_fireAlarmLinkageZone);
                }
            }
            catch (Exception)
            {
                //LoggerManager.WriteDebug(ex.ToString());
            }

        }

        private int GetFireAlarmZone(FireDetectorObject detector)
        {
            List<FireAlarmPartitionSetInfo> partitionSettings = _objPartitionSetting.GetAll();
            int parttion = partitionSettings.Where(it =>
            {
                return it.MainBoardCircuit == detector.CircuitNumber
                    && it.HighDeviceRange >= detector.DeviceID
                    && it.LowDeviceRange <= detector.DeviceID;
            }).Select(it => it.PlanPartition).FirstOrDefault();
            if (detector.EventType == FireAlarmEventType.FireAlarm)
            {
                return parttion;
            }
            else
            {
                return -1;
            }
        }

        public bool HasAlarmed
        {
            get => _hasAlarmed;
            protected set => _hasAlarmed = value;
        }

        protected abstract void UnpackMessage(byte[] message, out FireDetectorObject detector);

        protected readonly SerialMonitor serialMonitor = new SerialMonitor();
        private readonly CFireAlarmPartitionSet _objPartitionSetting = new CFireAlarmPartitionSet();
        public static readonly List<int> _fireAlarmLinkageZone = new List<int>();
        private bool _hasAlarmed = false;
    }
}
