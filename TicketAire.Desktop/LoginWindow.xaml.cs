using System.Windows;
using System.Windows.Input;
using TicketAire.Desktop.DAL;
using TicketAire.Desktop.DAL.Models;

namespace TicketAire.Desktop
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }


        private void LogIn_Click(object sender, RoutedEventArgs e)
        {
            lblError.Visibility = Visibility.Collapsed;
            var eid = txtEnterpriseId.Text.Trim();
            var key = txtAccessKey.Password.Trim();
            if (eid == "" || key == "")
            {
                lblError.Text = "Please enter both credentials";
                lblError.Visibility = Visibility.Visible;
                return;
            }

            User user = Database.AuthenticateUser(eid, key);
            if (user == null)
            {
                lblError.Text = "Invalid credentials";
                lblError.Visibility = Visibility.Visible;
                return;
            }

            Window dash = user.Role switch
            {
                "admin" => new AdminDashboard(user),
                "technician" => new TechnicianDashboard(user),
                "employee" => new EmployeeDashboard(user),
                _ => null
            };
            dash.Show();
            this.Close();
        }
    }
}
