namespace Library.ManageForbiddenWords_Parts
{
	public partial class ManageForbiddenWords
	{
		public event EventHandler<string>? ErrorOccurred;
		public event EventHandler<string>? ExploringStopped;

		public event EventHandler<short>? GetTotalCountOfDiscs;
		public event EventHandler<long>? GetTotalCountOfFiles;

		public event EventHandler<double>? DiscsProgressChanged;
		public event EventHandler<double>? FilesProgressChanged;
	}
}
