using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using System;
using System.Runtime.InteropServices;

namespace App1;

public sealed partial class MainWindow : Window
{
    private readonly NativeMethods.LowLevelKeyboardProc _proc;
    private IntPtr _hook;
    private int _keyA = 0x14;
    private int _keyB = 0x1B;

    public MainWindow()
    {
        InitializeComponent();
        _proc = HookCallback;
        Closed += (_, _) => StopHook();
    }

    private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _keyA = GetKey(KeyABox, 0x14);
        _keyB = GetKey(KeyBBox, 0x1B);
    }

    private static int GetKey(ComboBox box, int fallback) =>
        box.SelectedItem is ComboBoxItem i && int.TryParse(i.Tag?.ToString(), out var v) ? v : fallback;

    private void OnSwapToggleClick(object sender, RoutedEventArgs e)
    {
        if (SwapToggle.IsChecked == true)
        {
            _hook = NativeMethods.SetWindowsHookEx(NativeMethods.WH_KEYBOARD_LL, _proc, IntPtr.Zero, 0);
            if (_hook == IntPtr.Zero) { SwapToggle.IsChecked = false; return; }
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
        if ((info.flags & NativeMethods.LLKHF_INJECTED) != 0) return NativeMethods.CallNextHookEx(_hook, nCode, wParam, lParam);

        var msg = (uint)wParam;
        bool down = msg == NativeMethods.WM_KEYDOWN || msg == NativeMethods.WM_SYSKEYDOWN;
        bool up = msg == NativeMethods.WM_KEYUP || msg == NativeMethods.WM_SYSKEYUP;
        if (!down && !up) return NativeMethods.CallNextHookEx(_hook, nCode, wParam, lParam);

        var target = info.vkCode == _keyA ? _keyB : info.vkCode == _keyB ? _keyA : 0;
        if (target == 0) return NativeMethods.CallNextHookEx(_hook, nCode, wParam, lParam);

        var input = new NativeMethods.INPUT
        {
            type = NativeMethods.INPUT_KEYBOARD,
            U = new NativeMethods.InputUnion
            {
                ki = new NativeMethods.KEYBDINPUT { wVk = (ushort)target, dwFlags = up ? NativeMethods.KEYEVENTF_KEYUP : 0 }
            }
        };
        NativeMethods.SendInput(1, new[] { input }, Marshal.SizeOf<NativeMethods.INPUT>());
        return (IntPtr)1;
    }
}
