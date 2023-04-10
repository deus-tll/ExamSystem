using Library.Models;

namespace Library.ManageForbiddenWords_Parts
{
	public partial class ManageForbiddenWords
	{
		public Task StartExploringAsync() => Task.Factory.StartNew(StartExploring);

		public void StartExploring()
		{
			Explore();
		}

		private async void Explore()
		{
			DriveInfo[] drives = DriveInfo.GetDrives();
			_totalDiscCount = Convert.ToInt16(drives.Length);

			await WorkEstimate(drives);

			GetTotalCountOfDiscs?.Invoke(this, _totalDiscCount);
			GetTotalCountOfFiles?.Invoke(this, _totalFileCount);

			Parallel.ForEach(drives, drive =>
			{
				ExploreDrive(drive.RootDirectory);
			});

			await FullReport();
		}


		private void ExploreDrive(DirectoryInfo directoryInfo)
		{
			try
			{
				ExploreFiles(directoryInfo);
			}
			catch (OperationCanceledException ex)
			{
				ExploringStopped?.Invoke(this, ex.Message);
				return;
			}
			catch (Exception ex)
			{
				ErrorOccurred?.Invoke(this, ex.Message);
				return;
			}

			lock (_lockObjectDiscs)
			{
				var discsProgress = (double)++_currentDisc / _totalDiscCount * 100;
				DiscsProgressChanged?.Invoke(directoryInfo, discsProgress);
			}
		}


		private Task ExploreFiles(DirectoryInfo directoryInfo)
		{
			return Task.Run(() =>
			{
				foreach (FileInfo file in directoryInfo.GetFiles())
				{
					if (StoppingPausing())
						throw new OperationCanceledException();

					try
					{
						WorkWithFile(file);
					}
					catch (OperationCanceledException ex)
					{
						throw new OperationCanceledException(ex.Message);
					}
					catch (Exception ex)
					{
						throw new Exception(ex.Message);
					}

					lock (_lockObjectFiles)
					{
						var filesProgress = (double)++_currentFile / _totalFileCount * 100;
						FilesProgressChanged?.Invoke(file, filesProgress);
					}
				}
			});
		}


		private void WorkWithFile(FileInfo file)
		{
			if (_forbiddenWords is null)
				throw new Exception("List of forbidden words is empty");

			if (file.Extension.ToLower() == ".txt")
			{
				string text = File.ReadAllText(file.FullName);

				string[] allWordsFromFile = text.Split(new char[]
				{ ' ', ',', '.', '!', '?', '-', ';', ':', '\r', '\n' },
				StringSplitOptions.RemoveEmptyEntries);

				if (!allWordsFromFile.Intersect(_forbiddenWords).Any())
					return;

				try
				{
					WorkWithInfectedFile(file, allWordsFromFile);
				}
				catch (OperationCanceledException ex)
				{
					throw new OperationCanceledException(ex.Message);
				}
				catch (Exception ex)
				{
					throw new Exception(ex.Message);
				}
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
				if (StoppingPausing())
					throw new OperationCanceledException();

				count = allWordsFromFile.Count(w => forbiddenWord == w);

				if (count > 0)
					forbiddenWordsInFile.Add(new ForbiddenWord(forbiddenWord, count));
			}

			InfectedFile infectedFile = new(file.Name, file.FullName, file.Length, forbiddenWordsInFile);

			lock (_lockObjectInfectedFiles)
			{
				_infectedFiles.Add(infectedFile);
			}

			Task.Run(() => ReportInfectedFile(infectedFile));
		}


		public void StopExploring()
		{
			if (_cancellationTokenSourceStop.Token.CanBeCanceled)
				_cancellationTokenSourceStop.Cancel();
			else
				throw new Exception("Can't be stopped.");
		}


		public void PauseExploring()
		{
			if (_cancellationTokenSourcePause.Token.CanBeCanceled &&
				!_cancellationTokenSourcePause.IsCancellationRequested)
				_cancellationTokenSourcePause.Cancel();
			else
				throw new Exception("Can't be paused.");
		}


		public void Resume() => _cancellationTokenSourcePause = new CancellationTokenSource();



		private bool StoppingPausing()
		{
			while (true)
			{
				if (Stopping())
					return true;

				if (_cancellationTokenSourcePause.Token.IsCancellationRequested)
					Thread.Sleep(1000);
				else
					break;
			}

			return false;
		}


		private bool Stopping()
		{
			if (_cancellationTokenSourceStop.Token.IsCancellationRequested)
				return true;

			return false;
		}
	}
}
