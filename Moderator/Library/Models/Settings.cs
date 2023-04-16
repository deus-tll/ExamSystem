using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.Models
{
	public class Settings
	{
		public string? PathToReport { get; set; }
		public string? PathToReportOnSpecificTypedWords { get; set; }

		public List<string>? SpecificWords { get; set; }
		public List<string>? ForbiddenPrograms { get; set; }

		public bool GatheringStatistic { get; set; } = false;
		public bool PerformModeration { get; set; } = false;
	}
}
