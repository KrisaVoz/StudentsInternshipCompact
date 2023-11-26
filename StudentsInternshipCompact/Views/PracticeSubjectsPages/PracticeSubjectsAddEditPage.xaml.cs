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

namespace StudentsInternship.Views.PracticeSubjectsPages
{
    /// <summary>
    /// Логика взаимодействия для PracticeSubjectsAddEditPage.xaml
    /// </summary>
    public partial class PracticeSubjectsAddEditPage : Page
    {
        private PracticeSubjects currentElem = new PracticeSubjects();
        public PracticeSubjectsAddEditPage(PracticeSubjects elemData)
        {
            InitializeComponent();

            if (elemData != null)
            {
                btnSaveAndNew.Visibility = Visibility.Collapsed;
                Title = "Тематики практики. Редактирование";
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
            if (string.IsNullOrWhiteSpace(currentElem.PracticeSubjectName))
                err.AppendLine("Укажите название тематики");

            if (err.Length > 0)
            {
                MessageBox.Show(err.ToString(), "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            currentElem.PracticeSubjectName = currentElem.PracticeSubjectName.Trim();
            currentElem.PracticeSubjectName = Regex.Replace(currentElem.PracticeSubjectName, @"\s+", " ");

            if (currentElem.ID == 0)
            {
                App.Context.PracticeSubjects.Add(currentElem);
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
            if (string.IsNullOrWhiteSpace(currentElem.PracticeSubjectName))
                err.AppendLine("Укажите название тематики");

            if (err.Length > 0)
            {
                MessageBox.Show(err.ToString(), "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            currentElem.PracticeSubjectName = currentElem.PracticeSubjectName.Trim();
            currentElem.PracticeSubjectName = Regex.Replace(currentElem.PracticeSubjectName, @"\s+", " ");

            App.Context.PracticeSubjects.Add(currentElem);

            try
            {
                App.Context.SaveChanges();
                MessageBox.Show("Данные сохранены");
                currentElem = new PracticeSubjects();
                DataContext = currentElem;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }
    }
}

