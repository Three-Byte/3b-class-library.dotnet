using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace ThreeByte.Controls.Keyboard
{

	public struct KEYBDINPUT
	{
		public ushort wVk;
		public ushort wScan;
		public uint dwFlags;
		public long time;
		public uint dwExtraInfo;
	};

	[StructLayout(LayoutKind.Explicit, Size = 28)]
	public struct INPUT
	{
		[FieldOffset(0)]
		public uint type;
		[FieldOffset(4)]
		public KEYBDINPUT ki;
	};

    [StructLayout ( LayoutKind.Sequential, Pack=1 )]
    public struct InputKeys
    {
        public uint type;
        public uint wVk;
        public uint wScan;
        public uint dwFlags;
        public uint time;
        public uint dwExtra;
    }

 
	public class NativeWin32
    {
        //    public const ushort KEYEVENTF_KEYUP = 0x0002;
        public const uint INPUT_KEYBOARD = 1;
        public const uint KEYEVENTF_EXTENDEDKEY = 0x0001;
        public const uint KEYEVENTF_KEYUP = 0x0002;

		[DllImport("user32.dll")]
		public static extern Boolean Keybd_Event(int dwKey, byte bScan, Int32 dwFlags, Int32 dwExtraInfo);

        [DllImport("user32.dll")]
        public static extern short GetKeyState(int keyCode);

        [DllImport("user32.dll")]
        public static extern uint SendInput(uint nInputs, ref INPUT pInputs, int cbSize);
        
        [DllImport ("User32.DLL", EntryPoint="SendInput")]
        public static extern uint SendInput(uint nInputs, InputKeys[] inputs, int cbSize);

        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong);
        [DllImport("user32.dll", SetLastError = true)]
        public static extern UInt32 GetWindowLong(IntPtr hWnd, int nIndex);

    }
}
