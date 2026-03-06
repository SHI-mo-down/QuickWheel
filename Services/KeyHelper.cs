using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace QuickWheel.Services
{
    public static class KeyHelper
    {
        public static bool IsModifierKey(Key key)
        {
            return key switch
            {
                Key.LeftCtrl or Key.RightCtrl => true,
                Key.LeftShift or Key.RightShift => true,
                Key.LeftAlt or Key.RightAlt => true,
                Key.LWin or Key.RWin => true,
                Key.System => true,
                _ => false
            };
        }

        public static Key NormalizeModifierKey(Key key)
        {
            return key switch
            {
                Key.LeftCtrl or Key.RightCtrl => Key.LeftCtrl,
                Key.LeftShift or Key.RightShift => Key.LeftShift,
                Key.LeftAlt or Key.RightAlt => Key.LeftAlt,
                _ => key
            };
        }

        public static ModifierKeys KeyToModifiers(Key key)
        {
            return key switch
            {
                Key.LeftCtrl or Key.RightCtrl => ModifierKeys.Control,
                Key.LeftShift or Key.RightShift => ModifierKeys.Shift,
                Key.LeftAlt or Key.RightAlt => ModifierKeys.Alt,
                Key.LWin or Key.RWin => ModifierKeys.Windows,
                Key.System => ModifierKeys.Alt,
                _ => ModifierKeys.None
            };
        }

        public static ModifierKeys KeysToModifiers(IEnumerable<Key> keys)
        {
            ModifierKeys result = ModifierKeys.None;
            foreach (var key in keys)
            {
                result |= KeyToModifiers(key);
            }
            return result;
        }

        public static bool IsIgnoredKey(Key key)
        {
            return key switch
            {
                Key.LeftCtrl or Key.RightCtrl => true,
                Key.LeftShift or Key.RightShift => true,
                Key.LeftAlt or Key.RightAlt => true,
                Key.LWin or Key.RWin => true,
                Key.CapsLock or Key.NumLock or Key.PrintScreen or Key.Pause => true,
                _ => false
            };
        }

        public static bool IsValidPrimaryKey(Key key)
        {
            if (IsModifierKey(key))
                return false;

            if (key == Key.Escape || key == Key.Tab || key == Key.Back ||
                key == Key.Enter || key == Key.Space || key == Key.Delete ||
                key == Key.Insert || key == Key.Home || key == Key.End ||
                key == Key.PageUp || key == Key.PageDown)
                return false;

            if (key >= Key.A && key <= Key.Z)
                return true;

            if (key >= Key.D0 && key <= Key.D9)
                return true;

            if (key >= Key.NumPad0 && key <= Key.NumPad9)
                return true;

            if (key >= Key.F1 && key <= Key.F24)
                return true;

            return key switch
            {
                Key.Oem1 or Key.OemPlus or Key.OemComma or Key.OemMinus or 
                Key.OemPeriod or Key.OemQuestion or Key.OemTilde or 
                Key.OemOpenBrackets or Key.OemCloseBrackets or Key.OemPipe => true,
                _ => false
            };
        }

        public static string KeyToString(Key key)
        {
            if (key == Key.System)
                return "Alt";

            if (IsModifierKey(key))
            {
                return key switch
                {
                    Key.LeftCtrl or Key.RightCtrl => "Ctrl",
                    Key.LeftShift or Key.RightShift => "Shift",
                    Key.LeftAlt or Key.RightAlt => "Alt",
                    Key.LWin or Key.RWin => "Win",
                    _ => key.ToString()
                };
            }

            return key switch
            {
                Key.A => "A", Key.B => "B", Key.C => "C", Key.D => "D", Key.E => "E",
                Key.F => "F", Key.G => "G", Key.H => "H", Key.I => "I", Key.J => "J",
                Key.K => "K", Key.L => "L", Key.M => "M", Key.N => "N", Key.O => "O",
                Key.P => "P", Key.Q => "Q", Key.R => "R", Key.S => "S", Key.T => "T",
                Key.U => "U", Key.V => "V", Key.W => "W", Key.X => "X", Key.Y => "Y", Key.Z => "Z",
                Key.D0 => "0", Key.D1 => "1", Key.D2 => "2", Key.D3 => "3", Key.D4 => "4",
                Key.D5 => "5", Key.D6 => "6", Key.D7 => "7", Key.D8 => "8", Key.D9 => "9",
                Key.NumPad0 => "Num0", Key.NumPad1 => "Num1", Key.NumPad2 => "Num2",
                Key.NumPad3 => "Num3", Key.NumPad4 => "Num4", Key.NumPad5 => "Num5",
                Key.NumPad6 => "Num6", Key.NumPad7 => "Num7", Key.NumPad8 => "Num8",
                Key.NumPad9 => "Num9",
                Key.F1 => "F1", Key.F2 => "F2", Key.F3 => "F3", Key.F4 => "F4",
                Key.F5 => "F5", Key.F6 => "F6", Key.F7 => "F7", Key.F8 => "F8",
                Key.F9 => "F9", Key.F10 => "F10", Key.F11 => "F11", Key.F12 => "F12",
                Key.F13 => "F13", Key.F14 => "F14", Key.F15 => "F15", Key.F16 => "F16",
                Key.F17 => "F17", Key.F18 => "F18", Key.F19 => "F19", Key.F20 => "F20",
                Key.F21 => "F21", Key.F22 => "F22", Key.F23 => "F23", Key.F24 => "F24",
                Key.Space => "空格",
                Key.Enter => "回车",
                Key.Tab => "Tab",
                Key.Back => "退格",
                Key.Delete => "Delete",
                Key.Insert => "Insert",
                Key.Home => "Home", Key.End => "End",
                Key.PageUp => "PageUp", Key.PageDown => "PageDown",
                Key.Up => "↑", Key.Down => "↓", Key.Left => "←", Key.Right => "→",
                Key.Escape => "Esc",
                Key.Oem1 => ";",
                Key.OemPlus => "=",
                Key.OemComma => ",",
                Key.OemMinus => "-",
                Key.OemPeriod => ".",
                Key.OemQuestion => "/",
                Key.OemTilde => "`",
                Key.OemOpenBrackets => "[",
                Key.OemCloseBrackets => "]",
                Key.OemPipe => "\\",
                Key.Multiply => "*",
                Key.Add => "+",
                Key.Subtract => "-",
                Key.Divide => "/",
                Key.Decimal => ".",
                Key.OemClear => "Clear",
                Key.Pause => "Pause",
                Key.PrintScreen => "PrintScreen",
                Key.Scroll => "Scroll",
                _ => key.ToString()
            };
        }

        public static string ModifiersToString(ModifierKeys modifiers)
        {
            var parts = new List<string>();
            
            if (modifiers.HasFlag(ModifierKeys.Control))
                parts.Add("Ctrl");
            if (modifiers.HasFlag(ModifierKeys.Alt))
                parts.Add("Alt");
            if (modifiers.HasFlag(ModifierKeys.Shift))
                parts.Add("Shift");
            if (modifiers.HasFlag(ModifierKeys.Windows))
                parts.Add("Win");
            
            return string.Join(" + ", parts);
        }

        public static string HotkeyToString(ModifierKeys modifiers, Key key)
        {
            var parts = new List<string>();
            
            if (modifiers.HasFlag(ModifierKeys.Control))
                parts.Add("Ctrl");
            if (modifiers.HasFlag(ModifierKeys.Alt))
                parts.Add("Alt");
            if (modifiers.HasFlag(ModifierKeys.Shift))
                parts.Add("Shift");
            if (modifiers.HasFlag(ModifierKeys.Windows))
                parts.Add("Win");
            
            if (key != Key.None)
                parts.Add(KeyToString(key));
            
            return string.Join(" + ", parts);
        }
    }
}
