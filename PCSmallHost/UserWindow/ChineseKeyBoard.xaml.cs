using PCSmallHost.Util;
using PCSmallHost.DB.BLL;
using PCSmallHost.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.International.Converters.PinYinConverter;

namespace PCSmallHost.UserWindow
{
    /// <summary>
    /// 系统中文键盘
    /// </summary>
    public partial class ChineseKeyBoard : Window
    {
        /// <summary>
        /// 文本框内容
        /// </summary>
        public string Address;
        /// <summary>
        /// 拼音
        /// </summary>
        private string Spell;
        /// <summary>
        /// 中文字体颜色
        /// </summary>
        private string ChineseSpellForeground = "#07B6F7";      
        /// <summary>
        /// 是否中文输入
        /// </summary>
        private bool IsChineseInput;
        /// <summary>
        /// 是否大写输入
        /// </summary>
        private bool IsCapsInput;
        /// <summary>
        /// 是否数字输入
        /// </summary>
        //private bool IsNumberInput;
        /// <summary>
        /// 是否结束填写
        /// </summary>
        public bool IsFinishFill;       
        /// <summary>
        /// 数字输入数组索引
        /// </summary>
        private int NumberInputIndex;
        /// <summary>
        /// 中文当前页
        /// </summary>
        private int ChineseSpellCurrentPage = 1;
        /// <summary>
        /// 中文每页数量
        /// </summary>
        private int ChineseSpellPerPageCount = 12;
        /// <summary>
        /// 中文文本框高度
        /// </summary>
        private double LabelChineseSpellBySpellHeight = 50;
        /// <summary>
        /// 中文文本框宽度
        /// </summary>
        private double LabelChineseSpellBySpellWidth = 40;
        /// <summary>
        /// 中文文本框字体大小
        /// </summary>
        private double LabelChineseSpellBySpellFontSize = 25;
        /// <summary>
        /// 数字输入数组
        /// </summary>
        private string[] NumberInput;
        private List<char> ChineseChars = new List<char>();
        private List<int> StrokeNumber = new List<int>();


        public ChineseKeyBoard(string strAddress)
        {
            InitializeComponent();
            //InitDataSource();
            InitAddress(strAddress);
            InitNumberInput();
        }

        /// <summary>
        /// 初始化数字输入
        /// </summary>
        private void InitNumberInput()
        {
            NumberInput = new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "/", "*", "#", "@", "&", "!", "-", "?", "\"" };
        }

        /// <summary>
        /// 初始化地址
        /// </summary>
        /// <param name="strAddress"></param>
        private void InitAddress(string strAddress)
        {
            Address = strAddress;
            this.tbxAddress.Text = Address;
        }

        /// <summary>
        /// 关闭系统中文键盘
        /// </summary>
        private void CloseChineseKeyBoard()
        {
            this.Close();
        }

        /// <summary>
        /// 处理文本内容
        /// </summary>
        /// <param name="sender"></param>
        private void DealTextBoxContent(object sender)
        {
            Button Button = (sender as Button);
            if (Button.Content.ToString().Equals("DEL"))
            {
                if (IsChineseInput && Spell.Length > 0)
                {
                    DeleteSpell();
                    ChineseChars.Clear();
                    StrokeNumber.Clear();
                    GetChineseSpellBySpell();
                    ClearChineseSpellBySpell();
                    LoadChineseSpellBySpell();
                }
                else
                {
                    AddAddress();
                }
            }
            else if (Button.Content.ToString().Equals("TB"))
            {
                ClickTBButton();
            }
            else if (Button.Content.ToString().Equals("OK"))
            {
                GetAddress();
            }
            else if (Button.Content.ToString().Equals("CAPS"))
            {
                ClickCAPSButton(Button);
            }
            else if (Button.Content.ToString().Equals("caps"))
            {
                ClickcapsButton(Button);
            }
            else if (Button.Content.ToString().Equals("123"))
            {
                SwitchNumberMode(Button);
            }
            else if (Button.Content.ToString().Equals("abc"))
            {
                ReturnEnglishMode(Button);
            }
            else if (Button.Content.ToString().Equals("中"))
            {
                SwitchChineseMode(Button);
                ShowSwitchChineseSpellButton();
            }
            else if (Button.Content.ToString().Equals("英"))
            {
                SwitchEnglishMode(Button);
                ClearChineseSpellBySpell();
                HideSwitchChineseSpellButton();
            }
            else if (Button.Content.ToString().Equals("SPACE"))
            {
                ClickSpaceButton();
            }
            else
            {
                if (IsChineseInput)
                {
                    ChineseChars.Clear();
                    StrokeNumber.Clear();
                    AddSpell(Button.Content.ToString());
                    GetChineseSpellBySpell();
                    ClearChineseSpellBySpell();
                    LoadChineseSpellBySpell();
                }
                else
                {
                    Address += Button.Content.ToString();
                }
            }
            this.tbxAddress.Text = Address;
        }

