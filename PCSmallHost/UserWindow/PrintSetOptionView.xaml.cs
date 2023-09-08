using PCSmallHost.DB.BLL;
using PCSmallHost.DB.Model;
using System.Collections.Generic;
using System.Windows;

namespace PCSmallHost.UserWindow
{
    /// <summary>
    /// PrintSetOptionView.xaml 的交互逻辑
    /// </summary>
    public partial class PrintSetOptionView : Window
    {
        public PrintSetOptionView()
        {
            InitializeComponent();
        }
        private readonly CFaultPrintSetting faultPrintSetting = new CFaultPrintSetting();
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            List<FaultPrintSettingInfo> faultPrintSettingInfos = faultPrintSetting.GetAll();
            CbHostFaultInfo.IsChecked = faultPrintSettingInfos.Find(it => it.FaultType == 0).IsPrint == 0 ? false : true;
            CbEpsFaultInfo.IsChecked = faultPrintSettingInfos.Find(it => it.FaultType == 1).IsPrint == 0 ? false : true;
            CbLightFaultInfo.IsChecked = faultPrintSettingInfos.Find(it => it.FaultType == 2).IsPrint == 0 ? false : true;
        }
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            int IsPrintHost = CbHostFaultInfo.IsChecked == true ? 1 : 0;
            Update(1, 0, IsPrintHost);
            int IsPrintEps = CbEpsFaultInfo.IsChecked == true ? 1 : 0;
            Update(2, 1, IsPrintEps);
            int IsPrintLight = CbLightFaultInfo.IsChecked == true ? 1 : 0;
            Update(3, 2, IsPrintLight);
        }

        private void Update(int id, int FaultType, int IsPrint)
        {
            FaultPrintSettingInfo faultPrintSettingInfo = new FaultPrintSettingInfo
            {
                Id = id,
                FaultType = FaultType,
                IsPrint = IsPrint
            };
            faultPrintSetting.Update(faultPrintSettingInfo);
            Close();
        }
    }
}
