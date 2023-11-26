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

namespace StudentsInternship.Views.GroupsPages
{
    /// <summary>
    /// Логика взаимодействия для GroupsAddEditPage.xaml
    /// </summary>
    public partial class GroupsAddEditPage : Page
    {
        private Groups currentElem = new Groups();
        public GroupsAddEditPage(Groups elemData)
        {
            InitializeComponent();

            if (elemData != null)
            {
                btnSaveAndNew.Visibility = Visibility.Collapsed;
                Title = "Группы. Редактирование";
                currentElem = elemData;
            }
            DataContext = currentElem;

            cmbEducationalOrganisation.ItemsSource = App.Context.EducationalOrganisations.ToList();
            cmbxSpeciality.ItemsSource = App.Context.Specialties.ToList();
        }
        //__REGEXES START

        private void GroupNumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {

            Regex regex = new Regex(@"^[а-яА-ЯёЁa-zA-Z0-9-.]");
            e.Handled = !regex.IsMatch(e.Text);
        }
        private void GroupNumberPastingHandler(object sender, DataObjectPastingEventArgs e)
        {
            Regex regex = new Regex(@"^[а-яА-ЯёЁa-zA-Z0-9-.]");
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
            if (string.IsNullOrWhiteSpace(currentElem.GroupNumberName))
                err.AppendLine("Укажите номер группы");
            if (cmbxSpeciality.SelectedItem == null)
                err.AppendLine("Укажите специальность");
            if (cmbEducationalOrganisation.SelectedItem == null)
                err.AppendLine("Укажите образовательную организацию");

            if (err.Length > 0)
            {
                MessageBox.Show(err.ToString(), "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            currentElem.GroupNumberName = currentElem.GroupNumberName.Trim();
            currentElem.GroupNumberName = Regex.Replace(currentElem.GroupNumberName, @"\s+", " ");

            if (currentElem.ID == 0)
            {
                App.Context.Groups.Add(currentElem);
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
            if (string.IsNullOrWhiteSpace(currentElem.GroupNumberName))
                err.AppendLine("Укажите номер группы");
            if (cmbxSpeciality.SelectedItem == null)
                err.AppendLine("Укажите специальность");
            if (cmbEducationalOrganisation.SelectedItem == null)
                err.AppendLine("Укажите образовательную организацию");

            if (err.Length > 0)
            {
                MessageBox.Show(err.ToString(), "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            currentElem.GroupNumberName = currentElem.GroupNumberName.Trim();
            currentElem.GroupNumberName = Regex.Replace(currentElem.GroupNumberName, @"\s+", " ");

            App.Context.Groups.Add(currentElem);

            try
            {
                App.Context.SaveChanges();
                MessageBox.Show("Данные сохранены");
                currentElem = new Groups();
                DataContext = currentElem;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }
    }
}

