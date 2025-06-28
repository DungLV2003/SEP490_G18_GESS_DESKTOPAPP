using SEP490_G18_GESS_DESKTOPAPP.Helpers;
using SEP490_G18_GESS_DESKTOPAPP.ViewModels;
using SEP490_G18_GESS_DESKTOPAPP.ViewModels.Dialog;
using System;
using System.Windows;
using System.Windows.Media.Effects;

namespace SEP490_G18_GESS_DESKTOPAPP.Views.Dialog
{
    public partial class DialogThongBaoLoiView : Window
    {
        private Window _parentWindow;
        private Effect _originalEffect;

        public DialogThongBaoLoiView(DialogThongBaoLoiViewModel dtblviewModel)
        {
            InitializeComponent();
            this.DataContext = dtblviewModel;

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

                // Thêm Blur effect nhẹ hơn
                _parentWindow.Effect = new BlurEffect
                {
                    Radius = 5,
                    KernelType = KernelType.Gaussian
                };
            }
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