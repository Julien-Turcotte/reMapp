using System;
using System.Runtime.InteropServices;

namespace App1;

internal static class NativeMethods
{
    internal const int WH_KEYBOARD_LL = 13;
    internal const uint WM_KEYDOWN = 0x0100, WM_KEYUP = 0x0101, WM_SYSKEYDOWN = 0x0104, WM_SYSKEYUP = 0x0105;
    internal const uint LLKHF_INJECTED = 0x10, INPUT_KEYBOARD = 1, KEYEVENTF_KEYUP = 0x0002;

    internal delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

    [StructLayout(LayoutKind.Sequential)] internal struct KBDLLHOOKSTRUCT { internal uint vkCode, scanCode, flags, time; internal UIntPtr dwExtraInfo; }
    [StructLayout(LayoutKind.Sequential)] internal struct INPUT { internal uint type; internal InputUnion U; }
    [StructLayout(LayoutKind.Explicit)] internal struct InputUnion { [FieldOffset(0)] internal KEYBDINPUT ki; }
    [StructLayout(LayoutKind.Sequential)] internal struct KEYBDINPUT { internal ushort wVk, wScan; internal uint dwFlags, time; internal UIntPtr dwExtraInfo; }

    [DllImport("user32.dll", SetLastError = true)] internal static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);
    [DllImport("user32.dll", SetLastError = true)] internal static extern bool UnhookWindowsHookEx(IntPtr hhk);
    [DllImport("user32.dll")] internal static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);
    [DllImport("user32.dll", SetLastError = true)] internal static extern uint SendInput(uint cInputs, INPUT[] pInputs, int cbSize);
}
