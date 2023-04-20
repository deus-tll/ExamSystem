using System.Diagnostics;
using System.Management;

namespace Library.Models
{
	internal class ProcessWatcher
	{
		private readonly string PATH_TO_LOG_ERRORS;
		private readonly WqlEventQuery WQL_EVENT_QUERY;
		private ManagementEventWatcher? _watcher;
		public event EventHandler<Process>? ProcessStarted;
		public ProcessWatcher(string pathToLog)
		{
			PATH_TO_LOG_ERRORS = pathToLog;
			WQL_EVENT_QUERY = new("SELECT * FROM Win32_ProcessStartTrace");
		}

		public void StartWatch()
		{
			_watcher?.Stop();

			_watcher = new(WQL_EVENT_QUERY);
			_watcher.EventArrived += new EventArrivedEventHandler(Watcher_EventArrived);
			_watcher.Start();
		}

		public void StopWatch() => _watcher?.Stop();

		private void Watcher_EventArrived(object sender, EventArrivedEventArgs e)
		{
			try
			{
				int processId = Convert.ToInt32(e.NewEvent.Properties["ProcessId"].Value);
				Process process = Process.GetProcessById(processId);
				ProcessStarted?.Invoke(null, process);
			}
			catch (Exception ex)
			{
				LogWriter.WriteLog(PATH_TO_LOG_ERRORS, $"{ex.Message} | {DateTime.Now}");
			}
		}
	}
}
