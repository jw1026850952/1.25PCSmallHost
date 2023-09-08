using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace PCSmallHost.UserWindow
{
    /// <summary>
    /// 手动季检
    /// </summary>
    public partial class SeasonCheck : Window
    {
        /// <summary>
        /// 定时刷新年检进度
        /// </summary>
        private DispatcherTimer RefreshSeasonCheckProcess;
        /// <summary>
        /// 总共执行的时间
        /// </summary>
        private int TotalExecuteSecond;
        /// <summary>
        /// 需要执行的时间
        /// </summary>
        private int NeedExecuteSecond = 1800;
        /// <summary>
        /// 时分秒的间隔
        /// </summary>
        private int TimeInterval = 60;       

        public SeasonCheck()
        {
            InitializeComponent();
            InitTimer();
        }

        /// <summary>
        /// 初始化定时器
        /// </summary>
        private void InitTimer()
        {
            RefreshSeasonCheckProcess = new DispatcherTimer();
            RefreshSeasonCheckProcess.Interval = new TimeSpan(0, 0, 1);
            RefreshSeasonCheckProcess.Tick += RefreshSeasonCheckProcess_Tick;
            RefreshSeasonCheckProcess.IsEnabled = true;
        }

        private void RefreshSeasonCheckProcess_Tick(object sender, EventArgs e)
        {
            RefreshMessage();
        }

        /// <summary>
        /// 刷新界面内容
        /// </summary>
        private void RefreshMessage()
        {
            if (TotalExecuteSecond == NeedExecuteSecond)
            {               
                this.Close();
            }

            TotalExecuteSecond++;            
            this.labSeasonCheckMinute.Content = TotalExecuteSecond / TimeInterval;
            this.labSeasonCheckSecond.Content = TotalExecuteSecond % TimeInterval;            
        }
    }
}
