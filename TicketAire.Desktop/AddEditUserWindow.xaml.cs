using System;
using System.Windows;
using System.Windows.Controls;
using TicketAire.Desktop.DAL;
using TicketAire.Desktop.DAL.Models;

namespace TicketAire.Desktop
{
    public partial class AddEditUserWindow : Window
    {
        private readonly User _user;
        public bool IsSaved { get; private set; }

        public AddEditUserWindow(User user = null)
        {
            InitializeComponent();
            _user = user;
            if (_user != null)
            {
                txtEnterpriseId.Text = _user.EnterpriseId;
                txtFirstName.Text = _user.FirstName;
                txtLastName.Text = _user.LastName;
                txtEmail.Text = _user.Email;
                foreach (ComboBoxItem item in cmbRole.Items)
                    if ((string)item.Content == _user.Role)
                        cmbRole.SelectedItem = item;
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtEnterpriseId.Text)
                || string.IsNullOrWhiteSpace(txtFirstName.Text)
                || string.IsNullOrWhiteSpace(txtLastName.Text)
                || string.IsNullOrWhiteSpace(txtEmail.Text)
                || cmbRole.SelectedItem == null)
            {
                MessageBox.Show("All fields are required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var role = ((ComboBoxItem)cmbRole.SelectedItem).Content.ToString();

            if (_user == null)
            {
                Database.CreateUser(
                    txtEnterpriseId.Text.Trim(),
                    txtFirstName.Text.Trim(),
                    txtLastName.Text.Trim(),
                    txtEmail.Text.Trim(),
                    role);
            }
            else
            {
                _user.EnterpriseId = txtEnterpriseId.Text.Trim();
                _user.FirstName = txtFirstName.Text.Trim();
                _user.LastName = txtLastName.Text.Trim();
                _user.Email = txtEmail.Text.Trim();
                _user.Role = role;
                Database.UpdateUser(_user);
            }

            IsSaved = true;
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
