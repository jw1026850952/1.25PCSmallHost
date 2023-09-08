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
using System.Windows.Navigation;
using System.Windows.Shapes;
using PCSmallHost.DB.Model;
using PCSmallHost.Util;

namespace PCSmallHost.UserWindow
{
    /// <summary>
    /// 图形界面控制页面
    /// </summary>
    public partial class ControlIconSearchCode : UserControl
    {
        private object InfoDisBoxOrLight;
        private DistributionBoxInfo InfoDistributionBox;
        private LightInfo InfoLight;
        private Image EquipImage;

        public ControlIconSearchCode(DistributionBoxInfo infoDistributionBox, LightInfo infoLight, object infoDisBoxOrLight, Image image)
        {
            InitializeComponent();
            InitDataSource(infoDistributionBox, infoLight,infoDisBoxOrLight,image);
            InitInfo(infoDistributionBox, infoLight);
        }

        /// <summary>
        /// 初始化数据源
        /// </summary>
        private void InitDataSource(DistributionBoxInfo infoDistributionBox, LightInfo infoLight,object infoDisBoxOrLight, Image image)
        {
            this.InfoDisBoxOrLight = infoDisBoxOrLight;
            this.InfoDistributionBox = infoDistributionBox;
            this.InfoLight = infoLight;
            EquipImage = image;
            if (infoLight != null && infoLight.Code.Substring(0,1) != "1" && infoLight.Code.Substring(0,1) != "3" && infoLight.Code.Substring(0,1) != "5")
            {
                this.btnRotate.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                this.btnRotate.Visibility = System.Windows.Visibility.Hidden;
            }

            if(infoLight == null && infoDistributionBox != null)
            {
                this.btnRelation.Visibility = System.Windows.Visibility.Hidden;
            }
            if(infoLight != null)
            {
                this.btnRelation.Visibility = System.Windows.Visibility.Visible;
            }
        }

        /// <summary>
        /// 初始化信息
        /// </summary>
        private void InitInfo(DistributionBoxInfo infoDistributionBox, LightInfo infoLight)
        {
            if(infoLight != null)
            {
                this.labTitle.Content = string.Format("灯码：{0}", infoLight.Code);
            }
            else
            {
                if(InfoDisBoxOrLight is DistributionBoxInfo)
                {
                    this.labTitle.Content = string.Format("EPS码：{0}", infoDistributionBox.Code);
                }
                else
                {
                    if (InfoDisBoxOrLight.ToString() == "配电箱")
                    {
                        this.labTitle.Content = string.Format("EPS码：{0}", 0);
                    }
                    else
                    {
                        this.labTitle.Content = string.Format("灯码：{0}", 0);
                    }
                }
            }
        }

        /// <summary>
        /// 隐藏首页
        /// </summary>
        private void HideFirstPage()
        {
            this.btnRotate.Visibility = this.btnControl.Visibility = this.btnHideFault.Visibility = this.btnRemove.Visibility
            = this.btnDetailInfo.Visibility = System.Windows.Visibility.Hidden;
            this.btnRelation.Visibility = System.Windows.Visibility.Hidden;
            this.btnEdit.Visibility = System.Windows.Visibility.Hidden;
        }

        /// <summary>
        /// 打开控制页面
        /// </summary>
        private void OpenControlPage()
        {
            if(InfoLight != null)
            {
                if (InfoLight.LightClass != (int)EnumClass.LightClass.照明灯 && InfoLight.LightClass != (int)EnumClass.LightClass.双头灯)
                {
                    if(InfoLight.LightClass == (int)EnumClass.LightClass.双向标志灯 || InfoLight.LightClass == (int)EnumClass.LightClass.双向地埋灯)
                    {
                        this.btnSignalLightLeftOpen.Visibility = this.btnSignalLightRightOpen.Visibility
                        = System.Windows.Visibility.Visible;
                    }
                    this.btnSignalLightShine.Visibility = this.btnSignalLightMainEle.Visibility
                    = System.Windows.Visibility.Visible;
                }
                this.btnSignalLightOpen.Visibility = this.btnSignalLightClose.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                this.btnEmergencyByEPS.Visibility = this.btnMainEleByEPS.Visibility = System.Windows.Visibility.Visible;
            }
        }

