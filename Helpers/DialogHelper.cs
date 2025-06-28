using SEP490_G18_GESS_DESKTOPAPP.ViewModels;
using SEP490_G18_GESS_DESKTOPAPP.ViewModels.Dialog;
using SEP490_G18_GESS_DESKTOPAPP.Views.Dialog;
using System;
using System.Windows;

namespace SEP490_G18_GESS_DESKTOPAPP.Helpers
{
    public static class DialogHelper
    {
        /// <summary>
        /// Hiển thị dialog lỗi với các thông tin tùy chỉnh
        /// </summary>
        public static void ShowErrorDialog(string title, string message, string detail, Action onRetryAction = null, Window owner = null)
        {
            var viewModel = new DialogThongBaoLoiViewModel(title, message, detail, "Nhập lại", onRetryAction);
            var dialog = new DialogThongBaoLoiView(viewModel);

            if (owner != null)
            {
                dialog.Owner = owner;
            }
            else
            {
                // Tìm window đang active làm owner
                dialog.Owner = Application.Current.Windows.OfType<Window>()
                    .FirstOrDefault(w => w.IsActive);
            }

            dialog.ShowDialog();
        }

        /// <summary>
        /// Hiển thị dialog lỗi OTP
        /// </summary>
        public static void ShowOTPErrorDialog(Action clearOTPAction = null, Window owner = null)
        {
            ShowErrorDialog(
                "Xác thực thất bại",
                "Mã OTP không chính xác",
                "Vui lòng kiểm tra lại mã OTP được cung cấp và thử lại. Mã OTP có phân biệt chữ hoa chữ thường.",
                clearOTPAction,
                owner
            );
        }

        /// <summary>
        /// Hiển thị dialog lỗi kết nối
        /// </summary>
        public static void ShowConnectionErrorDialog(Window owner = null)
        {
            ShowErrorDialog(
                "Lỗi kết nối",
                "Không thể kết nối đến máy chủ",
                "Vui lòng kiểm tra kết nối mạng của bạn và thử lại.",
                null,
                owner
            );
        }

        /// <summary>
        /// Hiển thị dialog lỗi chung
        /// </summary>
        public static void ShowGeneralErrorDialog(string errorMessage, Window owner = null)
        {
            ShowErrorDialog(
                "Đã xảy ra lỗi",
                "Có lỗi trong quá trình xử lý",
                errorMessage,
                null,
                owner
            );
        }
    }
}