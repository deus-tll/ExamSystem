using Library.Models;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;

namespace Library.ManageForbiddenWords_Parts
{
	public partial class ManageForbiddenWords
	{
		private void ReportInfectedFile(InfectedFile infectedFile)
		{
			lock (_lockObjectReportingInfectedFiles)
			{
				string unique = DateTime.Now.ToString("yyyyMMddHHmmss");
				ReportCopy(infectedFile, unique);
				ReportCopyWithReplaces(infectedFile, unique);
			}
		}


		private void ReportCopy(InfectedFile infectedFile, string unique)
		{
			string destCopy = Path.Combine($"{PATH_TO_CURRENT_RESULTS}\\Copies", $"{infectedFile.NameWithoutExtension}_{unique}.txt");
			File.Copy(infectedFile.Path, destCopy);
		}


		private void ReportCopyWithReplaces(InfectedFile infectedFile, string unique)
		{
			if (_forbiddenWords is null)
				throw new Exception("List of forbidden words is empty");

			string destCopyWithReplace = Path.Combine($"{PATH_TO_CURRENT_RESULTS}\\CopiesWithReplaces", $"{infectedFile.NameWithoutExtension}_{unique}.txt");
			string replaceWord = "*******";

			using StreamReader reader = new(infectedFile.Path);
			using StreamWriter writer = new(destCopyWithReplace);
			while (!reader.EndOfStream)
			{
				string? line = reader.ReadLine();
				if (line == null) continue;

				string[] allWordsFromLine = line.Split(new char[]
				{ ' ', ',', '.', '!', '?', '-', ';', ':', '\r', '\n' },
				StringSplitOptions.RemoveEmptyEntries);

				
				if (allWordsFromLine.Intersect(_forbiddenWords).Any())
				{
					for (int i = 0; i < infectedFile.ForbiddenWords.Count; i++)
						line = Regex.Replace(line, $"\\b{infectedFile.ForbiddenWords[i].Word}\\b", replaceWord);
				}
				writer.WriteLine(line);
				writer.Flush();
			}
		}


		private async void FullReport()
		{
			using StreamWriter writer = new($"{PATH_TO_CURRENT_RESULTS}\\Report.txt");
			await writer.WriteLineAsync("Report on files found:\n");

			if (_infectedFiles.Count <= 0)
			{
				await writer.WriteLineAsync("No files with forbidden words were found.");
				return;
			}

			foreach (InfectedFile file in _infectedFiles)
			{
				await writer.WriteLineAsync($"File name: {file.Name}");
				await writer.WriteLineAsync($"File path: {file.Path}");
				await writer.WriteLineAsync($"File name: {file.Size}");

				await writer.WriteLineAsync("Forbidden words:");
				foreach (ForbiddenWord word in file.ForbiddenWords)
				{
					await writer.WriteLineAsync($"- Word: {word.Word}, Number of uses: {word.NumberOfUses}");
				}

				await writer.WriteLineAsync('\n');
			}

			List<ForbiddenWord> top10ForbiddenWords = _infectedFiles
				.SelectMany(file => file.ForbiddenWords)
				.GroupBy(word => word.Word)
				.Select(group => new ForbiddenWord(group.Key, group.Sum(word => word.NumberOfUses)))
				.OrderByDescending(word => word.NumberOfUses)
				.Take(10)
				.ToList();

			await writer.WriteLineAsync("Top 10 most popular forbidden words:");
			foreach (ForbiddenWord forbiddenWord in top10ForbiddenWords)
			{
				await writer.WriteLineAsync($"- Word: {forbiddenWord.Word}, Number of uses: {forbiddenWord.NumberOfUses}");
			}
		}
	}
}
