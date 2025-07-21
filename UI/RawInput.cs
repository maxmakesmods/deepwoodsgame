using DeepWoods.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

        private const ushort MOUSE_MOVE_RELATIVE = 0x00;
        private const ushort MOUSE_MOVE_ABSOLUTE = 0x01;
        private const ushort MOUSE_VIRTUAL_DESKTOP = 0x02;
        private const ushort MOUSE_ATTRIBUTES_CHANGED = 0x04;
        private const ushort MOUSE_MOVE_NOCOALESCE = 0x08;

        private const ushort RI_MOUSE_BUTTON_1_DOWN = 0x0001;
        private const ushort RI_MOUSE_BUTTON_1_UP = 0x0002;
        private const ushort RI_MOUSE_BUTTON_2_DOWN = 0x0004;
        private const ushort RI_MOUSE_BUTTON_2_UP = 0x0008;
        private const ushort RI_MOUSE_BUTTON_3_DOWN = 0x0010;
        private const ushort RI_MOUSE_BUTTON_3_UP = 0x0020;
        private const ushort RI_MOUSE_BUTTON_4_DOWN = 0x0040;
        private const ushort RI_MOUSE_BUTTON_4_UP = 0x0080;
        private const ushort RI_MOUSE_BUTTON_5_DOWN = 0x0100;
        private const ushort RI_MOUSE_BUTTON_5_UP = 0x0200;
        private const ushort RI_MOUSE_WHEEL = 0x0400;
        private const ushort RI_MOUSE_HWHEEL = 0x0800;

        private const ushort VK_CAPITAL = 0x14;
        private const ushort VK_NUMLOCK = 0x90;
        private const ushort RI_KEY_MAKE = 0;
        private const ushort RI_KEY_BREAK = 1;
        private const ushort RI_KEY_E0 = 2;
        private const ushort RI_KEY_E1 = 4;

        private const ushort VK_SHIFT = 0x10;
        private const ushort VK_CONTROL = 0x11;
        private const ushort VK_MENU = 0x12;

        private const ushort SCANCODE_LSHIFT = 42;
        private const ushort SCANCODE_RSHIFT = 54;
        private const ushort SCANCODE_LCONTROL = 29;
        private const ushort SCANCODE_RCONTROL = 57373;
        private const ushort SCANCODE_LMENU = 56;
        private const ushort SCANCODE_RMENU = 57400;

        private const ushort KEYBOARD_OVERRUN_MAKE_CODE = 0xFF;

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


        private static Dictionary<IntPtr, MouseState> mouseStates = new();
        private static List<IntPtr> mouseHandles = new();
        private static Dictionary<IntPtr, HashSet<Keys>> keyboardStates = new();
        private static List<IntPtr> keyboardHandles = new();


        public static KeyboardState GetKeyboardState(PlayerIndex playerIndex)
        {
            if (keyboardHandles.Count > (int)playerIndex
                && keyboardStates.TryGetValue(keyboardHandles[(int)playerIndex], out var value))
            {
                return new KeyboardState(value.ToArray(),
                    value.Contains(Keys.CapsLock),
                    value.Contains(Keys.NumLock));
            }
            return default;
        }

        public static MouseState GetMouseState(PlayerIndex playerIndex)
        {
            if (mouseHandles.Count > (int)playerIndex
                && mouseStates.TryGetValue(mouseHandles[(int)playerIndex], out var value))
            {
                return value;
            }
            return default;
        }

        internal static List<MouseState> GetMouseStates()
        {
            return mouseStates.Values.ToList();
        }

        private static void UpdateKeyboardKeys(HashSet<Keys> keys, RAWKEYBOARD keyboard)
        {
            if (keyboard.MakeCode == KEYBOARD_OVERRUN_MAKE_CODE)
                return;

            if (keyboard.VKey >= 0xff)
                return;

            if (keyboard.Flags.IsBitFlagSet(RI_KEY_BREAK))
            {
                keys.Remove(GetKeys(keyboard));
            }
            else
            {
                keys.Add(GetKeys(keyboard));
            }
        }

        private static Keys GetKeys(RAWKEYBOARD keyboard)
        { 
            ushort scanCode = keyboard.MakeCode;
            if (keyboard.Flags.IsBitFlagSet(RI_KEY_E0))
            {
                scanCode = (ushort)(scanCode | 0xe000);
            }
            if (keyboard.Flags.IsBitFlagSet(RI_KEY_E1))
            {
                scanCode = (ushort)(scanCode | 0xe100);
            }

            var vkeys = scanCode switch
            {
                SCANCODE_LSHIFT => Keys.LeftShift,
                SCANCODE_RSHIFT => Keys.RightShift,
                SCANCODE_LCONTROL => Keys.LeftControl,
                SCANCODE_RCONTROL => Keys.RightControl,
                SCANCODE_LMENU => Keys.LeftAlt,
                SCANCODE_RMENU => Keys.RightAlt,
                _ => (Keys)keyboard.VKey,
            };
            return vkeys;
        }

        private static MouseState CalculateNextMouseState(MouseState prevState, RAWMOUSE mouseData)
        {
            int mouseSpeed = 2;

            int x = 0;
            int y = 0;
            if (mouseData.ButtonFlags.IsBitFlagSet(MOUSE_MOVE_ABSOLUTE))
            {
                x = mouseData.LastX;
                y = mouseData.LastY;
            }
            else
            {
                x = prevState.X + mouseData.LastX * mouseSpeed;
                y = prevState.Y + mouseData.LastY * mouseSpeed;
            }

            int scrollWheelValue = 0;
            int horizontalScrollWheelValue = 0;
            if (mouseData.ButtonFlags.IsBitFlagSet(RI_MOUSE_WHEEL))
            {
                scrollWheelValue = mouseData.ButtonData;
            }
            else if (mouseData.ButtonFlags.IsBitFlagSet(RI_MOUSE_HWHEEL))
            {
                horizontalScrollWheelValue = mouseData.ButtonData;
            }

            return new MouseState(x, y,
                prevState.ScrollWheelValue + scrollWheelValue,
                GetButtonState(prevState.LeftButton, mouseData.ButtonFlags, RI_MOUSE_BUTTON_1_DOWN, RI_MOUSE_BUTTON_1_UP),
                GetButtonState(prevState.RightButton, mouseData.ButtonFlags, RI_MOUSE_BUTTON_2_DOWN, RI_MOUSE_BUTTON_2_UP),
                GetButtonState(prevState.MiddleButton, mouseData.ButtonFlags, RI_MOUSE_BUTTON_3_DOWN, RI_MOUSE_BUTTON_3_UP),
                GetButtonState(prevState.XButton1, mouseData.ButtonFlags, RI_MOUSE_BUTTON_4_DOWN, RI_MOUSE_BUTTON_4_UP),
                GetButtonState(prevState.XButton2, mouseData.ButtonFlags, RI_MOUSE_BUTTON_5_DOWN, RI_MOUSE_BUTTON_5_UP),
                 prevState.HorizontalScrollWheelValue + horizontalScrollWheelValue
            );
        }

        private static ButtonState GetButtonState(ButtonState prevState, ushort buttonFlags, ushort downFlag, ushort upFlag)
        {
            if (buttonFlags.IsBitFlagSet(downFlag))
            {
                return ButtonState.Pressed;
            }
            if (buttonFlags.IsBitFlagSet(upFlag))
            {
                return ButtonState.Released;
            }
            return prevState;
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
                        if (!keyboardStates.TryGetValue(raw.header.hDevice, out HashSet<Keys> value))
                        {
                            value = new HashSet<Keys>();
                            keyboardStates.Add(raw.header.hDevice, value);
                            keyboardHandles.Add(raw.header.hDevice);
                        }
                        UpdateKeyboardKeys(value, raw.keyboard);
                    }
                    else if (raw.header.dwType == RIM_TYPEMOUSE)
                    {
                        if (!mouseStates.TryGetValue(raw.header.hDevice, out MouseState value))
                        {
                            value = default;
                            mouseStates.Add(raw.header.hDevice, value);
                            mouseHandles.Add(raw.header.hDevice);
                        }
                        mouseStates[raw.header.hDevice] = CalculateNextMouseState(value, raw.mouse);
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
