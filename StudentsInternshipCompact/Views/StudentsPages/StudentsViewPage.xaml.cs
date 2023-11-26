using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
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

namespace StudentsInternship.Views.StudentsPages
{
    /// <summary>
    /// Логика взаимодействия для StudentsViewPage.xaml
    /// </summary>
    public partial class StudentsViewPage : Page
    {
        bool isImporting = false;
        List<Students> datagridSourceList = new List<Students>();
        private int PagesCount;
        private int NumberOfPage = 0;
        private int maxItemShow = 100;
        public StudentsViewPage()
        {
            InitializeComponent();

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
            if (!isImporting)
            {
                AddEditWindow windowAddEdit = new AddEditWindow();
                windowAddEdit.frameAddEdit.Navigate(new StudentsAddEditPage(null));
                windowAddEdit.Closed += (s, EventArgs) =>
                {
                    UpdateDataGrid();
                };
                windowAddEdit.Show();
            }
        }
        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (viewDataGrid.SelectedItems.Count > 0 && !isImporting)
            {
                var elemsToDelete = viewDataGrid.SelectedItems.Cast<Students>().ToList();
                if (MessageBox.Show($"Вы точно хотите удалить следующие {elemsToDelete.Count()} элементов?", "Внимание",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    try
                    {
                        App.Context.Students.RemoveRange(elemsToDelete);
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
                windowAddEdit.frameAddEdit.Navigate(new StudentsAddEditPage(viewDataGrid.SelectedItem as Students));
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
                if (isImporting)
                {
                    MessageBox.Show("Импорт завершён!", "Успех");
                    isImporting = false;
                }
                App.Context.SaveChanges();
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
            cmbdtGroupNumber.ItemsSource = App.Context.Groups.ToList();

            datagridSourceList = App.Context.Students.ToList();
            // Filtration
            switch (cmbFilter.SelectedIndex)
            {
                case 0:
                    break;
                case 1:
                    datagridSourceList = datagridSourceList.Where(p => p.ResidenceRegistration == true).ToList();
                    break;
                case 2:
                    datagridSourceList = datagridSourceList.Where(p => p.ResidenceRegistration == false).ToList();
                    break;
                case 3:
                    datagridSourceList = datagridSourceList.Where(p => p.Phone != null && p.Phone != "").ToList();
                    break;
                case 4:
                    datagridSourceList = datagridSourceList.Where(p => p.Phone == null || p.Phone == "").ToList();
                    break;
                default:
                    break;
            }
            //Search
            datagridSourceList = datagridSourceList.Where(p => p.Surname.ToLower().Contains(txtSearch.Text.ToLower()) ||
                p.Name.ToLower().Contains(txtSearch.Text.ToLower()) ||
                p.Patronymic.ToLower().Contains(txtSearch.Text.ToLower()) ||
                p.Phone.ToLower().Contains(txtSearch.Text.ToLower()) ||
                p.Groups.EducationalOrganisations.Name.ToLower().Contains(txtSearch.Text.ToLower()) ||
                p.Groups.GroupNumberName.ToLower().Contains(txtSearch.Text.ToLower()) ||
                p.Groups.Specialties.SpecialityName.ToLower().Contains(txtSearch.Text.ToLower()) ||
                p.Groups.Specialties.SpecialityNumber.ToLower().Contains(txtSearch.Text.ToLower())
            ).ToList();

            //Items Counter
            tbItemCounter.Text = datagridSourceList.Count.ToString() + " из " + App.Context.Students.Count().ToString();

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
            tbPageCounter.Text = $"Страница {NumberOfPage+1} из {PagesCount}";

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
                case null: case "":
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
                    case "Фамилия":
                        AddSortingVar("Surname", column);
                        break;
                    case "Имя":
                        AddSortingVar("Name", column);
                        break;
                    case "Отчество":
                        AddSortingVar("Patronymic", column);
                        break;
                    case "Телефон":
                        AddSortingVar("Phone", column);
                        break;
                    case "Специальность":
                        AddSortingVar("Speciality", column);
                        break;
                    case "Прописка":
                        AddSortingVar("Residence", column);
                        break;
                    case "Курс":
                        AddSortingVar("Course", column);
                        break;
                    case "Группа":
                        AddSortingVar("GroupNumber", column);
                        break;
                    case "Учебное заведение":
                        AddSortingVar("Organisation", column);
                        break;
                }
            }
        }

        //Sorting Method
        private void Sorting()
        {
            if (sort.Count > 0)
            {
                IOrderedEnumerable<Students> SortingList = null;
                foreach (SortElem elem in sort)
                {
                    switch (elem.SortName)
                    {
                        case "Surname":
                            if (elem.SortAscending)
                            {
                                if (SortingList != null)
                                    SortingList = SortingList.ThenBy(p => p.Surname);
                                else
                                    SortingList = datagridSourceList.OrderBy(p => p.Surname);
                                break;
                            }
                            else
                            {
                                if (SortingList != null)
                                    SortingList = SortingList.ThenByDescending(p => p.Surname);
                                else
                                    SortingList = datagridSourceList.OrderByDescending(p => p.Surname);
                                break;
                            }
                        case "Name":
                            if (elem.SortAscending)
                            {
                                if (SortingList != null)
                                    SortingList = SortingList.ThenBy(p => p.Name);
                                else
                                    SortingList = datagridSourceList.OrderBy(p => p.Name);
                                break;
                            }
                            else
                            {
                                if (SortingList != null)
                                    SortingList = SortingList.ThenByDescending(p => p.Name);
                                else
                                    SortingList = datagridSourceList.OrderByDescending(p => p.Name);
                                break;
                            }
                        case "Patronymic":
                            if (elem.SortAscending)
                            {
                                if (SortingList != null)
                                    SortingList = SortingList.ThenBy(p => p.Patronymic);
                                else
                                    SortingList = datagridSourceList.OrderBy(p => p.Patronymic);
                                break;
                            }
                            else
                            {
                                if (SortingList != null)
                                    SortingList = SortingList.ThenByDescending(p => p.Patronymic);
                                else
                                    SortingList = datagridSourceList.OrderByDescending(p => p.Patronymic);
                                break;
                            }
                        case "Phone":
                            if (elem.SortAscending)
                            {
                                if (SortingList != null)
                                    SortingList = SortingList.ThenBy(p => p.Phone);
                                else
                                    SortingList = datagridSourceList.OrderBy(p => p.Phone);
                                break;
                            }
                            else
                            {
                                if (SortingList != null)
                                    SortingList = SortingList.ThenByDescending(p => p.Phone);
                                else
                                    SortingList = datagridSourceList.OrderByDescending(p => p.Phone);
                                break;
                            }
                        case "Speciality":
                            if (elem.SortAscending)
                            {
                                if (SortingList != null)
                                    SortingList = SortingList.ThenBy(p => p.Groups.Specialties.SpecialityNumber).ThenBy(p => p.Groups.Specialties.SpecialityName);
                                else
                                    SortingList = datagridSourceList.OrderBy(p => p.Groups.Specialties.SpecialityNumber).ThenBy(p => p.Groups.Specialties.SpecialityName);
                                break;
                            }
                            else
                            {
                                if (SortingList != null)
                                    SortingList = SortingList.ThenByDescending(p => p.Groups.Specialties.SpecialityNumber).ThenByDescending(p => p.Groups.Specialties.SpecialityName);
                                else
                                    SortingList = datagridSourceList.OrderByDescending(p => p.Groups.Specialties.SpecialityNumber).ThenByDescending(p => p.Groups.Specialties.SpecialityName);
                                break;
                            }
                        case "Residence":
                            if (elem.SortAscending)
                            {
                                if (SortingList != null)
                                    SortingList = SortingList.ThenBy(p => p.ResidenceRegistration);
                                else
                                    SortingList = datagridSourceList.OrderBy(p => p.ResidenceRegistration);
                                break;
                            }
                            else
                            {
                                if (SortingList != null)
                                    SortingList = SortingList.ThenByDescending(p => p.ResidenceRegistration);
                                else
                                    SortingList = datagridSourceList.OrderByDescending(p => p.ResidenceRegistration);
                                break;
                            }
                        case "Course":
                            if (elem.SortAscending)
                            {
                                if (SortingList != null)
                                    SortingList = SortingList.ThenBy(p => p.Course);
                                else
                                    SortingList = datagridSourceList.OrderBy(p => p.Course);
                                break;
                            }
                            else
                            {
                                if (SortingList != null)
                                    SortingList = SortingList.ThenByDescending(p => p.Course);
                                else
                                    SortingList = datagridSourceList.OrderByDescending(p => p.Course);
                                break;
                            }
                        case "GroupNumber":
                            if (elem.SortAscending)
                            {
                                if (SortingList != null)
                                    SortingList = SortingList.ThenBy(p => p.Groups.GroupNumberName);
                                else
                                    SortingList = datagridSourceList.OrderBy(p => p.Groups.GroupNumberName);
                                break;
                            }
                            else
                            {
                                if (SortingList != null)
                                    SortingList = SortingList.ThenByDescending(p => p.Groups.GroupNumberName);
                                else
                                    SortingList = datagridSourceList.OrderByDescending(p => p.Groups.GroupNumberName);
                                break;
                            }
                        case "Organisation":
                            if (elem.SortAscending)
                            {
                                if (SortingList != null)
                                    SortingList = SortingList.ThenBy(p => p.Groups.EducationalOrganisations.Name);
                                else
                                    SortingList = datagridSourceList.OrderBy(p => p.Groups.EducationalOrganisations.Name);
                                break;
                            }
                            else
                            {
                                if (SortingList != null)
                                    SortingList = SortingList.ThenByDescending(p => p.Groups.EducationalOrganisations.Name);
                                else
                                    SortingList = datagridSourceList.OrderByDescending(p => p.Groups.EducationalOrganisations.Name);
                                break;
                            }
                        case null: case "":
                            SortingList = datagridSourceList.OrderBy(p => p.ID);
                            break;
                    }
                }
                datagridSourceList = SortingList.ToList();
            }
        }

        // Delete filters
        private void deleteFiltersButton_Click(object sender, RoutedEventArgs e)
        {
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

                        var FIOColumn = 0;
                        var surnameColumn = 0;
                        var firstNameColumn = 0;
                        var patronymicColumn = 0;
                        var educationalOrganisationColumn = 0;
                        var phoneColumn = 0;
                        var groupsColumn = 0;
                        var courseColumn = 0;
                        var registrationColumn = 0;
                        var specialityColumn = 0;

                        var addedValues = 0;
                        var updatedValues = 0;
                        var sameValue = 0;

                        using (XLWorkbook workBook = new XLWorkbook(ofd.FileName))
                        {
                            List<Students> studentsAddOrEditedList = new List<Students>();
                            var studentsContext = App.Context.Students.ToList();
                            var groupsContext = App.Context.Groups.ToList();
                            var specialitiesContext = App.Context.Specialties.ToList();
                            var educationalOrganisationsContext = App.Context.EducationalOrganisations.ToList();

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
                                            if (tempCellValue.Contains("фио") || (tempCellValue.Contains("им") && tempCellValue.Contains("фамили") && tempCellValue.Contains("отчеств")))
                                            {
                                                FIOColumn = cell.Address.ColumnNumber;
                                            }
                                            else if (tempCellValue.Contains("фамили") && FIOColumn == 0)
                                            {
                                                surnameColumn = cell.Address.ColumnNumber;
                                            }
                                            else if (tempCellValue.Contains("имя") || tempCellValue.Contains("имена") && FIOColumn == 0)
                                            {
                                                firstNameColumn = cell.Address.ColumnNumber;
                                            }
                                            else if (tempCellValue.Contains("отчеств") && FIOColumn == 0)
                                            {
                                                patronymicColumn = cell.Address.ColumnNumber;
                                            }
                                            if (tempCellValue.Contains("телефон"))
                                            {
                                                phoneColumn = cell.Address.ColumnNumber;
                                            }
                                            if (tempCellValue.Contains("специальност"))
                                            {
                                                specialityColumn = cell.Address.ColumnNumber;
                                            }
                                            if (tempCellValue.Contains("групп"))
                                            {
                                                groupsColumn = cell.Address.ColumnNumber;
                                            }
                                            if (tempCellValue.Contains("курс"))
                                            {
                                                courseColumn = cell.Address.ColumnNumber;
                                            }
                                            if ((tempCellValue.Contains("образовательн") || tempCellValue.Contains("учебн")) && (tempCellValue.Contains("организац") || tempCellValue.Contains("заведени") || tempCellValue.Contains("учреждени")))
                                            {
                                                educationalOrganisationColumn = cell.Address.ColumnNumber;
                                            }
                                            if (tempCellValue.Contains("специальност"))
                                            {
                                                specialityColumn = cell.Address.ColumnNumber;
                                            }
                                            if (tempCellValue.Contains("прописка") || ((tempCellValue.Contains("временн") || tempCellValue.Contains("постоянн")) && tempCellValue.Contains("регистраци")))
                                            {
                                                registrationColumn = cell.Address.ColumnNumber;
                                            }

                                        }
                                        string err = String.Empty;


                                        if (FIOColumn == 0 && (surnameColumn == 0 || firstNameColumn == 0))
                                        {
                                            err += "Не найдена колонка с ФИО. Импорт невозможен\n";
                                            errs.Append(err);
                                            return;
                                        }
                                        else if (surnameColumn == 0 && (firstNameColumn != 0 || patronymicColumn != 0))
                                        {
                                            err += "Не найдена колонка фамилии. Импорт невозможен\n";
                                            errs.Append(err);
                                            return;
                                        }
                                        else if (firstNameColumn == 0 && (surnameColumn != 0 || patronymicColumn != 0))
                                        {
                                            err += "Не найдена колонка имени. Импорт невозможен\n";
                                            errs.Append(err);
                                            return;
                                        }
                                        else if (patronymicColumn == 0 && (firstNameColumn != 0 || surnameColumn != 0))
                                        {
                                            err += "Не найдена колонка отчества\n";
                                        }
                                        if (phoneColumn == 0)
                                        {
                                            err += "Не найдена колонка номеров телефона\n";
                                        }
                                        if (groupsColumn == 0)
                                        {
                                            err += "Не найдена колонка групп\n";
                                        }
                                        else
                                        {
                                            if (educationalOrganisationColumn == 0)
                                            {
                                                err += "Не найдена колонка образовательных учреждений/организаций. Импорт невозможен\n";
                                                errs.Append(err);
                                                return;
                                            }
                                            if (specialityColumn == 0)
                                            {
                                                err += "Не найдена колонка специальности. Импорт невозможен\n";
                                                errs.Append(err);
                                                return;
                                            }
                                        }
                                        if (courseColumn == 0)
                                        {
                                            err += "Не найдена колонка номера курса\n";
                                        }
                                        if (registrationColumn == 0)
                                        {
                                            err += "Не найдена колонка регистраций (прописок)\n";
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
                                    var student = new Students();

                                    //Validation
                                    StringBuilder err = new StringBuilder();


                                    //Student Full Name|ФИО студента
                                    if (FIOColumn != 0)
                                    {
                                        if (row.Cell(FIOColumn).IsEmpty())
                                        {
                                            err.AppendLine("Строка " + row.RowNumber() + " - не добавлена (Отсутствует ФИО)");
                                            errs.Append(err.ToString());
                                            continue;
                                        }
                                        else
                                        {
                                            var tempCellValue = row.Cell(FIOColumn).Value.ToString().Trim();
                                            tempCellValue = Regex.Replace(tempCellValue, @"\s+", " ");

                                            var tempSplitedValue = tempCellValue.Split(' ');
                                            int i = 0; // 0 - Surname|Фамилия 1 - Name|Имя else|Иное - Patronymic|Отчество
                                            foreach (var x in tempSplitedValue)
                                            {
                                                if (i == 0)
                                                {
                                                    student.Surname = x;
                                                    i++;
                                                }
                                                else if (i == 1)
                                                {
                                                    student.Name = x;
                                                    i++;
                                                }
                                                else
                                                {
                                                    student.Patronymic += x + " ";
                                                }
                                            }


                                            if (student.Patronymic != null)
                                            {
                                                student.Patronymic = student.Patronymic.Trim();
                                            }


                                            if (student.Surname == null)
                                            {
                                                err.AppendLine("Строка " + row.RowNumber() + " - не добавлена (не удалось извлечь фамилию и имя)");
                                                errs.Append(err.ToString());
                                                continue;
                                            }
                                            else if (student.Name == null)
                                            {
                                                err.AppendLine("Строка " + row.RowNumber() + " - не добавлена (не удалось извлечь имя)");
                                                errs.Append(err.ToString());
                                                continue;
                                            }
                                        }
                                    }
                                    //If Full name is in different columns|Если ФИО в разных колонках
                                    else
                                    {
                                        if (row.Cell(firstNameColumn).IsEmpty())
                                        {
                                            err.AppendLine("Строка " + row.RowNumber() + " - не добавлена (Отсутствует имя студента)");
                                            errs.Append(err.ToString());
                                            continue;
                                        }
                                        else
                                        {
                                            var tempCellValue = row.Cell(firstNameColumn).Value.ToString().Trim();
                                            tempCellValue = tempCellValue.Replace(" ", "");
                                            student.Name = tempCellValue;
                                        }
                                        if (row.Cell(surnameColumn).IsEmpty())
                                        {
                                            err.AppendLine("Строка " + row.RowNumber() + " - не добавлена (Отсутствует имя студента)");
                                            errs.Append(err.ToString());
                                            continue;
                                        }
                                        else
                                        {
                                            var tempCellValue = row.Cell(surnameColumn).Value.ToString().Trim();
                                            tempCellValue = tempCellValue.Replace(" ", "");
                                            student.Surname = tempCellValue;
                                        }
                                        if (patronymicColumn != 0)
                                        {
                                            var tempCellValue = row.Cell(patronymicColumn).Value.ToString().Trim();
                                            tempCellValue = tempCellValue.Replace(" ", "");
                                            student.Patronymic = tempCellValue;
                                        }
                                    }



                                    //Phone number|Номер телефона
                                    if (phoneColumn != 0)
                                    {
                                        var tempCellValue = row.Cell(phoneColumn).Value.ToString().Trim();
                                        tempCellValue = Regex.Replace(tempCellValue, @"\s+", " ");
                                        student.Phone = tempCellValue;
                                    }


                                    //Course number|Номер курса
                                    if (courseColumn != 0)
                                    {
                                        var tempCellValue = row.Cell(courseColumn).Value.ToString();
                                        var tempOnlyDigits = string.Empty;

                                        if (!string.IsNullOrEmpty(tempCellValue))
                                        {
                                            foreach (var c in tempCellValue)
                                            {
                                                if (char.IsDigit(c))
                                                    tempOnlyDigits += c;
                                            }
                                            int tempNumber = 0;
                                            if (int.TryParse(tempOnlyDigits, out int result))
                                            {
                                                tempNumber = result;
                                                student.Course = tempNumber;
                                            }
                                            else
                                            {
                                                err.AppendLine("Строка " + row.RowNumber() + " - не удалось извлечь номер курса");
                                            }
                                        }
                                        else
                                        {
                                            err.AppendLine("Строка " + row.RowNumber() + " - не удалось извлечь номер курса");
                                        }
                                    }


                                    //Registration Required?|Нужна ли регистрация/прописка
                                    if (registrationColumn == 0)
                                        student.ResidenceRegistration = false;
                                    else
                                    {
                                        var tempCellValue = row.Cell(registrationColumn).Value.ToString().Trim().ToLower();
                                        tempCellValue = Regex.Replace(tempCellValue, @"\s+", " ");
                                        if (tempCellValue.Contains("да") || tempCellValue.Contains("+") || tempCellValue.Contains("1") || tempCellValue.Contains("есть") || tempCellValue.Contains("присутст"))
                                            student.ResidenceRegistration = true;
                                        else
                                            student.ResidenceRegistration = false;
                                    }


                                    //Groups|Группы
                                    //Educational organisations|Образовательные учреждения
                                    if (groupsColumn != 0)
                                    {
                                        if (row.Cell(groupsColumn).IsEmpty())
                                        {
                                            err.AppendLine("Строка " + row.RowNumber() + " - пустой номер группы");
                                        }
                                        else
                                        {
                                            var tempEducationalOrganisation = new EducationalOrganisations();
                                            if (row.Cell(educationalOrganisationColumn).IsEmpty())
                                            {
                                                err.AppendLine("Строка " + row.RowNumber() + " - не добавлена (не удалось извлечь образ. учреждение)");
                                                errs.Append(err.ToString());
                                                continue;
                                            }
                                            else
                                            {
                                                var tempEducationalOrgName = row.Cell(educationalOrganisationColumn).Value.ToString();
                                                tempEducationalOrgName = Regex.Replace(tempEducationalOrgName, @"[""''«»–—––]", " ");
                                                tempEducationalOrgName = tempEducationalOrgName.Trim();
                                                tempEducationalOrgName = Regex.Replace(tempEducationalOrgName, @"\s+", " ");
                                                if (educationalOrganisationsContext.Any(p => p.Name.ToLower() == tempEducationalOrgName.ToLower()))
                                                {
                                                    tempEducationalOrganisation = educationalOrganisationsContext.FirstOrDefault(p => p.Name.ToLower() == tempEducationalOrgName.ToLower());
                                                }
                                                else
                                                {
                                                    tempEducationalOrganisation.Name = tempEducationalOrgName;
                                                    educationalOrganisationsContext.Add(tempEducationalOrganisation);
                                                    App.Context.EducationalOrganisations.Add(tempEducationalOrganisation);
                                                }


                                                //Groups|Группы
                                                var tempGroupNumberName = row.Cell(groupsColumn).Value.ToString().Trim().ToLower();
                                                tempGroupNumberName = Regex.Replace(tempGroupNumberName, @"[./,""''«»<>()–—––]", "");
                                                tempGroupNumberName = tempGroupNumberName.Replace(" ", "");
                                                if (groupsContext.Any(p => p.GroupNumberName == tempGroupNumberName && p.EducationalOrganisations == tempEducationalOrganisation))
                                                {
                                                    student.Groups = groupsContext.FirstOrDefault(p => p.GroupNumberName == tempGroupNumberName && p.EducationalOrganisations == tempEducationalOrganisation);
                                                }
                                                else
                                                {
                                                    var tempGroup = new Groups();
                                                    tempGroup.GroupNumberName = tempGroupNumberName;
                                                    tempGroup.EducationalOrganisations = tempEducationalOrganisation;

                                                    if (row.Cell(specialityColumn).IsEmpty())
                                                    {
                                                        err.AppendLine("Строка " + row.RowNumber() + " - не добавлена (не удалось извлечь специальность)");
                                                        errs.Append(err.ToString());
                                                        continue;
                                                    }

                                                    else
                                                    {
                                                        var tempCellValue = row.Cell(specialityColumn).Value.ToString().Trim();
                                                        tempCellValue = Regex.Replace(tempCellValue, @"[–—––]", " ");
                                                        tempCellValue = Regex.Replace(tempCellValue, @"\s+", " ");

                                                        var tempSplitedValue = tempCellValue.Split(' ');
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
                                                                    tempSpeciality.SpecialityNumber = trimX;
                                                                }
                                                            }
                                                        }

                                                        if (tempSpeciality.SpecialityNumber != null)
                                                        {
                                                            if (specialitiesContext.Any(p => p.SpecialityNumber == tempSpeciality.SpecialityNumber))
                                                            {
                                                                tempSpeciality = specialitiesContext.FirstOrDefault(p => p.SpecialityNumber == tempSpeciality.SpecialityNumber);
                                                                tempGroup.Specialties = tempSpeciality;
                                                            }
                                                            else
                                                            {
                                                                if (tempSpeciality.SpecialityName != null)
                                                                {
                                                                    tempSpeciality.SpecialityName = tempSpeciality.SpecialityName.Trim('-', '*', '.', ',', '-', ' ');
                                                                    specialitiesContext.Add(tempSpeciality);
                                                                    App.Context.Specialties.Add(tempSpeciality);
                                                                    tempGroup.Specialties = tempSpeciality;
                                                                }
                                                                else
                                                                {
                                                                    err.AppendLine("Строка " + row.RowNumber() + " - не добавлена (не удалось извлечь название специальности)");
                                                                    errs.Append(err.ToString());
                                                                    continue;
                                                                }
                                                            }
                                                            student.Groups = tempGroup;
                                                            tempGroup.Students.Add(student);
                                                            groupsContext.Add(tempGroup);
                                                            App.Context.Groups.Add(tempGroup);
                                                        }
                                                        else
                                                        {
                                                            err.AppendLine("Строка " + row.RowNumber() + " - не добавлена (не удалось извлечь номер специальности)");
                                                            errs.Append(err.ToString());
                                                            continue;
                                                        }

                                                    }
                                                }
                                            }
                                        }
                                    }

