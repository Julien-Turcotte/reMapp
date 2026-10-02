using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Runtime.InteropServices;

namespace App1;

public sealed partial class MainWindow : Window
{
    private sealed record KeyOption(string Name, int Vk)
    {
        public override string ToString() => Name;
    }

    private readonly NativeMethods.LowLevelKeyboardProc _proc;
    private IntPtr _hook;
    private bool _isReady;
    private int _keyA = 0x14;
    private int _keyB = 0x1B;

    public MainWindow()
    {
        InitializeComponent();
        AppWindow.Resize(new Windows.Graphics.SizeInt32(240, 260));

        var options = new[]
        {
            new KeyOption("CapsLock", 0x14),
            new KeyOption("Esc", 0x1B),
            new KeyOption("Tab", 0x09),
            new KeyOption("LShift", 0xA0),
            new KeyOption("RShift", 0xA1),
            new KeyOption("LCtrl", 0xA2),
            new KeyOption("RCtrl", 0xA3),
            new KeyOption("LAlt", 0xA4),
            new KeyOption("RAlt", 0xA5),
            new KeyOption("Enter", 0x0D),
            new KeyOption("Backspace", 0x08)
        };

        KeyABox.ItemsSource = options;
        KeyBBox.ItemsSource = options;
        KeyABox.SelectedItem = options[0];
        KeyBBox.SelectedItem = options[1];
        _isReady = true;

        _proc = HookCallback; // kept in a field so it isn't garbage-collected
        Closed += (_, _) => StopHook();
    }

    private static int GetKey(ComboBox? box, int fallback) =>
        box?.SelectedItem is KeyOption k ? k.Vk : fallback;

    private void OnSwapToggleClick(object sender, RoutedEventArgs e)
    {
        if (!_isReady)
        {
            SwapToggle.IsChecked = false;
            return;
        }

        if (SwapToggle.IsChecked == true)
        {
            var keyA = GetKey(KeyABox, -1);
            var keyB = GetKey(KeyBBox, -1);
            if (keyA < 0 || keyB < 0 || keyA == keyB)
            {
                SwapToggle.IsChecked = false;
                return;
            }

            _keyA = keyA;
            _keyB = keyB;
            _hook = NativeMethods.SetWindowsHookEx(
                NativeMethods.WH_KEYBOARD_LL, _proc, NativeMethods.GetModuleHandle(null), 0);
            if (_hook == IntPtr.Zero)
            {
                SwapToggle.IsChecked = false;
                return;
            }

            SwapToggle.Content = "Swap: ON";
            KeyABox.IsEnabled = KeyBBox.IsEnabled = false;
            return;
        }

        StopHook();
        SwapToggle.Content = "Swap: OFF";
        KeyABox.IsEnabled = KeyBBox.IsEnabled = true;
    }

    private void StopHook()
    {
        if (_hook == IntPtr.Zero) return;
        NativeMethods.UnhookWindowsHookEx(_hook);
        _hook = IntPtr.Zero;
    }

    private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode < 0) return NativeMethods.CallNextHookEx(_hook, nCode, wParam, lParam);

        var info = Marshal.PtrToStructure<NativeMethods.KBDLLHOOKSTRUCT>(lParam);

        // Ignore our own injected events so we don't loop.
        if ((info.flags & NativeMethods.LLKHF_INJECTED) != 0)
            return NativeMethods.CallNextHookEx(_hook, nCode, wParam, lParam);

        var msg = (uint)wParam;
        bool down = msg == NativeMethods.WM_KEYDOWN || msg == NativeMethods.WM_SYSKEYDOWN;
        bool up = msg == NativeMethods.WM_KEYUP || msg == NativeMethods.WM_SYSKEYUP;
        if (!down && !up)
            return NativeMethods.CallNextHookEx(_hook, nCode, wParam, lParam);

        int target = info.vkCode == _keyA ? _keyB : info.vkCode == _keyB ? _keyA : 0;
        if (target == 0)
            return NativeMethods.CallNextHookEx(_hook, nCode, wParam, lParam);

        // Send the replacement; only swallow the original if it was accepted.
        if (NativeMethods.Send(target, up))
            return (IntPtr)1;

        return NativeMethods.CallNextHookEx(_hook, nCode, wParam, lParam);
    }
}