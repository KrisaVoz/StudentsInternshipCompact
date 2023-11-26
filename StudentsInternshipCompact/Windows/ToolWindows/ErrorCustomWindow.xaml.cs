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

namespace StudentsInternship.Windows.ToolWindows
{
    /// <summary>
    /// Логика взаимодействия для ErrorCustomWindow.xaml
    /// </summary>
    public partial class ErrorCustomWindow : Window
    {
        public ErrorCustomWindow(string errTitle, string errsMessageText)
        {
            InitializeComponent();
            windowTitle.Text = errTitle;
            ErrorMessage.Text = errsMessageText;
        }
        private void OKButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}

