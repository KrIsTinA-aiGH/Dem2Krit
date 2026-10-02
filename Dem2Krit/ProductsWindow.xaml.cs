using Dem2Krit.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Dem2Krit
{
    public partial class ProductsWindow : Window
    {
        private user _currentUser;

        public ProductsWindow(user currentUser)
        {
            InitializeComponent();
            _currentUser = currentUser;

            DisplayUserInfo();
            LoadManufacturers();
            UpdateProducts();
            CheckAccessRights(); 
        }
        private void BtnViewOrder_Click(object sender, RoutedEventArgs e)
        {
            OrdersListWindow ordersWindow = new OrdersListWindow(_currentUser);
            ordersWindow.ShowDialog();
        }
        private void ContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            var contextMenu = sender as ContextMenu;
            if (contextMenu != null)
            {
                foreach (var item in contextMenu.Items)
                {
                    if (item is MenuItem menuItem && menuItem.Header != null && menuItem.Header.ToString() == "Оформить заказ")
                    {
                        menuItem.IsEnabled = (_currentUser != null);
                    }
                }
            }
        }
        private void MenuItemCreateOrder_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUser == null)
            {
                MessageBox.Show("Для оформления заказа необходимо авторизоваться в системе!",
                                "Ограничение доступа",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                return;
            }

            var selectedTovar = LViewProducts.SelectedItem as tovar;

            if (selectedTovar == null)
            {
                MessageBox.Show("Выберите товар для оформления заказа!",
                                "Предупреждение",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                return;
            }

            OrderWindow orderWindow = new OrderWindow(selectedTovar, _currentUser);
            if (orderWindow.ShowDialog() == true)
            {
                UpdateProducts();
            }
        }
        private void DisplayUserInfo()
        {
            if (_currentUser != null)
            {
                TbUserFio.Text = $"{_currentUser.familia} {_currentUser.name} {_currentUser.otchestvo}".Trim();
                var role = Helper.GetContext().role.FirstOrDefault(r => r.ID == _currentUser.roleID);
                TbUserRole.Text = role != null ? role.name_role : "Пользователь";
            }
            else
            {
                TbUserFio.Text = "Гость";
                TbUserRole.Text = "Авторизация не выполнена";
            }
        }

        private void LoadManufacturers()
        {
            try
            {
                var manufacturers = Helper.GetContext().proizvoditel.ToList();
                manufacturers.Insert(0, new proizvoditel { ID = 0, name_proizvoditel = "Все производители" });
                CmbManufacturer.ItemsSource = manufacturers;
                CmbManufacturer.DisplayMemberPath = "name_proizvoditel";
                CmbManufacturer.SelectedValuePath = "ID";
                CmbManufacturer.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки производителей: {ex.Message}", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateProducts()
        {
            if (TbCount == null || CmbManufacturer == null || TbSearch == null || CmbSort == null)
                return;

            try
            {
                var currentProducts = Helper.GetContext().tovar.ToList();

                string searchText = TbSearch.Text.Trim().ToLower();
                if (!string.IsNullOrEmpty(searchText))
                {
                    currentProducts = currentProducts
                        .Where(p => p.naimenovanie_tovara != null && p.naimenovanie_tovara.ToLower().Contains(searchText))
                        .ToList();
                }

                if (CmbManufacturer.SelectedValue != null &&
                    int.TryParse(CmbManufacturer.SelectedValue.ToString(), out int selectedManufacturerId))
                {
                    if (selectedManufacturerId > 0)
                    {
                        currentProducts = currentProducts
                            .Where(p => p.proizvoditelID == selectedManufacturerId)
                            .ToList();
                    }
                }

                if (CmbSort.SelectedIndex == 1) 
                {
                    currentProducts = currentProducts.OrderBy(p => p.chena).ToList();
                }
                else if (CmbSort.SelectedIndex == 2) 
                {
                    currentProducts = currentProducts.OrderByDescending(p => p.chena).ToList();
                }

                int totalCount = Helper.GetContext().tovar.Count();
                TbCount.Text = $"{currentProducts.Count} из {totalCount}";
                LViewProducts.ItemsSource = currentProducts;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки списка товаров: {ex.Message}", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void FilterChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateProducts();
        }
        private void FilterChanged(object sender, TextChangedEventArgs e)
        {
            UpdateProducts();
        }
        private void BtnAddProduct_Click(object sender, RoutedEventArgs e)
        {
            AddEditProductWindow addEditWindow = new AddEditProductWindow(null);
            if (addEditWindow.ShowDialog() == true)
            {
                UpdateProducts();
            }
        }

        private void LViewProducts_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (_currentUser == null || (_currentUser.roleID != 1 && _currentUser.roleID != 2))
                return;

            var selectedTovar = LViewProducts.SelectedItem as tovar;
            if (selectedTovar != null)
            {
                AddEditProductWindow addEditWindow = new AddEditProductWindow(selectedTovar);
                if (addEditWindow.ShowDialog() == true)
                {
                    UpdateProducts();
                }
            }
        }
        private void BtnDeleteProduct_Click(object sender, RoutedEventArgs e)
        {
            var selectedTovar = LViewProducts.SelectedItem as tovar;

            if (selectedTovar == null)
            {
                MessageBox.Show("Выберите товар для удаления!", "Предупреждение",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MessageBox.Show($"Вы действительно хотите удалить товар \"{selectedTovar.naimenovanie_tovara}\"?",
                                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    string imageName = selectedTovar.foto;

                    Helper.GetContext().tovar.Remove(selectedTovar);
                    Helper.GetContext().SaveChanges();

                    if (!string.IsNullOrWhiteSpace(imageName))
                    {
                        string cleanFileName = System.IO.Path.GetFileName(imageName.Trim());

                        string imagePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", cleanFileName);

                        if (System.IO.File.Exists(imagePath))
                        {
                            try
                            {
                                System.IO.File.Delete(imagePath);
                            }
                            catch
                            {
                            }
                        }
                    }

                    MessageBox.Show("Товар и его изображение успешно удалены!", "Успех",
                                    MessageBoxButton.OK, MessageBoxImage.Information);

                    UpdateProducts();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении товара (возможно, он связан с заказами): {ex.Message}",
                                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }
        private void CheckAccessRights()
        {
            if (_currentUser != null)
            {
                BtnViewOrder.Visibility = Visibility.Visible;
            }
            if (_currentUser != null)
            {
                if (_currentUser.roleID == 1 || _currentUser.roleID == 2)
                {
                    BtnAddProduct.Visibility = Visibility.Visible;
                    BtnDeleteProduct.Visibility = Visibility.Visible;
                }
            }
        }
    }
}