using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TicketAire.Desktop.DAL;
using TicketAire.Desktop.DAL.Models;


namespace TicketAire.Desktop
{
    public partial class AdminDashboard : Window
    {
        private readonly User _currentUser;
        private List<User> _allUsers;
        private List<Ticket> _allTickets;

        public AdminDashboard(User currentUser)
        {
            InitializeComponent();
            _currentUser = currentUser;
            DataContext = new { CurrentUser = _currentUser };
            LoadUsers();
        }

        private void TxtUserSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                LoadUsers(txtUserSearch.Text);
                e.Handled = true;
            }
        }

        private void TxtTicketSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                allTicketsDataGrid.ItemsSource = Database.GetAllTickets(txtTicketSearch.Text.Trim());
                e.Handled = true;
            }
        }


        private void LoadUsers(string search = "")
        {
            _allUsers = Database.GetAllUsers(search);
            usersDataGrid.ItemsSource = _allUsers;
        }
        private void LoadTickets(string search = "")
        {
            _allTickets = Database.GetAllTickets(search);
            allTicketsDataGrid.ItemsSource = _allTickets;
        }

        private void NavigateToUserManagement(object s, RoutedEventArgs e)
        {
            userManagementView.Visibility = Visibility.Visible;
            allTicketsView.Visibility = Visibility.Collapsed;
        }

        private void NavigateToAllTickets(object s, RoutedEventArgs e)
        {
            userManagementView.Visibility = Visibility.Collapsed;
            allTicketsView.Visibility = Visibility.Visible;
            LoadTickets(txtTicketSearch.Text.Trim());
        }

        private void EditUser_Click(object s, RoutedEventArgs e)
        {
            if (usersDataGrid.SelectedItem is User u)
            {
                var dlg = new AddEditUserWindow(u);
                dlg.Owner = this;
                dlg.ShowDialog();
                if (dlg.IsSaved)
                    LoadUsers(txtUserSearch.Text);
            }
        }

        private void DeleteUser_Click(object s, RoutedEventArgs e)
        {
            if (usersDataGrid.SelectedItem is User u)
            {
                if (MessageBox.Show($"Delete {u.FullName}?", "Confirm",
                     MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    Database.DeleteUser(u.Id);
                    LoadUsers(txtUserSearch.Text);
                }
            }
        }

        private void AddUser_Click(object s, RoutedEventArgs e)
        {
            var dlg = new AddEditUserWindow();
            dlg.Owner = this;
            dlg.ShowDialog();
            if (dlg.IsSaved)
                LoadUsers(txtUserSearch.Text);
        }

        private void Logout(object s, RoutedEventArgs e)
        {
            new LoginWindow().Show();
            this.Close();
        }

        private void ViewTicket_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && int.TryParse(btn.Tag.ToString(), out var ticketId))
            {
                var details = new TicketDetailsWindow(_currentUser, ticketId);
                details.Owner = this;
                details.ShowDialog();
            }
        }

    }
}
