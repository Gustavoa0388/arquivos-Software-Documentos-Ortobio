using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;
using Newtonsoft.Json;
using PdfiumViewer;

namespace DocumentosOrtobio
{
    public partial class Form1 : Form
    {
        private readonly User loggedUser;
        private string currentFilePath1;
        private string currentFilePath2;

        private readonly Dictionary<string, List<string>> userPermissions;

        private readonly Dictionary<string, List<string>> categoriesWithSubmenus = new Dictionary<string, List<string>>
        {
            { "Documentos Vigentes", new List<string> { "DT", "EC", "EMF", "GR", "NP", "RM", "RMP", "SF" } },
            { "Documentos Obsoletos", new List<string> { "DT", "EC", "EMF", "GR", "NP", "RM", "RMP", "SF" } }
        };

        public Form1(User user)
        {
            InitializeComponent();
            loggedUser = user;
            userPermissions = LoadUserPermissions();

            LogActivity("Login");

            btnSettings.Visible = loggedUser.Role == "admin";
            ConfigurePdfViewerPermissions();
            PopulateCategoryComboBox(comboBoxCategory1);
            PopulateCategoryComboBox(comboBoxCategory2);
        }

        private Dictionary<string, List<string>> LoadUserPermissions()
        {
            return JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(File.ReadAllText("userPermissions.json"));
        }

        private void ConfigurePdfViewerPermissions()
        {
            if (loggedUser.Role != "admin")
            {
                pdfViewer1.ShowToolbar = false;
                pdfViewer2.ShowToolbar = false;
            }
        }

        private void PopulateCategoryComboBox(ComboBox comboBox)
        {
            comboBox.Items.Clear();
            comboBox.Items.Add("All Categories");

            foreach (var category in categoriesWithSubmenus.Keys)
            {
                if (UserCanAccessCategory(category))
                {
                    comboBox.Items.Add(category);
                }
            }

            comboBox.SelectedIndex = 0;
        }

        private void ComboBoxCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            var comboBox = sender as ComboBox;
            if (comboBox != null)
            {
                var subCategoryComboBox = comboBox == comboBoxCategory1 ? comboBoxSubCategory1 : comboBoxSubCategory2;
                PopulateSubCategoryComboBox(subCategoryComboBox, comboBox.SelectedItem.ToString());
            }
        }

        private void PopulateSubCategoryComboBox(ComboBox comboBox, string category)
        {
            comboBox.Items.Clear();
            comboBox.Items.Add("All Subcategories");

            if (categoriesWithSubmenus.ContainsKey(category))
            {
                foreach (var subCategory in categoriesWithSubmenus[category])
                {
                    if (UserCanAccessSubCategory(category, subCategory))
                    {
                        comboBox.Items.Add(subCategory);
                    }
                }
            }

            comboBox.SelectedIndex = 0;
        }

        private void ButtonSearch1_Click(object sender, EventArgs e)
        {
            LogActivity("Buscou na Categoria 1");
            string searchPattern = textBoxSearch1.Text;
            string selectedCategory = comboBoxCategory1.SelectedItem.ToString();
            string selectedSubCategory = comboBoxSubCategory1.SelectedItem.ToString();
            listBoxFiles1.Items.Clear();

            if (selectedCategory == "All Categories")
            {
                foreach (var category in categoriesWithSubmenus.Keys)
                {
                    if (UserCanAccessCategory(category))
                    {
                        SearchFiles(category, selectedSubCategory, searchPattern, listBoxFiles1);
                    }
                }
            }
            else
            {
                SearchFiles(selectedCategory, selectedSubCategory, searchPattern, listBoxFiles1);
            }
        }

        private void ButtonSearch2_Click(object sender, EventArgs e)
        {
            LogActivity("Buscou na Categoria 2");
            string searchPattern = textBoxSearch2.Text;
            string selectedCategory = comboBoxCategory2.SelectedItem.ToString();
            string selectedSubCategory = comboBoxSubCategory2.SelectedItem.ToString();
            listBoxFiles2.Items.Clear();

            if (selectedCategory == "All Categories")
            {
                foreach (var category in categoriesWithSubmenus.Keys)
                {
                    if (UserCanAccessCategory(category))
                    {
                        SearchFiles(category, selectedSubCategory, searchPattern, listBoxFiles2);
                    }
                }
            }
            else
            {
                SearchFiles(selectedCategory, selectedSubCategory, searchPattern, listBoxFiles2);
            }
        }

