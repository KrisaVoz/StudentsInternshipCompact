using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ClosedXML.Excel;
using Microsoft.Win32;
using StudentsInternship.Models;
using StudentsInternship.Windows;
using StudentsInternship.Windows.ToolWindows;

namespace StudentsInternship.Views.AgreementsPages
{
    /// <summary>
    /// Логика взаимодействия для AgreementsViewPage.xaml
    /// </summary>
    public partial class AgreementsViewPage : Page
    {
        bool isImporting = false;
        List<Agreements> datagridSourceList = new List<Agreements>();
        private int PagesCount;
        private int NumberOfPage = 0;
        private int maxItemShow = 200;
        public AgreementsViewPage()
        {
            InitializeComponent();

            dtpStartDate.SelectedDate = DateTime.Today;
            dtpStartDate.DisplayDateStart = new DateTime(1950, 1, 1);
            dtpStartDate.DisplayDateEnd = DateTime.Today;
            dtpEndDate.SelectedDate = DateTime.Today;
            dtpEndDate.DisplayDateStart = new DateTime(1950, 1, 1);

            cmbFilter.SelectedIndex = 0;
            UpdateDataGrid();
        }
        //#region Update database on Events
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateDataGrid();
        }
        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateDataGrid();
        }
        private void CBoxFilterBy_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateDataGrid();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            AddEditWindow windowAddEdit = new AddEditWindow();
            windowAddEdit.frameAddEdit.Navigate(new AgreementsAddEditPage(null));
            windowAddEdit.Closed += (s, EventArgs) =>
            {
                UpdateDataGrid();
            };
            windowAddEdit.Show();
        }
        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (viewDataGrid.SelectedItems.Count > 0 && !isImporting)
            {
                var elemsToDelete = viewDataGrid.SelectedItems.Cast<Agreements>().ToList();
                if (MessageBox.Show($"Вы точно хотите удалить следующие {elemsToDelete.Count()} элементов?", "Внимание",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    try
                    {
                        App.Context.Agreements.RemoveRange(elemsToDelete);
                        App.Context.SaveChanges();
                        MessageBox.Show("Данные удалены!");
                        UpdateDataGrid();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message.ToString());
                    }
                }
            }
        }
        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (viewDataGrid.SelectedItems.Count > 0 && !isImporting)
            {
                AddEditWindow windowAddEdit = new AddEditWindow();
                windowAddEdit.frameAddEdit.Navigate(new AgreementsAddEditPage(viewDataGrid.SelectedItem as Agreements));
                windowAddEdit.Closed += (s, EventArgs) =>
                {
                    UpdateDataGrid();
                };
                windowAddEdit.Show();
            }
        }

        public void RejectChanges()
        {
            foreach (var entry in App.Context.ChangeTracker.Entries())
            {
                switch (entry.State)
                {
                    case EntityState.Modified:
                    case EntityState.Deleted:
                        entry.State = EntityState.Modified; //Revert changes made to deleted entity.
                        entry.State = EntityState.Unchanged;
                        break;
                    case EntityState.Added:
                        entry.State = EntityState.Detached;
                        break;
                }
            }
        }
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                App.Context.SaveChanges();
                if (isImporting)
                {
                    MessageBox.Show("Импорт завершён!", "Успех");
                    isImporting = false;
                }
            }
            catch (Exception ex)
            {
                if (isImporting)
                {
                    MessageBox.Show("Импорт не удался!\nОтмена изменений", "Ошибка!");
                    isImporting = false;
                }

                MessageBox.Show(ex.Message);
                if (ex.InnerException != null)
                    MessageBox.Show(ex.InnerException.ToString());
                RejectChanges();
            }
        }

        //DataGrid Fill 
        private void UpdateDataGrid()
        {
            cmbdtAgreementType.ItemsSource = App.Context.AgreementTypes.ToList();

            datagridSourceList = App.Context.Agreements.ToList();
            // Filtration
            switch (cmbFilter.SelectedIndex)
            {
                case 0:
                    break;
                case 1:
                    datagridSourceList = datagridSourceList.Where(p => p.IsRegistrationRequired == true).ToList();
                    break;
                case 2:
                    datagridSourceList = datagridSourceList.Where(p => p.IsRegistrationRequired == false).ToList();
                    break;
                default:
                    break;
            }
            //Search
            datagridSourceList = datagridSourceList.Where(p => p.CompanyName.ToLower().Contains(txtSearch.Text.ToLower()) ||
                p.AgreementNumber.ToLower().Contains(txtSearch.Text.ToLower()) ||
                p.ContactPerson.ToLower().Contains(txtSearch.Text.ToLower()) ||
                p.ContactNumber.ToLower().Contains(txtSearch.Text.ToLower()) ||
                p.Remark.ToLower().Contains(txtSearch.Text.ToLower())
            ).ToList();
            //Date filtration
            if (dtpStartDate.SelectedDate != DateTime.Today || dtpEndDate.SelectedDate != DateTime.Today)
            {
                datagridSourceList = datagridSourceList.Where(p => p.AgreementEndDate >= dtpStartDate.SelectedDate && p.AgreementEndDate <= dtpEndDate.SelectedDate).ToList();
            }

            //Items Counter
            tbItemCounter.Text = datagridSourceList.Count.ToString() + " из " + App.Context.Agreements.Count().ToString();

            //Pages Counter
            if (datagridSourceList.Count % maxItemShow == 0)
            {
                PagesCount = datagridSourceList.Count / maxItemShow;
            }
            else
            {
                PagesCount = (datagridSourceList.Count / maxItemShow) + 1;
            }
            blockPagingControls.Visibility = Visibility.Visible;
            tbPageCounter.Text = $"Страница {NumberOfPage + 1} из {PagesCount}";

            //Empty DataGrid Alert
            if (PagesCount < 1)
            {
                blockPagingControls.Visibility = Visibility.Collapsed;
            }

            //Sorting
            Sorting();

            viewDataGrid.ItemsSource = datagridSourceList.Skip(maxItemShow * NumberOfPage).Take(maxItemShow).ToList();
            CheckPages();

            //Empty DataGrid Alert
            if (datagridSourceList.Count < 1)
                tbItemCounter.Text = "Ничего не найдено. Измените фильтры.";
        }

        #region Pagination controls
        private void PaginationMove()
        {
            viewDataGrid.ItemsSource = datagridSourceList.Skip(maxItemShow * NumberOfPage).Take(maxItemShow).ToList();
            tbPageCounter.Text = $"Страница {NumberOfPage + 1} из {PagesCount}";
            CheckPages();
        }
        private void CheckPages()
        {
            txtCurrentPage.Text = NumberOfPage + 1 + "";
            // Previous Pages Check
            if (NumberOfPage > 0)
            {
                btnPagingPrevious.IsEnabled = true;
                btnPagingStart.IsEnabled = true;
                btnPagingCurrentMinus1.Content = NumberOfPage;
                btnPagingCurrentMinus1.Visibility = Visibility.Visible;
                btnPagingCurrentMinus2.Visibility = Visibility.Collapsed;
                btnPagingCurrentMinus3.Visibility = Visibility.Collapsed;
                if (NumberOfPage > 1)
                {
                    btnPagingCurrentMinus2.Content = NumberOfPage - 1;
                    btnPagingCurrentMinus2.Visibility = Visibility.Visible;
                    if (NumberOfPage > 2)
                    {
                        btnPagingCurrentMinus3.Content = NumberOfPage - 2;
                        btnPagingCurrentMinus3.Visibility = Visibility.Visible;
                    }
                }
            }
            else
            {
                btnPagingCurrentMinus1.Visibility = Visibility.Collapsed;
                btnPagingCurrentMinus2.Visibility = Visibility.Collapsed;
                btnPagingCurrentMinus3.Visibility = Visibility.Collapsed;
                btnPagingPrevious.IsEnabled = false;
                btnPagingStart.IsEnabled = false;
            }

            // Next Pages Check
            if (NumberOfPage < PagesCount - 1)
            {
                btnPagingNext.IsEnabled = true;
                btnPagingEnd.IsEnabled = true;
                btnPagingCurrentPlus1.Content = NumberOfPage + 2;
                btnPagingCurrentPlus1.Visibility = Visibility.Visible;
                btnPagingCurrentPlus2.Visibility = Visibility.Collapsed;
                btnPagingCurrentPlus3.Visibility = Visibility.Collapsed;
                if (NumberOfPage < PagesCount - 2)
                {
                    btnPagingCurrentPlus2.Content = NumberOfPage + 3;
                    btnPagingCurrentPlus2.Visibility = Visibility.Visible;
                    if (NumberOfPage < PagesCount - 3)
                    {
                        btnPagingCurrentPlus3.Content = NumberOfPage + 4;
                        btnPagingCurrentPlus3.Visibility = Visibility.Visible;
                    }
                }
            }
            else
            {
                btnPagingCurrentPlus1.Visibility = Visibility.Collapsed;
                btnPagingCurrentPlus2.Visibility = Visibility.Collapsed;
                btnPagingCurrentPlus3.Visibility = Visibility.Collapsed;
                btnPagingNext.IsEnabled = false;
                btnPagingEnd.IsEnabled = false;
            }
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex(@"^[0-9]");
            e.Handled = !regex.IsMatch(e.Text);
        }
        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                e.Handled = true;
            }
        }

        private void PaginationButtonStart_Click(object sender, RoutedEventArgs e)
        {
            NumberOfPage = 0;
            PaginationMove();
        }

        private void PaginationButtonPrevious_Click(object sender, RoutedEventArgs e)
        {
            NumberOfPage--;
            PaginationMove();
        }

        private void PaginationButtonCurrentMinus3_Click(object sender, RoutedEventArgs e)
        {
            NumberOfPage -= 3;
            PaginationMove();
        }

        private void PaginationButtonCurrentMinus2_Click(object sender, RoutedEventArgs e)
        {
            NumberOfPage -= 2;
            PaginationMove();
        }

        private void PaginationButtonCurrentMinus1_Click(object sender, RoutedEventArgs e)
        {
            NumberOfPage--;
            PaginationMove();
        }

        private void PaginationButtonCurrentPlus1_Click(object sender, RoutedEventArgs e)
        {
            NumberOfPage++;
            PaginationMove();
        }

        private void PaginationButtonCurrentPlus2_Click(object sender, RoutedEventArgs e)
        {
            NumberOfPage += 2;
            PaginationMove();
        }

        private void PaginationButtonCurrentPlus3_Click(object sender, RoutedEventArgs e)
        {
            NumberOfPage += 3;
            PaginationMove();
        }

        private void PaginationButtonNext_Click(object sender, RoutedEventArgs e)
        {
            NumberOfPage++;
            PaginationMove();
        }

        private void PaginationButtonEnd_Click(object sender, RoutedEventArgs e)
        {
            NumberOfPage = PagesCount - 1;
            PaginationMove();
        }

        private void txtCurrentPage_TextChanged(object sender, RoutedEventArgs e)
        {
            if (!(txtCurrentPage.Text == "" || txtCurrentPage.Text == null))
            {
                int txtValue = Convert.ToInt32(txtCurrentPage.Text);
                txtValue--;
                if (txtValue >= 0 && txtValue < PagesCount)
                {
                    NumberOfPage = txtValue;
                    UpdateDataGrid();
                }
            }
        }
        #endregion Pagination Controls

        #region Sorting
        class SortElem
        {
            public bool SortAscending { get; set; }
            public string SortName { get; set; }

            public DataGridColumn Column { get; set; }

            public SortElem(bool sortAscending, string sortName, DataGridColumn column)
            {
                SortAscending = sortAscending;
                SortName = sortName;
                Column = column;
            }
        }
        private List<SortElem> sort = new List<SortElem>();
        private void AddSortingVar(string varName, DataGridColumn column)
        {
            if (sort.Count == 0 || !sort.Any(x => x.SortName == varName))
            {
                sort.Add(new SortElem(true, varName, column));
            }
            else
            {
                foreach (SortElem elem in sort.ToArray())
                {
                    if (elem.SortName == varName && elem.SortAscending)
                    {
                        elem.SortAscending = false;
                    }
                    else if (elem.SortName == varName && !elem.SortAscending)
                    {
                        sort.Remove(elem);
                    }
                }
            }
            UpdateDataGrid();
        }

        private void DataGridColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            var columnHeader = sender as DataGridColumnHeader;
            switch (columnHeader.Tag)
            {
                case null:
                case "":
                    columnHeader.Tag = "Sorted";
                    break;
                case "Sorted":
                    columnHeader.Tag = "SortedDesc";
                    break;
                case "SortedDesc":
                    columnHeader.Tag = "";
                    break;
            }
            var column = columnHeader.Column;
            if (columnHeader != null)
            {
                switch (column.Header.ToString())
                {
                    case "Номер договора":
                        AddSortingVar("AgreementNumber", column);
                        break;
                    case "Тип":
                        AddSortingVar("AgreementType", column);
                        break;
                    case "Дата заключения":
                        AddSortingVar("DateOfAgreement", column);
                        break;
                    case "Дата начала":
                        AddSortingVar("AgreementStartDate", column);
                        break;
                    case "Дата окончания":
                        AddSortingVar("AgreementEndDate", column);
                        break;
                    case "Пролонгация":
                        AddSortingVar("Prolongation", column);
                        break;
                    case "Название компании":
                        AddSortingVar("CompanyName", column);
                        break;
                    case "ИНН":
                        AddSortingVar("CompanyINN", column);
                        break;
                    case "Адрес":
                        AddSortingVar("CompanyLegalAddress", column);
                        break;
                    case "Контактное лицо":
                        AddSortingVar("ContactPerson", column);
                        break;
                    case "Номер телефона":
                        AddSortingVar("ContactNumber", column);
                        break;
                    case "Прописка":
                        AddSortingVar("IsRegistrationRequired", column);
                        break;
                    case "Примечание":
                        AddSortingVar("Remark", column);
                        break;
                }
            }
        }

        //Sorting Method
        private void Sorting()
        {
            if (sort.Count > 0)
            {
                IOrderedEnumerable<Agreements> SortingList = null;
                foreach (SortElem elem in sort)
                {
                    switch (elem.SortName)
                    {
                        case "AgreementNumber":
                            if (elem.SortAscending)
                            {
                                if (SortingList != null)
                                    SortingList = SortingList.ThenBy(p => p.AgreementNumber);
                                else
                                    SortingList = datagridSourceList.OrderBy(p => p.AgreementNumber);
                                break;
                            }
                            else
                            {
                                if (SortingList != null)
                                    SortingList = SortingList.ThenByDescending(p => p.AgreementNumber);
                                else
                                    SortingList = datagridSourceList.OrderByDescending(p => p.AgreementNumber);
                                break;
                            }
                        case "AgreementType":
                            if (elem.SortAscending)
                            {
                                if (SortingList != null)
                                    SortingList = SortingList.ThenBy(p => p.AgreementTypes.AgreementTypeName);
                                else
                                    SortingList = datagridSourceList.OrderBy(p => p.AgreementTypes.AgreementTypeName);
                                break;
                            }
                            else
                            {
                                if (SortingList != null)
                                    SortingList = SortingList.ThenByDescending(p => p.AgreementTypes.AgreementTypeName);
                                else
                                    SortingList = datagridSourceList.OrderByDescending(p => p.AgreementTypes.AgreementTypeName);
                                break;
                            }
                        case "DateOfAgreement":
                            if (elem.SortAscending)
                            {
                                if (SortingList != null)
                                    SortingList = SortingList.ThenBy(p => p.DateOfAgreement);
                                else
                                    SortingList = datagridSourceList.OrderBy(p => p.DateOfAgreement);
                                break;
                            }
                            else
                            {
                                if (SortingList != null)
                                    SortingList = SortingList.ThenByDescending(p => p.DateOfAgreement);
                                else
                                    SortingList = datagridSourceList.OrderByDescending(p => p.DateOfAgreement);
                                break;
                            }
                        case "AgreementStartDate":
                            if (elem.SortAscending)
                            {
                                if (SortingList != null)
                                    SortingList = SortingList.ThenBy(p => p.AgreementStartDate);
                                else
                                    SortingList = datagridSourceList.OrderBy(p => p.AgreementStartDate);
                                break;
                            }
                            else
                            {
                                if (SortingList != null)
                                    SortingList = SortingList.ThenByDescending(p => p.AgreementStartDate);
                                else
                                    SortingList = datagridSourceList.OrderByDescending(p => p.AgreementStartDate);
                                break;
                            }
                        case "AgreementEndDate":
                            if (elem.SortAscending)
                            {
                                if (SortingList != null)
                                    SortingList = SortingList.ThenBy(p => p.AgreementEndDate);
                                else
                                    SortingList = datagridSourceList.OrderBy(p => p.AgreementEndDate);
                                break;
                            }
                            else
                            {
                                if (SortingList != null)
                                    SortingList = SortingList.ThenByDescending(p => p.AgreementEndDate);
                                else
                                    SortingList = datagridSourceList.OrderByDescending(p => p.AgreementEndDate);
                                break;
                            }
                        case "Prolongation":
                            if (elem.SortAscending)
                            {
                                if (SortingList != null)
                                    SortingList = SortingList.ThenBy(p => p.Prolongation);
                                else
                                    SortingList = datagridSourceList.OrderBy(p => p.Prolongation);
                                break;
                            }
                            else
                            {
                                if (SortingList != null)
                                    SortingList = SortingList.ThenByDescending(p => p.Prolongation);
                                else
                                    SortingList = datagridSourceList.OrderByDescending(p => p.Prolongation);
                                break;
                            }
                        case "CompanyName":
                            if (elem.SortAscending)
                            {
                                if (SortingList != null)
                                    SortingList = SortingList.ThenBy(p => p.CompanyName);
                                else
                                    SortingList = datagridSourceList.OrderBy(p => p.CompanyName);
                                break;
                            }
                            else
                            {
                                if (SortingList != null)
                                    SortingList = SortingList.ThenByDescending(p => p.CompanyName);
                                else
                                    SortingList = datagridSourceList.OrderByDescending(p => p.CompanyName);
                                break;
                            }
                        case "CompanyINN":
                            if (elem.SortAscending)
                            {
                                if (SortingList != null)
                                    SortingList = SortingList.ThenBy(p => p.CompanyINN);
                                else
                                    SortingList = datagridSourceList.OrderBy(p => p.CompanyINN);
                                break;
                            }
                            else
                            {
                                if (SortingList != null)
                                    SortingList = SortingList.ThenByDescending(p => p.CompanyINN);
                                else
                                    SortingList = datagridSourceList.OrderByDescending(p => p.CompanyINN);
                                break;
                            }
                        case "CompanyLegalAddress":
                            if (elem.SortAscending)
                            {
                                if (SortingList != null)
                                    SortingList = SortingList.ThenBy(p => p.CompanyLegalAddress);
                                else
                                    SortingList = datagridSourceList.OrderBy(p => p.CompanyLegalAddress);
                                break;
                            }
                            else
                            {
                                if (SortingList != null)
                                    SortingList = SortingList.ThenByDescending(p => p.CompanyLegalAddress);
                                else
                                    SortingList = datagridSourceList.OrderByDescending(p => p.CompanyLegalAddress);
                                break;
                            }
                        case "ContactPerson":
                            if (elem.SortAscending)
                            {
                                if (SortingList != null)
                                    SortingList = SortingList.ThenBy(p => p.ContactPerson);
                                else
                                    SortingList = datagridSourceList.OrderBy(p => p.ContactPerson);
                                break;
                            }
                            else
                            {
                                if (SortingList != null)
                                    SortingList = SortingList.ThenByDescending(p => p.ContactPerson);
                                else
                                    SortingList = datagridSourceList.OrderByDescending(p => p.ContactPerson);
                                break;
                            }
                        case "ContactNumber":
                            if (elem.SortAscending)
                            {
                                if (SortingList != null)
                                    SortingList = SortingList.ThenBy(p => p.ContactNumber);
                                else
                                    SortingList = datagridSourceList.OrderBy(p => p.ContactNumber);
                                break;
                            }
                            else
                            {
                                if (SortingList != null)
                                    SortingList = SortingList.ThenByDescending(p => p.ContactNumber);
                                else
                                    SortingList = datagridSourceList.OrderByDescending(p => p.ContactNumber);
                                break;
                            }
                        case "IsRegistrationRequired":
                            if (elem.SortAscending)
                            {
                                if (SortingList != null)
                                    SortingList = SortingList.ThenBy(p => p.IsRegistrationRequired);
                                else
                                    SortingList = datagridSourceList.OrderBy(p => p.IsRegistrationRequired);
                                break;
                            }
                            else
                            {
                                if (SortingList != null)
                                    SortingList = SortingList.ThenByDescending(p => p.IsRegistrationRequired);
                                else
                                    SortingList = datagridSourceList.OrderByDescending(p => p.IsRegistrationRequired);
                                break;
                            }
                        case "Remark":
                            if (elem.SortAscending)
                            {
                                if (SortingList != null)
                                    SortingList = SortingList.ThenBy(p => p.Remark);
                                else
                                    SortingList = datagridSourceList.OrderBy(p => p.Remark);
                                break;
                            }
                            else
                            {
                                if (SortingList != null)
                                    SortingList = SortingList.ThenByDescending(p => p.Remark);
                                else
                                    SortingList = datagridSourceList.OrderByDescending(p => p.Remark);
                                break;
                            }
                        case null:
                        case "":
                            SortingList = datagridSourceList.OrderByDescending(p => p.DateOfAgreement);
                            break;
                    }
                }
                datagridSourceList = SortingList.ToList();
            }
            else
            {
                datagridSourceList = datagridSourceList.OrderByDescending(p => p.DateOfAgreement).ToList();
            }
        }

        // Delete filters
        private void deleteFiltersButton_Click(object sender, RoutedEventArgs e)
        {
            dtpStartDate.SelectedDate = DateTime.Today;
            dtpEndDate.SelectedDate = DateTime.Today;
            txtSearch.Text = "";
            cmbFilter.SelectedIndex = 0;
            sort.Clear();
            foreach (DataGridColumn column in viewDataGrid.Columns)
            {
                DataGridColumnHeader header = GetColumnHeaderFromColumn(column);
                header.Tag = "";
            }
            UpdateDataGrid();
        }

        // Properties for tag clear
        private DataGridColumnHeader GetColumnHeaderFromColumn(DataGridColumn column)
        {
            List<DataGridColumnHeader> columnHeaders = GetVisualChildCollection<DataGridColumnHeader>(viewDataGrid);
            foreach (DataGridColumnHeader columnHeader in columnHeaders)
            {
                if (columnHeader.Column == column)
                {
                    return columnHeader;
                }
            }
            return null;
        }

        public List<T> GetVisualChildCollection<T>(object parent) where T : Visual
        {
            List<T> visualCollection = new List<T>();
            GetVisualChildCollection(parent as DependencyObject, visualCollection);
            return visualCollection;
        }

        private void GetVisualChildCollection<T>(DependencyObject parent, List<T> visualCollection) where T : Visual
        {
            int count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                if (child is T)
                {
                    visualCollection.Add(child as T);
                }
                else if (child != null)
                {
                    GetVisualChildCollection(child, visualCollection);
                }
            }
        }
        #endregion Sorting 

        private void dtpStartDate_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateDataGrid();
        }

        private void dtpEndDate_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateDataGrid();
        }
        #region ExcelImport

        //Method to check digits|Метод проверки, что строка содержит только числа
        bool IsAllDigits(string s)
        {
            foreach (char c in s)
            {
                if (!char.IsDigit(c))
                    return false;
            }
            return true;
        }
        private void BtnExcelImport_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog() { Filter = "Excel Workbook|*.xls;*.xlsx", Multiselect = false };
            {
                if (ofd.ShowDialog() == true)
                {
                    try
                    {
                        CultureInfo culture = CultureInfo.CreateSpecificCulture("ru-RU");

                        StringBuilder errs = new StringBuilder();

                        var agreementNumberColumn = 0;
                        var agreementTypeColumn = 0;
                        var dateOfAgreementColumn = 0;
                        var companyNameColumn = 0;
                        var companyAdressColumn = 0;
                        var companyContactPersonColumn = 0;
                        var contactPhoneColumn = 0;
                        var agreementStartDateORColumn = 0;
                        var agreementEndDateColumn = 0;
                        var prolongationColumn = 0;
                        var numberOfPeopleColumn = 0;
                        var remarkColumn = 0;
                        var isRegistrationRequiredColumn = 0;
                        var specialitiesColumn = 0;

                        var addedValues = 0;
                        var updatedValues = 0;
                        var sameValue = 0;

                        using (XLWorkbook workBook = new XLWorkbook(ofd.FileName))
                        {
                            List<Agreements> agreementsAddOrEditedList = new List<Agreements>();
                            var agreementsContext = App.Context.Agreements.ToList();
                            var specialitiesContext = App.Context.Specialties.ToList();
                            var agreementtypesContext = App.Context.AgreementTypes.ToList();
                            var agreementspecialitiesContext = App.Context.AgreementSpeciality.ToList();

                            bool isFirstRow = true;
                            var worksheets = workBook.Worksheets;
                            IXLRows rows = null;
                            if (worksheets.Count == 1)
                            {
                                rows = workBook.Worksheets.First().RowsUsed();
                            }
                            else if (worksheets.Count > 1)
                            {
                                var dialog = new WorksheetSelectWindow(worksheets);
                                if (dialog.ShowDialog() == true)
                                {
                                    rows = workBook.Worksheet(dialog.ResponseIndex).RowsUsed();
                                }
                                else
                                {
                                    return;
                                }
                            }
                            else
                            {
                                MessageBox.Show("Листы не найдены!");
                                return;
                            }


                            foreach (var row in rows)
                            {
                                if (isFirstRow)
                                {
                                    if (row.CellsUsed().Count() > 1)
                                    {
                                        Mouse.OverrideCursor = Cursors.Wait;
                                        isFirstRow = false;
                                        foreach (IXLCell cell in row.Cells())
                                        {
                                            var tempCellValue = cell.Value.ToString().ToLower();
                                            tempCellValue = tempCellValue.Trim();
                                            tempCellValue = Regex.Replace(tempCellValue, @"\s+", " ");


                                            //Check if column is in the table|Проверка есть ли колонки в таблице Excel
                                            if (tempCellValue.Contains("номер") && tempCellValue.Contains("договор"))
                                            {
                                                agreementNumberColumn = cell.Address.ColumnNumber;
                                            }
                                            if (tempCellValue.Contains("тип") && tempCellValue.Contains("договор"))
                                            {
                                                agreementTypeColumn = cell.Address.ColumnNumber;
                                            }
                                            if (tempCellValue == "дата" || (tempCellValue.Contains("дат") && tempCellValue.Contains("заключени")))
                                            {
                                                dateOfAgreementColumn = cell.Address.ColumnNumber;
                                            }
                                            if (tempCellValue.Contains("названи") && tempCellValue.Contains("организац"))
                                            {
                                                companyNameColumn = cell.Address.ColumnNumber;
                                            }
                                            if (tempCellValue.Contains("адрес") && tempCellValue.Contains("юридическ"))
                                            {
                                                companyAdressColumn = cell.Address.ColumnNumber;
                                            }
                                            if ((tempCellValue.Contains("начало") && tempCellValue.Contains("договор")) ||
                                                (tempCellValue.Contains("срок") && tempCellValue.Contains("действия")))
                                            {
                                                agreementStartDateORColumn = cell.Address.ColumnNumber;
                                            }
                                            if ((tempCellValue.Contains("окончание") || tempCellValue.Contains("конец")) && tempCellValue.Contains("договор"))
                                            {
                                                agreementEndDateColumn = cell.Address.ColumnNumber;
                                            }
                                            if (tempCellValue.Contains("пролонгаци"))
                                            {
                                                prolongationColumn = cell.Address.ColumnNumber;
                                            }
                                            if (tempCellValue.Contains("контактн") && tempCellValue.Contains("лиц"))
                                            {
                                                companyContactPersonColumn = cell.Address.ColumnNumber;
                                            }
                                            if (tempCellValue.Contains("контактн") && tempCellValue.Contains("телефон"))
                                            {
                                                contactPhoneColumn = cell.Address.ColumnNumber;
                                            }
                                            if (tempCellValue.Contains("специальност"))
                                            {
                                                specialitiesColumn = cell.Address.ColumnNumber;
                                            }
                                            if ((tempCellValue.Contains("количество") || tempCellValue.Contains("кол-во") || tempCellValue.Contains("число")) &&
                                                    (tempCellValue.Contains("человек") || tempCellValue.Contains("студент") || tempCellValue.Contains("практикант")))
                                            {
                                                numberOfPeopleColumn = cell.Address.ColumnNumber;
                                            }
                                            if (tempCellValue.Contains("примечан") || tempCellValue.Contains("прим."))
                                            {
                                                remarkColumn = cell.Address.ColumnNumber;
                                            }
                                            if (tempCellValue.Contains("прописка") || ((tempCellValue.Contains("временн") || tempCellValue.Contains("постоянн")) && tempCellValue.Contains("регистраци")))
                                            {
                                                isRegistrationRequiredColumn = cell.Address.ColumnNumber;
                                            }

                                        }
                                        string err = String.Empty;

                                        if (agreementTypeColumn == 0)
                                            err += "Не найдена колонка типов договоров\n";
                                        if (dateOfAgreementColumn == 0)
                                            err += "Не найдена колонка дат заключения догоров\n";
                                        if (agreementStartDateORColumn == 0)
                                            err += "Не найдена колонка начала/общих сроков действия договоров\n";
                                        if (agreementEndDateColumn == 0)
                                            err += "Не найдена колонка окончания сроков действия договоров\n";
                                        if (companyAdressColumn == 0)
                                            err += "Не найдена колонка адреса компаний/организаций\n";
                                        if (companyContactPersonColumn == 0)
                                            err += "Не найдена колонка контактных лиц компаний/организаций\n";
                                        if (contactPhoneColumn == 0)
                                            err += "Не найдена колонка контактных телефонов компаний/организаций\n";
                                        if (prolongationColumn == 0)
                                            err += "Не найдена колонка данных о пролонгации договоров\n";
                                        if (numberOfPeopleColumn == 0)
                                            err += "Не найдена колонка количества человек/практикантов\n";
                                        if (specialitiesColumn == 0)
                                            err += "Не найдена колонка специальностей\n";
                                        if (remarkColumn == 0)
                                            err += "Не найдена колонка примечаний\n";
                                        if (isRegistrationRequiredColumn == 0)
                                            err += "Не найдена колонка о необходимости прописки\n";

                                        if (agreementNumberColumn == 0)
                                        {
                                            err += "Не найдена колонка номеров договоров. Импорт невозможен\n";
                                            errs.Append(err);
                                            return;
                                        }
                                        if (companyNameColumn == 0)
                                        {
                                            err += "Не найдена колонка названий компаний. Импорт невозможен\n";
                                            errs.Append(err);
                                            return;
                                        }

                                        if (err.Length > 0)
                                        {
                                            errs.Append(err);
                                        }
                                    }
                                    if (errs.Length > 0)
                                    {
                                        var dialog = new ErrorCustomWindow("Внимание!", errs.ToString());
                                        dialog.ShowDialog();
                                        /*MessageBox.Show(errs.ToString(), "Внимание!", MessageBoxButton.OK, MessageBoxImage.Warning);*/
                                        errs.Clear();
                                    }
                                }
                                else
                                {
                                    //Variables of agreement
                                    var isEditing = false;
                                    var agreement = new Agreements();

                                    //Validation
                                    StringBuilder err = new StringBuilder();


                                    //Agreement number|Номер договора
                                    if (row.Cell(agreementNumberColumn).IsEmpty())
                                    {
                                        err.AppendLine("Строка " + row.RowNumber() + " - не добавлена (Отсутствует номер договора)");
                                        errs.Append(err.ToString());
                                        continue;
                                    }
                                    else
                                    {
                                        var tempCellValue = row.Cell(agreementNumberColumn).Value.ToString().Trim();
                                        tempCellValue = Regex.Replace(tempCellValue, @"\s+", " ");
                                        if (agreementsContext.Any(p => p.AgreementNumber.ToLower() == tempCellValue.ToLower()))
                                        {
                                            isEditing = true;
                                            var tempagreement = agreementsContext.FirstOrDefault(p => p.AgreementNumber.ToLower() == tempCellValue.ToLower());
                                            agreement.AgreementNumber = tempagreement.AgreementNumber;
                                        }
                                        else
                                        {
                                            agreement.AgreementNumber = tempCellValue;
                                        }
                                    }


                                    //Company Name|Название компании/организации
                                    if (row.Cell(companyNameColumn).IsEmpty())
                                    {
                                        err.AppendLine("Строка " + row.RowNumber() + " - не добавлена (Отсутствует название компании)");
                                        errs.Append(err.ToString());
                                        continue;
                                    }
                                    else
                                    {
                                        var tempCellValue = row.Cell(companyNameColumn).Value.ToString().Trim();
                                        tempCellValue = Regex.Replace(tempCellValue, @"\s+", " ");
                                        agreement.CompanyName = tempCellValue;


                                        //CompanyINN|ИНН компании
                                        tempCellValue = Regex.Replace(tempCellValue, @"[a-zA-Zа-яА-ЯёЁ()*/.,<>''""№«»-]", "");

                                        var tempSplitedValue = tempCellValue.Split(' ');
                                        foreach (var x in tempSplitedValue)
                                        {
                                            if (x.Length > 8)
                                            {
                                                var trimX = x.Trim();
                                                if (IsAllDigits(trimX))
                                                {
                                                    agreement.CompanyINN = trimX;
                                                }
                                            }
                                        }
                                    }


                                    //Agreement Type|Тип договора
                                    if (agreementTypeColumn == 0)
                                        agreement.AgreementTypes.ID = 4;
                                    else
                                    {
                                        var tempCellValue = row.Cell(agreementTypeColumn).Value.ToString().Trim();
                                        tempCellValue = Regex.Replace(tempCellValue, @"\s+", " ");
                                        var agreementType = agreementtypesContext.FirstOrDefault(p => p.AgreementTypeName.ToLower() == tempCellValue.ToLower());
                                        if (agreementType != null)
                                        {
                                            agreement.AgreementTypes = agreementType;
                                        }
                                        else
                                        {
                                            agreement.AgreementTypes = agreementtypesContext.FirstOrDefault(p => p.ID == 4);
                                        }
                                    }


                                    //Date of agreement|Дата заключения договора
                                    if (dateOfAgreementColumn != 0)
                                    {
                                        var tempCellValue = row.Cell(dateOfAgreementColumn).Value.ToString().Trim();
                                        tempCellValue = Regex.Replace(tempCellValue, @"[a-zA-Zа-яА-ЯёЁ–—––]", " ");
                                        tempCellValue = tempCellValue.Replace('/', ' ');
                                        tempCellValue = tempCellValue.Replace('-', ' ');

                                        var tempSplitedValue = tempCellValue.Split(' ');
                                        bool firstDate = true;
                                        foreach (var x in tempSplitedValue)
                                        {
                                            if (x.Length > 5)
                                            {
                                                var trimX = x.Trim('.', '(', ')', '*');
                                                if (trimX == "0:00:00")
                                                {
                                                    continue;
                                                }
                                                DateTime tempDate = new DateTime();
                                                bool success = DateTime.TryParse(trimX, culture, DateTimeStyles.None, out tempDate);

                                                if (success)
                                                {
                                                    if (firstDate)
                                                    {
                                                        agreement.DateOfAgreement = tempDate;
                                                        firstDate = false;
                                                    }
                                                }
                                            }
                                        }
                                        if (agreement.DateOfAgreement == null)
                                        {
                                            err.AppendLine("Строка " + row.RowNumber() + " - не удалось извлечь дату заключения договора");
                                        }
                                    }


                                    List<DateTime> severalDatesStart = new List<DateTime>();
                                    List<DateTime> severalDatesEnd = new List<DateTime>();
                                    //If StartDate and EndDate column is in one cell|Если даты начала и окончания договора в одной клетке
                                    if (agreementStartDateORColumn != 0 && agreementEndDateColumn == 0)
                                    {
                                        var tempCellValue = row.Cell(agreementStartDateORColumn).Value.ToString().Trim();
                                        tempCellValue = Regex.Replace(tempCellValue, @"[a-zA-Zа-яА-ЯёЁ–—––]", " ");

                                        tempCellValue = tempCellValue.Replace('/', ' ');
                                        tempCellValue = tempCellValue.Replace('-', ' ');

                                        var tempSplitedValue = tempCellValue.Split(' ');
                                        bool firstDate = true;
                                        bool startDate = true;
                                        foreach (var x in tempSplitedValue)
                                        {
                                            if (x.Length > 5)
                                            {
                                                var trimX = x.Trim('.', '(', ')', '*');
                                                if (trimX == "0:00:00")
                                                {
                                                    continue;
                                                }
                                                DateTime tempDate = new DateTime();
                                                bool success = DateTime.TryParse(trimX, culture, DateTimeStyles.None, out tempDate);

                                                if (success)
                                                {
                                                    if (startDate)
                                                    {
                                                        if (firstDate)
                                                        {
                                                            agreement.AgreementStartDate = tempDate;
                                                            firstDate = false;
                                                        }
                                                        else
                                                        {
                                                            severalDatesStart.Add(tempDate);
                                                        }
                                                        startDate = false;

                                                    }
                                                    else
                                                    {
                                                        if (severalDatesStart.Count > 0)
                                                        {
                                                            severalDatesEnd.Add(tempDate);
                                                        }
                                                        else
                                                        {
                                                            agreement.AgreementEndDate = tempDate;

                                                        }

                                                        startDate = true;
                                                    }
                                                }
                                            }
                                        }
                                        if (agreement.AgreementStartDate == null)
                                        {
                                            err.AppendLine("Строка " + row.RowNumber() + " - не удалось извлечь даты действия договора");
                                        }
                                        else if (agreement.AgreementStartDate != null && agreement.AgreementEndDate == null)
                                        {
                                            agreement.AgreementEndDate = agreement.AgreementStartDate;
                                            agreement.AgreementStartDate = null;

                                            err.AppendLine("Строка " + row.RowNumber() + " - не удалось извлечь дату начала действия договора");
                                        }
                                        else if (severalDatesStart.Count > severalDatesEnd.Count)
                                        {
                                            var lastDateAtStart = severalDatesStart.Last();
                                            severalDatesEnd.Add(lastDateAtStart);
                                            severalDatesStart.Remove(lastDateAtStart);
                                        }
                                    }


                                    //StartDate column if it is in a different cell|Дата начала действия договора, если даты в разных клетках
                                    else if (agreementStartDateORColumn != 0 && agreementEndDateColumn != 0)
                                    {
                                        var tempCellValue = row.Cell(agreementStartDateORColumn).Value.ToString().Trim();
                                        tempCellValue = Regex.Replace(tempCellValue, @"[a-zA-Zа-яА-ЯёЁ–—––]", " ");
                                        tempCellValue = tempCellValue.Replace('/', ' ');
                                        tempCellValue = tempCellValue.Replace('-', ' ');

                                        var tempSplitedValue = tempCellValue.Split(' ');
                                        bool firstDate = true;
                                        foreach (var x in tempSplitedValue)
                                        {
                                            if (x.Length > 5)
                                            {
                                                var trimX = x.Trim('.', '(', ')', '*');
                                                if (trimX == "0:00:00")
                                                {
                                                    continue;
                                                }
                                                DateTime tempDate = new DateTime().Date;
                                                bool success = DateTime.TryParse(trimX, culture, DateTimeStyles.None, out tempDate);

                                                if (success)
                                                {
                                                    if (firstDate)
                                                    {
                                                        agreement.AgreementStartDate = tempDate.Date;
                                                        firstDate = false;
                                                    }
                                                    else
                                                    {
                                                        severalDatesStart.Add(tempDate.Date);
                                                    }
                                                }
                                            }
                                        }
                                        if (agreement.AgreementStartDate == null)
                                        {
                                            err.AppendLine("Строка " + row.RowNumber() + " - не удалось извлечь дату начала действия договора");
                                        }
                                    }


                                    //EndDate column if it is in a different cell|Дата окончания действия договора, если даты в разных клетках
                                    if (agreementEndDateColumn != 0)
                                    {
                                        var tempCellValue = row.Cell(agreementEndDateColumn).Value.ToString().Trim();
                                        tempCellValue = Regex.Replace(tempCellValue, @"[a-zA-Zа-яА-ЯёЁ–—––]", " ");
                                        tempCellValue = tempCellValue.Replace('/', ' ');
                                        tempCellValue = tempCellValue.Replace('-', ' ');

                                        var tempSplitedValue = tempCellValue.Split(' ');
                                        bool firstDate = true;
                                        foreach (var x in tempSplitedValue)
                                        {
                                            if (x.Length > 5)
                                            {
                                                var trimX = x.Trim('.', '(', ')', '*');
                                                if (trimX == "0:00:00")
                                                {
                                                    continue;
                                                }
                                                DateTime tempDate = new DateTime();
                                                bool success = DateTime.TryParse(trimX, culture, DateTimeStyles.None, out tempDate);

                                                if (success)
                                                {
                                                    if (firstDate)
                                                    {
                                                        agreement.AgreementEndDate = tempDate;
                                                        firstDate = false;
                                                    }
                                                    else
                                                    {
                                                        severalDatesEnd.Add(tempDate);
                                                    }
                                                }
                                            }
                                        }
                                        if (agreement.AgreementEndDate == null)
                                        {
                                            err.AppendLine("Строка " + row.RowNumber() + " - не удалось извлечь дату окончания действия договора");
                                        }
                                    }


                                    //Company Address|Юридический адрес компании
                                    if (companyAdressColumn != 0)
                                    {
                                        var tempCellValue = row.Cell(companyAdressColumn).Value.ToString().Trim();
                                        tempCellValue = Regex.Replace(tempCellValue, @"\s+", " ");
                                        agreement.CompanyLegalAddress = tempCellValue;
                                    }


                                    //Contact Person|Контактное лицо
                                    if (companyContactPersonColumn != 0)
                                    {
                                        var tempCellValue = row.Cell(companyContactPersonColumn).Value.ToString().Trim();
                                        tempCellValue = Regex.Replace(tempCellValue, @"\s+", " ");
                                        agreement.ContactPerson = tempCellValue;
                                    }


                                    //Contact Phone|Контактный телефон
                                    if (contactPhoneColumn != 0)
                                    {
                                        var tempCellValue = row.Cell(contactPhoneColumn).Value.ToString().Trim();
                                        tempCellValue = Regex.Replace(tempCellValue, @"\s+", " ");
                                        agreement.ContactNumber = tempCellValue;
                                    }


                                    //Prolongation|Пролонгация
                                    if (prolongationColumn == 0)
                                        agreement.Prolongation = false;
                                    else
                                    {
                                        var tempCellValue = row.Cell(prolongationColumn).Value.ToString().Trim().ToLower();
                                        tempCellValue = Regex.Replace(tempCellValue, @"\s+", " ");
                                        if (tempCellValue.Contains("да") || tempCellValue.Contains("+") || tempCellValue.Contains("1") || tempCellValue.Contains("есть"))
                                            agreement.Prolongation = true;
                                        else
                                            agreement.Prolongation = false;
                                    }


                                    //Number of people/students|Количество человек/студентов
                                    if (numberOfPeopleColumn != 0)
                                    {
                                        var tempCellValue = row.Cell(numberOfPeopleColumn).Value.ToString();
                                        var tempOnlyDigits = string.Empty;

                                        if (!string.IsNullOrEmpty(tempCellValue))
                                        {
                                            foreach (var c in tempCellValue)
                                            {
                                                if (char.IsDigit(c))
                                                    tempOnlyDigits += c;
                                                else
                                                    tempOnlyDigits += ' ';
                                            }
                                            tempOnlyDigits = Regex.Replace(tempOnlyDigits, @"\s+", " ");
                                            var splitOnlyDigits = tempOnlyDigits.Split(' ');
                                            int tempNumber = 0;
                                            foreach (var s in splitOnlyDigits)
                                            {
                                                if (int.TryParse(s, out int result))
                                                    tempNumber += result;
                                            }
                                            agreement.NumberOfPeople = tempNumber;
                                        }
                                    }


                                    //Registration Required?|Нужна ли регистрация/прописка
                                    if (isRegistrationRequiredColumn == 0)
                                        agreement.IsRegistrationRequired = false;
                                    else
                                    {
                                        var tempCellValue = row.Cell(isRegistrationRequiredColumn).Value.ToString().Trim().ToLower();
                                        tempCellValue = Regex.Replace(tempCellValue, @"\s+", " ");
                                        if (tempCellValue.Contains("да") || tempCellValue.Contains("+") || tempCellValue.Contains("1") || tempCellValue.Contains("есть") || tempCellValue.Contains("нужна") || tempCellValue.Contains("необходим"))
                                            agreement.IsRegistrationRequired = true;
                                        else
                                            agreement.IsRegistrationRequired = false;
                                    }


                                    //Remark|Примечание
                                    if (remarkColumn != 0)
                                    {
                                        var tempCellValue = row.Cell(remarkColumn).Value.ToString().Trim();
                                        tempCellValue = Regex.Replace(tempCellValue, @"\s+", " ");
                                        agreement.Remark = tempCellValue;
                                    }


                                    //Specialities|Специальности
                                    //Variables to contain agreementspeciality before adding it|Переменные, что бы хранить связь Договор-Специальность перед их добавлением
                                    List<AgreementSpeciality> agreementspecialityBeforeEditing = new List<AgreementSpeciality>();
                                    List<AgreementSpeciality> agreementspecialityList = new List<AgreementSpeciality>();
                                    if (specialitiesColumn != 0)
                                    {
                                        var tempCellValue = row.Cell(specialitiesColumn).Value.ToString().Trim();
                                        tempCellValue = Regex.Replace(tempCellValue, @"[-–—––]", " ");
                                        tempCellValue = Regex.Replace(tempCellValue, @"\s+", " ");

                                        var tempSplitedValue = tempCellValue.Split(' ');
                                        List<Specialties> tempSpecialities = new List<Specialties>();
                                        Specialties tempSpeciality = new Specialties();
                                        foreach (var x in tempSplitedValue)
                                        {
                                            if (!string.IsNullOrEmpty(tempSpeciality.SpecialityNumber))
                                            {
                                                tempSpeciality.SpecialityName += x + " ";
                                            }
                                            if (x.Length > 7)
                                            {
                                                var trimX = x.Trim('.', '(', ')', '*', ' ', '-');

                                                if (trimX.All(p => char.IsDigit(p) || p == '.'))
                                                {
                                                    if (!string.IsNullOrEmpty(tempSpeciality.SpecialityName))
                                                    {
                                                        tempSpeciality.SpecialityName = tempSpeciality.SpecialityName.Remove(tempSpeciality.SpecialityName.Length - (x.Length + 2));
                                                        tempSpeciality.SpecialityName = tempSpeciality.SpecialityName.Trim('-', '*', '.', ',', '-', ' ');
                                                        tempSpecialities.Add(tempSpeciality);
                                                        tempSpeciality = new Specialties();
                                                    }
                                                    tempSpeciality.SpecialityNumber = trimX;
                                                }
                                            }
                                        }
                                        if (tempSpeciality.SpecialityName != null)
                                        {
                                            tempSpeciality.SpecialityName = tempSpeciality.SpecialityName.Trim('-', '*', '.', ',', '-', ' ');
                                            tempSpecialities.Add(tempSpeciality);
                                        }

                                        //Adding specialties and AgreementsSpecialities|Добавляем специальности и связь договор специальность
                                        if (tempSpecialities.Count > 0)
                                        {
                                            if (isEditing)
                                            {
                                                if (agreementspecialitiesContext.Any(p => p.AgreementID == agreement.AgreementNumber))
                                                {
                                                    agreementspecialityBeforeEditing = agreementspecialitiesContext.Where(p => p.AgreementID == agreement.AgreementNumber).ToList();
                                                }
                                            }
                                            foreach (var specilityIn in tempSpecialities)
                                            {
                                                AgreementSpeciality agreementspecility = new AgreementSpeciality();
                                                if (specialitiesContext.Any(p => p.SpecialityNumber.ToLower() == specilityIn.SpecialityNumber.ToLower()))
                                                {
                                                    var specilityfound = specialitiesContext.First(p => p.SpecialityNumber.ToLower() == specilityIn.SpecialityNumber.ToLower());
                                                    agreementspecility.AgreementID = agreement.AgreementNumber;
                                                    agreementspecility.Specialties = specilityfound;
                                                    agreementspecility.SpecialityID = specilityfound.SpecialityNumber;
                                                    agreementspecialityList.Add(agreementspecility);
                                                }
                                                else
                                                {
                                                    if (!string.IsNullOrEmpty(specilityIn.SpecialityName))
                                                    {
                                                        specialitiesContext.Add(specilityIn);
                                                        agreementspecility.AgreementID = agreement.AgreementNumber;
                                                        agreementspecility.Specialties = specilityIn;
                                                        agreementspecialityList.Add(agreementspecility);
                                                    }
                                                    else
                                                    {
                                                        err.AppendLine("Строка " + row.RowNumber() + " - одна или несколько специальностей не добавлена(-ы), не найдено(-ы) название(-я)");
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            err.AppendLine("Строка " + row.RowNumber() + " - специальности не добавлены, не найдены коды");
                                        }
                                    }


                                    //Add an agreement if not editing one|Добавляем договор, если не редактируем
                                    if (!isEditing)
                                    {
                                        foreach (var elem in agreementspecialityList)
                                        {
                                            agreementspecialitiesContext.Add(elem);
                                            agreement.AgreementSpeciality.Add(elem); //!!!!!!
                                            App.Context.AgreementSpeciality.Add(elem);
                                        }
                                        agreementsContext.Add(agreement);
                                        App.Context.Agreements.Add(agreement);
                                        agreementsAddOrEditedList.Add(agreement);
                                        addedValues++;
                                    }
                                    else
                                    {
                                        bool agreeementchanged = false;
                                        var agreementBeforeEditing = agreementsContext.FirstOrDefault(p => p.AgreementNumber.ToLower() == agreement.AgreementNumber.ToLower());
                                        agreement.AgreementNumber = agreementBeforeEditing.AgreementNumber;

                                        if (agreement.AgreementNumber != agreementBeforeEditing.AgreementNumber ||
                                            agreement.AgreementTypes != agreementBeforeEditing.AgreementTypes ||
                                            agreement.CompanyName != agreementBeforeEditing.CompanyName ||
                                            agreement.CompanyINN != agreementBeforeEditing.CompanyINN ||
                                            agreement.AgreementStartDate != agreementBeforeEditing.AgreementStartDate ||
                                            agreement.AgreementEndDate != agreementBeforeEditing.AgreementEndDate ||
                                            agreement.CompanyLegalAddress != agreementBeforeEditing.CompanyLegalAddress ||
                                            agreement.ContactPerson != agreementBeforeEditing.ContactPerson ||
                                            agreement.ContactNumber != agreementBeforeEditing.ContactNumber ||
                                            agreement.NumberOfPeople != agreementBeforeEditing.NumberOfPeople ||
                                            agreement.DateOfAgreement != agreementBeforeEditing.DateOfAgreement ||
                                            agreement.Prolongation != agreementBeforeEditing.Prolongation ||
                                            agreement.Remark != agreementBeforeEditing.Remark ||
                                            agreement.IsRegistrationRequired != agreementBeforeEditing.IsRegistrationRequired)
                                        {
                                            agreeementchanged = true;
                                        }


                                        if (agreementspecialityList.Count != agreementspecialityBeforeEditing.Count)
                                        {
                                            agreeementchanged = true;
                                        }
                                        else
                                        {
                                            foreach (var elem in agreementspecialityBeforeEditing)
                                            {
                                                if (!agreementspecialityList.Any(p => p.Specialties == elem.Specialties))
                                                {
                                                    agreeementchanged = true;
                                                    break;
                                                }
                                            }
                                        }


                                        if (agreeementchanged)
                                        {
                                            /*agreementBeforeEditing = agreement;*/
                                            agreementBeforeEditing.AgreementNumber = agreement.AgreementNumber;
                                            agreementBeforeEditing.AgreementTypes = agreement.AgreementTypes;
                                            agreementBeforeEditing.CompanyName = agreement.CompanyName;
                                            agreementBeforeEditing.CompanyINN = agreement.CompanyINN;
                                            agreementBeforeEditing.AgreementStartDate = agreement.AgreementStartDate;
                                            agreementBeforeEditing.AgreementEndDate = agreement.AgreementEndDate;
                                            agreementBeforeEditing.CompanyLegalAddress = agreement.CompanyLegalAddress;
                                            agreementBeforeEditing.ContactPerson = agreement.ContactPerson;
                                            agreementBeforeEditing.ContactNumber = agreement.ContactNumber;
                                            agreementBeforeEditing.NumberOfPeople = agreement.NumberOfPeople;
                                            agreementBeforeEditing.DateOfAgreement = agreement.DateOfAgreement;
                                            agreementBeforeEditing.Prolongation = agreement.Prolongation;
                                            agreementBeforeEditing.Remark = agreement.Remark;
                                            agreementBeforeEditing.IsRegistrationRequired = agreement.IsRegistrationRequired;


                                            foreach (var elem in agreementspecialityBeforeEditing)
                                            {
                                                agreementspecialitiesContext.Remove(elem);
                                                App.Context.AgreementSpeciality.Remove(elem);
                                            }

                                            foreach (var item in agreementspecialityList)
                                            {
                                                agreementBeforeEditing.AgreementSpeciality.Add(item);
                                                agreementspecialitiesContext.Add(item);
                                                App.Context.AgreementSpeciality.Add(item);
                                            }

                                            agreementsAddOrEditedList.Add(agreementBeforeEditing);
                                            updatedValues++;
                                        }
                                        else
                                        {
                                            sameValue++;
                                        }
                                    }


                                    //Make an additional agreement if there is more that one date in agreements date columns|Создаём доп. договор, если есть дополнительные даты в сроках действия
                                    var numberOfDates = severalDatesEnd.Count > severalDatesStart.Count ? severalDatesEnd.Count : severalDatesStart.Count;

                                    if (numberOfDates > 0)
                                    {
                                        for (int i = 1; i <= numberOfDates; i++)
                                        {
                                            //Copy all variables from original agreement|Скопируем все переменные из оригинального договора
                                            var additionalAgreement = new Agreements();
                                            additionalAgreement.AgreementNumber = agreement.AgreementNumber;
                                            additionalAgreement.AgreementTypes = agreement.AgreementTypes;
                                            additionalAgreement.CompanyName = agreement.CompanyName;
                                            additionalAgreement.CompanyINN = agreement.CompanyINN;
                                            additionalAgreement.CompanyLegalAddress = agreement.CompanyLegalAddress;
                                            additionalAgreement.ContactPerson = agreement.ContactPerson;
                                            additionalAgreement.ContactNumber = agreement.ContactNumber;
                                            additionalAgreement.NumberOfPeople = agreement.NumberOfPeople;
                                            additionalAgreement.DateOfAgreement = agreement.DateOfAgreement;
                                            additionalAgreement.Prolongation = agreement.Prolongation;
                                            additionalAgreement.Remark = agreement.Remark;
                                            additionalAgreement.IsRegistrationRequired = agreement.IsRegistrationRequired;


                                            additionalAgreement.AgreementNumber += "/доп_" + i;

                                            var additionalEditing = false;
                                            if (agreementsContext.Any(p => p.AgreementNumber.ToLower() == additionalAgreement.AgreementNumber.ToLower()))
                                            {
                                                additionalEditing = true;
                                            }


                                            if (severalDatesStart.Count >= i)
                                            {
                                                additionalAgreement.AgreementStartDate = severalDatesStart[i - 1];
                                            }
                                            else
                                            {
                                                err.AppendLine("Строка " + row.RowNumber() + " - не удалось извлечь дополнительную дату начала действия договора");
                                            }


                                            if (severalDatesEnd.Count >= i)
                                            {
                                                additionalAgreement.AgreementEndDate = severalDatesEnd[i - 1];
                                            }
                                            else
                                            {
                                                err.AppendLine("Строка " + row.RowNumber() + " - не удалось извлечь дополнительную дату окончания действия договора");
                                            }

                                            //Adding if not found the additional agreement in database
                                            if (!additionalEditing)
                                            {
                                                foreach (var elem in agreementspecialityList)
                                                {
                                                    AgreementSpeciality agreementspeciality = new AgreementSpeciality();
                                                    agreementspeciality.Specialties = elem.Specialties;
                                                    agreementspeciality.AgreementID = additionalAgreement.AgreementNumber;
                                                    agreementspeciality.Agreements = additionalAgreement;
                                                    additionalAgreement.AgreementSpeciality.Add(agreementspeciality);
                                                    agreementspecialitiesContext.Add(agreementspeciality);
                                                    App.Context.AgreementSpeciality.Add(agreementspeciality);
                                                }

                                                agreementsContext.Add(additionalAgreement);
                                                App.Context.Agreements.Add(additionalAgreement); //!!!!!

                                                agreementsAddOrEditedList.Add(additionalAgreement);
                                                addedValues++;
                                            }
                                            //Editing the additional agreement
                                            else
                                            {
                                                bool aditionalagreeementchanged = false;

                                                var aditionalagreementBeforeEditing = agreementsContext.FirstOrDefault(p => p.AgreementNumber.ToLower() == additionalAgreement.AgreementNumber.ToLower());

                                                var aditionalAgreementSpecialityListBeforeEdit = agreementspecialitiesContext.Where(p => p.AgreementID == aditionalagreementBeforeEditing.AgreementNumber).ToList();


                                                //Check if additionalAgreement was changed
                                                additionalAgreement.AgreementNumber = aditionalagreementBeforeEditing.AgreementNumber;


                                                if (additionalAgreement != aditionalagreementBeforeEditing)
                                                    if (additionalAgreement.AgreementNumber != aditionalagreementBeforeEditing.AgreementNumber ||
                                                        additionalAgreement.AgreementTypes != aditionalagreementBeforeEditing.AgreementTypes ||
                                                        additionalAgreement.CompanyName != aditionalagreementBeforeEditing.CompanyName ||
                                                        additionalAgreement.CompanyINN != aditionalagreementBeforeEditing.CompanyINN ||
                                                        additionalAgreement.AgreementStartDate != aditionalagreementBeforeEditing.AgreementStartDate ||
                                                        additionalAgreement.AgreementEndDate != aditionalagreementBeforeEditing.AgreementEndDate ||
                                                        additionalAgreement.CompanyLegalAddress != aditionalagreementBeforeEditing.CompanyLegalAddress ||
                                                        additionalAgreement.ContactPerson != aditionalagreementBeforeEditing.ContactPerson ||
                                                        additionalAgreement.ContactNumber != aditionalagreementBeforeEditing.ContactNumber ||
                                                        additionalAgreement.NumberOfPeople != aditionalagreementBeforeEditing.NumberOfPeople ||
                                                        additionalAgreement.DateOfAgreement != aditionalagreementBeforeEditing.DateOfAgreement ||
                                                        additionalAgreement.Prolongation != aditionalagreementBeforeEditing.Prolongation ||
                                                        additionalAgreement.Remark != aditionalagreementBeforeEditing.Remark ||
                                                        additionalAgreement.IsRegistrationRequired != aditionalagreementBeforeEditing.IsRegistrationRequired)
                                                    {
                                                        aditionalagreeementchanged = true;
                                                    }

                                                if (agreementspecialityList.Count != aditionalAgreementSpecialityListBeforeEdit.Count)
                                                {
                                                    aditionalagreeementchanged = true;
                                                }
                                                else
                                                {
                                                    foreach (var elem in aditionalAgreementSpecialityListBeforeEdit)
                                                    {
                                                        if (!agreementspecialityList.Any(p => p.Specialties == elem.Specialties))
                                                        {
                                                            aditionalagreeementchanged = true;
                                                            break;
                                                        }
                                                    }
                                                }


                                                //Editing the additional agreement
                                                if (aditionalagreeementchanged)
                                                {
                                                    /*aditionalagreementBeforeEditing = additionalAgreement;*/
                                                    aditionalagreementBeforeEditing.AgreementNumber = additionalAgreement.AgreementNumber;
                                                    aditionalagreementBeforeEditing.AgreementTypes = additionalAgreement.AgreementTypes;
                                                    aditionalagreementBeforeEditing.CompanyName = additionalAgreement.CompanyName;
                                                    aditionalagreementBeforeEditing.CompanyINN = additionalAgreement.CompanyINN;
                                                    aditionalagreementBeforeEditing.AgreementStartDate = additionalAgreement.AgreementStartDate;
                                                    aditionalagreementBeforeEditing.AgreementEndDate = additionalAgreement.AgreementEndDate;
                                                    aditionalagreementBeforeEditing.CompanyLegalAddress = additionalAgreement.CompanyLegalAddress;
                                                    aditionalagreementBeforeEditing.ContactPerson = additionalAgreement.ContactPerson;
                                                    aditionalagreementBeforeEditing.ContactNumber = additionalAgreement.ContactNumber;
                                                    aditionalagreementBeforeEditing.NumberOfPeople = additionalAgreement.NumberOfPeople;
                                                    aditionalagreementBeforeEditing.DateOfAgreement = additionalAgreement.DateOfAgreement;
                                                    aditionalagreementBeforeEditing.Prolongation = additionalAgreement.Prolongation;
                                                    aditionalagreementBeforeEditing.Remark = additionalAgreement.Remark;
                                                    aditionalagreementBeforeEditing.IsRegistrationRequired = additionalAgreement.IsRegistrationRequired;


                                                    //Removing
                                                    foreach (var elem in aditionalAgreementSpecialityListBeforeEdit)
                                                    {
                                                        App.Context.AgreementSpeciality.Remove(elem);
                                                        agreementspecialitiesContext.Remove(elem);
                                                    }


                                                    //Adding again
                                                    foreach (var item in agreementspecialityList)
                                                    {
                                                        AgreementSpeciality agreementspeciality = new AgreementSpeciality();
                                                        agreementspeciality.Specialties = item.Specialties;
                                                        agreementspeciality.AgreementID = additionalAgreement.AgreementNumber;
                                                        agreementspeciality.Agreements = additionalAgreement;
                                                        aditionalagreementBeforeEditing.AgreementSpeciality.Add(agreementspeciality);
                                                        agreementspecialitiesContext.Add(agreementspeciality);
                                                        App.Context.AgreementSpeciality.Add(agreementspeciality);
                                                    }
                                                    agreementsAddOrEditedList.Add(aditionalagreementBeforeEditing);
                                                    updatedValues++;
                                                }
                                                else
                                                {
                                                    sameValue++;
                                                }
                                                /////
                                            }
                                        }
                                    }
                                    //Errors input|Ввод ошибок
                                    if (err.Length > 0)
                                    {
                                        errs.Append(err.ToString());
                                    }
                                }
                            }
                            Mouse.OverrideCursor = null;
                            //Errors output|Вывод ошибок
                            if (errs.Length > 0)
                            {
                                var dialog = new ErrorCustomWindow("Внимание!", errs.ToString());
                                dialog.ShowDialog();
                                /*MessageBox.Show(errs.ToString(), "Внимание!", MessageBoxButton.OK, MessageBoxImage.Warning);*/
                            }
                            if (updatedValues > 0 || addedValues > 0)
                            {
                                isImporting = true;
                                var dialog = new ErrorCustomWindow("Успех!", $"Добавлено: {addedValues} значений\nИзменено: {updatedValues} значения\nНайдёно идентичных: {sameValue} значений");
                                dialog.ShowDialog();
                                MessageBox.Show("Обновленные данные будут выведены на экран\nДанные ещё не сохранены. Сохраните их нажатием на кнопку 'Сохранить'", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Information);
                                viewDataGrid.ItemsSource = agreementsAddOrEditedList;
                                viewDataGrid.Items.Refresh();
                                /* MessageBox.Show($"Добавлено: {addedValues} значений\nИзменено: {updatedValues} значения\nНайдёно идентичных: {sameValue} значений", "Успех!", MessageBoxButton.OK, MessageBoxImage.Information);*/
                            }
                            else
                            {
                                var dialog = new ErrorCustomWindow("Успех!", $"Данные не были изменены.\nНайдено идентичных значений: {sameValue}");
                                dialog.ShowDialog();
                                /*MessageBox.Show($"Данные не были изменены.\nНайдено идентичных значений: {sameValue}", "Успех!", MessageBoxButton.OK, MessageBoxImage.Information);*/
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Импорт не удался!", "Ошибка!");
                        MessageBox.Show(ex.Message);
                        if (ex.InnerException != null)
                            MessageBox.Show(ex.InnerException.ToString());
                    }
                }
            }
        }
        #endregion
    }
}

