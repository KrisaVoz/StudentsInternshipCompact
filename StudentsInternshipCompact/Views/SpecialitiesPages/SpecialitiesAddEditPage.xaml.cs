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
using StudentsInternship.Models;

namespace StudentsInternship.Views.SpecialitiesPages
{
    /// <summary>
    /// Логика взаимодействия для SpecialitiesAddEditPage.xaml
    /// </summary>
    public partial class SpecialitiesAddEditPage : Page
    {
        bool isEditing = false;
        string specialityNumberBefore = string.Empty;

        private Specialties currentElem = new Specialties();
        public SpecialitiesAddEditPage(Specialties elemData)
        {
            InitializeComponent();

            if (elemData != null)
            {
                isEditing = true;
                btnSaveAndNew.Visibility = Visibility.Collapsed;
                Title = "Специальности. Редактирование";
                currentElem = elemData;
                specialityNumberBefore = elemData.SpecialityNumber;
                txtSpecialityNumber.IsEnabled = false;
            }
            DataContext = currentElem;
        }
        //__REGEXES START
        private void OnlyTextValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex(@"^[а-яА-ЯёЁa-zA-Z{""}«»,.()-]+$");
            e.Handled = !regex.IsMatch(e.Text);
        }
        private void TextPastingHandler(object sender, DataObjectPastingEventArgs e)
        {
            Regex regex = new Regex(@"^[а-яА-ЯёЁa-zA-Z""()0-9''<>.,/\№#«»-]+$");
            if (e.DataObject.GetDataPresent(typeof(String)))
            {
                String text = (String)e.DataObject.GetData(typeof(String));
                if (!regex.IsMatch(text))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex(@"^[0-9.]");
            e.Handled = !regex.IsMatch(e.Text);
        }
        private void NumberPastingHandler(object sender, DataObjectPastingEventArgs e)
        {
            Regex regex = new Regex(@"^[0-9.]+$");
            if (e.DataObject.GetDataPresent(typeof(String)))
            {
                String text = (String)e.DataObject.GetData(typeof(String));
                if (!regex.IsMatch(text))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }
        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                e.Handled = true;
            }
        }
        //__REGEXES END
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            // Check if fields are filled
            StringBuilder err = new StringBuilder();
            if (string.IsNullOrWhiteSpace(currentElem.SpecialityNumber))
                err.AppendLine("Укажите номер специальности");
            else if (App.Context.Specialties.Any(p => p.SpecialityNumber == currentElem.SpecialityNumber) && specialityNumberBefore != currentElem.SpecialityNumber)
                err.AppendLine("Номер специальности не может повторяться");
            if (string.IsNullOrWhiteSpace(currentElem.SpecialityName))
                err.AppendLine("Укажите название специальности");

            if (err.Length > 0)
            {
                MessageBox.Show(err.ToString(), "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            currentElem.SpecialityName = currentElem.SpecialityName.Trim();
            currentElem.SpecialityName = Regex.Replace(currentElem.SpecialityName, @"\s+", " ");

            if (!isEditing)
            {
                App.Context.Specialties.Add(currentElem);
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
            if (string.IsNullOrWhiteSpace(currentElem.SpecialityNumber))
                err.AppendLine("Укажите номер специальности");
            else if (App.Context.Specialties.Any(p => p.SpecialityNumber == currentElem.SpecialityNumber))
                err.AppendLine("Номер специальности не может повторяться");
            if (string.IsNullOrWhiteSpace(currentElem.SpecialityName))
                err.AppendLine("Укажите название специальности");

            if (err.Length > 0)
            {
                MessageBox.Show(err.ToString(), "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            currentElem.SpecialityName = currentElem.SpecialityName.Trim();
            currentElem.SpecialityName = Regex.Replace(currentElem.SpecialityName, @"\s+", " ");

            App.Context.Specialties.Add(currentElem);

            try
            {
                App.Context.SaveChanges();
                MessageBox.Show("Данные сохранены");
                currentElem = new Specialties(); 
                DataContext = currentElem;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }
    }
}
