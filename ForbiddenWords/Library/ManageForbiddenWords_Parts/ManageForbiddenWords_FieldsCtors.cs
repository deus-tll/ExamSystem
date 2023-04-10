using Library.Models;

namespace Library.ManageForbiddenWords_Parts
{
	public partial class ManageForbiddenWords
	{
		private string PATH_TO_RESULTS;
		private const string NAME_OF_RESULTS_DIRECTORY = "ForbiddenWordsResults";
		private readonly string? PATH_TO_WORDS;
		private readonly List<string>? _forbiddenWords;
		private readonly List<InfectedFile> _infectedFiles;
		private short _totalDiscCount = 0;
		private long _totalFileCount = 0;
		private short _currentDisc = 0;
		private long _currentFile = 0;
		private object _lockObjectDiscs = new();
		private object _lockObjectFiles = new();
		private object _lockObjectInfectedFiles = new();
		private object _lockObjectReportingInfectedFiles = new();
		private readonly CancellationTokenSource _cancellationTokenSourceStop = new();
		private CancellationTokenSource _cancellationTokenSourcePause = new();

		private ManageForbiddenWords(string pathToResults)
		{
			PATH_TO_RESULTS = pathToResults;
			_infectedFiles = new List<InfectedFile>();
			InitializeResultsFolder();
		}

		public ManageForbiddenWords(string pathToResults, string pathToWords) : this(pathToResults)
		{
			PATH_TO_WORDS = pathToWords;
			try
			{
				_forbiddenWords = GetForbiddenWords(PATH_TO_WORDS);
			}
			catch (Exception ex)
			{
				ErrorOccurred?.Invoke(this, ex.Message);
				return;
			}
		}

		public ManageForbiddenWords(string pathToResults, List<string> forbiddenWords) : this(pathToResults)
		{
			_forbiddenWords = forbiddenWords;
		}
	}
}
