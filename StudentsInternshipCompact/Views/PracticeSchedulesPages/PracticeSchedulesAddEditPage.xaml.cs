using StudentsInternship.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

namespace StudentsInternship.Views.PracticeSchedulesPages
{
    /// <summary>
    /// Логика взаимодействия для PracticeSchedulesAddEditPage.xaml
    /// </summary>
    public partial class PracticeSchedulesAddEditPage : Page
    {
        private PracticeSchedules currentElem = new PracticeSchedules();
        public PracticeSchedulesAddEditPage(PracticeSchedules elemData)
        {
            InitializeComponent();

            if (elemData != null)
            {
                btnSaveAndNew.Visibility = Visibility.Collapsed;
                Title = "Расписание практик. Редактирование";
                currentElem = elemData;
            }
            else
            {
                currentElem.PracticeStartDate = DateTime.Today;
                currentElem.PracticeEndDate = DateTime.Today;
            }
            DataContext = currentElem;

            dpStartDate.SelectedDate = DateTime.Today;
            dpStartDate.DisplayDateStart = new DateTime(1950, 1, 1);
            dpStartDate.DisplayDateEnd = DateTime.Today;
            dpEndDate.SelectedDate = DateTime.Today;
            dpEndDate.DisplayDateStart = new DateTime(1950, 1, 1);

            cmbxGroup.ItemsSource = App.Context.Groups.ToList();
            cmbPracticeSubject.ItemsSource = App.Context.PracticeSubjects.ToList();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            // Check if fields are filled
            StringBuilder err = new StringBuilder();
            if (cmbxGroup.SelectedItem == null)
                err.AppendLine("Укажите группу");
            if (cmbPracticeSubject.SelectedItem == null)
                err.AppendLine("Укажите тему практики");
            if (currentElem.PracticeStartDate == null)
                err.AppendLine("Укажите дату начала");
            if (currentElem.PracticeEndDate == null)
                err.AppendLine("Укажите дату окончания");
            if (currentElem.PracticeStartDate > currentElem.PracticeEndDate)
                err.AppendLine("Дата не может быть больше даты окончания");



            if (err.Length > 0)
            {
                MessageBox.Show(err.ToString(), "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }


            if (currentElem.ID == 0)
            {
                App.Context.PracticeSchedules.Add(currentElem);
            }

            try
            {
                App.Context.SaveChanges();
                MessageBox.Show("Данные сохранены");
                var wnd = Window.GetWindow(this);
                wnd.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }
        private void BtnSaveAndNew_Click(object sender, RoutedEventArgs e)
        {
            // Check if fields are filled
            StringBuilder err = new StringBuilder();
            if (cmbxGroup.SelectedItem == null)
                err.AppendLine("Укажите группу");
            if (cmbPracticeSubject.SelectedItem == null)
                err.AppendLine("Укажите тему практики");
            if (currentElem.PracticeStartDate == null)
                err.AppendLine("Укажите дату начала");
            if (currentElem.PracticeEndDate == null)
                err.AppendLine("Укажите дату окончания");
            if (currentElem.PracticeStartDate > currentElem.PracticeEndDate)
                err.AppendLine("Дата не может быть больше даты окончания");


            if (err.Length > 0)
            {
                MessageBox.Show(err.ToString(), "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }


            App.Context.PracticeSchedules.Add(currentElem);

            try
            {
                App.Context.SaveChanges();
                MessageBox.Show("Данные сохранены");
                currentElem = new PracticeSchedules();
                currentElem.PracticeStartDate = DateTime.Today;
                currentElem.PracticeEndDate = DateTime.Today;
                DataContext = currentElem;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }
    }
}

