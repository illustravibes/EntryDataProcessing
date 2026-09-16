using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Entry_Data_Processing.Features.RequestKodeBarang.Models;
using Entry_Data_Processing.Features.RequestKodeBarang.ViewModels;
using Wpf.Ui.Controls;

namespace Entry_Data_Processing.Features.RequestKodeBarang.Views.Dialogs
{
    public partial class ApprovalWizardDialog : Window
    {
        public bool IsApproved { get; private set; }
        private readonly ApprovalWizardViewModel _viewModel;

        public ApprovalWizardDialog(ApprovalWizardViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = viewModel;
            Owner = Application.Current.MainWindow;
        }

        private void OnHeaderMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }

        private void OnWindowKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                IsApproved = false;
                Close();
            }
        }

        private void OnSelectExistingMode(object sender, MouseButtonEventArgs e)
        {
            _viewModel.IsNewProductMode = false;
        }

        private void OnSelectNewMode(object sender, MouseButtonEventArgs e)
        {
            _viewModel.IsNewProductMode = true;
        }

        private void SearchableComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is not ComboBox cb) return;

            cb.ApplyTemplate();
            var textBox = cb.Template?.FindName("PART_EditableTextBox", cb) as System.Windows.Controls.TextBox ?? FindVisualChild<System.Windows.Controls.TextBox>(cb);
            if (textBox == null) return;

            bool isUpdating = false;

            textBox.TextChanged += (s, args) =>
            {
                if (isUpdating) return;
                if (!cb.IsKeyboardFocusWithin) return;

                var text = textBox.Text?.Trim() ?? string.Empty;
                var view = CollectionViewSource.GetDefaultView(cb.ItemsSource);
                if (view == null) return;

                if (string.IsNullOrEmpty(text))
                {
                    view.Filter = null;
                }
                else
                {
                    view.Filter = item =>
                    {
                        if (item == null) return false;
                        string str = item.ToString() ?? string.Empty;
                        if (item is FactoryDto f) str = f.BrPrdFacNm;
                        else if (item is ProductTypeDto p) str = p.BrJnsNm;
                        else if (item is UnitDto u) str = u.SatNm;
                        return str.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0;
                    };
                }

                if (!cb.IsDropDownOpen)
                {
                    cb.IsDropDownOpen = true;
                }
            };

            cb.DropDownOpened += (s, args) =>
            {
                if (cb.SelectedItem != null && textBox.Text == cb.SelectedItem.ToString())
                {
                    var view = CollectionViewSource.GetDefaultView(cb.ItemsSource);
                    if (view != null)
                    {
                        view.Filter = null;
                    }
                }
            };

            cb.SelectionChanged += (s, args) =>
            {
                isUpdating = true;
                var view = CollectionViewSource.GetDefaultView(cb.ItemsSource);
                if (view != null)
                {
                    view.Filter = null;
                }
                isUpdating = false;
            };
        }

        private static T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T typedChild)
                    return typedChild;

                var childOfChild = FindVisualChild<T>(child);
                if (childOfChild != null)
                    return childOfChild;
            }
            return null;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            IsApproved = false;
            Close();
        }

        private async void Submit_Click(object sender, RoutedEventArgs e)
        {
            var success = await _viewModel.SubmitApprovalAsync();
            if (success)
            {
                IsApproved = true;
                Close();
            }
        }
    }
}
