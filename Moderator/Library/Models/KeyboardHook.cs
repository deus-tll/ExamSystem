using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace Library.Models
{
	public class KeyboardHook
	{
		[DllImport("user32.dll")]
		private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc callback, IntPtr hInstance, uint threadId);

		[DllImport("user32.dll")]
		private static extern bool UnhookWindowsHookEx(IntPtr hInstance);

		[DllImport("user32.dll")]
		private static extern IntPtr CallNextHookEx(IntPtr idHook, int nCode, IntPtr wParam, IntPtr lParam);

		[DllImport("kernel32.dll")]
		private static extern IntPtr LoadLibrary(string lpFileName);

		[DllImport("user32.dll")]
		private static extern uint MapVirtualKey(uint uCode, uint uMapType);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		private static extern int GetKeyNameText(int lParam, StringBuilder lpString, int nSize);

		private const int WH_KEYBOARD_LL = 13;
		private const int WM_KEYDOWN = 0x0100;
		private static IntPtr _hookID = IntPtr.Zero;
		private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
		public event EventHandler<PressedKey>? PressedKey;

		public void SetHook()
		{
			using (Process process = Process.GetCurrentProcess())
			using (ProcessModule? module = process.MainModule)
			{
				if (module is null)
					throw new Exception("Module is null");
				IntPtr hInstance = LoadLibrary(module.ModuleName);
				_hookID = SetWindowsHookEx(WH_KEYBOARD_LL, KeyboardHookCallback, hInstance, 0);
			}
		}

		public void Unhook()
		{
			UnhookWindowsHookEx(_hookID);
		}


		private IntPtr KeyboardHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
		{
			if (nCode >= 0 && wParam == WM_KEYDOWN)
			{
				int vkCode = Marshal.ReadInt32(lParam);
				StringBuilder sb = new(256);
				uint scancode = MapVirtualKey((uint)vkCode, 0);
				int result = GetKeyNameText((int)(scancode << 16), sb, sb.Capacity);

				if (result > 0)
				{
					PressedKey?.Invoke(null, new PressedKey()
					{
						Key = sb.ToString(),
						PressedDateTime = DateTime.Now
					});
				}
			}
			return CallNextHookEx(_hookID, nCode, wParam, lParam);
		}
	}
}
