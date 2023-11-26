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
using ClosedXML.Excel;

namespace StudentsInternship.Windows.ToolWindows
{
    /// <summary>
    /// Логика взаимодействия для WorksheetSelectWindow.xaml
    /// </summary>
    public partial class WorksheetSelectWindow : Window
    {
        public WorksheetSelectWindow(IXLWorksheets worksheetsFromExcel)
        {
            InitializeComponent();
            var worksheets = worksheetsFromExcel;
            cmbWorksheetSelection.ItemsSource = worksheets;
        }
        public int ResponseIndex
        {
            get { return cmbWorksheetSelection.SelectedIndex + 1; }
            set { cmbWorksheetSelection.SelectedIndex = value; }
        }

        private void OKButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (MessageBox.Show($"Вы уверены, что хотите выбрать этот лист?",
                "Внимание", MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.Yes)
            {
                DialogResult = true;
            }
            else
                DialogResult = false;
        }
        private void CancelButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (MessageBox.Show("Вы уверены, что хотите выйти? Процесс извлечения данных не начнётся", "Внимание",
    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                DialogResult = false;
                Close();
            }
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
            if (MessageBox.Show("Вы уверены, что хотите выйти? Процесс извлечения данных не начнётся", "Внимание",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                DialogResult = false;
                Close();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
