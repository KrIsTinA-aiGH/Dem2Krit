using Dem2Krit.Models;
using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Dem2Krit
{
    public partial class AddEditProductWindow : Window
    {
        private tovar _currentTovar;
        private string _selectedImagePath = null;

        public AddEditProductWindow(tovar currentTovar)
        {
            InitializeComponent();

            _currentTovar = currentTovar ?? new tovar();
            DataContext = _currentTovar;

            LoadData();
        }

        private void LoadData()
        {
            try
            {
                CmbCategory.ItemsSource = Helper.GetContext().kategoria_tovara.ToList();
                CmbCategory.DisplayMemberPath = "name_kat_tovara";
                CmbCategory.SelectedValuePath = "ID";

                CmbManufacturer.ItemsSource = Helper.GetContext().proizvoditel.ToList();
                CmbManufacturer.DisplayMemberPath = "name_proizvoditel";
                CmbManufacturer.SelectedValuePath = "ID";

                CmbSupplier.ItemsSource = Helper.GetContext().postavshchik.ToList();
                CmbSupplier.DisplayMemberPath = "name_postavshchik";
                CmbSupplier.SelectedValuePath = "ID";

                if (_currentTovar.ID != 0)
                {
                    TbTitle.Text = "Редактирование товара";
                    TbName.Text = _currentTovar.naimenovanie_tovara;
                    CmbCategory.SelectedValue = _currentTovar.kategoria_tovaraID;
                    CmbManufacturer.SelectedValue = _currentTovar.proizvoditelID;
                    CmbSupplier.SelectedValue = _currentTovar.postavshchikID;
                    TbPrice.Text = _currentTovar.chena != null ? _currentTovar.chena.ToString() : "";
                    TbDiscount.Text = _currentTovar.skidka.ToString();
                    TbCount.Text = _currentTovar.kolichestvo_na_sklade.ToString();
                    TbDescription.Text = _currentTovar.opisanie_tovara;
                }
                else
                {
                    TbTitle.Text = "Добавление товара";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки справочников: {ex.Message}", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnSelectImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Изображения|*.jpg;*.jpeg;*.png;*.bmp";

            if (dlg.ShowDialog() == true)
            {
                _selectedImagePath = dlg.FileName;
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(_selectedImagePath);
                bitmap.CacheOption = BitmapCacheOption.OnLoad; 
                bitmap.EndInit();

                ImgProduct.Source = bitmap;
            }
        }
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TbName.Text))
            {
                MessageBox.Show("Заполните обязательное поле (Наименование)!", "Предупреждение",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                _currentTovar.naimenovanie_tovara = TbName.Text.Trim();
                if (CmbCategory.SelectedValue != null)
                    _currentTovar.kategoria_tovaraID = Convert.ToInt64(CmbCategory.SelectedValue);

                if (CmbManufacturer.SelectedValue != null)
                    _currentTovar.proizvoditelID = Convert.ToInt64(CmbManufacturer.SelectedValue);

                if (CmbSupplier.SelectedValue != null)
                    _currentTovar.postavshchikID = Convert.ToInt64(CmbSupplier.SelectedValue);

                var defaultUnit = Helper.GetContext().edinicha_izmerenia.FirstOrDefault();
                if (defaultUnit != null)
                {
                    _currentTovar.edinicha_izmereniaID = defaultUnit.ID;
                }

                _currentTovar.chena = TbPrice.Text.Trim();
                _currentTovar.skidka = byte.TryParse(TbDiscount.Text, out byte discount) ? discount : (byte)0;
                _currentTovar.kolichestvo_na_sklade = int.TryParse(TbCount.Text, out int count) ? count : 0;
                _currentTovar.opisanie_tovara = TbDescription.Text.Trim();

                if (!string.IsNullOrEmpty(_selectedImagePath))
                {
                    string fileName = Path.GetFileName(_selectedImagePath);
                    string destFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources");

                    if (!Directory.Exists(destFolder))
                    {
                        Directory.CreateDirectory(destFolder);
                    }

                    string destPath = Path.Combine(destFolder, fileName);

                    if (!string.Equals(_selectedImagePath, destPath, StringComparison.OrdinalIgnoreCase))
                    {
                        File.Copy(_selectedImagePath, destPath, true);
                    }

                    _currentTovar.foto = fileName;
                }

                if (_currentTovar.ID == 0)
                {
                    var maxId = Helper.GetContext().tovar.Any()
                        ? Helper.GetContext().tovar.Max(t => t.ID)
                        : 0;

                    long newId = maxId + 1;
                    _currentTovar.ID = newId;

                    _currentTovar.artikyl = $"ART{newId:D3}";

                    Helper.GetContext().tovar.Add(_currentTovar);
                }

                Helper.GetContext().SaveChanges();
                MessageBox.Show("Информация о товаре успешно сохранена!", "Успех",
                                MessageBoxButton.OK, MessageBoxImage.Information);

                DialogResult = true;
                this.Close();
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                string errorDetails = string.Join("\n", ex.EntityValidationErrors
                    .SelectMany(x => x.ValidationErrors)
                    .Select(x => x.ErrorMessage));
                MessageBox.Show($"Ошибка валидации полей:\n{errorDetails}", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                string innerError = ex.InnerException?.InnerException?.Message ?? ex.InnerException?.Message ?? ex.Message;
                MessageBox.Show($"Ошибка сохранения в БД:\n{innerError}", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            this.Close();
        }
    }
}