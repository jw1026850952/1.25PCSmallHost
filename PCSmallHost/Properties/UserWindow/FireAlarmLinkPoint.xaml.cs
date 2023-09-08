using System;
using System.Collections.Generic;
using System.IO;
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

namespace PCSmallHost.UserWindow
{
    /// <summary>
    /// 真实火灾联动图标
    /// </summary>
    public partial class FireAlarmLinkPoint : UserControl
    {
        public FireAlarmLinkPoint()
        {
            InitializeComponent();
            InitGifFile();
        }

        /// <summary>
        /// 初始化Gif动画
        /// </summary>
        private void InitGifFile()
        {
            string strGifFilePath = string.Format("{0}\\GifFilePath\\RealLinkage.gif", System.Windows.Forms.Application.StartupPath);
            //(this.winFireAlarmLinkPoint.Child as System.Windows.Forms.PictureBox).Image = PCSmallHost.Util.CommonFunct.ConvertGifFileToImage(strGifFilePath);
            //(this.winFireAlarmLinkPoint.Child as System.Windows.Forms.PictureBox).SendToBack();
        }
    }
}
