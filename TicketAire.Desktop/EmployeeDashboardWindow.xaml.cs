using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using TicketAire.Desktop.DAL;
using TicketAire.Desktop.DAL.Models;

namespace TicketAire.Desktop
{
    public partial class EmployeeDashboard : Window
    {
        private readonly User _currentUser;
        private List<Category> _cats;

        public EmployeeDashboard(User currentUser)
        {
            InitializeComponent();
            _currentUser = currentUser;
            DataContext = new { CurrentUser = _currentUser };
            LoadMyTickets();
            LoadCategories();
        }

        private void LoadMyTickets()
        {
            var tickets = Database.GetMyTickets(_currentUser.Id);
            myTicketsDataGrid.ItemsSource = tickets;
        }

        private void LoadCategories()
        {
            _cats = Database.GetAllCategories();
            cmbCategory.ItemsSource = _cats;
            cmbCategory.DisplayMemberPath = "Name";
            cmbCategory.SelectedValuePath = "Id";
        }

        private void NavigateToMyTickets(object s, RoutedEventArgs e)
        {
            myTicketsView.Visibility = Visibility.Visible;
            createTicketView.Visibility = Visibility.Collapsed;
        }

        private void NavigateToCreateTicket(object s, RoutedEventArgs e)
        {
            myTicketsView.Visibility = Visibility.Collapsed;
            createTicketView.Visibility = Visibility.Visible;
        }

        private void SubmitTicket_Click(object s, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTicketTitle.Text) ||
                cmbCategory.SelectedValue == null ||
                string.IsNullOrWhiteSpace(txtDescription.Text))
            {
                MessageBox.Show("Please fill all fields", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Database.CreateTicket(
                _currentUser.Id,
                (int)cmbCategory.SelectedValue,
                txtTicketTitle.Text.Trim(),
                txtDescription.Text.Trim(),
                ((ComboBoxItem)cmbPriority.SelectedItem).Content.ToString().ToLower()
            );
            MessageBox.Show("Ticket submitted!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            LoadMyTickets();
            NavigateToMyTickets(null, null);
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
private void Logout(object s, RoutedEventArgs e)
        {
            new LoginWindow().Show();
            this.Close();
        }
    }
}
