using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
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
using StudentsInternship.Views.StudentsPages;

namespace StudentsInternship.Windows
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        
        public MainWindow()
        {
            InitializeComponent();
            frameMain.Navigate(new StudentsViewPage());
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
        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show($"Вы уверены, что хотите вернуться?\nНесохраненные данные могут быть утеряны",
                "Внимание", MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.Yes)
            {
                RejectChanges();
                frameMain.GoBack();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void FrameMain_ContentRendered(object sender, EventArgs e)
        {
/*            if (frameMain.CanGoBack)
            {
                btnBack.Visibility = Visibility.Visible;
            }
            else
            {
                btnBack.Visibility = Visibility.Collapsed;
            }*/
        }

        private void ButtonAgreements_Click(object sender, RoutedEventArgs e)
        {
            if (frameMain.Content.ToString() != "StudentsInternship.Views.AgreementsPages.AgreementsViewPage")
            {
                frameMain.Navigate(new Views.AgreementsPages.AgreementsViewPage());
            }
        }

        private void ButtonSpecialties_Click(object sender, RoutedEventArgs e)
        {
            if (frameMain.Content.ToString() != "StudentsInternship.Views.SpecialitiesPages.SpecialitiesViewPage")
            {
                frameMain.Navigate(new Views.SpecialitiesPages.SpecialitiesViewPage());
            }
        }

        private void ButtonStudents_Click(object sender, RoutedEventArgs e)
        {
            if (frameMain.Content.ToString() != "StudentsInternship.Views.StudentsPages.StudentsViewPage")
            {
                frameMain.Navigate(new Views.StudentsPages.StudentsViewPage());
            }
        }

        private void ButtonEduInstitutions_Click(object sender, RoutedEventArgs e)
        {
            if (frameMain.Content.ToString() != "StudentsInternship.Views.EduInstitutionsPages.EduInstitutionsViewPage")
            {
                frameMain.Navigate(new Views.EduInstitutionsPages.EduInstitutionsViewPage());
            }
        }

        private void ButtonPlacements_Click(object sender, RoutedEventArgs e)
        {

        }



        #region SidePanelControl
        private void BG_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Tg_Btn.IsChecked = false;
        }

        // Start: MenuLeft PopupButton //
        private void btnPracticeDistributions_MouseEnter(object sender, MouseEventArgs e)
        {
            if (Tg_Btn.IsChecked == false)
            {
                Popup.PlacementTarget = btnPracticeDistributions;
                Popup.Placement = PlacementMode.Right;
                Popup.IsOpen = true;
                Header.PopupText.Text = "Распределения";
            }
        }

        private void btnPracticeDistributions_MouseLeave(object sender, MouseEventArgs e)
        {
            Popup.Visibility = Visibility.Collapsed;
            Popup.IsOpen = false;
        }

        private void btnStudents_MouseEnter(object sender, MouseEventArgs e)
        {
            if (Tg_Btn.IsChecked == false)
            {
                Popup.PlacementTarget = btnStudents;
                Popup.Placement = PlacementMode.Right;
                Popup.IsOpen = true;
                Header.PopupText.Text = "Студенты";
            }
        }

        private void btnStudents_MouseLeave(object sender, MouseEventArgs e)
        {
            Popup.Visibility = Visibility.Collapsed;
            Popup.IsOpen = false;
        }

        private void btnAgreements_MouseEnter(object sender, MouseEventArgs e)
        {
            if (Tg_Btn.IsChecked == false)
            {
                Popup.PlacementTarget = btnAgreements;
                Popup.Placement = PlacementMode.Right;
                Popup.IsOpen = true;
                Header.PopupText.Text = "Договоры";
            }
        }

        private void btnAgreements_MouseLeave(object sender, MouseEventArgs e)
        {
            Popup.Visibility = Visibility.Collapsed;
            Popup.IsOpen = false;
        }

        private void btnPracticeSchedules_MouseEnter(object sender, MouseEventArgs e)
        {
            if (Tg_Btn.IsChecked == false)
            {
                Popup.PlacementTarget = btnPracticeSchedules;
                Popup.Placement = PlacementMode.Right;
                Popup.IsOpen = true;
                Header.PopupText.Text = "Расписания практик";
            }
        }

        private void btnPracticeSchedules_MouseLeave(object sender, MouseEventArgs e)
        {
            Popup.Visibility = Visibility.Collapsed;
            Popup.IsOpen = false;
        }

        private void btnPracticeSubjects_MouseEnter(object sender, MouseEventArgs e)
        {
            if (Tg_Btn.IsChecked == false)
            {
                Popup.PlacementTarget = btnPracticeSubjects;
                Popup.Placement = PlacementMode.Right;
                Popup.IsOpen = true;
                Header.PopupText.Text = "Темы практик";
            }
        }

        private void btnPracticeSubjects_MouseLeave(object sender, MouseEventArgs e)
        {
            Popup.Visibility = Visibility.Collapsed;
            Popup.IsOpen = false;
        }

        private void btnGroups_MouseEnter(object sender, MouseEventArgs e)
        {
            if (Tg_Btn.IsChecked == false)
            {
                Popup.PlacementTarget = btnGroups;
                Popup.Placement = PlacementMode.Right;
                Popup.IsOpen = true;
                Header.PopupText.Text = "Группы";
            }
        }

        private void btnGroups_MouseLeave(object sender, MouseEventArgs e)
        {
            Popup.Visibility = Visibility.Collapsed;
            Popup.IsOpen = false;
        }

        private void btnSpecialties_MouseEnter(object sender, MouseEventArgs e)
        {
            if (Tg_Btn.IsChecked == false)
            {
                Popup.PlacementTarget = btnSpecialties;
                Popup.Placement = PlacementMode.Right;
                Popup.IsOpen = true;
                Header.PopupText.Text = "Специальности";
            }
        }

        private void btnSpecialties_MouseLeave(object sender, MouseEventArgs e)
        {
            Popup.Visibility = Visibility.Collapsed;
            Popup.IsOpen = false;
        }

        private void btnEducationalOrganisations_MouseEnter(object sender, MouseEventArgs e)
        {
            if (Tg_Btn.IsChecked == false)
            {
                Popup.PlacementTarget = btnEducationalOrganisations;
                Popup.Placement = PlacementMode.Right;
                Popup.IsOpen = true;
                Header.PopupText.Text = "Учебные заведения";
            }
        }

        private void btnEducationalOrganisations_MouseLeave(object sender, MouseEventArgs e)
        {
            Popup.Visibility = Visibility.Collapsed;
            Popup.IsOpen = false;
        }
        private void btnSetting_MouseEnter(object sender, MouseEventArgs e)
        {
            if (Tg_Btn.IsChecked == false)
            {
                Popup.PlacementTarget = btnSetting;
                Popup.Placement = PlacementMode.Right;
                Popup.IsOpen = true;
                Header.PopupText.Text = "Настройки";
            }
        }
        private void btnSetting_MouseLeave(object sender, MouseEventArgs e)
        {
            Popup.Visibility = Visibility.Collapsed;
            Popup.IsOpen = false;
        }

        #endregion

        private void btnPracticeDistributions_Click(object sender, RoutedEventArgs e)
        {
            if (frameMain.Content.ToString() != "StudentsInternship.Views.StudentDistributionsPages.StudentDistributionsViewPage")
            {
                frameMain.Navigate(new Views.StudentDistributionsPages.StudentDistributionsViewPage());
            }
        }

        private void btnAgreements_Click(object sender, RoutedEventArgs e)
        {
            if (frameMain.Content.ToString() != "StudentsInternship.Views.AgreementsPages.AgreementsViewPage")
            {
                frameMain.Navigate(new Views.AgreementsPages.AgreementsViewPage());
            }
        }

        private void btnStudents_Click(object sender, RoutedEventArgs e)
        {
            if (frameMain.Content.ToString() != "StudentsInternship.Views.StudentsPages.StudentsViewPage")
            {
                frameMain.Navigate(new Views.StudentsPages.StudentsViewPage());
            }
        }

        private void btnPracticeSchedules_Click(object sender, RoutedEventArgs e)
        {
            if (frameMain.Content.ToString() != "StudentsInternship.Views.PracticeSchedulesPages.PracticeSchedulesViewPage")
            {
                frameMain.Navigate(new Views.PracticeSchedulesPages.PracticeSchedulesViewPage());
            }
        }

        private void btnPracticeSubjects_Click(object sender, RoutedEventArgs e)
        {
            if (frameMain.Content.ToString() != "StudentsInternship.Views.PracticeSubjectsPages.PracticeSubjectsViewPage")
            {
                frameMain.Navigate(new Views.PracticeSubjectsPages.PracticeSubjectsViewPage());
            }
        }

        private void btnGroups_Click(object sender, RoutedEventArgs e)
        {
            if (frameMain.Content.ToString() != "StudentsInternship.Views.GroupsPages.GroupsViewPage")
            {
                frameMain.Navigate(new Views.GroupsPages.GroupsViewPage());
            }
        }

        private void btnSpecialties_Click(object sender, RoutedEventArgs e)
        {
            if (frameMain.Content.ToString() != "StudentsInternship.Views.SpecialitiesPages.SpecialitiesViewPage")
            {
                frameMain.Navigate(new Views.SpecialitiesPages.SpecialitiesViewPage());
            }
        }

        private void btnEducationalOrganisations_Click(object sender, RoutedEventArgs e)
        {
            if (frameMain.Content.ToString() != "StudentsInternship.Views.EduInstitutionsPages.EduInstitutionsViewPage")
            {
                frameMain.Navigate(new Views.EduInstitutionsPages.EduInstitutionsViewPage());
            }
        }
    }
}