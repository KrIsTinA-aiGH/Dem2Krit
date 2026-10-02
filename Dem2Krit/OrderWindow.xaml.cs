using Dem2Krit.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Dem2Krit
{
    public partial class OrderWindow : Window
    {
        private tovar _selectedTovar;
        private user _currentUser;
        private int _generatedCode;

        public OrderWindow(tovar selectedTovar, user currentUser)
        {
            InitializeComponent();
            _selectedTovar = selectedTovar;
            _currentUser = currentUser;

            LoadData();
        }

        private void LoadData()
        {
            try
            {
                TbProductName.Text = _selectedTovar.naimenovanie_tovara;

                var context = Helper.GetContext();

                var pickupList = context.pynkt_vydachu
                    .Include("gorod")
                    .Include("ylucha")
                    .ToList();

                if (pickupList.Count > 0)
                {
                    CmbPickupPoint.ItemsSource = pickupList;
                    CmbPickupPoint.DisplayMemberPath = "FullAddress"; 
                    CmbPickupPoint.SelectedValuePath = "ID";
                    CmbPickupPoint.SelectedIndex = 0;
                }
                else
                {
                    MessageBox.Show("В базе данных нет пунктов выдачи!", "Предупреждение",
                                    MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                Random rnd = new Random();
                _generatedCode = rnd.Next(100, 1000);
                TbCode.Text = $"Код получения: {_generatedCode}";

                CalculateTotal();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки пунктов выдачи: {ex.Message}", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CalculateTotal()
        {
            if (_selectedTovar == null || TbTotalSum == null) return;

            if (int.TryParse(TbCount.Text, out int count) && count > 0)
            {
                if (decimal.TryParse(_selectedTovar.chena, out decimal price))
                {
                    decimal discountPercent = Convert.ToDecimal(_selectedTovar.skidka);
                    decimal discountedPrice = price * (1 - discountPercent / 100m);
                    decimal total = discountedPrice * count;

                    TbTotalSum.Text = $"Сумма к оплате: {total:N2} руб.";
                    return;
                }
            }

            TbTotalSum.Text = "Сумма к оплате: 0 руб.";
        }

        private void TbCount_TextChanged(object sender, TextChangedEventArgs e)
        {
            CalculateTotal();
        }

        private void BtnSaveOrder_Click(object sender, RoutedEventArgs e)
        {
            if (CmbPickupPoint.SelectedValue == null)
            {
                MessageBox.Show("Выберите пункт выдачи!", "Предупреждение",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(TbCount.Text, out int count) || count <= 0)
            {
                MessageBox.Show("Введите корректное количество товара!", "Предупреждение",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var context = Helper.GetContext();

                long maxZakazId = context.zakaz.Any() ? context.zakaz.Max(z => z.ID) : 0;
                long newZakazId = maxZakazId + 1;

                zakaz newZakaz = new zakaz
                {
                    ID = newZakazId,
                    namer_zakazaID = newZakazId, 
                    data_zakaza = DateTime.Now,
                    data_dostavki = DateTime.Now.AddDays(3),
                    pynkt_vydachuID = Convert.ToInt64(CmbPickupPoint.SelectedValue),
                    kod_polychenia = _generatedCode,
                    statys_zakazaID = 1
                };

                if (_currentUser != null)
                {
                    newZakaz.userID = _currentUser.ID;
                }

                context.zakaz.Add(newZakaz);

                long maxSoderzhanieId = context.soderzhanie_zakaza.Any() ? context.soderzhanie_zakaza.Max(s => s.ID) : 0;

                soderzhanie_zakaza newSoderzhanie = new soderzhanie_zakaza
                {
                    ID = maxSoderzhanieId + 1, 
                    zakazID = newZakaz.ID,
                    artikylID = _selectedTovar.ID,
                    kolichestvo = count
                };

                context.soderzhanie_zakaza.Add(newSoderzhanie);

                // 4. Сохраняем в БД
                context.SaveChanges();

                MessageBox.Show($"Заказ №{newZakaz.ID} успешно оформлен!\nКод получения: {_generatedCode}",
                                "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                string innerError = ex.InnerException?.InnerException?.Message ?? ex.InnerException?.Message ?? ex.Message;
                MessageBox.Show($"Ошибка сохранения заказа:\n{innerError}", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            this.Close();
        }

        private void BtnPrintReceipt_Click(object sender, RoutedEventArgs e)
        {
            if (CmbPickupPoint.SelectedValue == null)
            {
                MessageBox.Show("Выберите пункт выдачи перед формированием талона!", "Предупреждение",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(TbCount.Text, out int count) || count <= 0)
            {
                MessageBox.Show("Укажите корректное количество товара!", "Предупреждение",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                System.Windows.Controls.PrintDialog printDialog = new System.Windows.Controls.PrintDialog();
                if (printDialog.ShowDialog() == true)
                {
                    var selectedPickup = CmbPickupPoint.SelectedItem as Dem2Krit.Models.pynkt_vydachu;
                    string pickupAddress = selectedPickup != null ? selectedPickup.FullAddress : CmbPickupPoint.Text;

                    FlowDocument doc = new FlowDocument();
                    doc.PagePadding = new Thickness(50);
                    doc.ColumnWidth = printDialog.PrintableAreaWidth;

                    Paragraph header = new Paragraph(new Run("ТАЛОН НА ПОЛУЧЕНИЕ ЗАКАЗА"))
                    {
                        FontSize = 18,
                        FontWeight = FontWeights.Bold,
                        TextAlignment = TextAlignment.Center,
                        Margin = new Thickness(0, 0, 0, 20)
                    };
                    doc.Blocks.Add(header);

                    doc.Blocks.Add(new Paragraph(new Run($"Дата заказа: {DateTime.Now:dd.MM.yyyy HH:mm}")));
                    doc.Blocks.Add(new Paragraph(new Run($"Товар: {_selectedTovar.naimenovanie_tovara}")));
                    doc.Blocks.Add(new Paragraph(new Run($"Количество: {count} шт.")));
                    doc.Blocks.Add(new Paragraph(new Run($"Пункт выдачи: {pickupAddress}")));
                    doc.Blocks.Add(new Paragraph(new Run($"{TbTotalSum.Text}")) { FontWeight = FontWeights.Bold });

                    Paragraph codeParagraph = new Paragraph(new Run($"КОД ПОЛУЧЕНИЯ: {_generatedCode}"))
                    {
                        FontSize = 16,
                        FontWeight = FontWeights.Bold,
                        Foreground = System.Windows.Media.Brushes.DarkGreen,
                        Margin = new Thickness(0, 15, 0, 0)
                    };
                    doc.Blocks.Add(codeParagraph);

                    IDocumentPaginatorSource idpSource = doc;
                    printDialog.PrintDocument(idpSource.DocumentPaginator, "Талон заказа");

                    MessageBox.Show("Талон успешно отправлен на печать!", "Печать",
                                    MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при формировании талона: {ex.Message}", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}