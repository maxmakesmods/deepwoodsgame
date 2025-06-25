using DeepWoods.Game;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace DeepWoods.UI
{
    internal class RawInput
    {
        private const int RID_INPUT = 0x10000003;
        private const int RIM_TYPEMOUSE = 0;
        private const int RIM_TYPEKEYBOARD = 1;
        private const int RIDEV_INPUTSINK = 0x00000100;
        private const int WH_GETMESSAGE = 3;
        private const int WM_INPUT = 0x00FF;

        private delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);
        private static IntPtr hookHandle;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool RegisterRawInputDevices(
            [In] RAWINPUTDEVICE[] pRawInputDevices,
            uint uiNumDevices,
            uint cbSize
        );

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetRawInputData(
            IntPtr hRawInput,
            uint uiCommand,
            IntPtr pData,
            ref uint pcbSize,
            uint cbSizeHeader
        );

        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32.dll")]
        private static extern uint GetCurrentThreadId();

        [StructLayout(LayoutKind.Sequential)]
        private struct RAWINPUTDEVICE
        {
            public ushort UsagePage;
            public ushort Usage;
            public uint Flags;
            public IntPtr Target;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RAWINPUTHEADER
        {
            public uint dwType;
            public uint dwSize;
            public IntPtr hDevice;
            public IntPtr wParam;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RAWKEYBOARD
        {
            public ushort MakeCode;
            public ushort Flags;
            public ushort Reserved;
            public ushort VKey;
            public uint Message;
            public uint ExtraInformation;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct RAWMOUSE
        {
            [FieldOffset(0)]
            public ushort Flags;
            [FieldOffset(4)]
            public ushort ButtonFlags;
            [FieldOffset(6)]
            public short ButtonData;
            [FieldOffset(12)]
            public int LastX;
            [FieldOffset(16)]
            public int LastY;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct RAWINPUT
        {
            [FieldOffset(0)]
            public RAWINPUTHEADER header;
            [FieldOffset(24)]
            public RAWMOUSE mouse;
            [FieldOffset(24)]
            public RAWKEYBOARD keyboard;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MSG
        {
            public IntPtr hwnd;
            public uint message;
            public UIntPtr wParam;
            public IntPtr lParam;
            public uint time;
            public POINT pt;
        }


        public static Dictionary<IntPtr, Point> mousePositions = new();


        public static Point AddMousePos(Point prevPos, int x, int y)
        {
            float mouseSpeed = 2f;

            return new(
                Math.Clamp((int)(prevPos.X + x * mouseSpeed), 0, DeepWoodsGame.window.ClientBounds.Width - 10),
                Math.Clamp((int)(prevPos.Y + y * mouseSpeed), 0, DeepWoodsGame.window.ClientBounds.Height - 10)
                );
        }


        private static void HandleRawInput(MSG msg)
        {
            uint dwSize = 0;
            _ = GetRawInputData(msg.lParam, RID_INPUT, IntPtr.Zero, ref dwSize, (uint)Marshal.SizeOf<RAWINPUTHEADER>());
            IntPtr buffer = Marshal.AllocHGlobal((int)dwSize);
            try
            {
                if (GetRawInputData(msg.lParam, RID_INPUT, buffer, ref dwSize, (uint)Marshal.SizeOf<RAWINPUTHEADER>()) == dwSize)
                {
                    RAWINPUT raw = Marshal.PtrToStructure<RAWINPUT>(buffer);
                    if (raw.header.dwType == RIM_TYPEKEYBOARD)
                    {
                        Debug.WriteLine(
                            $" hDevice {raw.header.hDevice}," +
                            $" MakeCode={raw.keyboard.MakeCode}," +
                            $" Flags={raw.keyboard.Flags}," +
                            $" Reserved={raw.keyboard.Reserved}," +
                            $" VKey={raw.keyboard.VKey}," +
                            $" Message={raw.keyboard.Message}");
                    }
                    else if (raw.header.dwType == RIM_TYPEMOUSE)
                    {
                        if (!mousePositions.ContainsKey(raw.header.hDevice))
                        {
                            mousePositions.Add(raw.header.hDevice, new Point());
                        }

                        mousePositions[raw.header.hDevice] = AddMousePos(mousePositions[raw.header.hDevice], raw.mouse.LastX, raw.mouse.LastY);


                        Debug.WriteLine(
                            $" hDevice {raw.header.hDevice}," +
                            $" Flags={raw.mouse.Flags}," +
                            $" ButtonFlags={raw.mouse.ButtonFlags}," +
                            $" ButtonData={raw.mouse.ButtonData}," +
                            $" LastX={raw.mouse.LastX}," +
                            $" LastY={raw.mouse.LastY}");
                    }
                }
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                var msg = (MSG)Marshal.PtrToStructure(lParam, typeof(MSG));
                if (msg.message == WM_INPUT)
                {
                    HandleRawInput(msg);
                }
            }
            return CallNextHookEx(hookHandle, nCode, wParam, lParam);
        }

        public static void Initialize(IntPtr hWnd)
        {
            RAWINPUTDEVICE[] rid = new RAWINPUTDEVICE[2];

            // keyboard
            rid[0].UsagePage = 0x01;
            rid[0].Usage = 0x06;
            rid[0].Flags = RIDEV_INPUTSINK;
            rid[0].Target = hWnd;

            // mouse
            rid[1].UsagePage = 0x01;
            rid[1].Usage = 0x02;
            rid[1].Flags = 0;
            rid[1].Target = hWnd;

            bool registered = RegisterRawInputDevices(rid, (uint)rid.Length, (uint)Marshal.SizeOf<RAWINPUTDEVICE>());
            if (!registered)
            {
                Debug.WriteLine($"RegisterRawInputDevices failed: {Marshal.GetLastWin32Error()}");
                return;
            }

            hookHandle = SetWindowsHookEx(WH_GETMESSAGE, HookCallback, GetModuleHandle(null), GetCurrentThreadId());
        }
    }
}