        /// <summary>
        /// 打开详细信息页面
        /// </summary>
        private void OpenDetailInfoPage()
        {
            string strDetailInfo = string.Empty;
            if (InfoLight != null)
            {
                strDetailInfo = string.Format("编码：{0}\r\n所在EPS：{1}\r\n类型：{2}\r\n位置：{3}\r\n初始状态：{4}\r\n当前状态：{5}\r\n 电池状态: {6}\r\n左预案：{7}，{8}，{9}\r\n右预案：{10}，{11}，{12}", InfoLight.Code, InfoDistributionBox.Code, CommonFunct.GetLightClass(InfoLight), InfoLight.Address, CommonFunct.GetLightStatus(InfoLight, InfoDistributionBox, true), InfoLight.Shield == 0 ? CommonFunct.GetLightStatus(InfoLight, InfoDistributionBox, false) : "正常", CommonFunct.GetLightBatteryStatus(InfoDistributionBox, InfoLight), InfoLight.PlanLeft1, InfoLight.PlanLeft2, InfoLight.PlanLeft3, InfoLight.PlanRight1, InfoLight.PlanRight2, InfoLight.PlanRight3);
            }
            else if(InfoDistributionBox != null)
            {
                strDetailInfo = string.Format("编码：{0}\r\n位置：{1}\r\n当前状态：{2}\r\n预案：{3}，{4}，{5}，{6}，{7}", InfoDistributionBox.Code, InfoDistributionBox.Address, CommonFunct.GetEPSStatus(InfoDistributionBox.Status), InfoDistributionBox.Plan1, InfoDistributionBox.Plan2, InfoDistributionBox.Plan3, InfoDistributionBox.Plan4, InfoDistributionBox.Plan5);
                //strDetailInfo = string.Format("编码：{0}\r\n位置：{1}\r\n当前状态：{2}\r\n预案：{3}，{4}，{5}，{6}，{7}", InfoDistributionBox.Code, InfoDistributionBox.Address, InfoDistributionBox.Status == 0 ? "通讯正常" : "通讯故障", InfoDistributionBox.Plan1, InfoDistributionBox.Plan2, InfoDistributionBox.Plan3, InfoDistributionBox.Plan4, InfoDistributionBox.Plan5);
            }
            else
            {
                if(InfoDisBoxOrLight is BlankIconInfo)
                {
                    BlankIconInfo infoBlankIcon = InfoDisBoxOrLight as BlankIconInfo;
                    strDetailInfo = string.Format("ID：{0}\r\n所在EPS：{1}\r\n类型：{2}\r\n位置：{3}\r\n初始状态：{4}\r\n当前状态：{5}\r\n左预案：{6}，{7}，{8}\r\n右预案：{9}，{10}，{11}", infoBlankIcon.ID, infoBlankIcon.DisboxCode, infoBlankIcon.Type, "安装位置未初始化", (infoBlankIcon.Type == "照明灯" || infoBlankIcon.Type == "双头灯") ? "|全灭|" : "|全亮|", "通信正常", 0, 0, 0, 0, 0, 0);
                }
                else if(InfoDisBoxOrLight.ToString() == "配电箱")
                {
                    strDetailInfo = string.Format("编码：{0}\r\n位置：{1}\r\n当前状态：{2}\r\n预案：{3}，{4}，{5}，{6}，{7}", 0, "安装位置未初始化","通信正常", 0, 0, 0, 0, 0);
                }
                else
                {
                    strDetailInfo = string.Format("编码：{0}\r\n所在EPS：{1}\r\n类型：{2}\r\n位置：{3}\r\n初始状态：{4}\r\n当前状态：{5}\r\n左预案：{6}，{7}，{8}\r\n右预案：{9}，{10}，{11}", 0, 0, InfoDisBoxOrLight.ToString(), "安装位置未初始化",(InfoDisBoxOrLight.ToString() == "照明灯" || InfoDisBoxOrLight.ToString() == "双头灯") ? "|全灭|":"|全亮|", "通信正常", 0, 0, 0, 0, 0, 0);
                }
            }
            this.labDetailInfo.Content = strDetailInfo;
            this.labDetailInfo.Visibility = System.Windows.Visibility.Visible;
        }

        /// <summary>
        /// 移除图标
        /// </summary>
        private void Remove()
        {
            MessageBoxResult MessageBoxResult = System.Windows.MessageBox.Show("是否要移除该图标？", "提示", MessageBoxButton.YesNo);
            if(MessageBoxResult == MessageBoxResult.Yes)
            {
                if(InfoLight != null)
                {
                    MainWindow.OnStartRemoveIconSearchCode(InfoLight);
                }
                else if(InfoDistributionBox != null)
                {
                    MainWindow.OnStartRemoveIconSearchCode(InfoDistributionBox);
                }
                else
                {
                    MainWindow.OnStartRemoveIconSearchCode(InfoDisBoxOrLight);
                }
            }
        }

        /// <summary>
        /// 单灯控制
        /// </summary>
        private void SingleLightControl(EnumClass.SingleLightControlClass SingleLightControlClass)
        {
            if(InfoDisBoxOrLight is DistributionBoxInfo || InfoDisBoxOrLight is LightInfo)
            {
                MainWindow.OnStartSingleLightControl(SingleLightControlClass, InfoLight.Code, InfoDistributionBox.Code);
            }
        }

        /// <summary>
        /// 指定EPS应急或者主电
        /// </summary>
        private void EmergencyOrMainEleByEPS(bool isEmergency)
        {
            if(InfoDisBoxOrLight is DistributionBoxInfo || InfoDisBoxOrLight is LightInfo)
            {
                MainWindow.OnStartEmergencyOrMainEleByEPS(isEmergency, InfoDistributionBox.Code);
            }
        }

