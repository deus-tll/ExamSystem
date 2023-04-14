using Library.Models;

namespace Library.ManageForbiddenWords_Parts
{
	public partial class ManageForbiddenWords
	{
		public event EventHandler<string>? ErrorOccurred;
		public event EventHandler? ExploringResumed;
		public event EventHandler? ExploringEnded;

		public event EventHandler<short>? GetTotalCountOfDiscs;
		public event EventHandler<long>? GetTotalCountOfFiles;

		public event EventHandler<ProgressChanged>? ProgressChanged;
		public event EventHandler<bool>? EstimateProgressChanged;
	}
}