        private void SearchFiles(string category, string subCategory, string searchPattern, ListBox listBox)
        {
            if (subCategory == "All Subcategories")
            {
                foreach (var subCat in categoriesWithSubmenus[category])
                {
                    if (UserCanAccessSubCategory(category, subCat))
                    {
                        string subCategoryPath = Path.Combine(@"\\ntortobio\Central\Arquivos Diversos\Instalação\Documentos", category, subCat);
                        string[] files = Directory.GetFiles(subCategoryPath, "*" + searchPattern + "*.pdf", SearchOption.AllDirectories);

                        foreach (string file in files)
                        {
                            listBox.Items.Add(Path.GetFileName(file));
                        }
                    }
                }
            }
            else
            {
                if (UserCanAccessSubCategory(category, subCategory))
                {
                    string subCategoryPath = Path.Combine(@"\\ntortobio\Central\Arquivos Diversos\Instalação\Documentos", category, subCategory);
                    string[] files = Directory.GetFiles(subCategoryPath, "*" + searchPattern + "*.pdf", SearchOption.AllDirectories);

                    foreach (string file in files)
                    {
                        listBox.Items.Add(Path.GetFileName(file));
                    }
                }
            }
        }

        private void ListBoxFiles1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBoxFiles1.SelectedItem != null)
            {
                string selectedFileName = listBoxFiles1.SelectedItem.ToString();
                currentFilePath1 = GetFilePath(selectedFileName);
                DisplayPdf1(currentFilePath1);
                LogActivity($"Visualizou o arquivo {selectedFileName} na Categoria 1");
            }
        }

        private void ListBoxFiles2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBoxFiles2.SelectedItem != null)
            {
                string selectedFileName = listBoxFiles2.SelectedItem.ToString();
                currentFilePath2 = GetFilePath(selectedFileName);
                DisplayPdf2(currentFilePath2);
                LogActivity($"Visualizou o arquivo {selectedFileName} na Categoria 2");
            }
        }

        private string GetFilePath(string fileName)
        {
            foreach (var category in categoriesWithSubmenus.Keys)
            {
                string categoryPath = Path.Combine(@"\\ntortobio\Central\Arquivos Diversos\Instalação\Documentos", category);
                var files = Directory.GetFiles(categoryPath, fileName, SearchOption.AllDirectories);
                if (files.Any())
                {
                    return files.First();
                }
                foreach (var subCategory in categoriesWithSubmenus[category])
                {
                    var subCategoryPath = Path.Combine(categoryPath, subCategory);
                    files = Directory.GetFiles(subCategoryPath, fileName, SearchOption.AllDirectories);
                    if (files.Any())
                    {
                        return files.First();
                    }
                }
            }
            return null;
        }

        private void DisplayPdf1(string filePath)
        {
            if (filePath != null)
            {
                var pdfDocument = PdfDocument.Load(filePath);
                pdfViewer1.Document = pdfDocument;
            }
        }

        private void DisplayPdf2(string filePath)
        {
            if (filePath != null)
            {
                var pdfDocument = PdfDocument.Load(filePath);
                pdfViewer2.Document = pdfDocument;
            }
        }

        private bool UserCanAccessCategory(string category)
        {
            return userPermissions.ContainsKey(loggedUser.Username) && userPermissions[loggedUser.Username].Contains(category);
        }

        private bool UserCanAccessSubCategory(string category, string subCategory)
        {
            return userPermissions.ContainsKey(loggedUser.Username) && userPermissions[loggedUser.Username].Contains(subCategory);
        }

        private void BtnSettings_Click(object sender, EventArgs e)
        {
            SettingsForm settingsForm = new SettingsForm();
            settingsForm.ShowDialog();
            LogActivity("Abriu o painel de configurações.");
        }

        private void BtnLogout_Click(object sender, EventArgs e)
        {
            LogActivity("Logout");
            UpdateUserLoginStatus(loggedUser.Username, false);
            this.Hide();
            LoginForm loginForm = new LoginForm();
            loginForm.Show();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            UpdateUserLoginStatus(loggedUser.Username, false);
            Application.Exit();
        }

        private void LogActivity(string activity)
        {
            string logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {GetLocalIPAddress()} - {loggedUser.Username} - {activity}{Environment.NewLine}";
            File.AppendAllText("activity_log.txt", logMessage);
        }

        private string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            return "Local IP Address Not Found!";
        }

        private void UpdateUserLoginStatus(string username, bool status)
        {
            var userLoginStatus = JsonConvert.DeserializeObject<Dictionary<string, bool>>(File.ReadAllText("userLoginStatus.json"));
            userLoginStatus[username] = status;
            File.WriteAllText("userLoginStatus.json", JsonConvert.SerializeObject(userLoginStatus, Formatting.Indented));
        }

        private void TextBoxSearch2_TextChanged(object sender, EventArgs e)
        {

        }

        private void ComboBoxSubCategory2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}