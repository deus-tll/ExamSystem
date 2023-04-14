using System.IO;
using System.Linq;

namespace Library.ManageForbiddenWords_Parts
{
	public partial class ManageForbiddenWords
	{
		public static List<string> GetForbiddenWords(string path)
		{
			List<string> result = new();
			using (StreamReader reader = new(path))
			{
				string? line;
				while ((line = reader.ReadLine()) is not null)
				{
					result.Add(line);
				}
			}

			if (result.Count <= 0)
				throw new Exception("There is no words in the file");

			return result;
		}


		private void InitializeResultsFolder()
		{
			string path = $"{PATH_TO_CURRENT_RESULTS}\\{NAME_OF_RESULTS_DIRECTORY}";
			string directory = "Results_";
			string resultFolderPath;

			static void action(string path)
			{
				_ = Directory.CreateDirectory(path);
				_ = Directory.CreateDirectory($"{path}\\Copies");
				_ = Directory.CreateDirectory($"{path}\\CopiesWithReplaces");
			}


			if (!Directory.Exists(path))
			{
				_ = Directory.CreateDirectory(path);
				resultFolderPath = $"{path}\\{directory}{1}";
				action(resultFolderPath);
			}
			else
			{
				DirectoryInfo directoryInfo = new(path);
				int count = directoryInfo.GetDirectories().Count(d => d.Name.Contains(directory));
				resultFolderPath = $"{path}\\{directory}{count + 1}";
			}

			action(resultFolderPath);
			PATH_TO_CURRENT_RESULTS = resultFolderPath;
		}


		private DriveInfo[]? EstimateAndGetDrives()
		{
			DriveInfo[] drives = DriveInfo.GetDrives();
			_totalDriveCount = Convert.ToInt16(drives.Length);

			if (_withInterface)
			{
				_pauseEvent = new ManualResetEvent(false);
				_cancellationTokenSourceStop = new();

				try
				{
					Task task = Task.Run(() => WorkEstimate(ref drives), _cancellationTokenSourceStop.Token);
					task.Wait();
				}
				catch (AggregateException ae)
				{
					foreach (var ex in ae.Flatten().InnerExceptions)
					{
						if (ex is OperationCanceledException)
						{
							return null;
						}
						else
							ErrorOccurred?.Invoke(null, $"{ex.HelpLink}\n{ex.InnerException}\n{ex.Source}\n{ex.Message}");
					}
				}

				GetTotalCountOfDiscs?.Invoke(this, _totalDriveCount);
				GetTotalCountOfFiles?.Invoke(this, _totalFileCount);

				SetProgress(0, _currentDrive, null);

				ProgressChanged?.Invoke("drive", _progressChanged); 
			}

			return drives;
		}


		private void SetProgress(double progress, long currentCountElements, string? FullName)
		{
			_progressChanged.Progress = progress;
			_progressChanged.CurrentCountElements = currentCountElements;
			_progressChanged.FullName = FullName;
		}


		private void WorkEstimate(ref DriveInfo[] drives)
		{
			EstimateProgressChanged?.Invoke(null, true);

			for (int i = 0; i < drives.Length; i++)
			{
				if (!drives[i].IsReady) continue;

				try
				{
					string drive = drives[i].RootDirectory.FullName;
					_totalFileCount += CountTextFiles(ref drive);
				}
				catch (OperationCanceledException ex)
				{
					throw new OperationCanceledException(ex.Message);
				}
				catch (Exception ex)
				{
					ErrorOccurred?.Invoke(this, ex.Message);
				}
			}

			EstimateProgressChanged?.Invoke(null, false);
		}

		private long CountTextFiles(ref string path)
		{
			long count = 0;

			try
			{
				string[] files = Directory.GetFiles(path, "*.txt");

				count = files.LongLength;

				var directories = Directory.GetDirectories(path);

				for (int i = 0; i < directories.Length; i++)
				{
					if (_restrictedDirectories.Contains(directories[i]))
						continue;

					CheckForPauseResumeStop();

					count += CountTextFiles(ref directories[i]);

					CheckForPauseResumeStopCycle();
				}
			}
			catch (UnauthorizedAccessException) { }
			catch (OperationCanceledException ex)
			{
				throw new OperationCanceledException(ex.Message);
			}

			return count;
		}


		private void CheckForPauseResumeStop()
		{
			if (_cancellationTokenSourceStop is not null &&
				_cancellationTokenSourceStop.Token.IsCancellationRequested)
				_cancellationTokenSourceStop.Token.ThrowIfCancellationRequested();

			if (_pauseEvent is not null &&
				_pauseEvent.WaitOne(0))
			{
				_pauseEvent.WaitOne();
			}
		}


		private void CheckForPauseResumeStopCycle()
		{
			while (_pauseEvent is not null &&
				   _pauseEvent.WaitOne(0))
			{
				if (_cancellationTokenSourceStop is not null &&
					_cancellationTokenSourceStop.Token.IsCancellationRequested)
					_cancellationTokenSourceStop.Token.ThrowIfCancellationRequested();

				_pauseEvent.WaitOne();
			}
		}
	}
}
