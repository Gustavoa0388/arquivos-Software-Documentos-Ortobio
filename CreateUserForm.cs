using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace DocumentosOrtobio
{
    public partial class CreateUserForm : Form
    {
        private readonly Dictionary<string, List<string>> categoriesWithSubmenus = new Dictionary<string, List<string>>
        {
            { "Documentos Vigentes", new List<string> { "DT", "EC", "EMF", "GR", "NP", "RM", "RMP", "SF" } },
            { "Documentos Obsoletos", new List<string> { "DT", "EC", "EMF", "GR", "NP", "RM", "RMP", "SF" } }
        };

        public CreateUserForm()
        {
            InitializeComponent();
            PopulateCheckedListBox();
        }

        private void PopulateCheckedListBox()
        {
            checkedListBoxCategories.Items.Clear();
            foreach (var category in categoriesWithSubmenus.Keys)
            {
                checkedListBoxCategories.Items.Add(category);
                foreach (var subCategory in categoriesWithSubmenus[category])
                {
                    checkedListBoxCategories.Items.Add("  " + subCategory);
                }
            }
        }

        private void BtnCreateUser_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text;
            string password = txtPassword.Text;
            string role = cmbRole.SelectedItem.ToString();

            var users = JsonConvert.DeserializeObject<List<User>>(File.ReadAllText("users.json"));

            if (users.Any(u => u.Username == username))
            {
                MessageBox.Show("Usuário já existe!");
                return;
            }

            var selectedPermissions = new List<string>();
            foreach (string item in checkedListBoxCategories.CheckedItems)
            {
                selectedPermissions.Add(item.Trim());
            }

            var newUser = new User { Username = username, Password = password, Role = role };
            users.Add(newUser);

            var userPermissions = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(File.ReadAllText("userPermissions.json"));
            userPermissions[username] = selectedPermissions;

            File.WriteAllText("users.json", JsonConvert.SerializeObject(users, Formatting.Indented));
            File.WriteAllText("userPermissions.json", JsonConvert.SerializeObject(userPermissions, Formatting.Indented));

            MessageBox.Show("Usuário criado com sucesso!");
            this.Close();
        }

        private void BtnViewUsers_Click(object sender, EventArgs e)
        {
            ViewUsersForm viewUsersForm = new ViewUsersForm();
            viewUsersForm.ShowDialog();
        }
    }
}