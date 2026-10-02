using System;
using System.Linq;
using System.Windows;
using Dem2Krit.Models;

namespace Dem2Krit
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string login = TbLogin.Text.Trim();
            string password = PbPassword.Password.Trim();

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Заполните поля Логин и Пароль!", "Предупреждение",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var currentUser = Helper.GetContext().user
                    .FirstOrDefault(u => u.login == login && u.parol == password);

                if (currentUser != null)
                {
                    string fullName = $"{currentUser.familia} {currentUser.name} {currentUser.otchestvo}".Trim();
                    MessageBox.Show($"Добро пожаловать, {fullName}!", "Успех",
                                    MessageBoxButton.OK, MessageBoxImage.Information);

                    ProductsWindow productsWindow = new ProductsWindow(currentUser);
                    productsWindow.Show();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Неверный логин или пароль!", "Ошибка авторизации",
                                    MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка подключения к базе данных: {ex.Message}", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnGuest_Click(object sender, RoutedEventArgs e)
        {
            ProductsWindow productsWindow = new ProductsWindow(null);
            productsWindow.Show();
            this.Close();
        }
    }
}