        private void btnControl_Click(object sender, RoutedEventArgs e)
        {
            HideFirstPage();
            OpenControlPage();
        }

        private void btnDetailInfo_Click(object sender, RoutedEventArgs e)
        {
            HideFirstPage();
            OpenDetailInfoPage();
        }

        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            Remove();
        }

        private void btnSignalLightOpen_Click(object sender, RoutedEventArgs e)
        {
            if(InfoLight.ErrorTime == string.Empty && InfoLight.IsEmergency == 0)
            {
                SingleLightControl(EnumClass.SingleLightControlClass.全亮);
            }
            int convert = InfoLight.Status & 0X07;
            InfoLight.Status -= convert;
            InfoLight.Status += 7;
        }

        private void btnSignalLightClose_Click(object sender, RoutedEventArgs e)
        {
            if (InfoLight.ErrorTime == string.Empty && InfoLight.IsEmergency == 0)
            {
                SingleLightControl(EnumClass.SingleLightControlClass.全灭);
            }
            int convert = InfoLight.Status & 0X07;
            InfoLight.Status -= convert;
        }

        private void btnSignalLightShine_Click(object sender, RoutedEventArgs e)
        {
            if (InfoLight.ErrorTime == string.Empty && InfoLight.IsEmergency == 0)
            {
                SingleLightControl(EnumClass.SingleLightControlClass.闪);
            }
        }

        private void btnSignalLightMainEle_Click(object sender, RoutedEventArgs e)
        {
            if (InfoLight.ErrorTime == string.Empty && InfoLight.IsEmergency == 0)
            {
                SingleLightControl(EnumClass.SingleLightControlClass.主电);
            }
            int convert = InfoLight.Status & 0X07;
            InfoLight.Status -= convert;
            InfoLight.Status += 7;
        }

        private void btnSignalLightLeftOpen_Click(object sender, RoutedEventArgs e)
        {
            if (InfoLight.ErrorTime == string.Empty && InfoLight.IsEmergency == 0)
            {
                SingleLightControl(EnumClass.SingleLightControlClass.左亮);
                if (InfoLight.LightClass == (int)EnumClass.LightClass.双向标志灯)
                {
                    EquipImage.Source = new BitmapImage(new Uri("\\Pictures\\DoubleMarkerLightLeftOpen.png", UriKind.Relative));
                }
                if (InfoLight.LightClass == (int)EnumClass.LightClass.双向地埋灯)
                {
                    EquipImage.Source = new BitmapImage(new Uri("\\Pictures\\DoubleBuriedLightLeftOpen.png", UriKind.Relative));
                }
                int convert = InfoLight.Status & 0X07;
                if(convert == 0)
                {
                    if ((InfoLight.Status & 0X04) != 0X04)
                    {
                        InfoLight.Status += 4;
                    }
                }
                else
                {
                    InfoLight.Status -= convert;
                    InfoLight.Status += 4;
                }
            }
        }

        private void btnSignalLightRightOpen_Click(object sender, RoutedEventArgs e)
        {
            if (InfoLight.ErrorTime == string.Empty && InfoLight.IsEmergency == 0)
            {
                SingleLightControl(EnumClass.SingleLightControlClass.右亮);
                if (InfoLight.LightClass == (int)EnumClass.LightClass.双向标志灯)
                {
                    EquipImage.Source = new BitmapImage(new Uri("\\Pictures\\DoubleMarkerLightRightOpen.png", UriKind.Relative));
                }
                if (InfoLight.LightClass == (int)EnumClass.LightClass.双向地埋灯)
                {
                    EquipImage.Source = new BitmapImage(new Uri("\\Pictures\\DoubleBuriedLightRightOpen.png", UriKind.Relative));
                }

                int convert = InfoLight.Status & 0X07;
                if (convert == 0)
                {
                    if ((InfoLight.Status & 0X01) != 0X01)
                    {
                        InfoLight.Status += 1;
                    }
                }
                else
                {
                    InfoLight.Status -= convert;
                    InfoLight.Status += 1;
                }
            }
        }

        private void btnEmergencyByEPS_Click(object sender, RoutedEventArgs e)
        {
            EmergencyOrMainEleByEPS(true);
        }

        private void btnMainEleByEPS_Click(object sender, RoutedEventArgs e)
        {
            EmergencyOrMainEleByEPS(false);
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.OnstartUpdateFacPlanInLayer(InfoDistributionBox, InfoLight,InfoDisBoxOrLight);
        }

        private void btnRotate_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.OnstartDeviceIconRotate(InfoDistributionBox, InfoLight);
        }

        private void btnRelation_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.OnStartLampRelationLines(InfoLight,InfoDistributionBox);
        }
    }
}
