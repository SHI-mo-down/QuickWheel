using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace QuickWheel.Services
{
    public enum HotkeyRecorderState
    {
        Idle,
        WaitingForFirstKey,
        RecordingModifiers,
        RecordingPrimaryKey,
        Completed
    }

    public class HotkeyRecorderService : IDisposable
    {
        private HotkeyRecorderState _state = HotkeyRecorderState.Idle;
        private ModifierKeys _capturedModifiers = ModifierKeys.None;
        private Key _capturedKey = Key.None;
        private HashSet<Key> _pressedKeys = new();
        private DateTime _lastKeyTime;
        private DispatcherTimer? _completionTimer;
        private DispatcherTimer? _timeoutTimer;
        private bool _disposed;

        public event EventHandler<HotkeyChangedEventArgs>? HotkeyChanged;
        public event EventHandler<HotkeyRecorderState>? StateChanged;
        public event EventHandler<HotkeyPreviewEventArgs>? PreviewHotkey;

        public HotkeyRecorderState State
        {
            get => _state;
            private set
            {
                if (_state != value)
                {
                    _state = value;
                    StateChanged?.Invoke(this, value);
                }
            }
        }

        public ModifierKeys CapturedModifiers => _capturedModifiers;
        public Key CapturedKey => _capturedKey;

        public HotkeyRecorderService()
        {
            InitializeTimers();
        }

        private void InitializeTimers()
        {
            _completionTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(300)
            };
            _completionTimer.Tick += OnCompletionTimerTick;

            _timeoutTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(10)
            };
            _timeoutTimer.Tick += OnTimeoutTimerTick;
        }

        public void StartRecording()
        {
            Reset();
            State = HotkeyRecorderState.WaitingForFirstKey;
            StartTimeoutTimer();
        }

        public void StopRecording()
        {
            StopTimers();
            State = HotkeyRecorderState.Idle;
        }

        public void CancelRecording()
        {
            Reset();
            State = HotkeyRecorderState.Idle;
        }

        public void Clear()
        {
            Reset();
            HotkeyChanged?.Invoke(this, new HotkeyChangedEventArgs(ModifierKeys.None, Key.None));
            State = HotkeyRecorderState.Idle;
        }

        private void Reset()
        {
            _capturedModifiers = ModifierKeys.None;
            _capturedKey = Key.None;
            _pressedKeys.Clear();
            StopTimers();
        }

        private void StopTimers()
        {
            _completionTimer?.Stop();
            _timeoutTimer?.Stop();
        }

        private void StartTimeoutTimer()
        {
            _timeoutTimer?.Stop();
            _timeoutTimer?.Start();
        }

        private void ResetTimeoutTimer()
        {
            _timeoutTimer?.Stop();
            _timeoutTimer?.Start();
        }

        public void ProcessKeyDown(KeyEventArgs e)
        {
            if (State == HotkeyRecorderState.Idle || State == HotkeyRecorderState.Completed)
                return;

            if (e.IsRepeat)
            {
                e.Handled = true;
                return;
            }

            _lastKeyTime = DateTime.Now;
            ResetTimeoutTimer();

            Key actualKey = GetActualKey(e);
            if (actualKey == Key.None)
            {
                e.Handled = true;
                return;
            }

            if (actualKey == Key.Escape)
            {
                CancelRecording();
                e.Handled = true;
                return;
            }

            if (actualKey == Key.Back)
            {
                Clear();
                e.Handled = true;
                return;
            }

            if (KeyHelper.IsIgnoredKey(actualKey))
            {
                e.Handled = true;
                return;
            }

            if (_pressedKeys.Contains(actualKey))
            {
                e.Handled = true;
                return;
            }

            _pressedKeys.Add(actualKey);

            bool isModifier = KeyHelper.IsModifierKey(actualKey);

            switch (State)
            {
                case HotkeyRecorderState.WaitingForFirstKey:
                    if (isModifier)
                    {
                        AddModifier(actualKey);
                        State = HotkeyRecorderState.RecordingModifiers;
                    }
                    else
                    {
                        SetPrimaryKey(actualKey);
                        State = HotkeyRecorderState.RecordingPrimaryKey;
                    }
                    break;

                case HotkeyRecorderState.RecordingModifiers:
                    if (isModifier)
                    {
                        AddModifier(actualKey);
                    }
                    else
                    {
                        SetPrimaryKey(actualKey);
                        State = HotkeyRecorderState.RecordingPrimaryKey;
                    }
                    break;

                case HotkeyRecorderState.RecordingPrimaryKey:
                    if (isModifier)
                    {
                        AddModifier(actualKey);
                        State = HotkeyRecorderState.RecordingModifiers;
                    }
                    break;
            }

            _completionTimer?.Stop();
            RaisePreviewHotkey();
            e.Handled = true;
        }

        public void ProcessKeyUp(KeyEventArgs e)
        {
            if (State == HotkeyRecorderState.Idle || State == HotkeyRecorderState.Completed)
                return;

            if (e.IsRepeat)
            {
                e.Handled = true;
                return;
            }

            Key actualKey = GetActualKey(e);
            if (actualKey == Key.None)
            {
                e.Handled = true;
                return;
            }

            if (KeyHelper.IsIgnoredKey(actualKey))
            {
                e.Handled = true;
                return;
            }

            _pressedKeys.Remove(actualKey);

            if (State == HotkeyRecorderState.RecordingModifiers || State == HotkeyRecorderState.RecordingPrimaryKey)
            {
                _capturedModifiers = Keyboard.Modifiers & (ModifierKeys.Control | ModifierKeys.Alt | ModifierKeys.Shift | ModifierKeys.Windows);

                _completionTimer?.Stop();
                _completionTimer?.Start();
            }

            e.Handled = true;
        }

        private Key GetActualKey(KeyEventArgs e)
        {
            if (e.Key == Key.System)
                return e.SystemKey;
            return e.Key;
        }

        private void AddModifier(Key key)
        {
            ModifierKeys modifier = KeyHelper.KeyToModifiers(key);
            _capturedModifiers |= modifier;
        }

        private void SetPrimaryKey(Key key)
        {
            _capturedKey = key;
        }

        private void OnCompletionTimerTick(object? sender, EventArgs e)
        {
            _completionTimer?.Stop();

            if (State == HotkeyRecorderState.Idle || State == HotkeyRecorderState.Completed)
                return;

            if (Keyboard.Modifiers == ModifierKeys.None && _pressedKeys.Count == 0)
            {
                CompleteRecording();
            }
        }

        private void OnTimeoutTimerTick(object? sender, EventArgs e)
        {
            CancelRecording();
        }

        private void CompleteRecording()
        {
            if (_capturedKey != Key.None)
            {
                State = HotkeyRecorderState.Completed;
                HotkeyChanged?.Invoke(this, new HotkeyChangedEventArgs(_capturedModifiers, _capturedKey));
            }
            else if (_capturedModifiers != ModifierKeys.None)
            {
                State = HotkeyRecorderState.Completed;
                HotkeyChanged?.Invoke(this, new HotkeyChangedEventArgs(_capturedModifiers, _capturedKey));
            }
            else
            {
                State = HotkeyRecorderState.Idle;
            }

            StopTimers();
        }

        private void RaisePreviewHotkey()
        {
            PreviewHotkey?.Invoke(this, new HotkeyPreviewEventArgs(_capturedModifiers, _capturedKey));
        }

        public string GetDisplayText()
        {
            return State switch
            {
                HotkeyRecorderState.Idle => GetIdleText(),
                HotkeyRecorderState.WaitingForFirstKey => "请按下热键组合...",
                HotkeyRecorderState.RecordingModifiers => GetModifiersText(),
                HotkeyRecorderState.RecordingPrimaryKey => GetPrimaryKeyText(),
                HotkeyRecorderState.Completed => GetCompletedText(),
                _ => "点击设置热键"
            };
        }

        private string GetIdleText()
        {
            if (_capturedModifiers == ModifierKeys.None && _capturedKey == Key.None)
                return "点击设置热键";

            return KeyHelper.HotkeyToString(_capturedModifiers, _capturedKey);
        }

        private string GetModifiersText()
        {
            if (_capturedModifiers == ModifierKeys.None)
                return "请按下热键...";

            return KeyHelper.ModifiersToString(_capturedModifiers);
        }

        private string GetPrimaryKeyText()
        {
            return KeyHelper.HotkeyToString(_capturedModifiers, _capturedKey);
        }

        private string GetCompletedText()
        {
            return KeyHelper.HotkeyToString(_capturedModifiers, _capturedKey);
        }

        public string GetStateText()
        {
            return State switch
            {
                HotkeyRecorderState.Idle => "点击开始录制",
                HotkeyRecorderState.WaitingForFirstKey => "请按下热键组合（如Ctrl+Shift+Q）",
                HotkeyRecorderState.RecordingModifiers => "继续添加修饰键，或按主键完成",
                HotkeyRecorderState.RecordingPrimaryKey => "已选择主键，可继续添加修饰键",
                HotkeyRecorderState.Completed => "录制完成",
                _ => "点击开始录制"
            };
        }

        public string GetHelpText()
        {
            return State switch
            {
                HotkeyRecorderState.Idle => "点击按钮开始录制",
                HotkeyRecorderState.WaitingForFirstKey => "按下任意键开始组合",
                HotkeyRecorderState.RecordingModifiers => "继续按住修饰键（Ctrl/Shift/Alt/Win）或按下主键",
                HotkeyRecorderState.RecordingPrimaryKey => "松开所有按键完成录制，或继续添加修饰键",
                HotkeyRecorderState.Completed => "热键录制完成",
                _ => "按ESC取消录制"
            };
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _completionTimer?.Stop();
                    _timeoutTimer?.Stop();
                }
                _disposed = true;
            }
        }

        ~HotkeyRecorderService()
        {
            Dispose(false);
        }
    }

    public class HotkeyChangedEventArgs : EventArgs
    {
        public ModifierKeys Modifiers { get; }
        public Key Key { get; }

        public HotkeyChangedEventArgs(ModifierKeys modifiers, Key key)
        {
            Modifiers = modifiers;
            Key = key;
        }
    }

    public class HotkeyPreviewEventArgs : EventArgs
    {
        public ModifierKeys Modifiers { get; }
        public Key Key { get; }

        public HotkeyPreviewEventArgs(ModifierKeys modifiers, Key key)
        {
            Modifiers = modifiers;
            Key = key;
        }
    }
}
