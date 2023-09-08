using PCSmallHost.DB.BLL;
using PCSmallHost.DB.Model;
using PCSmallHost.Util;
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
    /// Login1.xaml 的交互逻辑
    /// </summary>
    public partial class Login1 : Window
    {
        /// <summary>
        /// 定时刷新复位进度条值
        /// </summary>
        private System.Windows.Forms.Timer RefreshProgressBarValueTimer;
        /// <summary>
        /// 复位定时器间隔
        /// </summary>
        private int TimerInterval = 4000;
        /// <summary>
        /// 当前复位进度条值
        /// </summary>
        private double CurrentProgressBarValue;
        /// <summary>
        /// 执行复位指令休眠时间
        /// </summary>
        private int ExeInstructSleepTime = 50;

        /// <summary>
        /// 输入密码次数
        /// </summary>
        private int InputPassWordTime = 3;
        /// <summary>
        /// 所有的系统参数
        /// </summary>
        private List<GblSettingInfo> LstGblSetting;
        /// <summary>
        /// 是否登录
        /// </summary>
        private int VerifyClass;
        /// <summary>
        /// 是否成功验证
        /// </summary>
        public bool IsSuccessCheck;

        public Login1(List<GblSettingInfo> LstGblSettingInput, int VerifyClassInput)
        {
            InitializeComponent();
            InitTimer();
            InitDataSource(LstGblSettingInput, VerifyClassInput);
        }

        private void InitTimer()
        {
            RefreshProgressBarValueTimer = new System.Windows.Forms.Timer();
            RefreshProgressBarValueTimer.Interval = TimerInterval;
            RefreshProgressBarValueTimer.Enabled = true;
        }
        /// <summary>
        /// 点击返回按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnLoginReturn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// 点击OK按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnLoginConfirm_Click(object sender, RoutedEventArgs e)
        {
            LoginConfirm();
        }


        /// <summary>
        /// 点击数字按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnPassWord_Click(object sender, RoutedEventArgs e)
        {
            string strClickNum = (sender as Label).Content.ToString();
            this.LoginInput.Password += strClickNum;
        }

        /// <summary>
        /// 登录验证
        /// </summary>
        private void LoginConfirm()
        {
            GblSettingInfo infoGblSetting = null;
            CGblSetting ObjGblSetting = new CGblSetting();
            LstGblSetting = ObjGblSetting.GetAll();
            if (VerifyClass == (int)EnumClass.VerifyClass.登录验证 || VerifyClass == (int)EnumClass.VerifyClass.退出验证)
            {
                infoGblSetting = LstGblSetting.Find(x => x.Key == "UserPassWord");
            }
            else
            {
                infoGblSetting = LstGblSetting.Find(x => x.Key == "ManagerPassWord");
            }


            string strPassWord = CommonFunct.Md5Encrypt(this.LoginInput.Password);
            if (infoGblSetting != null && strPassWord == infoGblSetting.SetValue)
            {
                IsSuccessCheck = true;
                if (VerifyClass == (int)EnumClass.VerifyClass.复位验证)
                {
                    this.ResetProgress.Visibility = System.Windows.Visibility.Visible;
                    //PCSmallHost.MainWindow.ResetSystemNoLogin();
                    this.cvsMainWindow.IsEnabled = false;
                    RefreshProgressBarValueTimer_Tick();
                    this.ResetProgress.Visibility = System.Windows.Visibility.Hidden;
                    this.cvsMainWindow.IsEnabled = true;
                }
                this.Close();
            }
            else
            {
                InputPassWordTime--;
                if (InputPassWordTime != 0)
                {
                    CommonFunct.PopupWindow(string.Format("密码错误！请重新输入，您还有{0}次机会！", InputPassWordTime)); this.LoginInput.Clear();
                }
                else
                {
                    this.Close();
                }
            }
        }
        private void StopRefreshProgressBarValueTimer()
        {
            RefreshProgressBarValueTimer.Enabled = false;
        }

        private void RefreshProgressBarValueTimer_Tick()
        {
            StopRefreshProgressBarValueTimer();
            RefreshProgressBarValue();

            CurrentProgressBarValue = 0.0;
            RefreshProgressBarValueTimer.Enabled = true;
        }

        /// <summary>
        /// 刷新复位进度条值
        /// </summary>
        private void RefreshProgressBarValue()
        {
            while (CurrentProgressBarValue < this.pgbResetSystem.Maximum)
            {
                System.Windows.Forms.Application.DoEvents();
                System.Threading.Thread.Sleep(ExeInstructSleepTime);

                CurrentProgressBarValue++;
                this.pgbResetSystem.Value = CurrentProgressBarValue;
            }
        }

        /// <summary>
        /// 初始化数据源
        /// </summary>
        private void InitDataSource(List<GblSettingInfo> LstGblSettingInput, int VerifyClassInput)
        {
            LstGblSetting = LstGblSettingInput;
            VerifyClass = VerifyClassInput;
            if (VerifyClass == (int)EnumClass.VerifyClass.登录验证 || VerifyClass == (int)EnumClass.VerifyClass.退出验证)
            {
                this.labTitle.Content = "请输入密码！您共有3次机会";
                this.imgLogo.Source = new BitmapImage(new Uri("\\Pictures\\LoginLogo1.jpg", UriKind.Relative));
            }
            else
            {
                if(VerifyClass == (int)EnumClass.VerifyClass.复位验证)
                {
                    this.LoginImage.Source = new BitmapImage(new Uri("/Pictures/ResetBackground.png", UriKind.Relative));
                }
                else
                {
                    this.LoginImage.Source = new BitmapImage(new Uri("\\Pictures\\CompulsoryEmergencyLogin.png", UriKind.Relative));
                }
                this.labTitle.Content = "请确认你的身份，输入权限密码";
                this.imgLogo.Source = new BitmapImage(new Uri("\\Pictures\\Authority.png", UriKind.Relative));
            }
        }

        private void btnPassWord_Click(object sender, MouseButtonEventArgs e)
        {
            if (this.LoginInput.Password.Count() < 6)
            {
                string strClickNum = (sender as Label).Content.ToString();
                this.LoginInput.Password += strClickNum;
            }
        }

        private void btnLoginConfirm_Click(object sender, MouseButtonEventArgs e)
        {
            LoginConfirm();
        }

        private void btnLoginReturn_Click(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

    }
}

