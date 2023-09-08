using PCSmallHost.ConSysProtocol;
using PCSmallHost.DB.BLL;
using PCSmallHost.DB.Model;
using PCSmallHost.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PCSmallHost.UserWindow
{
    /// <summary>
    /// AdvancedSettings.xaml 的交互逻辑
    /// </summary>
    public partial class AdvancedSettings : Window
    {
        CGblSetting ObjGblSetting = new CGblSetting();

        public AdvancedSettings()
        {
            InitializeComponent();
            InitDataSource();
        }

        private void InitDataSource()
        {
            List<GblSettingInfo> LstGblSetting = ObjGblSetting.GetAll();
            GblSettingInfo infoGblSetting = LstGblSetting.Find(x => x.Key == "BackUpIntervalTime");

            #region 初始化备份间隔时间
            ObservableCollection<string> ComIntervalTime = new ObservableCollection<string>();//记录已经连接的转折点的下标
            ComIntervalTime.Add("12小时");
            ComIntervalTime.Add("1天");
            ComIntervalTime.Add("5天");
            ComIntervalTime.Add("10天");
            ComIntervalTime.Add("20天");
            ComIntervalTime.Add("1月");

            this.IntervalTime.ItemsSource = ComIntervalTime;
            int index = -1;
            for(int i = 0; i < ComIntervalTime.Count; i++)
            {
                if(infoGblSetting.SetValue == ComIntervalTime[i])
                {
                    index = i;
                    break;
                }
            }
            this.IntervalTime.SelectedIndex = index;
            #endregion

            this.EmergencySwitch.Checked -= EmergencySwitch_Checked;
            this.EmergencySwitch.Unchecked -= EmergencySwitch_Unchecked;
            this.EmergencySwitch.IsChecked = Convert.ToBoolean(LstGblSetting.Find(x => x.Key == "IsBreakComEmergency").SetValue);
            this.EmergencySwitch.Checked += EmergencySwitch_Checked;
            this.EmergencySwitch.Unchecked += EmergencySwitch_Unchecked;

            this.SaveAddress.Text = LstGblSetting.Find(x => x.Key == "DataSavePath").SetValue;

            this.IsAutomaticBackups.Checked -= IsAutomaticBackups_Checked;
            this.IsAutomaticBackups.IsChecked = Convert.ToBoolean(LstGblSetting.Find(x => x.Key == "IsAutomaticBackups").SetValue);
            this.IsAutomaticBackups.Checked += IsAutomaticBackups_Checked;
        }

        private void btnSelectPath_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();
            dialog.Description = "请选择文件夹";
            dialog.SelectedPath = "C:\\";
            if(dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string path = dialog.SelectedPath;
                if (!path.Contains("backup"))
                {
                    string newPath = path + @"\backup";
                    if(!Directory.Exists(newPath))
                    {
                        //创建backup文件夹
                        Directory.CreateDirectory(newPath);
                    }
                    else
                    {
                        path = newPath;
                    }
                }
                if ((sender as Button).Name == "btnBackUpPath") this.SaveAddress.SelectedText = path;
                else if((sender as Button).Name == "btnManual") this.ManualPath.SelectedText = path;
            }
        }

        private void btnRestorePath_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog()
            {
                Filter = "Files(*.db)|*.db"
            };
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                this.FilePath.SelectedText = dialog.FileName;
            }
        }

        private void EmergencySwitch_Checked(object sender, RoutedEventArgs e)
        {
            if (Protocol.OpenEmergencySwitch())
            {
                CommonFunct.PopupWindow("成功开启断通讯应急功能!");
            }
            else
            {
                this.EmergencySwitch.Unchecked -= EmergencySwitch_Unchecked;
                this.EmergencySwitch.IsChecked = false;
                this.EmergencySwitch.Unchecked += EmergencySwitch_Unchecked;
                CommonFunct.PopupWindow("开启失败!");
            }
        }

        private void EmergencySwitch_Unchecked(object sender, RoutedEventArgs e)
        {
            if (Protocol.CloseEmergencySwitch())
            {
                CommonFunct.PopupWindow("成功关闭断通讯应急功能!");
            }
            else
            {
                this.EmergencySwitch.Checked -= EmergencySwitch_Checked;
                this.EmergencySwitch.IsChecked = true;
                this.EmergencySwitch.Checked += EmergencySwitch_Checked;
                CommonFunct.PopupWindow("关闭失败!");
            }
        }

        private void IsAutomaticBackups_Checked(object sender, RoutedEventArgs e)
        {
            if(this.IntervalTime.SelectedIndex == -1 || this.SaveAddress.Text == " ")
            {
                this.IsAutomaticBackups.IsChecked = false;
                CommonFunct.PopupWindow("请选择备份间隔时间以及文件存储路径!");
            }
            else
            {
                BackUpData(this.SaveAddress);//数据备份
            }
        }

        private void btnSaveManually_Click(object sender, RoutedEventArgs e)
        {
            if(this.ManualPath.Text != null)
            {
                BackUpData(this.ManualPath);//数据备份
            }
            else
            {
                CommonFunct.PopupWindow("请选择保存路径!");
            }
        }

        private void btnRestoreData_Click(object sender, RoutedEventArgs e)
        {
            if(this.FilePath.Text != null)
            {
                RestoreBackUpData();//恢复数据
            }
            else
            {
                CommonFunct.PopupWindow("请选择数据文件!");
            }
        }

        /// <summary>
        /// 备份数据
        /// </summary>
        private void BackUpData(TextBox textbox)
        {
            string appStartupPath = System.IO.Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
            string dbFilePath = appStartupPath + @"\PCSmallHost.db";
            string dbNewFilePath = textbox.Text + @"\PCSmallHostBackUp" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".db";

            try
            {
                File.Copy(dbFilePath, dbNewFilePath);//拷贝数据库文件
                if (File.Exists(dbNewFilePath))
                {
                    CommonFunct.PopupWindow("数据备份完成！\n 文件路径：\n" + dbNewFilePath);
                }
            }
            catch
            {
                CommonFunct.PopupWindow("数据备份失败！");
            }
        }

        /// <summary>
        /// 恢复数据
        /// </summary>
        private void RestoreBackUpData()
        {
            if (this.FilePath.Text != null)
            {
                //当前数据库文件的路径
                string dbFilePath = System.IO.Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) + @"\PCSmallHost.db";

                //恢复（覆盖）文件
                File.Copy(this.FilePath.Text, dbFilePath, true);//拷贝文件，存在则覆盖
                if (File.GetLastWriteTime(this.FilePath.Text).ToString("yyyyMMddHHmmss") == File.GetLastWriteTime(dbFilePath).ToString("yyyyMMddHHmmss"))
                {
                    //通过比较2个文件的修改日期，进行判断
                    CommonFunct.PopupWindow("恢复备份数据完成！");
                    App.Restartup();//系统重启，使用新数据库运行

                }
                else
                {
                    CommonFunct.PopupWindow("恢复备份数据失败！\n 请手动拷贝文件进行恢复！");
                }
            }
        }

        /// <summary>
        /// 保存高级设置
        /// </summary>
        private void SaveSettings()
        {
            List<GblSettingInfo> LstGblSetting = ObjGblSetting.GetAll();
            GblSettingInfo infoGblSetting = LstGblSetting.Find(x => x.Key == "IsBreakComEmergency");
            infoGblSetting.SetValue = this.EmergencySwitch.IsChecked.ToString();

            infoGblSetting = LstGblSetting.Find(x => x.Key == "IsAutomaticBackups");
            infoGblSetting.SetValue = this.IsAutomaticBackups.IsChecked.ToString();

            infoGblSetting = LstGblSetting.Find(x => x.Key == "BackUpIntervalTime");
            infoGblSetting.SetValue = this.IntervalTime.SelectedValue.ToString();

            infoGblSetting = LstGblSetting.Find(x => x.Key == "DataSavePath");
            infoGblSetting.SetValue = this.SaveAddress.Text;
            ObjGblSetting.Save(LstGblSetting);
        }

        private void btnSettingsOK_Click(object sender, RoutedEventArgs e)
        {
            SaveSettings();
            this.Close();
        }

        private void btnSettingCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnSettingApply_Click(object sender, RoutedEventArgs e)
        {
            SaveSettings();
        }

        private void txt_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //(sender as Label).Background = CommonFunct.GetBrush("#FFFFFF");
            
            this.Automatic.Visibility = System.Windows.Visibility.Visible;
            this.Manual.Visibility = System.Windows.Visibility.Hidden;
        }

        private void ManualMute_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //(sender as Label).Background = CommonFunct.GetBrush("#FFFFFF");
            this.Automatic.Visibility = System.Windows.Visibility.Hidden;
            this.Manual.Visibility = System.Windows.Visibility.Visible;
        }
    }
}
