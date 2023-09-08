using PCSmallHost.DB.BLL;
using PCSmallHost.DB.Model;
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
    /// Register.xaml 的交互逻辑
    /// </summary>
    public partial class Register : Window
    {
        /// <summary>
        /// 单个数字的最大值
        /// </summary>
        private int MaxValue = 9;
        /// <summary>
        /// 主机ID长度
        /// </summary>
        private int MyHostIDLength = 11;
        /// <summary>
        /// 申请码长度
        /// </summary>
        private int RequestCodeLength = 4;
        /// <summary>
        /// 验证码长度
        /// </summary>
        private int RegisterCodeLength = 18;
        /// <summary>
        /// 起始年份
        /// </summary>
        private string StartYear = "2017";
        /// <summary>
        /// 比例因子
        /// </summary>
        private int ScaleFactor = 7;
        /// <summary>
        /// 一年12个月
        /// </summary>
        private int MonthFromYear = 12;

        private CGblSetting ObjGblSetting = new CGblSetting();
        private List<GblSettingInfo> LstGblSetting = new List<GblSettingInfo>();

        public Register()
        {
            InitializeComponent();
            InitDataSource();
        }

        /// <summary>
        /// 初始化数据源
        /// </summary>
        private void InitDataSource()
        {
            LstGblSetting = ObjGblSetting.GetAll();
            GblSettingInfo infoGblSetting = LstGblSetting.Find(x => x.Key == "LicenseDate");
            this.labLincenseDate.Content = infoGblSetting.SetValue;//有效日期
            infoGblSetting = LstGblSetting.Find(x => x.Key == "MyHostID");
            if (infoGblSetting.SetValue == string.Empty)
            {
                infoGblSetting.SetValue = GetMyHostID();
                ObjGblSetting.Update(infoGblSetting);
            }
            this.labMyHostID.Content = infoGblSetting.SetValue;
            this.labRequestCode.Content = GetRequestCode();
        }

        /// <summary>
        /// 生成主机ID
        /// </summary>
        /// <returns></returns>
        private string GetMyHostID()
        {
            string strMyHostID = string.Empty;
            Random random = new Random();
            for (int i = 0; i < MyHostIDLength; i++)
            {
                strMyHostID = strMyHostID + random.Next(MaxValue).ToString();
            }
            return strMyHostID;
        }

        /// <summary>
        /// 获取申请码
        /// </summary>
        /// <returns></returns>
        private string GetRequestCode()
        {
            string strRequestCode = string.Empty;
            Random random = new Random();
            for (int i = 0; i < RequestCodeLength; i++)
            {
                strRequestCode = strRequestCode + random.Next(MaxValue).ToString();
            }
            return strRequestCode;
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            string strMyHostID = this.labMyHostID.Content.ToString();
            string strRequestCode = this.labRequestCode.Content.ToString();
            string strRegisterCode = this.tbxRegistCode.Text;
            if (strRegisterCode == string.Empty)
            {
                MessageBox.Show("请填写注册码！", "提示");
                this.tbxRegistCode.Focus();
                return;
            }
            else if (strRegisterCode.Length != RegisterCodeLength)
            {
                MessageBox.Show("注册码长度有误，请重新填写！", "提示");
                this.tbxRegistCode.Focus();
                return;
            }
            else
            {
                int totalMonth;
                int monthSecond;
                int monthFirst = Convert.ToInt32(strRegisterCode.Substring(8, 2));
                monthFirst = monthFirst - Convert.ToInt32(StartYear.Substring(3, 1)) * ScaleFactor;
                if (Convert.ToInt32(strRegisterCode.Substring(1, 1)) != monthFirst)
                {
                    monthSecond = Convert.ToInt32(strRegisterCode.Substring(6, 2));
                    monthSecond = monthSecond - Convert.ToInt32(StartYear.Substring(2, 1)) * ScaleFactor;
                    totalMonth = Convert.ToInt32(string.Format("{0}{1}", monthFirst, monthSecond));
                }
                else
                {
                    monthSecond = Convert.ToInt32(strRegisterCode.Substring(8, 2));
                    monthSecond = monthSecond - Convert.ToInt32(StartYear.Substring(3, 1)) * ScaleFactor;
                    int monthThird = Convert.ToInt32(strRegisterCode.Substring(6, 2));
                    monthThird = monthThird - Convert.ToInt32(StartYear.Substring(2, 1)) * ScaleFactor;
                    totalMonth = Convert.ToInt32(string.Format("{0}{1}{2}", monthFirst, monthSecond, monthThird));
                }


                DateTime dtLicenseDate = Convert.ToDateTime(this.labLincenseDate.Content);
                if (dtLicenseDate != DateTime.MinValue)
                {
                    dtLicenseDate = dtLicenseDate.AddYears(totalMonth / MonthFromYear).AddMonths(totalMonth % MonthFromYear);
                }
                else
                {
                    dtLicenseDate = DateTime.Now.AddYears(totalMonth / MonthFromYear).AddMonths(totalMonth % MonthFromYear);
                }
                GblSettingInfo infoGblSetting = ObjGblSetting.GetAll().Find(x => x.Key == "LicenseDate");
                infoGblSetting.SetValue = dtLicenseDate.ToString();
                ObjGblSetting.Update(infoGblSetting);
                this.Close();
            }
        }
    }
}
