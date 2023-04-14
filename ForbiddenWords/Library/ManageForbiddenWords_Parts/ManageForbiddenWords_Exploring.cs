using Library.Models;
using System.IO;

namespace Library.ManageForbiddenWords_Parts
{
	public partial class ManageForbiddenWords
	{
		public async Task StartExploringAsync()
		{
			await Task.Run(StartExploring);
		}


		public void StartExploring()
		{
			Explore();
		}

		private void Explore()
		{
			DriveInfo[]? drives = EstimateAndGetDrives();

			if (drives is null)
			{
				ExploringEnded?.Invoke(null, new EventArgs());
				return;
			}


			if (_withInterface)
			{
				_cancellationTokenSourceStop = new();
				_pauseEvent = new ManualResetEvent(false);
			}

			List<Task> tasks = SetOrNotCancellationAndStart(drives, _cancellationTokenSourceStop);


			try
			{
				Task.WaitAll(tasks.ToArray());
			}
			catch (AggregateException ae)
			{
				foreach (var ex in ae.Flatten().InnerExceptions)
				{
					if (ex is OperationCanceledException)
					{
						//nothing
					}
					else
						ErrorOccurred?.Invoke(null, $"{ex.HelpLink}\n{ex.InnerException}\n{ex.Source}\n{ex.Message}");
				}
			}

			FullReport();

			ExploringEnded?.Invoke(_infectedFiles, new EventArgs());
		}


		private List<Task> SetOrNotCancellationAndStart(DriveInfo[] drives, CancellationTokenSource? cancellationSource)
		{
			List<Task> tasks = new(drives.Length);

			foreach (var drive in drives)
			{
				Task task;
				if (cancellationSource is not null)
					task = new(() => ExploreDrive(drive.RootDirectory), cancellationSource.Token);
				else
					task = new(() => ExploreDrive(drive.RootDirectory));

				tasks.Add(task);
			}

			tasks.ForEach(task => task.Start());

			return tasks;
		}


		private void ExploreDrive(DirectoryInfo directoryInfo)
		{
			try
			{
				ExploreDirectory(directoryInfo);
			}
			catch (OperationCanceledException)
			{
				throw;
			}
			catch (Exception)
			{
				throw;
			}

			lock (_lockObjectElements)
			{
				SetProgress((double)++_currentDrive / _totalDriveCount * 100,
							_currentDrive, 
							directoryInfo.FullName);

				ProgressChanged?.Invoke("drive", _progressChanged);
			}
        }


		private void ExploreDirectory(DirectoryInfo directoryInfo)
		{
			try
			{
				if (_restrictedDirectories.Contains(directoryInfo.FullName))
					return;

				ExploreFiles(directoryInfo);

				foreach (var subdirectory in directoryInfo.GetDirectories())
				{
					lock (_lockObjectPauseEvent)
					{
						CheckForPauseResumeStop();
					}

					ExploreDirectory(subdirectory);

					lock (_lockObjectPauseEvent)
					{
						CheckForPauseResumeStopCycle();
					}
				}
			}
			catch (UnauthorizedAccessException) { }
			catch (OperationCanceledException)
			{
				throw;
			}
			catch (Exception)
			{
				throw;
			}
		}


		private void ExploreFiles(DirectoryInfo directoryInfo)
		{
			foreach (FileInfo file in directoryInfo.GetFiles("*.txt"))
			{
				try
				{
					if (file.FullName == PATH_TO_WORDS)
						continue;

					lock (_lockObjectPauseEvent)
					{
						CheckForPauseResumeStop();
					}

					WorkWithFile(file);

					lock (_lockObjectPauseEvent)
					{
						CheckForPauseResumeStopCycle();
					}
				}
				catch (UnauthorizedAccessException) { }
				catch (OperationCanceledException)
				{
					throw;
				}
				catch (Exception)
				{
					throw;
				}
			}
		}


		private void WorkWithFile(FileInfo file)
		{
			if (_forbiddenWords is null)
				throw new Exception("List of forbidden words is empty");

			string text;

			try
			{
				text = File.ReadAllText(file.FullName);
			}
			catch (Exception)
			{
				text = string.Empty;
			}

			string[] allWordsFromFile = text.Split(_splitChars,
			StringSplitOptions.RemoveEmptyEntries);

			if (!allWordsFromFile.Intersect(_forbiddenWords).Any())
			{
				ShowFilesProgress(file);
				return;
			}

			try
			{
				WorkWithInfectedFile(file, allWordsFromFile);
			}
			catch (UnauthorizedAccessException) { }
			catch (Exception)
			{
				throw;
			}

			ShowFilesProgress(file);
		}


		private void ShowFilesProgress(FileInfo file)
		{
			lock (_lockObjectElements)
			{
				SetProgress((double)++_currentFile / _totalFileCount * 100,
							_currentFile,
							file.FullName);

				ProgressChanged?.Invoke("file", _progressChanged);
			}
		}


		private void WorkWithInfectedFile(FileInfo file, string[] allWordsFromFile)
		{
			if (_forbiddenWords is null)
				throw new Exception("List of forbidden words is empty");

			List<ForbiddenWord> forbiddenWordsInFile = new();

			int count;

			foreach (string forbiddenWord in _forbiddenWords)
			{
				count = allWordsFromFile.Count(w => forbiddenWord == w);

				if (count > 0)
					forbiddenWordsInFile.Add(new ForbiddenWord(forbiddenWord, count));
			}

			InfectedFile infectedFile = new(file.Name, Path.GetFileNameWithoutExtension(file.FullName), file.FullName, file.Length, forbiddenWordsInFile);

			lock (_lockObjectInfectedFiles)
			{
				_infectedFiles.Add(infectedFile);
			}

			ReportInfectedFile(infectedFile);
		}


		public void StopExploring()
		{
			if(_cancellationTokenSourceStop?.Token.CanBeCanceled is not null &&
				_cancellationTokenSourceStop.Token.CanBeCanceled)
				_cancellationTokenSourceStop.Cancel();
			else
				throw new Exception("Can't be stopped.");
		}


		public void PauseExploring()
		{
			_pauseEvent?.Set();
		}


		public void ResumeExploring()
		{
			_pauseEvent?.Reset();

			ExploringResumed?.Invoke(null, new EventArgs());
		}
	}
}
