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
    /// 系统数字键盘
    /// </summary>
    public partial class KeyBoard : Window
    {
        /// <summary>
        /// 文本框内容
        /// </summary>
        public string TextBoxContent;

        public KeyBoard(string strContent)
        {
            InitializeComponent();
            InitDataSource(strContent);
        }

        /// <summary>
        /// 初始化数据源
        /// </summary>
        private void InitDataSource(string strContent)
        {
            TextBoxContent = strContent;
            this.tbContent.Text = TextBoxContent;
            this.tbContent.SelectionStart = TextBoxContent.Length;
        }

        /// <summary>
        /// 退出界面
        /// </summary>
        private void Exit(bool isConfirm)
        {
            if(!isConfirm)
            {
                this.tbContent.Clear();
            }
            TextBoxContent = this.tbContent.Text != string.Empty ? this.tbContent.Text : "0";
            this.Close();           
        }

        /// <summary>
        /// 回退输入的值
        /// </summary>
        private void SpaceBack()
        {
            int selectionStart = this.tbContent.SelectionStart;//获取鼠标聚焦的位置
            int selectionLength = this.tbContent.SelectionLength;
            string strContent = this.tbContent.Text.Trim();//Trim() 移除所有前导空白字符和尾部空白字符
            if (strContent != string.Empty)
            {
                if (selectionLength != 0)
                {
                    strContent = strContent.Remove(selectionStart, selectionLength);              
                }
                else
                {
                    selectionStart--;
                    if (selectionStart != -1)
                    {
                        strContent = strContent.Remove(selectionStart, 1);
                    }  
                }

                this.tbContent.Text = strContent;
                if (selectionStart != -1)
                {
                    this.tbContent.SelectionStart = selectionStart;
                }
            }
        }

        /// <summary>
        /// 获取输入的值
        /// </summary>
        /// <param name="Button"></param>
        private void GetNumber(Button Button)
        {
            int selectionStart = this.tbContent.SelectionStart;
            int selectionLength = this.tbContent.SelectionLength;
            string strContent = this.tbContent.Text.Trim();
            if(selectionLength != 0)
            {
                strContent = strContent.Remove(selectionStart, selectionLength);
                strContent = strContent.Insert(selectionStart, Button.Content.ToString());                
            }
            else
            {
                strContent = strContent.Insert(selectionStart, Button.Content.ToString());
                selectionStart++;
            }
            selectionStart++;
            this.tbContent.Text = strContent;
            this.tbContent.SelectionStart = selectionStart;
        }

        private void btnNumber_Click(object sender, RoutedEventArgs e)
        {
            GetNumber(sender as Button);
        }

        private void btnSpaceBack_Click(object sender, RoutedEventArgs e)
        {
            SpaceBack();
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            this.tbContent.Clear();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            Exit(true);
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            Exit(false);
        }
    }
}
