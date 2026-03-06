using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using QuickWheel.Services;

namespace QuickWheel.Controls
{
    public partial class HotkeyEditor : UserControl
    {
        private HotkeyRecorderService? _recorder;
        private Storyboard? _recordingAnimation;
        private Storyboard? _waitingAnimation;
        private Storyboard? _conflictAnimation;
        private Storyboard? _pressedAnimation;

        public static readonly DependencyProperty HotkeyModifiersProperty =
            DependencyProperty.Register(nameof(HotkeyModifiers), typeof(ModifierKeys), typeof(HotkeyEditor),
                new FrameworkPropertyMetadata(ModifierKeys.None, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnHotkeyChanged));

        public static readonly DependencyProperty HotkeyKeyProperty =
            DependencyProperty.Register(nameof(HotkeyKey), typeof(Key), typeof(HotkeyEditor),
                new FrameworkPropertyMetadata(Key.None, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnHotkeyChanged));

        public static readonly DependencyProperty IsRecordingProperty =
            DependencyProperty.Register(nameof(IsRecording), typeof(bool), typeof(HotkeyEditor),
                new PropertyMetadata(false, OnIsRecordingChanged));

        public static readonly DependencyProperty IsValidProperty =
            DependencyProperty.Register(nameof(IsValid), typeof(bool), typeof(HotkeyEditor),
                new PropertyMetadata(true));

        public static readonly DependencyProperty DisplayTextProperty =
            DependencyProperty.Register(nameof(DisplayText), typeof(string), typeof(HotkeyEditor),
                new PropertyMetadata("点击设置热键"));

        public static readonly DependencyProperty ShowClearButtonProperty =
            DependencyProperty.Register(nameof(ShowClearButton), typeof(bool), typeof(HotkeyEditor),
                new PropertyMetadata(false));

        public static readonly DependencyProperty HelpTextProperty =
            DependencyProperty.Register(nameof(HelpText), typeof(string), typeof(HotkeyEditor),
                new PropertyMetadata("点击开始录制"));

        public static readonly DependencyProperty ShowHelpTextProperty =
            DependencyProperty.Register(nameof(ShowHelpText), typeof(bool), typeof(HotkeyEditor),
                new PropertyMetadata(false));

        public static readonly DependencyProperty RecorderStateProperty =
            DependencyProperty.Register(nameof(RecorderState), typeof(HotkeyRecorderState), typeof(HotkeyEditor),
                new PropertyMetadata(HotkeyRecorderState.Idle, OnRecorderStateChanged));

        public ModifierKeys HotkeyModifiers
        {
            get => (ModifierKeys)GetValue(HotkeyModifiersProperty);
            set => SetValue(HotkeyModifiersProperty, value);
        }

        public Key HotkeyKey
        {
            get => (Key)GetValue(HotkeyKeyProperty);
            set => SetValue(HotkeyKeyProperty, value);
        }

        public bool IsRecording
        {
            get => (bool)GetValue(IsRecordingProperty);
            set => SetValue(IsRecordingProperty, value);
        }

        public bool IsValid
        {
            get => (bool)GetValue(IsValidProperty);
            set => SetValue(IsValidProperty, value);
        }

        public string DisplayText
        {
            get => (string)GetValue(DisplayTextProperty);
            set => SetValue(DisplayTextProperty, value);
        }

        public bool ShowClearButton
        {
            get => (bool)GetValue(ShowClearButtonProperty);
            set => SetValue(ShowClearButtonProperty, value);
        }

        public string HelpText
        {
            get => (string)GetValue(HelpTextProperty);
            set => SetValue(HelpTextProperty, value);
        }

        public bool ShowHelpText
        {
            get => (bool)GetValue(ShowHelpTextProperty);
            set => SetValue(ShowHelpTextProperty, value);
        }

        public HotkeyRecorderState RecorderState
        {
            get => (HotkeyRecorderState)GetValue(RecorderStateProperty);
            set => SetValue(RecorderStateProperty, value);
        }

