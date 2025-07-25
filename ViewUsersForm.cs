﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace DocumentosOrtobio
{
    public partial class ViewUsersForm : Form
    {
        private readonly Dictionary<string, List<string>> categoriesWithSubmenus = new Dictionary<string, List<string>>
        {
            { "Documentos Vigentes", new List<string> { "DT", "EC", "EMF", "GR", "NP", "RM", "RMP", "SF" } },
            { "Documentos Obsoletos", new List<string> { "DT", "EC", "EMF", "GR", "NP", "RM", "RMP", "SF" } }
        };

        public ViewUsersForm()
        {
            InitializeComponent();
            LoadUsers();
        }

        private void LoadUsers()
        {
            var users = JsonConvert.DeserializeObject<List<User>>(File.ReadAllText("users.json"));
            listBoxUsers.Items.Clear();
            foreach (var user in users)
            {
                listBoxUsers.Items.Add(user.Username);
            }
        }

        private void ListBoxUsers_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBoxUsers.SelectedItem != null)
            {
                string selectedUsername = listBoxUsers.SelectedItem.ToString();
                var users = JsonConvert.DeserializeObject<List<User>>(File.ReadAllText("users.json"));
                var user = users.First(u => u.Username == selectedUsername);
                txtUsername.Text = user.Username;
                txtPassword.Text = user.Password;
                cmbRole.SelectedItem = user.Role;

                var userPermissions = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(File.ReadAllText("userPermissions.json"));
                PopulateCheckedListBox();
                CheckUserPermissions(userPermissions[selectedUsername]);
            }
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

        private void CheckUserPermissions(List<string> userPermissions)
        {
            for (int i = 0; i < checkedListBoxCategories.Items.Count; i++)
            {
                string item = checkedListBoxCategories.Items[i].ToString().Trim();
                checkedListBoxCategories.SetItemChecked(i, userPermissions.Contains(item));
            }
        }

        private void BtnSaveChanges_Click(object sender, EventArgs e)
        {
            string selectedUsername = listBoxUsers.SelectedItem.ToString();
            var users = JsonConvert.DeserializeObject<List<User>>(File.ReadAllText("users.json"));
            var user = users.First(u => u.Username == selectedUsername);

            user.Username = txtUsername.Text;
            user.Password = txtPassword.Text;
            user.Role = cmbRole.SelectedItem.ToString();

            var selectedPermissions = new List<string>();
            foreach (string item in checkedListBoxCategories.CheckedItems)
            {
                selectedPermissions.Add(item.Trim());
            }

            var userPermissions = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(File.ReadAllText("userPermissions.json"));
            userPermissions[selectedUsername] = selectedPermissions;

            File.WriteAllText("users.json", JsonConvert.SerializeObject(users, Formatting.Indented));
            File.WriteAllText("userPermissions.json", JsonConvert.SerializeObject(userPermissions, Formatting.Indented));

            MessageBox.Show("Usuário atualizado com sucesso!");
        }

        private void BtnDeleteUser_Click(object sender, EventArgs e)
        {
            if (listBoxUsers.SelectedItem != null)
            {
                string selectedUsername = listBoxUsers.SelectedItem.ToString();
                var users = JsonConvert.DeserializeObject<List<User>>(File.ReadAllText("users.json"));
                var userPermissions = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(File.ReadAllText("userPermissions.json"));

                users.RemoveAll(u => u.Username == selectedUsername);
                userPermissions.Remove(selectedUsername);

                File.WriteAllText("users.json", JsonConvert.SerializeObject(users, Formatting.Indented));
                File.WriteAllText("userPermissions.json", JsonConvert.SerializeObject(userPermissions, Formatting.Indented));

                LoadUsers();

                MessageBox.Show("Usuário excluído com sucesso!");
            }
            else
            {
                MessageBox.Show("Por favor, selecione um usuário para excluir.");
            }
        }
    }
}