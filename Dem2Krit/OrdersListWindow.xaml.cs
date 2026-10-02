using System;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using Dem2Krit.Models;

namespace Dem2Krit
{
    public partial class OrdersListWindow : Window
    {
        private user _currentUser;

        public OrdersListWindow(user currentUser)
        {
            InitializeComponent();
            _currentUser = currentUser;
            LoadOrders();
        }

        private void LoadOrders()
        {
            try
            {
                var context = Helper.GetContext();

                var query = context.zakaz
                    .Include(z => z.pynkt_vydachu)
                    .Include(z => z.pynkt_vydachu.gorod)
                    .Include(z => z.pynkt_vydachu.ylucha)
                    .AsQueryable();

                if (_currentUser != null && _currentUser.roleID != 1 && _currentUser.roleID != 2)
                {
                    query = query.Where(z => z.userID == _currentUser.ID);
                }

                DgOrders.ItemsSource = query.OrderByDescending(z => z.data_zakaza).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки заказов: {ex.Message}", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}