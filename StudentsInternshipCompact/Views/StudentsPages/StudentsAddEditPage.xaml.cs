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

namespace StudentsInternship.Views.StudentsPages
{
    /// <summary>
    /// Логика взаимодействия для StudentsAddEditPage.xaml
    /// </summary>
    public partial class StudentsAddEditPage : Page
    {
        private Students currentElem = new Students();
        public StudentsAddEditPage(Students elemData)
        {
            InitializeComponent();

            if (elemData != null)
            {
                btnSaveAndNew.Visibility = Visibility.Collapsed;
                Title = "Студенты. Редактирование";
                currentElem = elemData;
            }
            DataContext = currentElem;
            
            cmbxGroup.ItemsSource = App.Context.Groups.ToList();
        }
        //__REGEXES START
        private void OnlyTextValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex(@"^[а-яА-ЯёЁa-zA-Z]+$");
            e.Handled = !regex.IsMatch(e.Text);
        }
        private void NamePastingHandler(object sender, DataObjectPastingEventArgs e)
        {
            Regex regex = new Regex(@"^[а-яА-ЯёЁa-zA-Z]");
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
            Regex regex = new Regex(@"^[0-9]");
            e.Handled = !regex.IsMatch(e.Text);
        }
        private void NumberPastingHandler(object sender, DataObjectPastingEventArgs e)
        {
            Regex regex = new Regex(@"^[0-9]");
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
        private void GroupNumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex(@"^[0-9-]");
            e.Handled = !regex.IsMatch(e.Text);
        }
        private void PhoneValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex(@"^[доб0-9(),.+-]");
            e.Handled = !regex.IsMatch(e.Text);
        }
        private void PhonePastingHandler(object sender, DataObjectPastingEventArgs e)
        {
            Regex regex = new Regex(@"^[доб0-9(),.+-]");
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
        private void DoubleValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex(@"^(0|([1-9][0-9]*))(\\.[0-9]+)?$");
            e.Handled = !regex.IsMatch(e.Text);
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
            if (string.IsNullOrWhiteSpace(currentElem.Surname))
                err.AppendLine("Укажите фамилию");
            if (string.IsNullOrWhiteSpace(currentElem.Name))
                err.AppendLine("Укажите имя");
            if (currentElem.Course < 1 || currentElem.Course > 6)
                err.AppendLine("Укажите курс от 1 до 6");
            if (string.IsNullOrWhiteSpace(currentElem.Phone))
                if (MessageBox.Show($"Номер телефона не заполнен\nВы уверены, что хотите продолжить?",
                "Внимание", MessageBoxButton.YesNo, MessageBoxImage.Exclamation) != MessageBoxResult.Yes)
                {
                    return;
                }
            if (cmbxGroup.SelectedItem == null)
                err.AppendLine("Укажите группу");

            if (err.Length > 0)
            {
                MessageBox.Show(err.ToString(), "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (currentElem.Phone != null)
            {
                currentElem.Phone = currentElem.Phone.Trim();
                currentElem.Phone = Regex.Replace(currentElem.Phone, @"\s+", " ");
            }
            if (currentElem.Course == 0)
            {
                currentElem.Course = null;
            }

            if (currentElem.ID == 0)
            {
                App.Context.Students.Add(currentElem);
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
            if (string.IsNullOrWhiteSpace(currentElem.Surname))
                err.AppendLine("Укажите фамилию");
            if (string.IsNullOrWhiteSpace(currentElem.Name))
                err.AppendLine("Укажите имя");
            if (currentElem.Course < 1 || currentElem.Course > 6)
                err.AppendLine("Укажите курс от 1 до 6");
            if (string.IsNullOrWhiteSpace(currentElem.Phone))
                if (MessageBox.Show($"Номер телефона не заполнен\nВы уверены, что хотите продолжить?",
                "Внимание", MessageBoxButton.YesNo, MessageBoxImage.Exclamation) != MessageBoxResult.Yes)
                {
                    return;
                }
            if (cmbxGroup.SelectedItem == null)
                err.AppendLine("Укажите группу");

            if (err.Length > 0)
            {
                MessageBox.Show(err.ToString(), "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (currentElem.Phone != null)
            {
                currentElem.Phone = currentElem.Phone.Trim();
                currentElem.Phone = Regex.Replace(currentElem.Phone, @"\s+", " ");
            }
            if (currentElem.Course == 0)
            {
                currentElem.Course = null;
            }

            App.Context.Students.Add(currentElem);

            try
            {
                App.Context.SaveChanges();
                MessageBox.Show("Данные сохранены");
                currentElem = new Students();
                DataContext = currentElem;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }
    }
}
