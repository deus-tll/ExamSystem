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
			string path = $"{PATH_TO_RESULTS}\\{NAME_OF_RESULTS_DIRECTORY}";
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
			PATH_TO_RESULTS = resultFolderPath;
		}


		private Task WorkEstimate(DriveInfo[] drives)
		{
			return Task.Run(() =>
			{
				foreach (DriveInfo drive in drives)
				{
					if (drive.IsReady)
					{
						_totalFileCount += CountTextFiles(drive.RootDirectory.FullName);
					}
				}
			});
		}


		private int CountTextFiles(string path)
		{
			int count = 0;

			try
			{
				string[] files = Directory.GetFiles(path, "*.txt");

				count += files.Length;

				foreach (string directory in Directory.GetDirectories(path))
				{
					count += CountTextFiles(directory);
				}
			}
			catch (UnauthorizedAccessException ex)
			{
				ErrorOccurred?.Invoke(this, ex.Message);
			}

			return count;
		}
	}
}
