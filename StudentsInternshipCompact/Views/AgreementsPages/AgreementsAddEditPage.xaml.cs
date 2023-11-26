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

namespace StudentsInternship.Views.AgreementsPages
{
    /// <summary>
    /// Логика взаимодействия для AgreementsAddEditPage.xaml
    /// </summary>
    public partial class AgreementsAddEditPage : Page
    {
        private Agreements currentElem = new Agreements();

        bool isEditing = false;
        string agreementNumberBefore = string.Empty;
        public List<AgreementSpeciality> agreementSpecialityList = new List<AgreementSpeciality>();
        public List<Specialties> datagridList = new List<Specialties>();
        public List<Specialties> specialtisAvailableList = App.Context.Specialties.ToList();
        public AgreementsAddEditPage(Agreements elemData)
        {
            InitializeComponent();

            if (elemData != null)
            {
                isEditing = true;
                btnSaveAndNew.Visibility = Visibility.Collapsed;
                Title = "Договоры. Редактирование";
                currentElem = elemData;
                agreementNumberBefore = elemData.AgreementNumber;
                txtAgreementNumber.IsEnabled = false;

                agreementSpecialityList = App.Context.AgreementSpeciality.Where(p => p.Agreements.AgreementNumber == currentElem.AgreementNumber).ToList();
                foreach (var item in agreementSpecialityList)
                    datagridList.Add(item.Specialties);
            }
            else
            {
                currentElem.DateOfAgreement = DateTime.Today;
                currentElem.AgreementStartDate = DateTime.Today;
                currentElem.AgreementEndDate = DateTime.Today;
            }
            DataContext = currentElem;

            
            dpAgreementDate.DisplayDateStart = new DateTime(1950, 1, 1);
            dpAgreementDate.DisplayDateEnd = DateTime.Today;
            dpStartDate.DisplayDateStart = new DateTime(1950, 1, 1);
            dpStartDate.DisplayDateEnd = DateTime.Today;
            dpEndDate.DisplayDateStart = new DateTime(1950, 1, 1);

            cmbAgreementTypes.ItemsSource = App.Context.AgreementTypes.ToList();
            CheckSpecialities();

        }

        #region Regexes