                                    var studentBeforeEditing = new Students();
                                    if (student.Phone != null)
                                    {
                                        string tempOnlyDigits = string.Empty;
                                        foreach (var c in student.Phone)
                                        {
                                            if (char.IsDigit(c))
                                                tempOnlyDigits += c;
                                        }
                                        if (tempOnlyDigits.Length > 5)
                                        {
                                            foreach (var elem in studentsContext)
                                            {
                                                string tempOnlyDigitsTemp = string.Empty;
                                                foreach (var c in elem.Phone)
                                                {
                                                    if (char.IsDigit(c))
                                                        tempOnlyDigitsTemp += c;
                                                }
                                                if (tempOnlyDigitsTemp == tempOnlyDigits)
                                                {
                                                    studentBeforeEditing = elem;
                                                    isEditing = true;
                                                    break;
                                                }
                                            }
                                        }
                                    }


                                    //Add an agreement if not editing one|Добавляем договор, если не редактируем
                                    if (!isEditing)
                                    {
                                        studentsContext.Add(student);
                                        App.Context.Students.Add(student);
                                        studentsAddOrEditedList.Add(student);
                                        addedValues++;
                                    }
                                    else
                                    {
                                        bool studentChanged = false;
                                        student.ID = studentBeforeEditing.ID;

                                        if (student.Name != studentBeforeEditing.Name ||
                                            student.Surname != studentBeforeEditing.Surname ||
                                            student.Patronymic != studentBeforeEditing.Patronymic ||
                                            student.Phone != studentBeforeEditing.Phone ||
                                            student.ResidenceRegistration != studentBeforeEditing.ResidenceRegistration ||
                                            student.Groups != studentBeforeEditing.Groups ||
                                            student.Course != studentBeforeEditing.Course)
                                        {
                                            studentChanged = true;
                                        }


                                        if (studentChanged)
                                        {
                                            /*agreementBeforeEditing = agreement;*/
                                            studentBeforeEditing.Name = student.Name;
                                            studentBeforeEditing.Surname = student.Surname;
                                            studentBeforeEditing.Patronymic = student.Patronymic;
                                            studentBeforeEditing.Phone = student.Phone;
                                            studentBeforeEditing.ResidenceRegistration = student.ResidenceRegistration;
                                            studentBeforeEditing.Groups = student.Groups;
                                            studentBeforeEditing.Course = student.Course;

                                            studentsAddOrEditedList.Add(studentBeforeEditing);
                                            updatedValues++;
                                        }
                                        else
                                        {
                                            sameValue++;
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
                                viewDataGrid.ItemsSource = studentsAddOrEditedList;
                                cmbdtGroupNumber.ItemsSource = groupsContext;
                                foreach (var elem in groupsContext)
                                {
                                    MessageBox.Show(elem.GroupNumberName);
                                }
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
    }
}