        public event EventHandler<HotkeyChangedEventArgs>? HotkeyChanged;

        public HotkeyEditor()
        {
            InitializeComponent();
            _recorder = new HotkeyRecorderService();
            _recorder.HotkeyChanged += OnRecorderHotkeyChanged;
            _recorder.PreviewHotkey += OnRecorderPreviewHotkey;
            _recorder.StateChanged += OnRecorderStateChanged;

            _recordingAnimation = (Storyboard)Resources["RecordingAnimation"];
            _waitingAnimation = (Storyboard)Resources["WaitingAnimation"];
            _conflictAnimation = (Storyboard)Resources["ConflictAnimation"];
            _pressedAnimation = (Storyboard)Resources["PressedAnimation"];

            UpdateDisplayText();
            UpdateHelpText();
            UpdateVisualState();
        }

        private void OnRecorderHotkeyChanged(object? sender, HotkeyChangedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                if (e.Key == Key.None && e.Modifiers != ModifierKeys.None)
                {
                    DisplayText = "至少需要一个主键 (A-Z, 0-9, F1-F12)";
                    HelpText = "请添加一个主键";
                    IsValid = false;
                    RecorderState = HotkeyRecorderState.RecordingModifiers;
                }
                else if (e.Key == Key.None && e.Modifiers == ModifierKeys.None)
                {
                    HotkeyModifiers = ModifierKeys.None;
                    HotkeyKey = Key.None;
                    IsRecording = false;
                    RecorderState = HotkeyRecorderState.Idle;
                    UpdateDisplayText();
                    UpdateHelpText();
                    UpdateVisualState();
                }
                else
                {
                    HotkeyModifiers = e.Modifiers;
                    HotkeyKey = e.Key;
                    IsRecording = false;
                    IsValid = true;
                    RecorderState = HotkeyRecorderState.Completed;
                    UpdateDisplayText();
                    UpdateHelpText();
                    UpdateVisualState();
                    HotkeyChanged?.Invoke(this, new HotkeyChangedEventArgs(e.Modifiers, e.Key));
                }
            });
        }

        private void OnRecorderPreviewHotkey(object? sender, HotkeyPreviewEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                DisplayText = _recorder?.GetDisplayText() ?? "请按下热键...";
                HelpText = _recorder?.GetHelpText() ?? "";
            });
        }

        private void OnRecorderStateChanged(object? sender, HotkeyRecorderState e)
        {
            Dispatcher.Invoke(() =>
            {
                RecorderState = e;
                IsRecording = e != HotkeyRecorderState.Idle && e != HotkeyRecorderState.Completed;
                UpdateVisualState();

                if (e == HotkeyRecorderState.Idle || e == HotkeyRecorderState.Completed)
                {
                    UpdateDisplayText();
                    UpdateHelpText();
                }
                else
                {
                    DisplayText = _recorder?.GetDisplayText() ?? "";
                    HelpText = _recorder?.GetHelpText() ?? "";
                    ShowHelpText = true;
                }
            });
        }

        private static void OnHotkeyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is HotkeyEditor editor)
            {
                editor.UpdateDisplayText();
                editor.UpdateHelpText();
                editor.UpdateVisualState();
            }
        }

        private static void OnIsRecordingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is HotkeyEditor editor)
            {
                editor.UpdateVisualState();
            }
        }

        private static void OnRecorderStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is HotkeyEditor editor)
            {
                editor.UpdateVisualState();
            }
        }

        private void UpdateVisualState()
        {
            _recordingAnimation?.Stop();
            _waitingAnimation?.Stop();
            _conflictAnimation?.Stop();

            switch (RecorderState)
            {
                case HotkeyRecorderState.Idle:
                case HotkeyRecorderState.Completed:
                    RecordButtonText.Text = "录制";
                    RecordIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Microphone;
                    MainBorder.BorderBrush = (System.Windows.Media.Brush)FindResource("MaterialDesignDivider");
                    
                    if (HotkeyKey != Key.None || HotkeyModifiers != ModifierKeys.None)
                    {
                        RecordIcon.Foreground = System.Windows.Media.Brushes.Green;
                    }
                    else
                    {
                        RecordIcon.Foreground = (System.Windows.Media.Brush)FindResource("MaterialDesignBody");
                    }
                    ShowHelpText = false;
                    break;

                case HotkeyRecorderState.WaitingForFirstKey:
                    RecordButtonText.Text = "停止";
                    RecordIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Stop;
                    RecordIcon.Foreground = System.Windows.Media.Brushes.Orange;
                    _waitingAnimation?.Begin();
                    break;

                case HotkeyRecorderState.RecordingModifiers:
                case HotkeyRecorderState.RecordingPrimaryKey:
                    RecordButtonText.Text = "停止";
                    RecordIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Stop;
                    RecordIcon.Foreground = System.Windows.Media.Brushes.DeepSkyBlue;
                    _recordingAnimation?.Begin();
                    break;
            }
        }

        private void UpdateDisplayText()
        {
            if (HotkeyModifiers == ModifierKeys.None && HotkeyKey == Key.None)
            {
                DisplayText = "点击设置热键";
                ShowClearButton = false;
                return;
            }

            DisplayText = KeyHelper.HotkeyToString(HotkeyModifiers, HotkeyKey);
            ShowClearButton = true;
        }

        private void UpdateHelpText()
        {
            HelpText = RecorderState switch
            {
                HotkeyRecorderState.Idle => "点击开始录制",
                HotkeyRecorderState.WaitingForFirstKey => "请按下热键组合（如Ctrl+Shift+Q）",
                HotkeyRecorderState.RecordingModifiers => "继续添加修饰键，或按主键完成，按ESC取消",
                HotkeyRecorderState.RecordingPrimaryKey => "已选择主键，可继续添加修饰键",
                HotkeyRecorderState.Completed => "录制完成",
                _ => "点击开始录制"
            };

            ShowHelpText = RecorderState != HotkeyRecorderState.Idle && RecorderState != HotkeyRecorderState.Completed;
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!IsRecording || _recorder == null)
                return;

            _pressedAnimation?.Begin();
            _recorder.ProcessKeyDown(e);
        }

        private void OnPreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (!IsRecording || _recorder == null)
                return;

            _recorder.ProcessKeyUp(e);
        }

        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!IsRecording)
            {
                StartRecording();
            }
        }

        private void OnRecordButtonClick(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            if (IsRecording)
            {
                StopRecording();
            }
            else
            {
                StartRecording();
            }
        }

        private void StartRecording()
        {
            _recorder?.StartRecording();
            IsRecording = true;
            RecorderState = HotkeyRecorderState.WaitingForFirstKey;
            Focus();
            DisplayText = "请按下热键组合...";
            HelpText = "请按下热键组合（如Ctrl+Shift+Q）";
            ShowHelpText = true;
            UpdateVisualState();
        }

        private void StopRecording()
        {
            _recorder?.StopRecording();
            IsRecording = false;
            RecorderState = HotkeyRecorderState.Idle;
            UpdateDisplayText();
            UpdateHelpText();
            UpdateVisualState();
        }

        private void OnMouseEnter(object sender, MouseEventArgs e)
        {
            if (!IsRecording && RecorderState == HotkeyRecorderState.Idle)
            {
                MainBorder.Background = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(248, 248, 248));
            }
        }

        private void OnMouseLeave(object sender, MouseEventArgs e)
        {
            if (!IsRecording && RecorderState == HotkeyRecorderState.Idle)
            {
                MainBorder.Background = (System.Windows.Media.Brush)FindResource("MaterialDesignCardBackground");
            }
        }

        private void OnClearButtonClick(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            HotkeyModifiers = ModifierKeys.None;
            HotkeyKey = Key.None;
            _recorder?.Clear();
            IsRecording = false;
            RecorderState = HotkeyRecorderState.Idle;
            UpdateDisplayText();
            UpdateHelpText();
            UpdateVisualState();
        }
    }
}
