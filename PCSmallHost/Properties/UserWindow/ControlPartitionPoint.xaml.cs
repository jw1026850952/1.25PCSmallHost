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
using PCSmallHost.DB.Model;
using PCSmallHost.DB.BLL;

namespace PCSmallHost.UserWindow
{
    /// <summary>
    /// 报警器控制
    /// </summary>
    public partial class ControlPartitionPoint : Window
    {
        private PlanPartitionPointRecordInfo InfoPlanPartitionPointRecord;
        private CPlanPartitionPointRecord ObjPlanPartitionPointRecord = new CPlanPartitionPointRecord();

        public ControlPartitionPoint(PlanPartitionPointRecordInfo infoPlanPartitionPointRecord, List<FireAlarmPartitionSetInfo> LstFireAlarmPartitionSet)
        {
            InitializeComponent();
            InitDataSource(infoPlanPartitionPointRecord, LstFireAlarmPartitionSet);
        }

        /// <summary>
        /// 初始化数据源
        /// </summary>
        private void InitDataSource(PlanPartitionPointRecordInfo infoPlanPartitionPointRecord, List<FireAlarmPartitionSetInfo> LstFireAlarmPartitionSet)
        {
            InfoPlanPartitionPointRecord = infoPlanPartitionPointRecord;
            List<int> LstPartitionPoint = LstFireAlarmPartitionSet.Select(x => x.PlanPartition).Distinct<int>().OrderBy(x => x).ToList<int>();
            this.cbxSetPartitionPoint.ItemsSource = LstPartitionPoint;
            this.cbxSetPartitionPoint.SelectedIndex = LstPartitionPoint.FindIndex(x => x == infoPlanPartitionPointRecord.PlanPartition);
        }

        /// <summary>
        /// 修改报警点
        /// </summary>
        private void UpdatePartitionPoint()
        {
            InfoPlanPartitionPointRecord.PlanPartition = Convert.ToInt32(this.cbxSetPartitionPoint.SelectedItem);
            ObjPlanPartitionPointRecord.Update(InfoPlanPartitionPointRecord);
            this.Close();
        }

        /// <summary>
        /// 修改报警点的预案
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            UpdatePartitionPoint();
        }

        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult MessageBoxResult = System.Windows.MessageBox.Show("是否要移除该图标？", "提示", MessageBoxButton.YesNo);
            if (MessageBoxResult == MessageBoxResult.Yes)
            {
                this.Close();
                MainWindow.OnStartRemovePartitionPoint(InfoPlanPartitionPointRecord);
            }
        }
    }
}
