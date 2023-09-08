using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
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
        private List<EscapeLinesInfo> LstCurrentEscapeLines = new List<EscapeLinesInfo>();
        private CPlanPartitionPointRecord ObjPlanPartitionPointRecord = new CPlanPartitionPointRecord();

        public ControlPartitionPoint(PlanPartitionPointRecordInfo infoPlanPartitionPointRecord, List<FireAlarmPartitionSetInfo> LstFireAlarmPartitionSet,List<EscapeLinesInfo> LstEscapeLines)
        {
            InitializeComponent();
            InitDataSource(infoPlanPartitionPointRecord, LstFireAlarmPartitionSet,LstEscapeLines);
        }

        /// <summary>
        /// 初始化数据源
        /// </summary>
        private void InitDataSource(PlanPartitionPointRecordInfo infoPlanPartitionPointRecord, List<FireAlarmPartitionSetInfo> LstFireAlarmPartitionSet,List<EscapeLinesInfo> LstEscapeLine)
        {
            InfoPlanPartitionPointRecord = infoPlanPartitionPointRecord;
            LstCurrentEscapeLines = LstEscapeLine;
            List<int> LstPartitionPoint = LstFireAlarmPartitionSet.Select(x => x.PlanPartition).Distinct<int>().OrderBy(x => x).ToList<int>();
            this.cbxSetPartitionPoint.ItemsSource = LstPartitionPoint;
            this.cbxSetPartitionPoint.SelectedIndex = LstPartitionPoint.FindIndex(x => x == infoPlanPartitionPointRecord.PlanPartition);
            List<string> LstEscapeLineName = new List<string>();
            for(int i=0;i< LstCurrentEscapeLines.Count;i++)
            {
                LstEscapeLineName.Add(LstCurrentEscapeLines[i].Name);
            }
            this.cbxSetEscapeLine.ItemsSource = LstEscapeLineName;
            this.cbxSetEscapeLine.SelectedIndex = 0;
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

        private void btnBinding_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.OnStartRemoveSelectedEscapeLine();
            EscapeLinesInfo infoEscapeLine = LstCurrentEscapeLines.Find(x => x.Name == this.cbxSetEscapeLine.SelectedItem.ToString());
            InfoPlanPartitionPointRecord.EscapeLineID = infoEscapeLine.ID;
            //ObjPlanPartitionPointRecord.Update(InfoPlanPartitionPointRecord);
            //this.Close();
            UpdatePartitionPoint();
        }

        private void LineName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string LineName = this.cbxSetEscapeLine.SelectedItem.ToString();
            MainWindow.OnSartShowSelectEscapeLine(LineName);
        }

        //private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        //{
        //    string LineName = this.cbxSetEscapeLine.SelectedItem.ToString();
        //    MainWindow.OnSartShowSelectEscapeLine(LineName);
        //}

        //private void Window_Loaded(object sender, RoutedEventArgs e)
        //{
        //    string LineName = this.cbxSetEscapeLine.SelectedItem.ToString();
        //    MainWindow.OnSartShowSelectEscapeLine(LineName);
        //}
    }
}
