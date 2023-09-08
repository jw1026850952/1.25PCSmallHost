using PCSmallHost.GraphicalFunction.ViewModel;
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

namespace PCSmallHost.UserWindow
{
    /// <summary>
    /// UserControlMenuItem.xaml 的交互逻辑
    /// </summary>
    public partial class UserControlMenuItem : UserControl
    {
        MainWindow _context;
        public UserControlMenuItem(ItemMenu itemMenu, MainWindow context)
        {
            InitializeComponent();

            _context = context;

            ExpanderMenu.Visibility = itemMenu.SubItems == null ? Visibility.Hidden : Visibility.Visible;
            ExpanderMenu.Header = itemMenu.Header;
            //ListViewItemMenu.Visibility = itemMenu.SubItems == null ? Visibility.Visible : Visibility.Hidden;

            this.DataContext = itemMenu;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _context.SwitchScreen((sender as Button).Content.ToString());
        }
    }
}
