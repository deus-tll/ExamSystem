using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace Library.Models
{
	public partial class ManageMonitoring
	{
		#region Fields and Ctors
		private readonly string PATH_TO_LOG_ERRORS;
		private readonly Settings SETTINGS;
		private IntPtr hookKeyboard = IntPtr.Zero;
		private ProcessWatcher? _processWatcher;
		private StatisticsReport? _statisticsReport;
		private bool _hasStatisticsReportBeenUpdated = false;
		private ModeratingReport? _moderatingReport;
		private bool _hasModeratingReportBeenUpdated = false;
		private StringBuilder _currentWord;
		private readonly System.Timers.Timer TIMER;

		private object _lockObjectToModeratingReport;
		private object _lockObjectToStatisticsReport;

		public ManageMonitoring(string pathToLog, Settings settings)
		{
			PATH_TO_LOG_ERRORS = pathToLog;
			SETTINGS = settings;
			_currentWord = new();
			_lockObjectToModeratingReport = new();
			_lockObjectToStatisticsReport = new();
			TIMER = new()
			{
				Interval = 5000,
			};

			TIMER.Elapsed += Timer_Elapsed;
		}
		#endregion


		#region Managing
		public void StartMonitoring() => Monitoring();


		public void StopMonitoring()
		{
			if (hookKeyboard != IntPtr.Zero)
				KeyboardHook.UnhookWindowsHookEx(hookKeyboard);

			_processWatcher?.StopWatch();
			TIMER.Stop();

			_hasStatisticsReportBeenUpdated = true;
			_hasModeratingReportBeenUpdated = true;
			TimerElapsedStatistics();
			TimerElapsedModerating();
		}


		private void Monitoring()
		{
			try
			{
				if (SETTINGS.GatheringStatistic)
					GetStatisticsReport();
				if (SETTINGS.PerformModeration)
					GetModeratingReport();
			}
			catch (Exception ex)
			{
				LogWriter.WriteLog(PATH_TO_LOG_ERRORS, $"{ex.Message} | {DateTime.Now}");
			}

			SetHook();
			StartWatchingProcesses();

			TIMER.Start();
		}


		private void SetHook()
		{
			if (hookKeyboard == IntPtr.Zero)
				hookKeyboard = KeyboardHook.SetHook(HookProcedure, KeyboardHook.HookType.WH_KEYBOARD_LL);
		}


		private void StartWatchingProcesses()
		{
			_processWatcher = new ProcessWatcher(PATH_TO_LOG_ERRORS);
			_processWatcher.ProcessStarted += ProcessWatcher_ProcessStarted;
			_processWatcher.StartWatch();
		}


		private IntPtr HookProcedure(int nCode, IntPtr wParam, IntPtr lParam)
		{
			try
			{
				HandleHook(nCode, wParam, lParam);
			}
			catch (Exception ex)
			{
				LogWriter.WriteLog(PATH_TO_LOG_ERRORS, $"{ex.Message} | {DateTime.Now}");
			}
			
			return KeyboardHook.CallNextHookEx(hookKeyboard, nCode, wParam, lParam);
		}

		private void HandleHook(int nCode, IntPtr wParam, IntPtr lParam)
		{
			if (nCode >= 0 && wParam == KeyboardHook.WM_KEYDOWN)
			{
				KeyboardHook.KBDLLHOOKSTRUCT? hookStruct = Marshal.PtrToStructure<KeyboardHook.KBDLLHOOKSTRUCT>(lParam);
				if (hookStruct is null) return;

				StringBuilder sb = new(256);
				uint scancode = KeyboardHook.MapVirtualKey(hookStruct.vkCode, 0);
				int result = KeyboardHook.GetKeyNameText((int)(scancode << 16), sb, sb.Capacity);

				bool shiftPressed = (KeyboardHook.GetKeyState(KeyboardHook.VK_SHIFT) & 0x8000) != 0;

				if (result > 0)
				{
					string key = shiftPressed ? sb.ToString().ToUpper() : sb.ToString().ToLower();

					PressedKeyHandler(new PressedKey()
					{
						Key = key,
						PressedDateTime = DateTime.Now
					});
				}
			}
		}
		#endregion


		#region Timer handlers
		private async void Timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
		{
			await Task.Run(TimerElapsedStatistics);
			await Task.Run(TimerElapsedModerating);
		}


		private void TimerElapsedStatistics()
		{
			lock (_lockObjectToStatisticsReport)
			{
				if (!SETTINGS.GatheringStatistic) return;
				if (!_hasStatisticsReportBeenUpdated) return;

				if (_statisticsReport is not null && SETTINGS.PathStatisticsReport is not null)
					ManageReports.SaveStatisticsReport(_statisticsReport, SETTINGS.PathStatisticsReport);

				_hasStatisticsReportBeenUpdated = false;
			}
		}


		private void TimerElapsedModerating()
		{
			lock (_lockObjectToModeratingReport)
			{
				if (!SETTINGS.PerformModeration) return;
				if (!_hasModeratingReportBeenUpdated) return;

				if (_moderatingReport is not null && SETTINGS.PathModeratingReport is not null)
					ManageReports.SaveModeratingReport(_moderatingReport, SETTINGS.PathModeratingReport);

				_hasModeratingReportBeenUpdated = false;
			}
		}
		#endregion


		#region Handlers
		private async void ProcessWatcher_ProcessStarted(object? sender, Process process)
		{
			await Task.Run(() => ProcessStartedHandlerStatistics(process));
			await Task.Run(() => ProcessStartedHandlerModerating(process));
		}


		private async void PressedKeyHandler(PressedKey pressedKey)
		{
			await Task.Run(() => PressedKeyHandlerStatistics(pressedKey));
			await Task.Run(() => PressedKeyHandlerModerating(pressedKey));
		}


		private void ProcessStartedHandlerStatistics(Process process)
		{
			lock (_lockObjectToStatisticsReport)
			{
				string? pathToExe = process.MainModule?.FileName;
				if (_statisticsReport is null ||
					!SETTINGS.GatheringStatistic ||
					pathToExe is null ||
					SETTINGS?.ForbiddenPrograms?.Contains(pathToExe) == true) return;

				LaunchedProgram launchedProgram = new()
				{
					Name = process.ProcessName,
					PathToExe = pathToExe,
					WindowHandle = process.MainWindowHandle,
					StartedDateTime = DateTime.Now,
				};

				_statisticsReport.LaunchedPrograms.Add(launchedProgram);

				_hasStatisticsReportBeenUpdated = true;
			}
		}


		private void ProcessStartedHandlerModerating(Process process)
		{
			lock (_lockObjectToModeratingReport)
			{
				if (_moderatingReport is null) return;
				if (!SETTINGS.PerformModeration) return;

				CheckForbiddenProgram(process);
			}
		}


		private void PressedKeyHandlerStatistics(PressedKey pressedKey)
		{
			lock (_lockObjectToStatisticsReport)
			{
				if (_statisticsReport is null || !SETTINGS.GatheringStatistic) return;

				_statisticsReport.PressedKeys.Add(pressedKey);
				_hasStatisticsReportBeenUpdated = true;
			}
		}


		private void PressedKeyHandlerModerating(PressedKey pressedKey)
		{
			lock (_lockObjectToModeratingReport)
			{
				if (_moderatingReport is null) return;
				if (!SETTINGS.PerformModeration) return;

				if (MyRegex().IsMatch(pressedKey.Key))
					_currentWord.Append(pressedKey.Key);
				else
					CheckCurrentWord();
			}
		}
		#endregion



		#region Additional Methods
		private void GetStatisticsReport()
		{
			if (SETTINGS.PathStatisticsReport is null)
				throw new NullReferenceException($"{nameof(SETTINGS.PathStatisticsReport)} is null");

			_statisticsReport = ManageReports.GetStatisticsReport(SETTINGS.PathStatisticsReport);
		}


		private void GetModeratingReport()
		{
			if (SETTINGS.PathModeratingReport is null)
				throw new NullReferenceException($"{nameof(SETTINGS.PathModeratingReport)} is null");

			_moderatingReport = ManageReports.GetModeratingReport(SETTINGS.PathModeratingReport);
		}


		private void CheckCurrentWord()
		{
			string word = _currentWord.ToString();
			if (!string.IsNullOrEmpty(word))
			{
				if (SETTINGS.SpecificWords?.Contains(word) == false) return;

				TypedWord typedWord = new()
				{
					Word = word,
					TypedDateTime = DateTime.Now
				};

				_moderatingReport?.TypedWords.Add(typedWord);

				_currentWord.Clear();

				_hasModeratingReportBeenUpdated = true;
			}
		}


		private void CheckForbiddenProgram(Process process)
		{
			string? pathToExe = process.MainModule?.FileName;
			if (pathToExe is null) return;
			if (SETTINGS.ForbiddenPrograms?.Contains(pathToExe) == true)
			{
				LaunchedProgram launchedProgram = new()
				{
					Name = process.ProcessName,
					PathToExe = process.MainModule?.FileName,
					WindowHandle = process.MainWindowHandle,
					StartedDateTime = DateTime.Now,
				};

				process.Kill();

				_moderatingReport?.LaunchedForbiddenPrograms.Add(launchedProgram);

				_hasModeratingReportBeenUpdated = true;
			}
		}

		[GeneratedRegex("^[\\p{L}\\p{Mn}\\p{Pd}']$")]
		private static partial Regex MyRegex();
		#endregion
	}
}
