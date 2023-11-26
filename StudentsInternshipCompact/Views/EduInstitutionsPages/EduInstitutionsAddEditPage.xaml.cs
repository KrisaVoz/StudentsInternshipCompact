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

namespace StudentsInternship.Views.EduInstitutionsPages
{
    /// <summary>
    /// Логика взаимодействия для EduInstitutionsAddEditPage.xaml
    /// </summary>
    public partial class EduInstitutionsAddEditPage : Page
    {
        private EducationalOrganisations currentElem = new EducationalOrganisations();
        public EduInstitutionsAddEditPage(EducationalOrganisations elemData)
        {
            InitializeComponent();

            if (elemData != null)
            {
                btnSaveAndNew.Visibility = Visibility.Collapsed;
                Title = "Учебные заведения. Редактирование";
                currentElem = elemData;
            }
            DataContext = currentElem;
        }
        //__REGEXES START
        private void OnlyTextValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex(@"^[а-яА-ЯёЁa-zA-Z0-9{""}«»,.()-]+$");
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
            if (string.IsNullOrWhiteSpace(currentElem.Name))
                err.AppendLine("Укажите название заведения");

            if (err.Length > 0)
            {
                MessageBox.Show(err.ToString(), "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            currentElem.Name = currentElem.Name.Trim();
            currentElem.Name = Regex.Replace(currentElem.Name, @"\s+", " ");

            if (currentElem.ID == 0)
            {
                App.Context.EducationalOrganisations.Add(currentElem);
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
            if (string.IsNullOrWhiteSpace(currentElem.Name))
                err.AppendLine("Укажите название заведения");

            if (err.Length > 0)
            {
                MessageBox.Show(err.ToString(), "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            currentElem.Name = currentElem.Name.Trim();
            currentElem.Name = Regex.Replace(currentElem.Name, @"\s+", " ");

            App.Context.EducationalOrganisations.Add(currentElem);

            try
            {
                App.Context.SaveChanges();
                MessageBox.Show("Данные сохранены");
                currentElem = new EducationalOrganisations();
                DataContext = currentElem;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }
    }
}
