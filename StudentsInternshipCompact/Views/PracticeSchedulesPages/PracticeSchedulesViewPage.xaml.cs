using StudentsInternship.Models;
using StudentsInternship.Windows;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
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

namespace StudentsInternship.Views.PracticeSchedulesPages
{
    /// <summary>
    /// Логика взаимодействия для PracticeSchedulesViewPage.xaml
    /// </summary>
    public partial class PracticeSchedulesViewPage : Page
    {
        List<PracticeSchedules> datagridSourceList = new List<PracticeSchedules>();
        private int PagesCount;
        private int NumberOfPage = 0;
        private int maxItemShow = 100;
        public PracticeSchedulesViewPage()
        {
            InitializeComponent();

            dtpStartDate.SelectedDate = DateTime.Today;
            dtpStartDate.DisplayDateStart = new DateTime(1950, 1, 1);
            dtpStartDate.DisplayDateEnd = DateTime.Today;
            dtpEndDate.SelectedDate = DateTime.Today;
            dtpEndDate.DisplayDateStart = new DateTime(1950, 1, 1);

            List<Groups> cmbFill = new List<Groups>();
            var tempEduInst = new EducationalOrganisations();
            tempEduInst.Name = "";
            var firstItem = new Groups();
            firstItem.GroupNumberName = "Выберите группу";
            firstItem.EducationalOrganisations = tempEduInst;
            cmbFill.Add(firstItem);
            cmbFill.AddRange(App.Context.Groups.ToList());
            cmbFilter.ItemsSource = cmbFill;
            ///////////////
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
            windowAddEdit.frameAddEdit.Navigate(new PracticeSchedulesAddEditPage(null));
            windowAddEdit.Closed += (s, EventArgs) =>
            {
                UpdateDataGrid();
            };
            windowAddEdit.Show();
        }
        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (viewDataGrid.SelectedItems.Count > 0)
            {
                var elemsToDelete = viewDataGrid.SelectedItems.Cast<PracticeSchedules>().ToList();
                if (MessageBox.Show($"Вы точно хотите удалить следующие {elemsToDelete.Count()} элементов?", "Внимание",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    try
                    {
                        App.Context.PracticeSchedules.RemoveRange(elemsToDelete);
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
            if (viewDataGrid.SelectedItems.Count > 0)
            {
                AddEditWindow windowAddEdit = new AddEditWindow();
                windowAddEdit.frameAddEdit.Navigate(new PracticeSchedulesAddEditPage(viewDataGrid.SelectedItem as PracticeSchedules));
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
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                if (ex.InnerException != null)
                    MessageBox.Show(ex.InnerException.ToString());
                RejectChanges();
            }
        }

        //DataGrid Fill 
        private void UpdateDataGrid()
        {
            cmbdtGroups.ItemsSource = App.Context.Groups.ToList();
            cmbdtPracticeSubject.ItemsSource = App.Context.PracticeSubjects.ToList();

            datagridSourceList = App.Context.PracticeSchedules.ToList();
            // Filtration
            if (cmbFilter.SelectedIndex > 0)
            {
                datagridSourceList = datagridSourceList.Where(p => p.Groups == cmbFilter.SelectedItem).ToList();
            }
            //Search
            datagridSourceList = datagridSourceList.Where(p => p.Groups.GroupNumberName.ToLower().Contains(txtSearch.Text.ToLower()) ||
                p.Groups.EducationalOrganisations.Name.ToLower().Contains(txtSearch.Text.ToLower()) ||
                p.PracticeSubjects.PracticeSubjectName.ToLower().Contains(txtSearch.Text.ToLower()) ||
                p.Groups.Specialties.NumberNameSpeciality.ToLower().Contains(txtSearch.Text.ToLower())
            ).ToList();
            //Date filtration
            if (dtpStartDate.SelectedDate != DateTime.Today || dtpEndDate.SelectedDate != DateTime.Today)
            {
                datagridSourceList = datagridSourceList.Where(p => p.PracticeStartDate >= dtpStartDate.SelectedDate && p.PracticeEndDate <= dtpEndDate.SelectedDate).ToList();
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
                    case "Группа":
                        AddSortingVar("GroupNumber", column);
                        break;
                    case "Даты начала":
                        AddSortingVar("PracticeStartDate", column);
                        break;
                    case "Даты окончания":
                        AddSortingVar("PracticeEndDate", column);
                        break;
                    case "Тема":
                        AddSortingVar("SubjectName", column);
                        break;
                    case "Образовательная организация":
                        AddSortingVar("EducationalOrganisationsName", column);
                        break;
                }
            }
        }

        //Sorting Method
        private void Sorting()
        {
            if (sort.Count > 0)
            {
                IOrderedEnumerable<PracticeSchedules> SortingList = null;
                foreach (SortElem elem in sort)
                {
                    switch (elem.SortName)
                    {
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
                        case "PracticeStartDate":
                            if (elem.SortAscending)
                            {
                                if (SortingList != null)
                                    SortingList = SortingList.ThenBy(p => p.PracticeStartDate);
                                else
                                    SortingList = datagridSourceList.OrderBy(p => p.PracticeStartDate);
                                break;
                            }
                            else
                            {
                                if (SortingList != null)
                                    SortingList = SortingList.ThenByDescending(p => p.PracticeStartDate);
                                else
                                    SortingList = datagridSourceList.OrderByDescending(p => p.PracticeStartDate);
                                break;
                            }
                        case "PracticeEndDate":
                            if (elem.SortAscending)
                            {
                                if (SortingList != null)
                                    SortingList = SortingList.ThenBy(p => p.PracticeEndDate);
                                else
                                    SortingList = datagridSourceList.OrderBy(p => p.PracticeEndDate);
                                break;
                            }
                            else
                            {
                                if (SortingList != null)
                                    SortingList = SortingList.ThenByDescending(p => p.PracticeEndDate);
                                else
                                    SortingList = datagridSourceList.OrderByDescending(p => p.PracticeEndDate);
                                break;
                            }
                        case "SubjectName":
                            if (elem.SortAscending)
                            {
                                if (SortingList != null)
                                    SortingList = SortingList.ThenBy(p => p.PracticeSubjects.PracticeSubjectName);
                                else
                                    SortingList = datagridSourceList.OrderBy(p => p.PracticeSubjects.PracticeSubjectName);
                                break;
                            }
                            else
                            {
                                if (SortingList != null)
                                    SortingList = SortingList.ThenByDescending(p => p.PracticeSubjects.PracticeSubjectName);
                                else
                                    SortingList = datagridSourceList.OrderByDescending(p => p.PracticeSubjects.PracticeSubjectName);
                                break;
                            }
                        case "EducationalOrganisations":
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
                        case null:
                        case "":
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
    }
}
