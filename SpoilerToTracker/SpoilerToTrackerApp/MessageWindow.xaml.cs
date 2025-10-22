using SpoilerToTrackerConverter.SpoilerLog.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
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

namespace SpoilerToTrackerApp
{
    /// <summary>
    /// Interaction logic for MessageWindow.xaml
    /// </summary>
    public partial class MessageWindow : Window
    {
        public MessageWindow(List<DungeonReward> dungeonRewards, string message)
        {
            InitializeComponent();
            SpoilerDataGrid.ItemsSource = dungeonRewards;
        }
        public void Close_Click(object sender, RoutedEventArgs e) 
        { 
            this.Close();
        }

    }
}
