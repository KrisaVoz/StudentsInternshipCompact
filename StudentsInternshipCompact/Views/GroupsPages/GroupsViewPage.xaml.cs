using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
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
using StudentsInternship.Models;
using StudentsInternship.Windows;

namespace StudentsInternship.Views.GroupsPages
{
    /// <summary>
    /// Логика взаимодействия для GroupsViewPage.xaml
    /// </summary>
    public partial class GroupsViewPage : Page
    {
        List<Groups> datagridSourceList = new List<Groups>();
        private int PagesCount;
        private int NumberOfPage = 0;
        private int maxItemShow = 100;
        public GroupsViewPage()
        {
            InitializeComponent();

            List<EducationalOrganisations> cmbFill = new List<EducationalOrganisations>();
            var firstItem = new EducationalOrganisations();
            firstItem.Name = "Выберите организацию";
            cmbFill.Add(firstItem);
            cmbFill.AddRange(App.Context.EducationalOrganisations.ToList());
            cmbFilter.ItemsSource = cmbFill;
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
            windowAddEdit.frameAddEdit.Navigate(new GroupsAddEditPage(null));
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
                var elemsToDelete = viewDataGrid.SelectedItems.Cast<Groups>().ToList();
                if (MessageBox.Show($"Вы точно хотите удалить следующие {elemsToDelete.Count()} элементов?", "Внимание",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    try
                    {
                        App.Context.Groups.RemoveRange(elemsToDelete);
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
                windowAddEdit.frameAddEdit.Navigate(new GroupsAddEditPage(viewDataGrid.SelectedItem as Groups));
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
            cmbdtEducationalOrganisation.ItemsSource = App.Context.EducationalOrganisations.ToList();
            cmbdtSpecialty.ItemsSource = App.Context.Specialties.ToList();

            datagridSourceList = App.Context.Groups.ToList();
            //Search
            datagridSourceList = datagridSourceList.Where(p => p.GroupNumberName.ToLower().Contains(txtSearch.Text.ToLower()) ||
                p.Specialties.SpecialityNumber.ToLower().Contains(txtSearch.Text.ToLower()) ||
                p.Specialties.SpecialityName.ToLower().Contains(txtSearch.Text.ToLower()) ||
                p.EducationalOrganisations.Name.ToLower().Contains(txtSearch.Text.ToLower())
            ).ToList();
            if (cmbFilter.SelectedIndex > 0)
            {
                datagridSourceList = datagridSourceList.Where(p => p.EducationalOrganisations == cmbFilter.SelectedItem).ToList();
            }


            //Items Counter
            tbItemCounter.Text = datagridSourceList.Count.ToString() + " из " + App.Context.Groups.Count().ToString();

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
                    case "Образовательное учреждение":
                        AddSortingVar("EducationalOrganisation", column);
                        break;
                    case "Специальность":
                        AddSortingVar("Speciality", column);
                        break;
                    case "Номер группы":
                        AddSortingVar("GroupNumber", column);
                        break;
                }
            }
        }

        //Sorting Method
        private void Sorting()
        {
            if (sort.Count > 0)
            {
                IOrderedEnumerable<Groups> SortingList = null;
                foreach (SortElem elem in sort)
                {
                    switch (elem.SortName)
                    {
                        case "Speciality":
                            if (elem.SortAscending)
                            {
                                if (SortingList != null)
                                    SortingList = SortingList.ThenByDescending(p => p.Specialties.SpecialityNumber).ThenByDescending(p => p.Specialties.SpecialityName);
                                else
                                    SortingList = datagridSourceList.OrderByDescending(p => p.Specialties.SpecialityNumber).ThenByDescending(p => p.Specialties.SpecialityName);
                                break;
                            }
                            else
                            {
                                if (SortingList != null)
                                    SortingList = SortingList.ThenBy(p => p.Specialties.SpecialityNumber).ThenBy(p => p.Specialties.SpecialityName);
                                else
                                    SortingList = datagridSourceList.OrderBy(p => p.Specialties.SpecialityNumber).ThenBy(p => p.Specialties.SpecialityName);
                                break;
                            }
                        case "GroupNumber":
                            if (elem.SortAscending)
                            {
                                if (SortingList != null)
                                    SortingList = SortingList.ThenBy(p => p.GroupNumberName);
                                else
                                    SortingList = datagridSourceList.OrderBy(p => p.GroupNumberName);
                                break;
                            }
                            else
                            {
                                if (SortingList != null)
                                    SortingList = SortingList.ThenByDescending(p => p.GroupNumberName);
                                else
                                    SortingList = datagridSourceList.OrderByDescending(p => p.GroupNumberName);
                                break;
                            }
                        case "EducationalOrganisation":
                            if (elem.SortAscending)
                            {
                                if (SortingList != null)
                                    SortingList = SortingList.ThenBy(p => p.EducationalOrganisations.Name);
                                else
                                    SortingList = datagridSourceList.OrderBy(p => p.EducationalOrganisations.Name);
                                break;
                            }
                            else
                            {
                                if (SortingList != null)
                                    SortingList = SortingList.ThenByDescending(p => p.EducationalOrganisations.Name);
                                else
                                    SortingList = datagridSourceList.OrderByDescending(p => p.EducationalOrganisations.Name);
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
            txtSearch.Text = "";
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
    }
}
