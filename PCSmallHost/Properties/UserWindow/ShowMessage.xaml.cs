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

namespace PCSmallHost.UserWindow
{
    /// <summary>
    /// 用于提示信息的公共窗体
    /// </summary>
    public partial class ShowMessage : Window
    {
        /// <summary>
        /// 记录窗体打开与关闭的定时器
        /// </summary>
        private System.Windows.Forms.Timer Timer;

        public ShowMessage(string strMsg)
        {
            InitializeComponent();
            InitTimer();
            ShowInfo(strMsg);
        }

        /// <summary>
        /// 初始化定时器
        /// </summary>
        private void InitTimer()
        {
            Timer = new System.Windows.Forms.Timer();
            Timer.Interval = 1000;
            Timer.Tick += Timer_Tick;
            Timer.Enabled = true;
        }

        /// <summary>
        /// 显示具体内容
        /// </summary>
        private void ShowInfo(string strMsg)
        {
            this.tbShowMessage.Text = strMsg;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            this.Hide();
            Timer.Enabled = false;
        }
    }
}