        private void TextValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex(@"^[а-яА-ЯёЁa-zA-Z""()0-9''<>.,/\№#«»-]+$");
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
        private void AgreementNumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex(@"^[0-9#№«»""]");
            e.Handled = !regex.IsMatch(e.Text);
        }
        private void AgreementNumberPastingHandler(object sender, DataObjectPastingEventArgs e)
        {
            Regex regex = new Regex(@"^[0-9#№«»""]");
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
        private void NameValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex(@"^[а-яА-ЯёЁa-zA-Z.]");
            e.Handled = !regex.IsMatch(e.Text);
        }
        private void NamePastingHandler(object sender, DataObjectPastingEventArgs e)
        {
            Regex regex = new Regex(@"^[а-яА-ЯёЁa-zA-Z.]");
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
        #endregion Regexes

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (datagridSpecilities.SelectedItems.Count > 0)
            {
                var elemsToDelete = datagridSpecilities.SelectedItems.Cast<Specialties>().ToList();
                if (MessageBox.Show($"Вы точно хотите удалить следующие {elemsToDelete.Count()} элементов?", "Внимание",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    foreach (var item in elemsToDelete)
                    {
                        datagridList.Remove(item);
                        foreach (var elem in agreementSpecialityList)
                        {
                            if (elem.Specialties == item)
                                App.Context.AgreementSpeciality.Remove(elem);
                        }
                    }
                    try
                    {
                        MessageBox.Show("Данные удалены!");
                        CheckSpecialities();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message.ToString());
                    }
                }
            }
        }
        private void CheckSpecialities()
        {
            specialtisAvailableList = App.Context.Specialties.ToList();
            if (datagridList.Count > 0)
            {
                foreach (var item in datagridList)
                {
                    if (specialtisAvailableList.Contains(item))
                    {
                        specialtisAvailableList.Remove(item);
                    }
                }
            }
            if (cmbSpecialities.SelectedItem != null)
            {
                var specialitySelected = (Specialties)cmbSpecialities.SelectedItem;
                if (!datagridList.Any(p => p.AgreementSpeciality == specialitySelected.AgreementSpeciality))
                {
                    datagridList.Add(specialitySelected);
                }
            }

            cmbSpecialities.ItemsSource = specialtisAvailableList;
            cmbSpecialities.SelectedIndex = -1;

            datagridSpecilities.ItemsSource = datagridList;
            datagridSpecilities.Items.Refresh();
        }
        private void cmbSpecialities_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CheckSpecialities();
        }
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            // Check if fields are filled
            StringBuilder err = new StringBuilder();
            if (string.IsNullOrWhiteSpace(currentElem.AgreementNumber))
                err.AppendLine("Укажите номер договора");
            else if (App.Context.Agreements.Any(p => p.AgreementNumber == currentElem.AgreementNumber) && agreementNumberBefore != currentElem.AgreementNumber)
                err.AppendLine("Номер договора не может повторяться");
            if (cmbAgreementTypes.SelectedItem == null)
                err.AppendLine("Укажите тип договора");
            if (currentElem.DateOfAgreement == null)
                err.AppendLine("Укажите дату заключения");
            if (currentElem.DateOfAgreement > DateTime.Now)
                err.AppendLine("Дата заключения больше сегоднешней");
            if (currentElem.AgreementStartDate == null && cmbAgreementTypes.SelectedIndex != 3)
                err.AppendLine("Укажите дату начала действия договора");
            if (currentElem.AgreementEndDate == null && cmbAgreementTypes.SelectedIndex != 3)
                err.AppendLine("Укажите дату окончания действия договора");
            if (currentElem.AgreementEndDate < currentElem.AgreementStartDate)
                err.AppendLine("Дата окончания меньше даты начала действия");
            if (string.IsNullOrWhiteSpace(currentElem.CompanyName))
                err.AppendLine("Укажите название компании");
            if (string.IsNullOrWhiteSpace(currentElem.ContactPerson))
                err.AppendLine("Укажите контактное лицо");
            if (string.IsNullOrWhiteSpace(currentElem.ContactNumber))
                err.AppendLine("Укажите контактный номер");
            if (string.IsNullOrWhiteSpace(currentElem.CompanyLegalAddress))
                err.AppendLine("Укажите юридический адрес");

            if (err.Length > 0)
            {
                MessageBox.Show(err.ToString(), "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            currentElem.CompanyName = currentElem.CompanyName.Trim();
            currentElem.CompanyName = Regex.Replace(currentElem.CompanyName, @"\s+", " ");

            currentElem.ContactPerson = currentElem.ContactPerson.Trim();
            currentElem.ContactPerson = Regex.Replace(currentElem.ContactPerson, @"\s+", " ");

            currentElem.ContactNumber = currentElem.ContactNumber.Trim();
            currentElem.ContactNumber = Regex.Replace(currentElem.ContactNumber, @"\s+", " ");

            currentElem.CompanyLegalAddress = currentElem.CompanyLegalAddress.Trim();
            currentElem.CompanyLegalAddress = Regex.Replace(currentElem.CompanyLegalAddress, @"\s+", " ");

            if (currentElem.Remark != null)
            {
                currentElem.Remark = currentElem.Remark.Trim();
                currentElem.Remark = Regex.Replace(currentElem.Remark, @"\s+", " ");
            }

            if (!isEditing)
            {
                App.Context.Agreements.Add(currentElem);
            }

            try
            {
                App.Context.SaveChanges();
                foreach (var item in datagridList)
                {
                    if (!agreementSpecialityList.Any(p => p.Specialties == item))
                    {
                        AgreementSpeciality agreementspeciality = new AgreementSpeciality();
                        agreementspeciality.SpecialityID = item.SpecialityNumber;
                        agreementspeciality.Specialties = item;
                        agreementspeciality.AgreementID = currentElem.AgreementNumber;
                        agreementspeciality.DateOfAdding = null;
                        App.Context.AgreementSpeciality.Add(agreementspeciality);
                    }
                }
                App.Context.SaveChanges();
                MessageBox.Show("Данные сохранены");
                var wnd = Window.GetWindow(this);
                wnd.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
                if (ex.InnerException != null)
                    MessageBox.Show(ex.InnerException.ToString());
            }
        }
        private void BtnSaveAndNew_Click(object sender, RoutedEventArgs e)
        {
            // Check if fields are filled
            StringBuilder err = new StringBuilder();
            if (string.IsNullOrWhiteSpace(currentElem.AgreementNumber))
                err.AppendLine("Укажите номер договора");
            else if (App.Context.Agreements.Any(p => p.AgreementNumber == currentElem.AgreementNumber) && agreementNumberBefore != currentElem.AgreementNumber)
                err.AppendLine("Номер договора не может повторяться");
            if (cmbAgreementTypes.SelectedItem == null)
                err.AppendLine("Укажите тип договора");
            if (currentElem.DateOfAgreement == null)
                err.AppendLine("Укажите дату заключения");
            if (currentElem.DateOfAgreement > DateTime.Now)
                err.AppendLine("Дата заключения больше сегоднешней");
            if (currentElem.AgreementStartDate == null && cmbAgreementTypes.SelectedIndex != 3)
                err.AppendLine("Укажите дату начала действия договора");
            if (currentElem.AgreementEndDate == null && cmbAgreementTypes.SelectedIndex != 3)
                err.AppendLine("Укажите дату окончания действия договора");
            if (currentElem.AgreementEndDate < currentElem.AgreementStartDate)
                err.AppendLine("Дата окончания меньше даты начала действия");
            if (string.IsNullOrWhiteSpace(currentElem.CompanyName))
                err.AppendLine("Укажите название компании");
            if (string.IsNullOrWhiteSpace(currentElem.ContactPerson))
                err.AppendLine("Укажите контактное лицо");
            if (string.IsNullOrWhiteSpace(currentElem.ContactNumber))
                err.AppendLine("Укажите контактный номер");
            if (string.IsNullOrWhiteSpace(currentElem.CompanyLegalAddress))
                err.AppendLine("Укажите юридический адрес");

            if (err.Length > 0)
            {
                MessageBox.Show(err.ToString(), "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            currentElem.CompanyName = currentElem.CompanyName.Trim();
            currentElem.CompanyName = Regex.Replace(currentElem.CompanyName, @"\s+", " ");

            currentElem.ContactPerson = currentElem.ContactPerson.Trim();
            currentElem.ContactPerson = Regex.Replace(currentElem.ContactPerson, @"\s+", " ");

            currentElem.ContactNumber = currentElem.ContactNumber.Trim();
            currentElem.ContactNumber = Regex.Replace(currentElem.ContactNumber, @"\s+", " ");

            currentElem.CompanyLegalAddress = currentElem.CompanyLegalAddress.Trim();
            currentElem.CompanyLegalAddress = Regex.Replace(currentElem.CompanyLegalAddress, @"\s+", " ");

            if (!string.IsNullOrWhiteSpace(currentElem.Remark))
            {
                currentElem.Remark = currentElem.Remark.Trim();
                currentElem.Remark = Regex.Replace(currentElem.Remark, @"\s+", " ");
            }


            App.Context.Agreements.Add(currentElem);

            try
            {
                App.Context.SaveChanges();
                MessageBox.Show("Данные сохранены");
                
                foreach (var item in datagridList)
                {
                    AgreementSpeciality agreementspeciality = new AgreementSpeciality();
                    agreementspeciality.SpecialityID = item.SpecialityNumber;
                    agreementspeciality.AgreementID = currentElem.AgreementNumber;
                    App.Context.AgreementSpeciality.Add(agreementspeciality);
                }
                datagridList = new List<Specialties>();
                specialtisAvailableList = App.Context.Specialties.ToList();
                datagridSpecilities.ItemsSource = datagridList;
                datagridSpecilities.Items.Refresh();
                cmbSpecialities.ItemsSource = specialtisAvailableList;
                cmbSpecialities.Items.Refresh();
                currentElem = new Agreements();
                currentElem.DateOfAgreement = DateTime.Today;
                currentElem.AgreementStartDate = DateTime.Today;
                currentElem.AgreementEndDate = DateTime.Today;
                DataContext = currentElem;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }
    }
}