        /// <summary>
        /// 获取根据拼音过滤的中文
        /// </summary>
        private void GetChineseSpellBySpell()
        {
            if (ChineseChar.GetChars(Spell + "1") != null)
            {
                ChineseChars.AddRange(ChineseChar.GetChars(Spell + "1"));
            }
            if (ChineseChar.GetChars(Spell + "2") != null)
            {
                ChineseChars.AddRange(ChineseChar.GetChars(Spell + "2"));
            }
            if (ChineseChar.GetChars(Spell + "3") != null)
            {
                ChineseChars.AddRange(ChineseChar.GetChars(Spell + "3"));
            }
            if (ChineseChar.GetChars(Spell + "4") != null)
            {
                ChineseChars.AddRange(ChineseChar.GetChars(Spell + "4"));
            }
            if (ChineseChar.GetChars(Spell + "5") != null)
            {
                ChineseChars.AddRange(ChineseChar.GetChars(Spell + "5"));
            }

            if (ChineseChars.Count > 0)
            {
                ChineseChars = ChineseChars.Distinct().ToList();

                for (int a = 0; a < ChineseChars.Count; a++)
                {
                    StrokeNumber.Add(ChineseChar.GetStrokeNumber(ChineseChars[a]));
                }


                //插入排序
                for (int i = 1; i < StrokeNumber.Count; i++)
                {
                    int inservalue = StrokeNumber[i];
                    char chinese = ChineseChars[i];
                    int insertindex = i - 1;
                    while (insertindex >= 0 && inservalue < StrokeNumber[insertindex])
                    {
                        StrokeNumber[insertindex + 1] = StrokeNumber[insertindex];
                        ChineseChars[insertindex + 1] = ChineseChars[insertindex];
                        insertindex--;
                    }
                    StrokeNumber[insertindex + 1] = inservalue;
                    ChineseChars[insertindex + 1] = chinese;
                }
            }
        }

        /// <summary>
        /// 清空根据拼音过滤的中文
        /// </summary>
        private void ClearChineseSpellBySpell()
        {
            this.stpChineseBySpell.Children.Clear();
        }

        /// <summary>
        /// 加载根据拼音过滤的中文
        /// </summary>
        private void LoadChineseSpellBySpell()
        {            
            for (int i = (ChineseSpellCurrentPage - 1) * ChineseSpellPerPageCount; i < ChineseSpellCurrentPage * ChineseSpellPerPageCount; i++)
            {
                if(i >= ChineseChars.Count)
                {
                    break;
                }

                Label Label = new Label
                {
                    Height = LabelChineseSpellBySpellHeight,
                    Width = LabelChineseSpellBySpellWidth,
                    Content = ChineseChars[i],
                    FontSize = LabelChineseSpellBySpellFontSize,
                    Foreground = CommonFunct.GetBrush(ChineseSpellForeground),
                    HorizontalAlignment = HorizontalAlignment.Left
                };
                Label.MouseDown += labChineseSpellBySpell_MouseDown;

                if (i % ChineseSpellPerPageCount != 0)
                {
                    Label.Margin = new Thickness((i%12) * LabelChineseSpellBySpellHeight, -LabelChineseSpellBySpellHeight, 0, 0);
                }
                this.stpChineseBySpell.Children.Add(Label);
            }
        }

        /// <summary>
        /// 删除拼音
        /// </summary>
        private void DeleteSpell()
        {
            Spell = Spell.Substring(0, Spell.Length - 1);
            this.labSpell.Content = Spell;
        }

        /// <summary>
        /// 添加拼音
        /// </summary>
        private void AddSpell(string strAddress)
        {
            Spell += strAddress;
            this.labSpell.Content = Spell;
        }

        /// <summary>
        /// 添加地址
        /// </summary>
        private void AddAddress()
        {
            if (Address.Length > 0)
            {
                Address = Address.Substring(0, Address.Length - 1);
            }
        }

        /// <summary>
        /// 切换英文模式
        /// </summary>
        private void SwitchEnglishMode(Button Button)
        {
            IsChineseInput = false;
            Button.Content = "中";          
        }

