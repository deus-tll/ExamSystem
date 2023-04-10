using Library.Models;
using System.Text.RegularExpressions;

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
			string destCopy = Path.Combine($"{PATH_TO_RESULTS}\\Copies", $"{infectedFile.Name}_{unique}");
			File.Copy(infectedFile.Path, destCopy);
		}


		private void ReportCopyWithReplaces(InfectedFile infectedFile, string unique)
		{
			string destCopyWithReplace = Path.Combine($"{PATH_TO_RESULTS}\\CopiesWithReplaces", $"{infectedFile.Name}_{unique}");
			string replaceWord = "*******";

			using StreamReader reader = new(infectedFile.Path);
			using StreamWriter writer = new(destCopyWithReplace);
			while (!reader.EndOfStream)
			{
				string? line = reader.ReadLine();
				if (line == null) continue;

				for (int i = 0; i < infectedFile.ForbiddenWords.Count; i++)
				{
					string newLine = Regex.Replace(line, "\\b" + infectedFile.ForbiddenWords[i] + "\\b", replaceWord);
					writer.WriteLine(newLine);
				}
			}
		}


		private async Task FullReport()
		{
			using StreamWriter writer = new(PATH_TO_RESULTS);
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
