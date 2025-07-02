using ICSharpCode.AvalonEdit.Highlighting;
using SEP490_G18_GESS_DESKTOPAPP.Helpers;
using SEP490_G18_GESS_DESKTOPAPP.Models.Enum;
using SEP490_G18_GESS_DESKTOPAPP.ViewModels;
using SEP490_G18_GESS_DESKTOPAPP.ViewModels.Dialog;
using SEP490_G18_GESS_DESKTOPAPP.Views.Dialog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static SEP490_G18_GESS_DESKTOPAPP.ViewModels.LamBaiThiViewModel;

namespace SEP490_G18_GESS_DESKTOPAPP.Views
{
    /// <summary>
    /// Interaction logic for LamBaiThiView.xaml
    /// 
    /// CHỨC NĂNG BẢO MẬT ĐÃ ĐƯỢC TÍCH HỢP:
    /// ====================================
    /// 
    /// 1. CHẶN PHÍM TẮT HỆ THỐNG:
    ///    - Windows Key (trái/phải) → Không thể mở Start Menu
    ///    - Alt+Tab → Không thể chuyển ứng dụng
    ///    - Alt+Esc → Không thể chuyển ứng dụng
    ///    - Ctrl+Esc → Không thể mở Start Menu
    ///    - Alt+F4 → Hiển thị dialog xác nhận thoát
    ///    - Ctrl+Shift+Esc → Hiển thị dialog xác nhận thoát (thay vì Task Manager)
    ///    - Alt+Space → Không thể mở System Menu
    /// 
    /// 2. CHẶN PHÍM CHỨC NĂNG:
    ///    - F1 → Không thể mở Help
    ///    - F5, Ctrl+R → Không thể refresh (tránh mất dữ liệu)
    ///    - F12 → Không thể mở Developer Tools
    ///    - Print Screen → Không thể chụp màn hình
    ///    - Pause/Break → Chặn
    /// 
    /// 3. CHẶN PHÍM TẮT TRÌNH DUYỆT/EDITOR:
    ///    - Ctrl+W → Không thể đóng tab/window
    ///    - Ctrl+T → Không thể mở tab mới
    ///    - Ctrl+N → Không thể mở window mới
    ///    - Ctrl+Shift+N → Không thể mở incognito window
    ///    - Ctrl+U → Không thể xem source code
    ///    - Ctrl+Shift+I → Không thể mở Developer Tools
    ///    - Ctrl+Shift+C → Không thể inspect element
    /// 
    /// 4. CHỐ ĐỘ TOÀN MÀN HÌNH:
    ///    - Fullscreen, không border, không resize
    ///    - Luôn ở trên cùng (Topmost)
    ///    - Ẩn khỏi taskbar
    ///    - Vô hiệu hóa System Menu
    /// 
    /// 5. XỬ LÝ THOÁT KHẨN CẤP:
    ///    - Mọi thao tác thoát đều hiển thị dialog xác nhận
    ///    - Auto-submit bài thi khi xác nhận thoát
    ///    - Hook keyboard toàn hệ thống (Low-level)
    /// 
    /// ⚠️  LƯU Ý: Ctrl+Alt+Del không thể chặn vì đây là System Interrupt
    /// </summary>
    public partial class LamBaiThiView : Window
    {
        private bool _isExamSubmitted = false;
        private bool _isUpdatingEditorFromViewModel = false; // Flag để tránh infinite loop
        private bool _isExitDialogShowing = false;
        // Windows API để chặn phím tắt
        // Windows API để chặn phím tắt
        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc callback, IntPtr hInstance, uint threadId);

        [DllImport("user32.dll")]
        private static extern bool UnhookWindowsHookEx(IntPtr hInstance);

        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(IntPtr idHook, int nCode, int wParam, IntPtr lParam);

        [DllImport("kernel32.dll")]
        private static extern IntPtr LoadLibrary(string lpFileName);

        // Windows API Constants
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int GWL_STYLE = -16;
        private const int WS_SYSMENU = 0x80000;
        
        // Virtual Key Codes for Windows keys
        private const int VK_LWIN = 0x5B;
        private const int VK_RWIN = 0x5C;

        private IntPtr _hookID = IntPtr.Zero;
        private LowLevelKeyboardProc _proc;

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        private LamBaiThiViewModel ViewModel => this.DataContext as LamBaiThiViewModel;

        public LamBaiThiView()
        {
            InitializeComponent();
            System.Diagnostics.Debug.WriteLine($"[DEBUG] LamBaiThiView Constructor: Window instance = {this.GetHashCode()}");
            // Hook keyboard events
            _proc = HookCallback;
            _hookID = SetHook(_proc);

            // Register PreviewKeyDown
            this.PreviewKeyDown += OnPreviewKeyDown;
            // Handle window closing event
            this.Closing += LamBaiThiView_Closing;
            // Debug AvalonEdit highlighting capabilities
            DebugAvalonEditHighlighting();
            SetupExamMode();
            // DataContext sẽ được gán từ ngoài khi khởi tạo
            this.Loaded += (s, e) =>
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG] LamBaiThiView.Loaded: Window instance = {this.GetHashCode()}");
                
