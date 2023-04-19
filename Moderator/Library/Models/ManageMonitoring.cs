﻿using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace Library.Models
{
	public class ManageMonitoring
	{
		private readonly string PATH_TO_LOG_ERRORS;
		private readonly Settings SETTINGS;
		private KeyboardHook? _keyboardHook;
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

		private void Timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
		{
			ThreadPool.QueueUserWorkItem(_ => TimerElapsedStatistics());
			ThreadPool.QueueUserWorkItem(_ => TimerElapsedModerating());
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


		public void StartMonitorig()
		{
			Monitoring();
		}

		public void StopMonitoring()
		{
			_keyboardHook?.Unhook();
			_processWatcher?.StopWatch();
			TIMER.Stop();
			TimerElapsedStatistics();
			TimerElapsedModerating();
		}

		private void Monitoring()
		{
			try
			{
				if (SETTINGS.GatheringStatistic)
					GetStatisticsReport();
				if(SETTINGS.PerformModeration)
					GetModeratingReport();
			}
			catch (Exception ex)
			{
				WriteLog($"{ex.Message} | {DateTime.Now}");
			}

			SetHook();
			StartWatchingProcesses();

			TIMER.Start();
		}

		private void SetHook()
		{
			_keyboardHook = new KeyboardHook();
			_keyboardHook.PressedKey += KeyboardHook_PressedKey;
			_keyboardHook.SetHook();
		}

		private void StartWatchingProcesses()
		{
			_processWatcher = new ProcessWatcher();
			_processWatcher.ProcessStarted += ProcessWatcher_ProcessStarted;
			_processWatcher.StartWatch();
		}

		private void ProcessWatcher_ProcessStarted(object? sender, Process process)
		{
			ProcessStartedHandler(process);
		}

		private void ProcessStartedHandler(Process process)
		{
			ThreadPool.QueueUserWorkItem((param) => ProcessStartedHandlerStatistics(process));
			ThreadPool.QueueUserWorkItem((param) => ProcessStartedHandlerModerating(process));
		}

		private void ProcessStartedHandlerStatistics(Process process)
		{
			lock (_lockObjectToStatisticsReport)
			{
				if (_statisticsReport is null || !SETTINGS.GatheringStatistic) return;

				LaunchedProgram launchedProgram = new()
				{
					Name = process.ProcessName,
					MachineName = process.MachineName,
					PathToExe = process.MainModule?.FileName,
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

				string? pathToExe = process.MainModule?.FileName;
				if (pathToExe is null) return;
				if (SETTINGS.ForbiddenPrograms?.Contains(pathToExe) == true)
				{
					LaunchedProgram launchedProgram = new()
					{
						Name = process.ProcessName,
						MachineName = process.MachineName,
						PathToExe = process.MainModule?.FileName,
						WindowHandle = process.MainWindowHandle,
						StartedDateTime = DateTime.Now,
					};

					process.Kill();

					_moderatingReport.LaunchedForbiddenPrograms.Add(launchedProgram);

					_hasModeratingReportBeenUpdated = true;
				}
			}
		}

		private void KeyboardHook_PressedKey(object? sender, PressedKey pressedKey)
		{
			PressedKeyHandler(pressedKey);
		}

		private void PressedKeyHandler(PressedKey pressedKey)
		{
			ThreadPool.QueueUserWorkItem((param) => PressedKeyHandlerStatistics(pressedKey));
			ThreadPool.QueueUserWorkItem((param) => PressedKeyHandlerModerating(pressedKey));
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

				if (Regex.IsMatch(pressedKey.Key, @"\p{L}"))
					_currentWord.Append(pressedKey.Key);
				else
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

						_moderatingReport.TypedWords.Add(typedWord);

						_currentWord.Clear();

						_hasModeratingReportBeenUpdated = true;
					}
				}
			}
		}

		private void GetStatisticsReport()
		{
			if (SETTINGS.PathStatisticsReport is null)
				throw new NullReferenceException($"{nameof(SETTINGS.PathStatisticsReport)} is null");

			_statisticsReport = ManageReports.GetStatisticsReport(SETTINGS.PathStatisticsReport);
		}


		private void GetModeratingReport()
		{
			if (SETTINGS.PathStatisticsReport is null)
				throw new NullReferenceException($"{nameof(SETTINGS.PathStatisticsReport)} is null");

			_moderatingReport = ManageReports.GetModeratingReport(SETTINGS.PathStatisticsReport);
		}


		private void WriteLog(string log)
		{
			using StreamWriter writer = new(PATH_TO_LOG_ERRORS, true);
			writer.WriteLine(log);
		}
	}
}
