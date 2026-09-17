using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
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
        private readonly Wpf.Ui.ISnackbarService? _snackbarService;
        private readonly Wpf.Ui.Controls.SnackbarPresenter? _previousPresenter;

        public ApprovalWizardDialog(ApprovalWizardViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = viewModel;
            Owner = Application.Current.MainWindow;

            try
            {
                _snackbarService = App.GetService<Wpf.Ui.ISnackbarService>();
                if (Application.Current.MainWindow is MainWindow mainWin)
                {
                    _previousPresenter = mainWin.RootSnackbar;
                }

                Loaded += (s, e) =>
                {
                    _snackbarService?.SetSnackbarPresenter(DialogSnackbar);
                };

                Closed += (s, e) =>
                {
                    if (_previousPresenter != null)
                    {
                        _snackbarService?.SetSnackbarPresenter(_previousPresenter);
                    }
                };
            }
            catch
            {
                // Fallback
            }
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
            SetupSearchableComboBox(cb);
        }

        private void SetupSearchableComboBox(ComboBox cb)
        {
            cb.ApplyTemplate();
            var textBox = cb.Template?.FindName("PART_EditableTextBox", cb) as System.Windows.Controls.TextBox ?? FindVisualChild<System.Windows.Controls.TextBox>(cb);

            if (textBox == null)
            {
                // When ComboBox is initially inside a Collapsed panel/tab, attach once it becomes visible
                DependencyPropertyChangedEventHandler? handler = null;
                handler = (s, args) =>
                {
                    if (cb.IsVisible)
                    {
                        cb.IsVisibleChanged -= handler;
                        cb.Dispatcher.BeginInvoke(new Action(() => SetupSearchableComboBox(cb)), System.Windows.Threading.DispatcherPriority.Loaded);
                    }
                };
                cb.IsVisibleChanged += handler;
                return;
            }

            // Reliable VisualBrush Watermark / Placeholder
            if (cb.Tag is string placeholder && !string.IsNullOrWhiteSpace(placeholder))
            {
                void UpdateWatermark()
                {
                    bool show = string.IsNullOrEmpty(textBox.Text) && cb.SelectedItem == null;
                    if (show)
                    {
                        var textBlock = new System.Windows.Controls.TextBlock
                        {
                            Text = placeholder,
                            Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#94A3B8")),
                            FontSize = 12.5,
                            Margin = new Thickness(6, 0, 0, 0),
                            VerticalAlignment = VerticalAlignment.Center
                        };
                        textBox.Background = new VisualBrush(textBlock)
                        {
                            Stretch = Stretch.None,
                            AlignmentX = AlignmentX.Left,
                            AlignmentY = AlignmentY.Center
                        };
                    }
                    else
                    {
                        textBox.Background = Brushes.Transparent;
                    }
                }

                textBox.TextChanged += (s, args) => UpdateWatermark();
                cb.SelectionChanged += (s, args) => UpdateWatermark();
                textBox.GotFocus += (s, args) => UpdateWatermark();
                textBox.LostFocus += (s, args) => UpdateWatermark();
                cb.IsVisibleChanged += (s, args) => UpdateWatermark();
                UpdateWatermark();
            }

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
                        if (item is FactoryDto f) str = f.DisplayText;
                        else if (item is ProductTypeDto p) str = p.DisplayText;
                        else if (item is UnitDto u) str = u.DisplayText;
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
                if (cb.SelectedItem != null && !string.IsNullOrWhiteSpace(textBox.Text) && textBox.Text == cb.SelectedItem.ToString())
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

            textBox.LostFocus += (s, args) =>
            {
                if (cb.SelectedItem == null && !string.IsNullOrWhiteSpace(textBox.Text) && cb.ItemsSource != null)
                {
                    var text = textBox.Text.Trim();
                    foreach (var item in cb.ItemsSource)
                    {
                        if (item == null) continue;
                        if (item is UnitDto u && (string.Equals(u.SatKd, text, StringComparison.OrdinalIgnoreCase) || string.Equals(u.SatNm, text, StringComparison.OrdinalIgnoreCase)))
                        {
                            cb.SelectedItem = u;
                            break;
                        }
                        else if (item is FactoryDto f && (string.Equals(f.BrPrdFacKd, text, StringComparison.OrdinalIgnoreCase) || string.Equals(f.BrPrdFacNm, text, StringComparison.OrdinalIgnoreCase)))
                        {
                            cb.SelectedItem = f;
                            break;
                        }
                        else if (item is ProductTypeDto p && (string.Equals(p.BrJnsKd, text, StringComparison.OrdinalIgnoreCase) || string.Equals(p.BrJnsNm, text, StringComparison.OrdinalIgnoreCase)))
                        {
                            cb.SelectedItem = p;
                            break;
                        }
                        else if (string.Equals(item.ToString(), text, StringComparison.OrdinalIgnoreCase))
                        {
                            cb.SelectedItem = item;
                            break;
                        }
                    }
                }
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

        private async void NextStep_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await _viewModel.NextStepAsync();
            }
            catch (Exception ex)
            {
                _viewModel.SetValidationWarning("Terjadi Kesalahan", $"Gagal memvalidasi data: {ex.Message}");
            }
        }

        private async void Submit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var success = await _viewModel.SubmitApprovalAsync();
                if (success)
                {
                    IsApproved = true;
                    Close();
                }
            }
            catch (Exception ex)
            {
                _viewModel.SetValidationWarning("Terjadi Kesalahan", $"Gagal memproses approval: {ex.Message}");
            }
        }
    }
}
