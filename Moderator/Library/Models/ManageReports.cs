using Newtonsoft.Json;

namespace Library.Models
{
	public static class ManageReports
	{
		public static StatisticsReport GetStatisticsReport(string path)
		{
			string json = CheckFileAndGetContent(path);
			StatisticsReport? statisticsReport = JsonConvert.DeserializeObject<StatisticsReport>(json);
			if (statisticsReport is null) return new();

			return statisticsReport;
		}

		public static ModeratingReport GetModeratingReport(string path)
		{
			string json = CheckFileAndGetContent(path);
			ModeratingReport? moderatingReport = JsonConvert.DeserializeObject<ModeratingReport>(json);
			if (moderatingReport is null) return new();
			return moderatingReport;
		}

		private static string CheckFileAndGetContent(string path)
		{
			if (!File.Exists(path)) return "";
			return File.ReadAllText(path);
		}

		public static void SaveStatisticsReport(StatisticsReport statisticsReport, string path)
		{
			JsonSerializer serializer = new();
			using StreamWriter sw = new(path);
			using JsonWriter writer = new JsonTextWriter(sw);
			serializer.Serialize(writer, statisticsReport);
		}

		public static void SaveModeratingReport(ModeratingReport moderatingReport, string path)
		{
			JsonSerializer serializer = new();
			using StreamWriter sw = new(path);
			using JsonWriter writer = new JsonTextWriter(sw);
			serializer.Serialize(writer, moderatingReport);
		}
	}
}
