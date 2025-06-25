using System;
using System.Runtime.InteropServices;

namespace DeepWoods.UI
{
    internal class WindowHelper
    {
        [DllImport("SDL2.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool SDL_GetWindowWMInfo(IntPtr window, ref SDL_SysWMinfo info);

        private enum SysWMType : uint
        {
            SDL_SYSWM_UNKNOWN,
            SDL_SYSWM_WINDOWS,
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SDL_version
        {
            public byte major;
            public byte minor;
            public byte patch;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SDL_SysWMinfo
        {
            public SDL_version version;
            public SysWMType subsystem;
            public SDL_SysWMinfo_win info;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SDL_SysWMinfo_win
        {
            public IntPtr hwnd;
        }

        public static IntPtr GetRealHWNDFromSDL(IntPtr sdlWindow)
        {
            SDL_SysWMinfo info = new();
            info.version.major = 2;
            info.version.minor = 0;
            info.version.patch = 14;
            SDL_GetWindowWMInfo(sdlWindow, ref info);
            return info.info.hwnd;
        }
    }
}
