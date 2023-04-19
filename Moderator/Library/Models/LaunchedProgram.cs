namespace Library.Models
{
	public class LaunchedProgram
	{
		public string? Name { get; set; }
		public string? MachineName { get; set; }
		public string? PathToExe { get; set; }
		public nint WindowHandle { get; set; }

		public DateTime StartedDateTime { get; set; }
	}
}
