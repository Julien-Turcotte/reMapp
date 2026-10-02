using System;
using System.Runtime.InteropServices;

namespace App1;

internal static class NativeMethods
{
    internal const int WH_KEYBOARD_LL = 13;
    internal const uint WM_KEYDOWN = 0x0100, WM_KEYUP = 0x0101, WM_SYSKEYDOWN = 0x0104, WM_SYSKEYUP = 0x0105;
    internal const uint LLKHF_INJECTED = 0x10, INPUT_KEYBOARD = 1;
    internal const uint KEYEVENTF_EXTENDEDKEY = 0x0001, KEYEVENTF_KEYUP = 0x0002;
    internal const uint MAPVK_VK_TO_VSC = 0;

    internal delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

    [StructLayout(LayoutKind.Sequential)]
    internal struct KBDLLHOOKSTRUCT
    {
        internal uint vkCode, scanCode, flags, time;
        internal UIntPtr dwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct MOUSEINPUT
    {
        internal int dx, dy;
        internal uint mouseData, dwFlags, time;
        internal UIntPtr dwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct KEYBDINPUT
    {
        internal ushort wVk, wScan;
        internal uint dwFlags, time;
        internal UIntPtr dwExtraInfo;
    }

    // Must contain MOUSEINPUT (the largest member) so INPUT is 40 bytes on x64.
    [StructLayout(LayoutKind.Explicit)]
    internal struct InputUnion
    {
        [FieldOffset(0)] internal MOUSEINPUT mi;
        [FieldOffset(0)] internal KEYBDINPUT ki;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct INPUT
    {
        internal uint type;
        internal InputUnion U;
    }

    [DllImport("user32.dll", SetLastError = true)]
    internal static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", SetLastError = true)]
    internal static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll")]
    internal static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll", SetLastError = true)]
    internal static extern uint SendInput(uint cInputs, INPUT[] pInputs, int cbSize);

    [DllImport("user32.dll")]
    internal static extern uint MapVirtualKey(uint uCode, uint uMapType);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    internal static extern IntPtr GetModuleHandle(string? lpModuleName);

    // Sends one synthetic key event. Returns true if Windows accepted it.
    internal static bool Send(int vk, bool up)
    {
        uint flags = up ? KEYEVENTF_KEYUP : 0;
        if (IsExtended(vk)) flags |= KEYEVENTF_EXTENDEDKEY;

        var input = new INPUT
        {
            type = INPUT_KEYBOARD,
            U = { ki = new KEYBDINPUT
            {
                wVk = (ushort)vk,
                wScan = (ushort)MapVirtualKey((uint)vk, MAPVK_VK_TO_VSC),
                dwFlags = flags
            } }
        };

        uint sent = SendInput(1, new[] { input }, Marshal.SizeOf<INPUT>());
#if DEBUG
        System.Diagnostics.Debug.WriteLine(
            $"SendInput vk=0x{vk:X2} up={up} sent={sent} size={Marshal.SizeOf<INPUT>()} err={Marshal.GetLastWin32Error()}");
#endif
        return sent == 1;
    }

    // Keys that need KEYEVENTF_EXTENDEDKEY: right Ctrl/Alt, arrows, etc.
    private static bool IsExtended(int vk) =>
        vk == 0xA3 /*RCtrl*/ || vk == 0xA5 /*RAlt*/ ||
        (vk >= 0x21 && vk <= 0x28) /*PgUp..Down*/ || vk == 0x2D || vk == 0x2E /*Ins, Del*/;
}