        /// <summary>
        /// 返回英文模式
        /// </summary>
        /// <param name="sender"></param>
        private void ReturnEnglishMode(Button Button)
        {
            NumberInputIndex = 0;
            //IsNumberInput = false;
            Button.Content = "123";

            for (int i = 0; i < this.stpChineseKeyBoard.Children.Count; i++)
            {
                Button ButtonSub = this.stpChineseKeyBoard.Children[i] as Button;
                if (ButtonSub.Content.ToString().Length > 1)
                {
                    continue;
                }

                ButtonSub.Content = ButtonSub.Tag;
                NumberInputIndex++;

                if (NumberInputIndex == NumberInput.Length)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// 切换数字输入
        /// </summary>
        /// <param name="sender"></param>
        private void SwitchNumberMode(Button Button)
        {
            NumberInputIndex = 0;
            //IsNumberInput = true;
            Button.Content = "abc";
            
            for (int i = 0; i < this.stpChineseKeyBoard.Children.Count; i++)
            {
                Button ButtonSub = this.stpChineseKeyBoard.Children[i] as Button;
                if(ButtonSub.Content.ToString().Length > 1)               
                {
                    continue;                  
                }

                ButtonSub.Tag = ButtonSub.Content;
                ButtonSub.Content = NumberInput[NumberInputIndex];
                NumberInputIndex++;

                if (NumberInputIndex == NumberInput.Length)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// 切换中文模式
        /// </summary>
        /// <param name="sender"></param>
        private void SwitchChineseMode(Button Button)
        {
            IsChineseInput = true;
            Spell = string.Empty;
            this.labSpell.Content = null;

            Button.Content = "英";
            
            if (IsCapsInput)
            {
                foreach (UIElement UIElement in this.stpChineseKeyBoard.Children)
                {
                    Button ButtonSub = UIElement as Button;
                    if (ButtonSub.Content.ToString().Equals("caps"))
                    {
                        DealTextBoxContent(ButtonSub);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// 显示切换中文按钮
        /// </summary>
        private void ShowSwitchChineseSpellButton()
        {
            this.btnLastSpellPage.Visibility = this.btnNextSpellPage.Visibility = System.Windows.Visibility.Visible;
        }

        /// <summary>
        /// 隐藏切换中文按钮
        /// </summary>
        private void HideSwitchChineseSpellButton()
        {
            this.btnLastSpellPage.Visibility = this.btnNextSpellPage.Visibility = System.Windows.Visibility.Hidden;
        }

        /// <summary>
        /// 点击CAPS按钮
        /// </summary>
        private void ClickCAPSButton(Button Button)
        {
            IsCapsInput = true;
            Button.Content = Button.Content.ToString().ToLower();
           
            foreach (UIElement UIElement in this.stpChineseKeyBoard.Children)
            {
                Button ButtonSub = UIElement as Button;
                if (ButtonSub.Content.ToString().Length == 1)
                {
                    ButtonSub.Content = ButtonSub.Content.ToString().ToUpper();
                }
            }
        }

        private void ClickcapsButton(Button Button)
        {
            IsCapsInput = false;
            Button.Content = Button.Content.ToString().ToUpper();
           
            foreach (UIElement UIElement in this.stpChineseKeyBoard.Children)
            {
                Button ButtonSub = UIElement as Button;
                if (ButtonSub.Content.ToString().Length == 1)
                {
                    ButtonSub.Content = ButtonSub.Content.ToString().ToLower();
                }
            }
        }

        /// <summary>
        /// 点击空格按钮
        /// </summary>
        private void ClickSpaceButton()
        {
            Address += " ";
        }

        /// <summary>
        /// 点击TB按钮
        /// </summary>
        private void ClickTBButton()
        {
            Address += "  ";
        }

        /// <summary>
        /// 获取地址
        /// </summary>
        private void GetAddress()
        {
            IsFinishFill = true;
            this.Close();
        }

        /// <summary>
        /// 返回上一页
        /// </summary>
        private bool ReturnLastSpellPage()
        {
            if(ChineseSpellCurrentPage == 1)
            {
                return false;
            }

            ChineseSpellCurrentPage--;
            return true;
        }

        /// <summary>
        /// 进入下一页
        /// </summary>
        private bool EnterLastSpellPage()
        {
            if (ChineseSpellCurrentPage == ChineseChars.Count / ChineseSpellPerPageCount + 1)
            {
                return false;
            }

            ChineseSpellCurrentPage++;
            return true;
        }

        private void labChineseSpellBySpell_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Spell = string.Empty;
            Address += (sender as Label).Content.ToString();

            this.labSpell.Content = null;
            this.tbxAddress.Text = Address;
            this.stpChineseBySpell.Children.Clear();
        }

        private void Alphabet_Click(object sender, RoutedEventArgs e)
        {             
            DealTextBoxContent(sender);
        }

        private void CloseChineseKeyBoard_Click(object sender, RoutedEventArgs e)
        {
            CloseChineseKeyBoard();
        }

        private void btnLastSpellPage_Click(object sender, RoutedEventArgs e)
        {
            bool isSuccess = ReturnLastSpellPage();
            if(isSuccess)
            {
                ClearChineseSpellBySpell();
                LoadChineseSpellBySpell();
            }
        }

        private void btnNextSpellPage_Click(object sender, RoutedEventArgs e)
        {
            bool isSuccess = EnterLastSpellPage();
            if(isSuccess)
            {
                ClearChineseSpellBySpell();
                LoadChineseSpellBySpell();
            }
        }        
    }
}
