using Library.Models;

namespace Library.ManageForbiddenWords_Parts
{
	public partial class ManageForbiddenWords
	{
		private string PATH_TO_CURRENT_RESULTS;
		private readonly string MAIN_PATH_TO_ALL_RESULTS;
		private const string NAME_OF_RESULTS_DIRECTORY = "ForbiddenWordsResults";
		private readonly string? PATH_TO_WORDS;
		private readonly List<string>? _forbiddenWords;
		private readonly List<string> _restrictedDirectories;
		private readonly List<InfectedFile> _infectedFiles;
		private readonly char[] _splitChars;
		private short _totalDriveCount = 0;
		private long _totalFileCount = 0;
		private short _currentDrive = 0;
		private long _currentFile = 0;
		private object _lockObjectElements = new();
		private object _lockObjectInfectedFiles = new();
		private object _lockObjectReportingInfectedFiles = new();
		private object _lockObjectPauseEvent = new();
		private CancellationTokenSource? _cancellationTokenSourceStop;
		private readonly ProgressChanged _progressChanged = new();
		private ManualResetEvent? _pauseEvent;
		private readonly bool _withInterface;


		private ManageForbiddenWords(string pathToResults, bool withInterface)
		{
			MAIN_PATH_TO_ALL_RESULTS = pathToResults;
			PATH_TO_CURRENT_RESULTS = pathToResults;
			_infectedFiles = new List<InfectedFile>();
			_withInterface = withInterface;
			_restrictedDirectories = new List<string>()
			{
				"D:\\$RECYCLE.BIN",
				"C:\\$Recycle.Bin",
				"C:\\$SysReset",
				MAIN_PATH_TO_ALL_RESULTS,
				"D:\\System Volume Information",
				"C:\\System Volume Information"
			};

			_splitChars = new char[]
			{ ' ', ',', '.', '!', '?', '-', ';', ':', '\r', '\n' };
			InitializeResultsFolder();
		}


		public ManageForbiddenWords(string pathToResults, string pathToWords, bool withInterface) : this(pathToResults, withInterface)
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


		public ManageForbiddenWords(string pathToResults, List<string> forbiddenWords, bool withInterface) : this(pathToResults, withInterface)
		{
			_forbiddenWords = forbiddenWords;
		}
	}
}
