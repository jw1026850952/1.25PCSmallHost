using SunFo.OS_SmallHost_.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SunFo.OS_SmallHost_.UserWindow
{
    /// <summary>
    /// 模拟联动窗口
    /// </summary>
    public partial class SimulateLink : Window
    {
        private GifImage GifImage;
        /// <summary>
        /// 应急计时
        /// </summary>
        private Timer EmergencyTimer;
        /// <summary>
        /// 应急总计时
        /// </summary>
        private int TotalEmergencyTime;
        /// <summary>
        /// 应急总计时
        /// </summary>
        public string StrTotalEmergencyTime;
        /// <summary>
        /// 文件路径
        /// </summary>
        private string GifFilePath;
        /// <summary>
        /// 时分秒间隔
        /// </summary>
        private int TimeInterval = 60;
        /// <summary>
        /// 分割索引
        /// </summary>
        private int SplitIndexOf;

        public SimulateLink(int fireAlarmZoneNumber)
        {
            InitializeComponent();
            InitTimer();
            InitDataSource(fireAlarmZoneNumber);            
            //InitMediaElement();
            //InitGifFilePath();
        }

        /// <summary>
        /// 初始化数据源
        /// </summary>
        private void InitDataSource(int fireAlarmZoneNumber)
        {            
            this.labFireAlarmZoneNumber.Content = fireAlarmZoneNumber;
        }

        /// <summary>
        /// 初始化定时器
        /// </summary>
        private void InitTimer()
        {
            EmergencyTimer = new Timer();
            EmergencyTimer.Interval = 1000;
            EmergencyTimer.Tick += Timer_Tick;           
            EmergencyTimer.Enabled = true;
        }
       
        /// <summary>
        /// 计算应急计时
        /// </summary>
        private void CalculateEmergencyTime()
        {
            TotalEmergencyTime++;
            this.labEmergencyTime.Content = string.Format("{0}:{1}:{2}", CommonFunct.ForMatTime(TotalEmergencyTime / (TimeInterval * TimeInterval)), CommonFunct.ForMatTime(TotalEmergencyTime / TimeInterval), CommonFunct.ForMatTime(TotalEmergencyTime % TimeInterval));
        }

        /// <summary>
        /// 初始化Gif文件路径
        /// </summary>
        private void InitGifFilePath()
        {
            GifFilePath = System.Windows.Forms.Application.StartupPath;
            SplitIndexOf = GifFilePath.IndexOf("\\bin");
            GifFilePath = string.Format("{0}\\Pictures\\SimulateLink.gif", GifFilePath.Substring(0, SplitIndexOf));
        }

        /// <summary>
        /// 关闭模拟联动窗口
        /// </summary>
        private void CloseSimulateLink()
        {
            EmergencyTimer.Enabled = false;
            StrTotalEmergencyTime = this.labEmergencyTime.Content.ToString();
            this.Close();
        }

        /// <summary>
        /// 循环播放
        /// </summary>
        /// <param name="sender"></param>
        private void CirculatePlay(object sender)
        {
            (sender as MediaElement).Source = null;
            (sender as MediaElement).Stop();
            (sender as MediaElement).Source = new Uri(GifFilePath, UriKind.Absolute);
            (sender as MediaElement).Play();
        }

        /// <summary>
        /// 开启播放
        /// </summary>
        /// <param name="sender"></param>
        private void StartPlay(object sender)
        {            
            (sender as MediaElement).Source = new Uri(GifFilePath, UriKind.Absolute);
            (sender as MediaElement).Play();
        }

        private void StopPlay(object sender)
        {         
            (sender as MediaElement).Stop();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            CalculateEmergencyTime();
        }

        private void btnCloseSimulateLink_Click(object sender, RoutedEventArgs e)
        {
            CloseSimulateLink();           
        }

        private void MediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {            
            CirculatePlay(sender);
        }

        private void MediaElement_Loaded(object sender, RoutedEventArgs e)
        {
            StartPlay(sender);
        }

        private void MediaElement_Unloaded(object sender, RoutedEventArgs e)
        {
            StopPlay(sender);
        }
    }
}
