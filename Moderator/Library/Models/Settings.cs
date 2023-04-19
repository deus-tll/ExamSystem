namespace Library.Models
{
	public class Settings
	{
		public string? PathStatisticsReport { get; set; }
		public string? PathModeratingReport { get; set; }

		public List<string>? SpecificWords { get; set; }
		public List<string>? ForbiddenPrograms { get; set; }

		public bool GatheringStatistic { get; set; } = false;
		public bool PerformModeration { get; set; } = false;
	}
}
