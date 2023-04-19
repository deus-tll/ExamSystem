using System;
namespace Library.Models
{
	public class StatisticsReport
	{
		public List<PressedKey> PressedKeys { get; set; } = new();
		public List<LaunchedProgram> LaunchedPrograms { get; set; } = new();
	}
}
