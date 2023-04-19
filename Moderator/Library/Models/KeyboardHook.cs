using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace Library.Models
{
	public class KeyboardHook
	{
		public const int WM_KEYDOWN = 0x0100;


		public delegate IntPtr HookProc(int code, IntPtr wParam, IntPtr lParam);


		[DllImport("user32.dll", SetLastError = true)]
		public static extern IntPtr SetWindowsHookEx(HookType hookType, HookProc lpfn, IntPtr hMod, uint dwThreadId);


		[DllImport("user32.dll")]
		public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);


		[DllImport("user32.dll", SetLastError = true)]
		public static extern bool UnhookWindowsHookEx(IntPtr hhk);


		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		public static extern IntPtr GetModuleHandle([MarshalAs(UnmanagedType.LPWStr)] string lpModuleName);


		[DllImport("user32.dll")]
		public static extern uint MapVirtualKey(uint uCode, uint uMapType);


		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		public static extern int GetKeyNameText(int lParam, StringBuilder lpString, int nSize);


		public static IntPtr SetHook(HookProc hookProc, HookType hookType)
		{
			using Process process = Process.GetCurrentProcess();
			using ProcessModule? module = process.MainModule;

			if (module == null) return IntPtr.Zero;

			IntPtr handle = GetModuleHandle(module.ModuleName);

			if (handle != IntPtr.Zero)
				return SetWindowsHookEx(hookType, hookProc, handle, 0);
			else
				return IntPtr.Zero;
		}


		[StructLayout(LayoutKind.Sequential)]
		public class KBDLLHOOKSTRUCT
		{
			public uint vkCode;
			public uint scanCode;
			public KBDLLHOOKSTRUCTFlags flags;
			public uint time;
			public UIntPtr dwExtraInfo;
		}


		[Flags]
		public enum KBDLLHOOKSTRUCTFlags : uint
		{
			LLKHF_EXTENDED = 0x01,
			LLKHF_INJECTED = 0x10,
			LLKHF_ALTDOWN = 0x20,
			LLKHF_UP = 0x80,
		}


		public enum HookType : int
		{
			WH_KEYBOARD_LL = 13
		}
	}
}
