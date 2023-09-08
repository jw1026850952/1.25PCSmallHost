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
    /// 手动月检
    /// </summary>
    public partial class MonthCheck : Window
    {
        /// <summary>
        /// 定时刷新月检进度
        /// </summary>
        private DispatcherTimer RefreshMonthCheckProcess;
        /// <summary>
        /// 总共执行的时间
        /// </summary>
        private int TotalExecuteSecond;
        /// <summary>
        /// 需要执行的时间
        /// </summary>
        private int NeedExecuteSecond = 450;
        /// <summary>
        /// 时分秒的间隔
        /// </summary>
        private int TimeInterval = 60;       

        public MonthCheck(string strMonthCheckTitle)
        {
            InitializeComponent();
            InitMonythCheckTitle(strMonthCheckTitle);
            InitTimer();
        }

        /// <summary>
        /// 初始化定时器
        /// </summary>
        private void InitTimer()
        {
            RefreshMonthCheckProcess = new DispatcherTimer();
            RefreshMonthCheckProcess.Interval = new TimeSpan(0, 0, 1);
            RefreshMonthCheckProcess.Tick += RefreshMonthCheckProcess_Tick;
            RefreshMonthCheckProcess.IsEnabled = true;
        }

        /// <summary>
        /// 初始化月检标题
        /// </summary>
        /// <param name="strMonthCheckTitle"></param>
        private void InitMonythCheckTitle(string strMonthCheckTitle)
        {
            this.labMonthCheckTitle.Content = strMonthCheckTitle;
        }

        private void RefreshMonthCheckProcess_Tick(object sender, EventArgs e)
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
            this.labMonthCheckMinute.Content = TotalExecuteSecond / TimeInterval;
            this.labMonthCheckSecond.Content = TotalExecuteSecond % TimeInterval;  
        }
    }
}
