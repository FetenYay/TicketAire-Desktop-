using System.Windows;
using TicketAire.Desktop.DAL;
using TicketAire.Desktop.DAL.Models;

namespace TicketAire.Desktop
{
    public partial class TicketDetailsWindow : Window
    {
        private readonly User _currentUser;
        private readonly int _ticketId;

        public TicketDetailsWindow(User currentUser, int ticketId)
        {
            InitializeComponent();
            _currentUser = currentUser;
            _ticketId = ticketId;

            ConfigureUiByRole();
            LoadTicketAndComments();
        }

        private void LoadTicketAndComments()
        {
            var ticket = Database.GetTicketById(_ticketId);
            TitleTextBlock.Text = ticket.Title;
            CategoryTextBlock.Text = ticket.CategoryName;
            PriorityTextBlock.Text = ticket.Priority;
            StatusTextBlock.Text = ticket.Status;

            bool isTech = _currentUser.Role.Equals("technician", StringComparison.OrdinalIgnoreCase);
            bool alreadyResolved = ticket.Status.Equals("Resolved", StringComparison.OrdinalIgnoreCase);

            if (!isTech)
            {
                btnResolve.IsEnabled = false;
                btnResolve.Content = "Resolved";
            }
            else if (alreadyResolved)
            {
                btnResolve.IsEnabled = false;
                btnResolve.Content = "Resolved";
            }
            else
            {
                btnResolve.IsEnabled = true;
                btnResolve.Content = "Resolve";
            }

            var comments = Database.GetCommentsForTicket(_ticketId)
                .Select(c => new
                {
                    AuthorName = c.UserName,
                    CreatedAt = c.CreatedAt.ToString("g"),
                    c.Content,
                    c.IsSolution
                })
                .ToList();

            CommentsDataGrid.ItemsSource = comments;
        }



        private void Resolve_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to mark this ticket as resolved?",
                                "Confirm Resolve",
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            Database.ResolveTicket(_ticketId);

            
            MessageBox.Show("Ticket has been marked as resolved.",
                            "Resolved",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);

            StatusTextBlock.Text = "Resolved";

            btnResolve.IsEnabled = false;

            this.Close();
        }


        private void ConfigureUiByRole()
        {
            switch (_currentUser.Role.ToLower())
            {
                case "employee":
                    CommentPanel.Visibility = Visibility.Visible;
                    TechActionsPanel.Visibility = Visibility.Collapsed;
                    break;

                case "technician":
                    CommentPanel.Visibility = Visibility.Visible;
                    TechActionsPanel.Visibility = Visibility.Visible;
                    break;

                case "admin":
                    CommentPanel.Visibility = Visibility.Collapsed;
                    TechActionsPanel.Visibility = Visibility.Collapsed;
                    break;

                default:
                    CommentPanel.Visibility = Visibility.Collapsed;
                    TechActionsPanel.Visibility = Visibility.Collapsed;
                    break;
            }
        }

        private void AddComment_Click(object sender, RoutedEventArgs e)
        {
            var text = txtNewComment.Text.Trim();
            if (string.IsNullOrEmpty(text))
            {
                MessageBox.Show("Enter a comment before adding.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            char solFlag;
            if (chkIsSolution.IsVisible && chkIsSolution.IsChecked == true)
                solFlag = 'Y';
            else
                solFlag = 'N';

            Database.AddComment(_ticketId, _currentUser.Id, text, solFlag);

            txtNewComment.Clear();
            chkIsSolution.IsChecked = false;

            LoadTicketAndComments();
        }






    }
}
