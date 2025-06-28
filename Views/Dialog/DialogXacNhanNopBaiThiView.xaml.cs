using SEP490_G18_GESS_DESKTOPAPP.Helpers;
using SEP490_G18_GESS_DESKTOPAPP.ViewModels.Dialog;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace SEP490_G18_GESS_DESKTOPAPP.Views.Dialog
{
    /// <summary>
    /// Interaction logic for DialogXacNhanNopBaiThiView.xaml
    /// </summary>
    public partial class DialogXacNhanNopBaiThiView : Window
    {
        private Window _parentWindow;
        private Effect _originalEffect;
        private Border _darkOverlay;

        public DialogXacNhanNopBaiThiView(DialogXacNhanNopBaiThiViewModel viewModel)
        {
            InitializeComponent();
            this.DataContext = viewModel;

            // Cấu hình dialog
            this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            this.ShowInTaskbar = false;

            AnimationHelper.ApplyFadeIn(this, 300);
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            // Tìm parent window
            _parentWindow = Application.Current.Windows.OfType<Window>()
                .FirstOrDefault(w => w.IsActive && w != this);

            if (_parentWindow != null)
            {
                // Lưu effect gốc
                _originalEffect = _parentWindow.Effect;

                // Thêm Blur effect
                _parentWindow.Effect = new BlurEffect
                {
                    Radius = 5,
                    KernelType = KernelType.Gaussian
                };
                CreateDarkOverlay();
            }
        }

        private void CreateDarkOverlay()
        {
            // Tìm Grid chính của parent window
            var parentGrid = FindVisualChild<Grid>(_parentWindow);
            if (parentGrid != null)
            {
                // Tạo overlay đen với opacity 50%
                _darkOverlay = new Border
                {
                    Background = new SolidColorBrush(Color.FromArgb(128, 0, 0, 0)), // 50% black
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch,
                    IsHitTestVisible = false // Không chặn mouse events
                };

                // Đặt overlay lên trên tất cả các controls khác
                Grid.SetRowSpan(_darkOverlay, parentGrid.RowDefinitions.Count == 0 ? 1 : parentGrid.RowDefinitions.Count);
                Grid.SetColumnSpan(_darkOverlay, parentGrid.ColumnDefinitions.Count == 0 ? 1 : parentGrid.ColumnDefinitions.Count);

                // Thêm overlay vào cuối để nó nằm trên cùng
                parentGrid.Children.Add(_darkOverlay);
                Panel.SetZIndex(_darkOverlay, 9999);
            }
        }

        private T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T typedChild)
                    return typedChild;

                var result = FindVisualChild<T>(child);
                if (result != null)
                    return result;
            }
            return null;
        }

        protected override void OnClosed(EventArgs e)
        {
            // Khôi phục parent window
            if (_parentWindow != null)
            {
                _parentWindow.Effect = _originalEffect;
            }

            base.OnClosed(e);
        }
    }
}