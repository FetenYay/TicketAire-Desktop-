using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using TicketAire.Desktop.DAL;
using TicketAire.Desktop.DAL.Models;

namespace TicketAire.Desktop
{
    public partial class TechnicianDashboard : Window
    {
        private readonly User _currentUser;

        public TechnicianDashboard(User currentUser)
        {
            InitializeComponent();
            _currentUser = currentUser;
            DataContext = new { CurrentUser = _currentUser };
            LoadAssignedTickets();
            LoadTicketHistory();
        }

        private void LoadAssignedTickets()
        {
            var tickets = Database.GetAssignedTickets(_currentUser.Id);
            ticketsDataGrid.ItemsSource = tickets;
        }

        private void LoadTicketHistory()
        {
            var history = Database.GetAllTicketHistory()
                .Select(h => new
                {
                    h.TicketId,
                    h.OldStatus,
                    h.NewStatus,
                    ChangedAt = h.ChangedAt.ToString("g")
                })
                .ToList();

            historyDataGrid.ItemsSource = history;
        }

        private void NavigateToAssignedTickets(object s, RoutedEventArgs e)
        {
            assignedTicketsView.Visibility = Visibility.Visible;
            ticketHistoryView.Visibility = Visibility.Collapsed;
        }

        private void NavigateToTicketHistory(object s, RoutedEventArgs e)
        {
            assignedTicketsView.Visibility = Visibility.Collapsed;
            ticketHistoryView.Visibility = Visibility.Visible;
        
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