                // Debug ViewModel instance
                if (ViewModel != null)
                {
                    System.Diagnostics.Debug.WriteLine($"[DEBUG] ViewModel instance = {ViewModel.GetHashCode()}");
                    System.Diagnostics.Debug.WriteLine($"[DEBUG] AllPracticeQuestions count = {ViewModel.AllPracticeQuestions?.Count ?? 0}");
                    
                    if (ViewModel.AllPracticeQuestions != null)
                    {
                        for (int i = 0; i < ViewModel.AllPracticeQuestions.Count; i++)
                        {
                            var question = ViewModel.AllPracticeQuestions[i];
                            System.Diagnostics.Debug.WriteLine($"[DEBUG] Question[{i}]: Id={question.PracticeQuestionId}, Content='{question.Content?.Substring(0, Math.Min(50, question.Content?.Length ?? 0))}...', Answer='{question.Answer}'");
                        }
                    }
                    
                    ViewModel.PropertyChanged += (sender, args) =>
                    {
                        System.Diagnostics.Debug.WriteLine($"[DEBUG] ViewModel PropertyChanged: {args.PropertyName}");
                        
                        if (args.PropertyName == nameof(ViewModel.CurrentQuestionIndex) ||
                            args.PropertyName == nameof(ViewModel.CurrentPracticeQuestion))
                        {
                            // Khi thay đổi câu hỏi, cập nhật UI
                            UpdatePracticeQuestionUI();
                        }
                    };
                    
                    // Khởi tạo ban đầu
                    UpdatePracticeQuestionUI();
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("[DEBUG] ViewModel is NULL in Loaded event!");
                }
            };
            
            // Debug DataContext changes
            this.DataContextChanged += (s, e) =>
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG] DataContext changed: Old={e.OldValue?.GetHashCode()}, New={e.NewValue?.GetHashCode()}");
                if (e.NewValue is LamBaiThiViewModel newViewModel)
                {
                    System.Diagnostics.Debug.WriteLine($"[DEBUG] New ViewModel AllPracticeQuestions count = {newViewModel.AllPracticeQuestions?.Count ?? 0}");
                }
            };
           

            AnimationHelper.ApplyFadeIn(this);
        }

        private void DebugAvalonEditHighlighting()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[DEBUG] ===== AvalonEdit Highlighting Debug =====");
                
                var highlightingManager = HighlightingManager.Instance;
                System.Diagnostics.Debug.WriteLine($"[DEBUG] HighlightingManager instance: {highlightingManager?.GetHashCode() ?? 0}");
                
                if (highlightingManager?.HighlightingDefinitions != null)
                {
                    var definitions = highlightingManager.HighlightingDefinitions.ToList();
                    System.Diagnostics.Debug.WriteLine($"[DEBUG] Total highlighting definitions: {definitions.Count}");
                    
                    foreach (var def in definitions)
                    {
                        System.Diagnostics.Debug.WriteLine($"[DEBUG] - Available: '{def.Name}'");
                    }
                    
                    // Test specific definitions we need
                    var testDefinitions = new[] { "C#", "Python", "Java", "SQL", "Text" };
                    foreach (var testName in testDefinitions)
                    {
                        var def = highlightingManager.GetDefinition(testName);
                        System.Diagnostics.Debug.WriteLine($"[DEBUG] Test definition '{testName}': {(def != null ? "FOUND" : "NOT FOUND")}");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("[ERROR] HighlightingDefinitions collection is NULL!");
                }
                
                System.Diagnostics.Debug.WriteLine("[DEBUG] ============================================");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR] DebugAvalonEditHighlighting failed: {ex.Message}");
            }
        }

        private void SetupExamMode()
        {
            try
            {
                // Full screen mode - chế độ toàn màn hình
                this.WindowState = WindowState.Maximized;
                this.WindowStyle = WindowStyle.None;
                this.ResizeMode = ResizeMode.NoResize;
                this.Topmost = true; // Luôn ở trên cùng

                // Ẩn taskbar để ngăn chuyển ứng dụng
                this.ShowInTaskbar = false;

                // Ẩn con trỏ chuột khi di chuyển ra ngoài vùng làm bài (tùy chọn)
                // this.Cursor = Cursors.None;

                System.Diagnostics.Debug.WriteLine("[DEBUG] Exam mode setup completed - Full screen, no taskbar, topmost");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR] SetupExamMode failed: {ex.Message}");
            }
        }

        // Method để hiển thị cảnh báo khi phát hiện hành vi bất thường
        private void ShowSecurityWarning(string action)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[WARNING] Security violation detected: {action}");
                
                // Có thể hiển thị thông báo cảnh báo
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    // Tùy chọn: Hiển thị thông báo cảnh báo
                    // MessageBox.Show(
                    //     $"Phát hiện hành vi không được phép: {action}\nVui lòng không thử thoát khỏi ứng dụng thi.",
                    //     "Cảnh báo bảo mật",
                    //     MessageBoxButton.OK,
                    //     MessageBoxImage.Warning
                    // );
                }));

                // Log hành vi để báo cáo sau
                // TODO: Có thể ghi log vào file hoặc gửi về server
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR] ShowSecurityWarning failed: {ex.Message}");
            }
        }
        
        private void UpdatePracticeQuestionUI()
        {
            if (ViewModel != null && ViewModel.ExamType == ExamType.Practice &&
                ViewModel.CurrentPracticeQuestion != null)
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG] UpdatePracticeQuestionUI: Question {ViewModel.CurrentQuestionIndex + 1}");
            }
        }
    

        private void ApplyEditorMode(ICSharpCode.AvalonEdit.TextEditor editor, string mode)
        {
            System.Diagnostics.Debug.WriteLine($"[DEBUG] ApplyEditorMode STARTING: mode='{mode}', editor={editor?.GetHashCode()}");
            
            if (editor == null)
            {
                System.Diagnostics.Debug.WriteLine("[ERROR] ApplyEditorMode: editor is NULL!");
                return;
            }

            // Log available highlighting definitions
            var availableHighlightings = HighlightingManager.Instance.HighlightingDefinitions.Select(h => h.Name).ToList();
            System.Diagnostics.Debug.WriteLine($"[DEBUG] Available highlighting definitions: {string.Join(", ", availableHighlightings)}");

            var highlighting = mode switch
            {
                "CSharp" => HighlightingManager.Instance.GetDefinition("C#"),
                "Python" => HighlightingManager.Instance.GetDefinition("Python"),
                "Java" => HighlightingManager.Instance.GetDefinition("Java"),
                "SQL" => HighlightingManager.Instance.GetDefinition("TSQL"), // Use TSQL instead of SQL
                _ => null // Text mode - NO highlighting
            };

            System.Diagnostics.Debug.WriteLine($"[DEBUG] ApplyEditorMode: Retrieved highlighting definition = {highlighting?.Name ?? "NULL"}");

            // Apply highlighting
            editor.SyntaxHighlighting = highlighting;
            System.Diagnostics.Debug.WriteLine($"[DEBUG] ApplyEditorMode: Applied SyntaxHighlighting = {editor.SyntaxHighlighting?.Name ?? "NULL"}");

            // Configure editor based on mode
            if (mode == "Text")
            {
                // Text mode: NO line numbers, NO highlighting, word wrap ON
                editor.ShowLineNumbers = false;
                editor.WordWrap = true;
                editor.SyntaxHighlighting = null; // Explicitly remove highlighting
                System.Diagnostics.Debug.WriteLine($"[DEBUG] ApplyEditorMode: Text mode applied - ShowLineNumbers={editor.ShowLineNumbers}, WordWrap={editor.WordWrap}, SyntaxHighlighting=NULL");
            }
            else
            {
                // Code mode: line numbers ON, highlighting ON, word wrap OFF
                editor.ShowLineNumbers = true;
                editor.WordWrap = false;
                System.Diagnostics.Debug.WriteLine($"[DEBUG] ApplyEditorMode: Code mode applied - ShowLineNumbers={editor.ShowLineNumbers}, WordWrap={editor.WordWrap}, SyntaxHighlighting={editor.SyntaxHighlighting?.Name ?? "NULL"}");
            }

            // Force refresh the editor
            try
            {
                editor.InvalidateVisual();
                editor.UpdateLayout(); // Additional refresh
                System.Diagnostics.Debug.WriteLine("[DEBUG] ApplyEditorMode: InvalidateVisual() and UpdateLayout() called successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR] ApplyEditorMode: Refresh failed: {ex.Message}");
            }

            System.Diagnostics.Debug.WriteLine($"[DEBUG] ApplyEditorMode COMPLETED: mode={mode}, final ShowLineNumbers={editor.ShowLineNumbers}, final SyntaxHighlighting={editor.SyntaxHighlighting?.Name ?? "NULL"}");
        }

        private void PracticeAnswerEditor_Loaded(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("[DEBUG] ===== PracticeAnswerEditor_Loaded CALLED =====");
            
            if (sender is ICSharpCode.AvalonEdit.TextEditor editor)
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG] Editor Loaded: Tag={editor.Tag}, Text='{editor.Text}'");
                System.Diagnostics.Debug.WriteLine($"[DEBUG] Editor Loaded: Window instance = {this.GetHashCode()}");
                System.Diagnostics.Debug.WriteLine($"[DEBUG] Editor Loaded: ViewModel instance = {ViewModel?.GetHashCode() ?? 0}");
                System.Diagnostics.Debug.WriteLine($"[DEBUG] Editor Loaded: AllPracticeQuestions count = {ViewModel?.AllPracticeQuestions?.Count ?? 0}");
                
                // Debug current syntax highlighting capability
                System.Diagnostics.Debug.WriteLine($"[DEBUG] Editor Loaded: Current SyntaxHighlighting = {editor.SyntaxHighlighting?.Name ?? "NULL"}");
                System.Diagnostics.Debug.WriteLine($"[DEBUG] Editor Loaded: ShowLineNumbers = {editor.ShowLineNumbers}");
                
                if (editor.Tag is int questionId)
                {
                    System.Diagnostics.Debug.WriteLine($"[DEBUG] Editor Loaded: Looking for questionId = {questionId}");
                    
                    if (ViewModel?.AllPracticeQuestions != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"[DEBUG] Editor Loaded: Available question IDs:");
                        foreach (var q in ViewModel.AllPracticeQuestions)
                        {
                            System.Diagnostics.Debug.WriteLine($"  - QuestionId = {q.PracticeQuestionId}, Answer = '{q.Answer}', IsCurrent = {q.IsCurrent}");
                        }
                        
                        var question = ViewModel.AllPracticeQuestions.FirstOrDefault(q => q.PracticeQuestionId == questionId);
                        if (question != null)
                        {
                            System.Diagnostics.Debug.WriteLine($"[DEBUG] Editor Loaded: Found question, setting text = '{question.Answer ?? string.Empty}'");
                            editor.Text = question.Answer ?? string.Empty;
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"[DEBUG] Editor Loaded: Question with ID {questionId} NOT FOUND!");
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"[DEBUG] Editor Loaded: ViewModel or AllPracticeQuestions is NULL!");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[DEBUG] Editor Loaded: Tag is not int! Tag type = {editor.Tag?.GetType()}, Value = {editor.Tag}");
                }
                
                // Tìm ComboBox mode trong cùng Border/StackPanel cha - search in multiple levels
                System.Diagnostics.Debug.WriteLine("[DEBUG] Editor Loaded: Searching for ComboBox...");
                
                // Try to find the Grid parent first
                var gridParent = FindVisualParent<Grid>(editor);
                if (gridParent != null)
                {
                    System.Diagnostics.Debug.WriteLine($"[DEBUG] Editor Loaded: Found Grid parent {gridParent.GetHashCode()}");
                    
                    var comboBox = FindVisualChild<ComboBox>(gridParent);
                    if (comboBox != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"[DEBUG] Editor Loaded: Found ComboBox {comboBox.GetHashCode()} in Grid");
                        System.Diagnostics.Debug.WriteLine($"[DEBUG] Editor Loaded: ComboBox.SelectedIndex = {comboBox.SelectedIndex}");
                        System.Diagnostics.Debug.WriteLine($"[DEBUG] Editor Loaded: ComboBox.SelectedItem = {comboBox.SelectedItem}");
                        System.Diagnostics.Debug.WriteLine($"[DEBUG] Editor Loaded: ComboBox.Tag = {comboBox.Tag}");
                        
                        // Apply mode based on ComboBox selection
                        string mode = "Text";
                        if (comboBox.SelectedItem is ComboBoxItem selectedItem)
                        {
                            mode = selectedItem.Tag?.ToString() ?? "Text";
                            System.Diagnostics.Debug.WriteLine($"[DEBUG] Editor Loaded: Mode from ComboBox = '{mode}'");
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"[DEBUG] Editor Loaded: ComboBox SelectedItem is not ComboBoxItem, using default Text mode");
                        }
                        
                        System.Diagnostics.Debug.WriteLine($"[DEBUG] Editor Loaded: Applying mode '{mode}' from ComboBox selection");
                        ApplyEditorMode(editor, mode);
                        
                        return; // Found ComboBox, apply mode and exit
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("[DEBUG] Editor Loaded: ComboBox not found in Grid parent");
                        
                        // Debug: List all children in Grid
                        System.Diagnostics.Debug.WriteLine("[DEBUG] Children in Grid parent:");
                        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(gridParent); i++)
                        {
                            var child = VisualTreeHelper.GetChild(gridParent, i);
                            System.Diagnostics.Debug.WriteLine($"  Child[{i}]: {child.GetType().Name}");
                            
                            // If it's a Border, check its children too
                            if (child is Border border)
                            {
                                System.Diagnostics.Debug.WriteLine($"    Border children:");
                                for (int j = 0; j < VisualTreeHelper.GetChildrenCount(border); j++)
                                {
                                    var borderChild = VisualTreeHelper.GetChild(border, j);
                                    System.Diagnostics.Debug.WriteLine($"      BorderChild[{j}]: {borderChild.GetType().Name}");
                                }
                            }
                        }
                    }
                }
                
                // Apply default Test mode with debug
                System.Diagnostics.Debug.WriteLine("[DEBUG] Editor Loaded: Applying default Text mode");
                ApplyEditorMode(editor, "Text");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[ERROR] Editor Loaded: Sender is not TextEditor!");
            }
            
            System.Diagnostics.Debug.WriteLine("[DEBUG] ===== PracticeAnswerEditor_Loaded COMPLETED =====");
        }

        private void AnswerModeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("[DEBUG] AnswerModeComboBox_SelectionChanged TRIGGERED");
            
            if (sender is ComboBox comboBox)
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG] ComboBox found: {comboBox.GetHashCode()}");
                System.Diagnostics.Debug.WriteLine($"[DEBUG] ComboBox.Tag: {comboBox.Tag}");
                System.Diagnostics.Debug.WriteLine($"[DEBUG] ComboBox.SelectedIndex: {comboBox.SelectedIndex}");
                System.Diagnostics.Debug.WriteLine($"[DEBUG] ComboBox.SelectedItem: {comboBox.SelectedItem}");
                
                if (comboBox.SelectedItem is ComboBoxItem selectedItem)
                {
                    System.Diagnostics.Debug.WriteLine($"[DEBUG] SelectedItem found: Content='{selectedItem.Content}', Tag='{selectedItem.Tag}'");
                    
                    if (comboBox.Tag is int questionId)
                    {
                        string mode = selectedItem.Tag?.ToString() ?? "Text";
                        System.Diagnostics.Debug.WriteLine($"[DEBUG] Mode to apply: '{mode}' for questionId={questionId}");
                        
                        // FIX: Tìm Grid parent thay vì Border (vì ComboBox và Editor ở 2 Border khác nhau)
                        var parentGrid = FindVisualParent<Grid>(comboBox);
                        if (parentGrid != null)
                        {
                            System.Diagnostics.Debug.WriteLine($"[DEBUG] Parent Grid found: {parentGrid.GetHashCode()}");
                            
                            // Tìm editor có cùng Tag (questionId)
                            var editor = FindVisualChildren<ICSharpCode.AvalonEdit.TextEditor>(parentGrid)
                                         .FirstOrDefault(e => e.Tag?.ToString() == questionId.ToString());
                            
                            if (editor != null)
                            {
                                System.Diagnostics.Debug.WriteLine($"[DEBUG] Editor found: {editor.GetHashCode()}, current SyntaxHighlighting: {editor.SyntaxHighlighting?.Name ?? "NULL"}");
                                System.Diagnostics.Debug.WriteLine($"[DEBUG] Editor current ShowLineNumbers: {editor.ShowLineNumbers}");
                                
                                ApplyEditorMode(editor, mode);
                                
                                // Verify changes after applying
                                System.Diagnostics.Debug.WriteLine($"[DEBUG] After ApplyEditorMode - SyntaxHighlighting: {editor.SyntaxHighlighting?.Name ?? "NULL"}");
                                System.Diagnostics.Debug.WriteLine($"[DEBUG] After ApplyEditorMode - ShowLineNumbers: {editor.ShowLineNumbers}");
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine($"[ERROR] Không tìm thấy editor với questionId={questionId}");
                                
                                // Debug: List all editors found
                                var allEditors = FindVisualChildren<ICSharpCode.AvalonEdit.TextEditor>(parentGrid).ToList();
                                System.Diagnostics.Debug.WriteLine($"[DEBUG] Found {allEditors.Count} editors in Grid:");
                                foreach (var ed in allEditors)
                                {
                                    System.Diagnostics.Debug.WriteLine($"  Editor: {ed.GetHashCode()}, Tag: {ed.Tag}");
                                }
                            }
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("[ERROR] Không tìm thấy parent Grid cho ComboBox");
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"[ERROR] ComboBox.Tag is not int! Type: {comboBox.Tag?.GetType()}, Value: {comboBox.Tag}");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("[ERROR] SelectedItem is not ComboBoxItem!");
                    System.Diagnostics.Debug.WriteLine($"[DEBUG] SelectedItem type: {comboBox.SelectedItem?.GetType()}");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[ERROR] Sender is not ComboBox!");
                System.Diagnostics.Debug.WriteLine($"[DEBUG] Sender type: {sender?.GetType()}");
            }
        }

        private T FindVisualParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);

            if (parentObject == null) return null;

            T parent = parentObject as T;
            if (parent != null)
                return parent;
            else
                return FindVisualParent<T>(parentObject);
        }


        private T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                if (child != null && child is T)
                    return (T)child;
                else
                {
                    T childOfChild = FindVisualChild<T>(child);
                    if (childOfChild != null)
                        return childOfChild;
                }
            }
            return null;
        }
        private IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (var curProcess = System.Diagnostics.Process.GetCurrentProcess())
            using (var curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                    LoadLibrary(curModule.ModuleName), 0);
            }
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);

                // Chặn nút Windows (Left và Right Windows key)
                if (vkCode == VK_LWIN || vkCode == VK_RWIN)
                {
                    System.Diagnostics.Debug.WriteLine("[DEBUG] Blocked Windows key");
                    return (IntPtr)1; // Block the key
                }

                // Ctrl+Esc (Start Menu)
                if (Keyboard.Modifiers == ModifierKeys.Control && vkCode == (int)Key.Escape)
                {
                    System.Diagnostics.Debug.WriteLine("[DEBUG] Blocked Ctrl+Esc");
                    return (IntPtr)1; // Block the key
                }

                // Alt+Tab (Switch applications)
                if (Keyboard.Modifiers == ModifierKeys.Alt && vkCode == (int)Key.Tab)
                {
                    System.Diagnostics.Debug.WriteLine("[DEBUG] Blocked Alt+Tab");
                    return (IntPtr)1; // Block the key
                }

                // Alt+Esc (Switch applications)
                if (Keyboard.Modifiers == ModifierKeys.Alt && vkCode == (int)Key.Escape)
                {
                    System.Diagnostics.Debug.WriteLine("[DEBUG] Blocked Alt+Esc");
                    return (IntPtr)1; // Block the key
                }

                // Ctrl+Shift+Esc (Task Manager)
                if (Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift) && vkCode == (int)Key.Escape)
                {
                    System.Diagnostics.Debug.WriteLine("[DEBUG] Blocked Ctrl+Shift+Esc - showing exit dialog");
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        ShowExitConfirmationDialog();
                    });
                    return (IntPtr)1; // Block the key
                }

                // Alt+F4
                if (Keyboard.Modifiers == ModifierKeys.Alt && vkCode == (int)Key.F4)
                {
                    System.Diagnostics.Debug.WriteLine("[DEBUG] Blocked Alt+F4 - showing exit dialog");
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        ShowExitConfirmationDialog();
                    });
                    return (IntPtr)1; // Block the key
                }

                // F1 (Help)
                if (vkCode == (int)Key.F1)
                {
                    System.Diagnostics.Debug.WriteLine("[DEBUG] Blocked F1");
                    return (IntPtr)1; // Block the key
                }

                // Ctrl+Shift+I (Developer Tools in some apps)
                if (Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift) && vkCode == (int)Key.I)
                {
                    System.Diagnostics.Debug.WriteLine("[DEBUG] Blocked Ctrl+Shift+I");
                    return (IntPtr)1; // Block the key
                }

                // F12 (Developer Tools)
                if (vkCode == (int)Key.F12)
                {
                    System.Diagnostics.Debug.WriteLine("[DEBUG] Blocked F12");
                    return (IntPtr)1; // Block the key
                }

                // Ctrl+U (View Source)
                if (Keyboard.Modifiers == ModifierKeys.Control && vkCode == (int)Key.U)
                {
                    System.Diagnostics.Debug.WriteLine("[DEBUG] Blocked Ctrl+U");
                    return (IntPtr)1; // Block the key
                }

                // Ctrl+Shift+C (Inspect Element)
                if (Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift) && vkCode == (int)Key.C)
                {
                    System.Diagnostics.Debug.WriteLine("[DEBUG] Blocked Ctrl+Shift+C");
                    return (IntPtr)1; // Block the key
                }

                // Ctrl+R và F5 (Refresh - có thể gây mất dữ liệu)
                if ((Keyboard.Modifiers == ModifierKeys.Control && vkCode == (int)Key.R) || vkCode == (int)Key.F5)
                {
                    System.Diagnostics.Debug.WriteLine("[DEBUG] Blocked Refresh key");
                    return (IntPtr)1; // Block the key
                }

                // Alt+Space (System menu)
                if (Keyboard.Modifiers == ModifierKeys.Alt && vkCode == (int)Key.Space)
                {
                    System.Diagnostics.Debug.WriteLine("[DEBUG] Blocked Alt+Space");
                    return (IntPtr)1; // Block the key
                }

                // Ctrl+Alt+Del không thể được hoàn toàn chặn vì đây là phím tắt cấp hệ thống
                // nhưng chúng ta có thể giám sát và hiển thị thông báo khi người dùng quay lại
                if (Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Alt) && vkCode == (int)Key.Delete)
                {
                    System.Diagnostics.Debug.WriteLine("[DEBUG] Detected Ctrl+Alt+Del - cannot block system-level shortcut");
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        // Đánh dấu có thể cần phải kiểm tra khi người dùng quay lại
                        // (có thể đặt flag nào đó ở đây)
                    });
                }
            }
            return CallNextHookEx(_hookID, nCode, (int)wParam, lParam);
        }


        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            // Lấy handle của window
            var hwnd = new WindowInteropHelper(this).Handle;

            // Disable system menu (Alt+F4, Alt+Space)
            var currentStyle = GetWindowLong(hwnd, GWL_STYLE);
            SetWindowLong(hwnd, GWL_STYLE, currentStyle & ~WS_SYSMENU);
        }
        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"[DEBUG] OnPreviewKeyDown: Key={e.Key}, Modifiers={Keyboard.Modifiers}");

            // Chặn Alt + Tab (backup - chủ yếu được xử lý trong HookCallback)
            if (Keyboard.Modifiers == ModifierKeys.Alt && e.Key == Key.Tab)
            {
                System.Diagnostics.Debug.WriteLine("[DEBUG] OnPreviewKeyDown: Blocked Alt+Tab");
                e.Handled = true;
                return;
            }

            // Chặn Windows key (backup - chủ yếu được xử lý trong HookCallback)
            if (e.Key == Key.LWin || e.Key == Key.RWin)
            {
                System.Diagnostics.Debug.WriteLine("[DEBUG] OnPreviewKeyDown: Blocked Windows key");
                e.Handled = true;
                return;
            }

            // Chặn Ctrl+W (Đóng tab/window)
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.W)
            {
                System.Diagnostics.Debug.WriteLine("[DEBUG] OnPreviewKeyDown: Blocked Ctrl+W");
                e.Handled = true;
                return;
            }

            // Chặn Ctrl+T (New tab)
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.T)
            {
                System.Diagnostics.Debug.WriteLine("[DEBUG] OnPreviewKeyDown: Blocked Ctrl+T");
                e.Handled = true;
                return;
            }

            // Chặn Ctrl+N (New window)
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.N)
            {
                System.Diagnostics.Debug.WriteLine("[DEBUG] OnPreviewKeyDown: Blocked Ctrl+N");
                e.Handled = true;
                return;
            }

            // Chặn Ctrl+Shift+N (New incognito window)
            if (Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift) && e.Key == Key.N)
            {
                System.Diagnostics.Debug.WriteLine("[DEBUG] OnPreviewKeyDown: Blocked Ctrl+Shift+N");
                e.Handled = true;
                return;
            }

            // Chặn các phím chức năng F1-F12 (trừ một số cần thiết cho bài thi)
            if (e.Key >= Key.F1 && e.Key <= Key.F12)
            {
                // Cho phép F9, F10 nếu cần thiết cho editor
                if (e.Key != Key.F9 && e.Key != Key.F10)
                {
                    System.Diagnostics.Debug.WriteLine($"[DEBUG] OnPreviewKeyDown: Blocked function key {e.Key}");
                    e.Handled = true;
                    return;
                }
            }

            // Chặn Print Screen
            if (e.Key == Key.PrintScreen)
            {
                System.Diagnostics.Debug.WriteLine("[DEBUG] OnPreviewKeyDown: Blocked PrintScreen");
                e.Handled = true;
                return;
            }

            // Chặn Pause/Break
            if (e.Key == Key.Pause)
            {
                System.Diagnostics.Debug.WriteLine("[DEBUG] OnPreviewKeyDown: Blocked Pause");
                e.Handled = true;
                return;
            }
        }

        private void LamBaiThiView_Closing(object sender, CancelEventArgs e)
        {
            //Nếu bài thi đã được nộp thì cho phép đóng
            if (_isExamSubmitted)
            {
                // Unhook keyboard để tránh memory leak
                UnhookWindowsHookEx(_hookID);
                return;
            }

            // Ngăn chặn đóng window khi đang thi nếu không qua dialog xác nhận
            if (!_isExitDialogShowing)
            {
                e.Cancel = true;
                ShowExitConfirmationDialog();
            }
        }
        //protected override void OnClosed(EventArgs e)
        //{
        //    // Unhook keyboard khi đóng form để tránh memory leak
        //    UnhookWindowsHookEx(_hookID);
        //    base.OnClosed(e);
        //}

        private void ShowExitConfirmationDialog()
        {
            System.Diagnostics.Debug.WriteLine("[DEBUG] ShowExitConfirmationDialog: Starting...");
            
            if (_isExitDialogShowing)
            {
                System.Diagnostics.Debug.WriteLine("[DEBUG] ShowExitConfirmationDialog: Dialog already showing, returning");
                return;
            }

            _isExitDialogShowing = true;
            System.Diagnostics.Debug.WriteLine("[DEBUG] ShowExitConfirmationDialog: Set _isExitDialogShowing = true");

            // Tạo action để xử lý khi người dùng xác nhận thoát
            Action confirmAction = async () =>
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine("[DEBUG] ShowExitConfirmationDialog: Confirm action executed");

                    // Nộp bài và CHỜ hoàn thành
                    await ViewModel.SubmitExamAsync(true);

                    // Đánh dấu đã nộp để có thể đóng window nếu cần
                    _isExamSubmitted = true;

                    // KHÔNG gọi Application.Current.Shutdown();
                    // ViewModel sẽ tự chuyển về trang kết quả
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[ERROR] ShowExitConfirmationDialog: {ex.Message}");
                    MessageBox.Show($"Lỗi khi nộp bài thi: {ex.Message}", "Lỗi",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    _isExitDialogShowing = false;
                    System.Diagnostics.Debug.WriteLine("[DEBUG] ShowExitConfirmationDialog: Reset _isExitDialogShowing = false");
                }
            };

            try
            {
                // Hiển thị dialog xác nhận
                var viewModel = new DialogExitConfirmationViewModel(confirmAction);
                var dialog = new DialogExitConfirmationView(viewModel);

                dialog.Owner = this;
                dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                
                System.Diagnostics.Debug.WriteLine("[DEBUG] ShowExitConfirmationDialog: About to show dialog");
                dialog.ShowDialog();
                System.Diagnostics.Debug.WriteLine("[DEBUG] ShowExitConfirmationDialog: Dialog closed");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR] ShowExitConfirmationDialog: Failed to show dialog: {ex.Message}");
                MessageBox.Show($"Lỗi hiển thị dialog: {ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                // Reset flag sau khi dialog đóng
                _isExitDialogShowing = false;
                System.Diagnostics.Debug.WriteLine("[DEBUG] ShowExitConfirmationDialog: Final reset _isExitDialogShowing = false");
            }
        }


        //protected override void OnClosing(CancelEventArgs e)
        //{
        //    //Nếu bài thi đã được nộp thì cho phép đóng
        //    if (_isExamSubmitted)
        //    {
        //        base.OnClosing(e);
        //        return;
        //    }
        //    //// Ngăn chặn đóng window khi đang thi
        //    //e.Cancel = true;

        //    //MessageBox.Show(
        //    //    "Không thể thoát trong khi đang làm bài thi!\nVui lòng nộp bài trước khi thoát.",
        //    //    "Thông báo",
        //    //    MessageBoxButton.OK,
        //    //    MessageBoxImage.Warning
        //    //);
        //}

        // Phương thức để set flag khi nộp bài
        public void SetExamSubmitted()
        {
            _isExamSubmitted = true;
        }
        // Event handlers - logic được xử lý qua PropertyChanged trong ViewModel
        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton radioButton && radioButton.DataContext is AnswerViewModel answer)
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG] RadioButton_Checked: Answer {answer.AnswerId}");
                System.Diagnostics.Debug.WriteLine($"  - CurrentQuestion: {ViewModel.CurrentQuestion?.QuestionId}");
                System.Diagnostics.Debug.WriteLine($"  - IsMultipleChoice: {ViewModel.CurrentQuestion?.IsMultipleChoice}");
                System.Diagnostics.Debug.WriteLine($"  - ⚠️ SHOULD NOT FIRE for Multiple Choice questions!");
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.DataContext is AnswerViewModel answer)
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG] CheckBox_Checked: Answer {answer.AnswerId}");
                System.Diagnostics.Debug.WriteLine($"  - CurrentQuestion: {ViewModel.CurrentQuestion?.QuestionId}");
                System.Diagnostics.Debug.WriteLine($"  - IsMultipleChoice: {ViewModel.CurrentQuestion?.IsMultipleChoice}");
                System.Diagnostics.Debug.WriteLine($"  - ✅ Expected for Multiple Choice questions");
            }
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.DataContext is AnswerViewModel answer)
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG] CheckBox_Unchecked: Answer {answer.AnswerId}");
                System.Diagnostics.Debug.WriteLine($"  - CurrentQuestion: {ViewModel.CurrentQuestion?.QuestionId}");
                System.Diagnostics.Debug.WriteLine($"  - IsMultipleChoice: {ViewModel.CurrentQuestion?.IsMultipleChoice}");
            }
        }

        // Sự kiện Loaded cho AvalonEdit - đồng bộ dữ liệu từ ViewModel vào Editor
        private void PracticeAnswerEditor_TextChanged(object sender, EventArgs e)
        {
            if (sender is ICSharpCode.AvalonEdit.TextEditor editor)
            {
                // IMPORTANT: Debug line breaks preservation with character analysis
                var text = editor.Text ?? "";
                var textLength = text.Length;
                var lineCount = text.Split('\n').Length;
                var hasLineBreaks = text.Contains('\n') || text.Contains('\r');
                
                // Show each character to debug line endings
                var charAnalysis = "";
                for (int i = 0; i < Math.Min(text.Length, 200); i++)
                {
                    var c = text[i];
                    if (c == '\r') charAnalysis += "\\r";
                    else if (c == '\n') charAnalysis += "\\n";
                    else if (c == '\t') charAnalysis += "\\t";
                    else if (char.IsControl(c)) charAnalysis += $"\\x{(int)c:X2}";
                    else charAnalysis += c;
                }
                
                System.Diagnostics.Debug.WriteLine($"[DEBUG] ===== TextChanged Analysis =====");
                System.Diagnostics.Debug.WriteLine($"[DEBUG] Editor Tag: {editor.Tag}");
                System.Diagnostics.Debug.WriteLine($"[DEBUG] Text Length: {textLength}");
                System.Diagnostics.Debug.WriteLine($"[DEBUG] Line Count: {lineCount}");
                System.Diagnostics.Debug.WriteLine($"[DEBUG] Has Line Breaks: {hasLineBreaks}");
                System.Diagnostics.Debug.WriteLine($"[DEBUG] Character Analysis (first 200): '{charAnalysis}'");
                System.Diagnostics.Debug.WriteLine($"[DEBUG] Raw bytes: {string.Join(" ", System.Text.Encoding.UTF8.GetBytes(text.Substring(0, Math.Min(50, text.Length))).Select(b => b.ToString("X2")))}");
                
                if (editor.Tag is int questionId)
                {
                    if (ViewModel?.AllPracticeQuestions != null)
                    {
                        var question = ViewModel.AllPracticeQuestions.FirstOrDefault(q => q.PracticeQuestionId == questionId);
                        if (question != null)
                        {
                            if (text != question.Answer)
                            {
                                var oldLines = question.Answer?.Split('\n')?.Length ?? 0;
                                
                                System.Diagnostics.Debug.WriteLine($"[DEBUG] Updating Answer for QuestionId={questionId}");
                                System.Diagnostics.Debug.WriteLine($"[DEBUG] Old answer lines: {oldLines}");
                                System.Diagnostics.Debug.WriteLine($"[DEBUG] New answer lines: {lineCount}");
                                
                                question.Answer = text; // Store exactly what editor has
                                
                                // Test GetAnswer() method to see what gets processed
                                var processedAnswer = question.GetAnswer();
                                var processedLines = processedAnswer?.Split('\n')?.Length ?? 0;
                                var processedHasLineBreaks = processedAnswer?.Contains('\n') == true || processedAnswer?.Contains('\r') == true;
                                
                                System.Diagnostics.Debug.WriteLine($"[DEBUG] GetAnswer() result:");
                                System.Diagnostics.Debug.WriteLine($"[DEBUG]   Processed Length: {processedAnswer?.Length ?? 0}");
                                System.Diagnostics.Debug.WriteLine($"[DEBUG]   Processed Lines: {processedLines}");
                                System.Diagnostics.Debug.WriteLine($"[DEBUG]   Processed Has Line Breaks: {processedHasLineBreaks}");
                                System.Diagnostics.Debug.WriteLine($"[DEBUG]   First 100 chars: '{processedAnswer?.Substring(0, Math.Min(100, processedAnswer?.Length ?? 0))}'");

                                // Update current question tracking
                                foreach (var q in ViewModel.AllPracticeQuestions)
                                    q.IsCurrent = (q.PracticeQuestionId == questionId);
                                    
                                var idx = ViewModel.AllPracticeQuestions.FindIndex(q => q.PracticeQuestionId == questionId);
                                if (idx >= 0)
                                {
                                    ViewModel.CurrentQuestionIndex = idx;
                                }
                                
                                System.Diagnostics.Debug.WriteLine($"[DEBUG] Answer update completed successfully");
                            }
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"[DEBUG] Question with ID {questionId} NOT FOUND!");
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"[DEBUG] ViewModel or AllPracticeQuestions is NULL!");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[DEBUG] Tag is not int! Tag type = {editor.Tag?.GetType()}, Value = {editor.Tag}");
                }
                
                System.Diagnostics.Debug.WriteLine($"[DEBUG] ================================");
            }
        }

        // Thêm debug khi submit bài tự luận
        public void DebugLogPracticeAnswersBeforeSubmit()
        {
            System.Diagnostics.Debug.WriteLine("[DEBUG] ====== SUBMIT DEBUG INFO ======");
            System.Diagnostics.Debug.WriteLine($"[DEBUG] Window instance = {this.GetHashCode()}");
            System.Diagnostics.Debug.WriteLine($"[DEBUG] ViewModel instance = {ViewModel?.GetHashCode() ?? 0}");
            
            if (ViewModel?.AllPracticeQuestions != null)
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG] AllPracticeQuestions count = {ViewModel.AllPracticeQuestions.Count}");
                System.Diagnostics.Debug.WriteLine("[DEBUG] ====== Dữ liệu sẽ gửi lên API khi submit ======");
                
                for (int i = 0; i < ViewModel.AllPracticeQuestions.Count; i++)
                {
                    var q = ViewModel.AllPracticeQuestions[i];
                    var answer = q.GetAnswer(); // Get the processed answer that will be sent to API
                    var lineCount = answer?.Split('\n')?.Length ?? 0;
                    var hasLineBreaks = answer?.Contains('\n') == true || answer?.Contains('\r') == true;
                    
                    System.Diagnostics.Debug.WriteLine($"[DEBUG] Question[{i}]: PracticeQuestionId={q.PracticeQuestionId}");
                    System.Diagnostics.Debug.WriteLine($"[DEBUG]   Raw Answer: '{q.Answer}'");
                    System.Diagnostics.Debug.WriteLine($"[DEBUG]   Processed Answer: '{answer}'");
                    System.Diagnostics.Debug.WriteLine($"[DEBUG]   Length: {answer?.Length ?? 0}, Lines: {lineCount}, HasLineBreaks: {hasLineBreaks}");
                    System.Diagnostics.Debug.WriteLine($"[DEBUG]   IsCurrent: {q.IsCurrent}");
                    System.Diagnostics.Debug.WriteLine($"[DEBUG]   ---");
                }
                System.Diagnostics.Debug.WriteLine("[DEBUG] =============================================");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[DEBUG] ViewModel or AllPracticeQuestions is NULL when trying to submit!");
            }
        }

        // Thêm method để debug ViewModel instance từ bên ngoài
        public void DebugViewModelInstance()
        {
            System.Diagnostics.Debug.WriteLine($"[DEBUG] DebugViewModelInstance called on Window {this.GetHashCode()}");
            System.Diagnostics.Debug.WriteLine($"[DEBUG] DataContext type = {this.DataContext?.GetType().Name}");
            System.Diagnostics.Debug.WriteLine($"[DEBUG] DataContext instance = {this.DataContext?.GetHashCode() ?? 0}");
            System.Diagnostics.Debug.WriteLine($"[DEBUG] ViewModel property instance = {ViewModel?.GetHashCode() ?? 0}");
            System.Diagnostics.Debug.WriteLine($"[DEBUG] Are they the same? = {this.DataContext == ViewModel}");
            
            if (ViewModel != null)
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG] AllPracticeQuestions count = {ViewModel.AllPracticeQuestions?.Count ?? 0}");
                System.Diagnostics.Debug.WriteLine($"[DEBUG] ExamType = {ViewModel.ExamType}");
                System.Diagnostics.Debug.WriteLine($"[DEBUG] CurrentQuestionIndex = {ViewModel.CurrentQuestionIndex}");
            }
        }

        // Method để test highlighting trực tiếp lên editor hiện tại
        public void TestHighlightingModes()
        {
            System.Diagnostics.Debug.WriteLine("[DEBUG] ===== TestHighlightingModes =====");
            
            // Tìm tất cả editors hiện tại
            var editors = FindVisualChildren<ICSharpCode.AvalonEdit.TextEditor>(this).ToList();
            System.Diagnostics.Debug.WriteLine($"[DEBUG] Found {editors.Count} TextEditors in window");
            
            foreach (var editor in editors)
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG] Testing editor {editor.GetHashCode()}, Tag={editor.Tag}");
                
                // Test different highlighting modes
                var modes = new[] { "Text", "CSharp", "Python", "Java", "SQL" };
                foreach (var mode in modes)
                {
                    System.Diagnostics.Debug.WriteLine($"[DEBUG] Testing mode: {mode}");
                    ApplyEditorMode(editor, mode);
                    
                    // Verify after applying
                    System.Diagnostics.Debug.WriteLine($"[DEBUG] After applying {mode}: SyntaxHighlighting={editor.SyntaxHighlighting?.Name ?? "NULL"}, ShowLineNumbers={editor.ShowLineNumbers}");
                }
                
                // Reset to Text mode
                ApplyEditorMode(editor, "Text");
                break; // Only test first editor
            }
            
            System.Diagnostics.Debug.WriteLine("[DEBUG] ===== TestHighlightingModes COMPLETED =====");
        }

        // Helper method to find all children of specific type
        private IEnumerable<T> FindVisualChildren<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                    if (child != null && child is T)
                        yield return (T)child;

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                        yield return childOfChild;
                }
            }
        }

        // Event handler cho test button
        private void TestHighlightingButton_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("[DEBUG] Test Highlighting Button Clicked!");
            TestHighlightingModes();
        }
    }

    #region Template Selectors

    public class AnswerTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (container is FrameworkElement element && item is QuestionViewModel question)
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG] AnswerTemplateSelector: Question {question.QuestionId}, IsMultipleChoice={question.IsMultipleChoice}");
                
                if (question.IsMultipleChoice)
                {
                    var template = element.FindResource("MultipleChoiceTemplate") as DataTemplate;
                    System.Diagnostics.Debug.WriteLine($"[DEBUG] Selected MultipleChoiceTemplate for question {question.QuestionId}");
                    return template;
                }
                else
                {
                    var template = element.FindResource("SingleChoiceTemplate") as DataTemplate;
                    System.Diagnostics.Debug.WriteLine($"[DEBUG] Selected SingleChoiceTemplate for question {question.QuestionId}");
                    return template;
                }
            }
            
            return base.SelectTemplate(item, container);
        }
    }

    #endregion

    #region Converters

    public class ExamTypeToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var result = Visibility.Collapsed;
            
            if (value is ExamType examType && parameter is string expectedType)
            {
                if (expectedType == "MultipleChoice")
                    result = examType == ExamType.MultipleChoice ? Visibility.Visible : Visibility.Collapsed;
                else if (expectedType == "Practice")
                    result = examType == ExamType.Practice ? Visibility.Visible : Visibility.Collapsed;
            }
            
            System.Diagnostics.Debug.WriteLine($"[DEBUG] ExamTypeToVisibilityConverter: ExamType={value}, Expected={parameter}, Result={result}");
            
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // Converter để chọn style cho button câu hỏi
    public class QuestionButtonStyleConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length >= 4)
            {
                bool isAnswered = values[0] is bool answered && answered;
                bool isCurrent = values[1] is bool current && current;
                bool isMarked = values[2] is bool marked && marked;
                FrameworkElement element = values[3] as FrameworkElement;

                // Debug
                System.Diagnostics.Debug.WriteLine($"QuestionButtonStyleConverter: IsAnswered={isAnswered}, IsCurrent={isCurrent}, IsMarked={isMarked}");

                if (element != null)
                {
                    try
                    {
                        if (isCurrent)
                            return element.FindResource("CurrentButtonStyle");
                        else if (isMarked)
                            return element.FindResource("MarkedButtonStyle");
                        else if (isAnswered)
                            return element.FindResource("AnsweredButtonStyle");
                        else
                            return element.FindResource("NumberButtonStyle");
                    }
                    catch
                    {
                        // Fallback to default style if resource not found
                        return DependencyProperty.UnsetValue;
                    }
                }
            }
            return DependencyProperty.UnsetValue;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // Converter để xác định RadioButton hay CheckBox
    public class BoolToAnswerSelectionModeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isMultipleChoice)
            {
                return isMultipleChoice ? "CheckBox" : "RadioButton";
            }
            return "RadioButton";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // Converter cho việc hiển thị text của mark button
    public class IsMarkedToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isMarked)
            {
                return isMarked ? "Bỏ đánh dấu" : "Đánh dấu";
            }
            return "Đánh dấu";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // Converter để format alphabet cho câu trả lời (A, B, C, D...)
    public class IndexToAlphabetConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length >= 2 && values[0] is IList list && values[1] != null)
            {
                int index = list.IndexOf(values[1]);
                if (index >= 0)
                {
                    return (char)('A' + index);
                }
            }
            return "A";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // ===== CÁC CONVERTERS CÒN THIẾU =====

    // Converter to add 1 to index (for display)
    public class AddOneConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int index)
            {
                return index + 1;
            }
            return 1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int index)
            {
                return index - 1;
            }
            return 0;
        }
    }

    // Converter for null to visibility
    public class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isInverse = parameter?.ToString() == "Inverse";

            if (value == null || (value is string str && string.IsNullOrEmpty(str)))
            {
                return isInverse ? Visibility.Visible : Visibility.Collapsed;
            }

            return isInverse ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // Extended BooleanToVisibilityConverter with parameter support
    public class BoolToVisibilityParameterConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isInverse = parameter?.ToString() == "Inverse";

            // Debug: Log conversion for IsMultipleChoice
            var resultVisibility = Visibility.Collapsed;
            if (value is bool boolVal)
            {
                if (isInverse)
                {
                    resultVisibility = boolVal ? Visibility.Collapsed : Visibility.Visible;
                }
                else
                {
                    resultVisibility = boolVal ? Visibility.Visible : Visibility.Collapsed;
                }
            }
            else
            {
                resultVisibility = isInverse ? Visibility.Visible : Visibility.Collapsed;
            }
            
            string templateType = isInverse ? "SingleChoice (RadioButton)" : "MultipleChoice (CheckBox)";
            System.Diagnostics.Debug.WriteLine($"[DEBUG] BoolToVisibilityConverter ({templateType}): IsMultipleChoice={value}, showing={resultVisibility}");

            if (value is bool boolValue)
            {
                if (isInverse)
                {
                    return boolValue ? Visibility.Collapsed : Visibility.Visible;
                }
                else
                {
                    return boolValue ? Visibility.Visible : Visibility.Collapsed;
                }
            }

            return isInverse ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    #endregion
}