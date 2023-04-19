namespace Library.Models
{
	public class ModeratingReport
	{
		public List<TypedWord> TypedWords { get; set; } = new();
		public List<LaunchedProgram> LaunchedForbiddenPrograms { get; set; } = new();
	}
}
