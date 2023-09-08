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
using PCSmallHost.DB.BLL;
using PCSmallHost.DB.Model;
using PCSmallHost.Util;

namespace PCSmallHost.UserWindow
{
    /// <summary>
    /// 分区设置
    /// </summary>
    public partial class PartitionSet : Window
    {
        /// <summary>
        /// 所有分区设置
        /// </summary>
        private List<FireAlarmPartitionSetInfo> LstFireAlarmPartitionSet;
        private CFireAlarmPartitionSet ObjFireAlarmPartitionSet = new CFireAlarmPartitionSet();

        public PartitionSet(List<FireAlarmPartitionSetInfo> LstFireAlarmPartitionSetInPut)
        {
            InitializeComponent();
            InitDataSource(LstFireAlarmPartitionSetInPut);
            LoadFireAlarmLinkDeviceClass();
        }

        /// <summary>
        /// 加载火灾报警器设备类型数据
        /// </summary>
        private void LoadFireAlarmLinkDeviceClass()
        {
            EnumClass.FireAlarmLinkDeviceClass FireAlarmDeviceClass = new EnumClass.FireAlarmLinkDeviceClass();
            ((System.Windows.Controls.DataGridComboBoxColumn)(dgPartitionSet.Columns[3])).ItemsSource = Enum.GetValues(FireAlarmDeviceClass.GetType());
        }

        /// <summary>
        /// 初始化数据源
        /// </summary>
        private void InitDataSource(List<FireAlarmPartitionSetInfo> LstFireAlarmPartitionSetInPut)
        {
            LstFireAlarmPartitionSet = LstFireAlarmPartitionSetInPut;
            LstFireAlarmPartitionSet.Sort();
            LstFireAlarmPartitionSet.Add(new FireAlarmPartitionSetInfo());
            this.dgPartitionSet.ItemsSource = LstFireAlarmPartitionSet;
        }

        /// <summary>
        /// 刷新数据源
        /// </summary>
        private void RefreshDataSource()
        {
            this.dgPartitionSet.ItemsSource = null;
            this.dgPartitionSet.ItemsSource = LstFireAlarmPartitionSet;
        }

        /// <summary>
        /// 改变单元格的值
        /// </summary>
        private bool ChangeCellsValue()
        {
            DataGridCellInfo DataGridCellInfo = this.dgPartitionSet.CurrentCell;
            if (DataGridCellInfo.Column == null || DataGridCellInfo.Column == this.dgPartitionSet.Columns[3] || this.dgPartitionSet.SelectedIndex == -1)
            {
                return false;
            }

            string strSortMemberPath = DataGridCellInfo.Column.SortMemberPath;
            FireAlarmPartitionSetInfo infoFireAlarmPartitionSet = LstFireAlarmPartitionSet[this.dgPartitionSet.SelectedIndex];

            KeyBoard KeyBoard = new KeyBoard(infoFireAlarmPartitionSet.GetType().GetProperty(strSortMemberPath).GetValue(infoFireAlarmPartitionSet).ToString());
            KeyBoard.ShowDialog();
           
            infoFireAlarmPartitionSet.GetType().GetProperty(strSortMemberPath).SetValue(infoFireAlarmPartitionSet, Convert.ToInt32(KeyBoard.TextBoxContent));
            return true;
        }

        /// <summary>
        /// 关闭分区设置页面
        /// </summary>
        private void ClosePartitionSetPage()
        {
            this.Close();
        }

        /// <summary>
        /// 删除分区设置选中的数据
        /// </summary>
        private bool DeletePartitionSet()
        {
            if (this.dgPartitionSet.SelectedIndex == -1)
            {
                return false;                
            }

            MessageBoxResult MessageBoxResult = System.Windows.MessageBox.Show("你是否确认删除选中的分区设置数据？", "提示", MessageBoxButton.YesNo);
            if (MessageBoxResult == MessageBoxResult.No)
            {
                return false;                
            }

            FireAlarmPartitionSetInfo infoFireAlarmPartitionSet = LstFireAlarmPartitionSet[this.dgPartitionSet.SelectedIndex];
            LstFireAlarmPartitionSet.Remove(infoFireAlarmPartitionSet);
            ObjFireAlarmPartitionSet.Delete(infoFireAlarmPartitionSet.ID);            
            return true;
        }

        /// <summary>
        /// 确认分区设置数据修改
        /// </summary>
        private void ConfirmPartitionSet()
        {
            if (LstFireAlarmPartitionSet.Count != 0)
            {
                MessageBoxResult MessageBoxResult = System.Windows.MessageBox.Show("是否确认修改分区设置？", "提示", MessageBoxButton.YesNo);
                if (MessageBoxResult == MessageBoxResult.Yes)
                {
                    ObjFireAlarmPartitionSet.Save(LstFireAlarmPartitionSet);
                }
            }
        }

        /// <summary>
        /// 分区设置改变行选择
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgPartitionSet_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.dgPartitionSet.SelectedIndex == LstFireAlarmPartitionSet.Count - 1)
            {
                LstFireAlarmPartitionSet.Add(new FireAlarmPartitionSetInfo());
                this.dgPartitionSet.ItemsSource = null;
                this.dgPartitionSet.ItemsSource = LstFireAlarmPartitionSet;
            }
        }

        private void dgPartitionSet_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            bool isSuccess = ChangeCellsValue();
            if (isSuccess)
            {              
                RefreshDataSource();
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            bool isSuccess = DeletePartitionSet();
            if(isSuccess)
            {
                RefreshDataSource();
            }
        }

        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            ConfirmPartitionSet();
        }

        private void btnReturn_Click(object sender, RoutedEventArgs e)
        {
            ClosePartitionSetPage();
        }

        private void dgPartitionSet_TouchDown(object sender, TouchEventArgs e)
        {
            bool isSuccess = ChangeCellsValue();
            if (isSuccess)
            {
                RefreshDataSource();
            }
        }
    }
}
