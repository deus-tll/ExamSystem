using System.Diagnostics;
using System.Management;

namespace Library.Models
{
	internal class ProcessWatcher
	{
		private readonly WqlEventQuery WQL_EVENT_QUERY;
		private ManagementEventWatcher? _watcher;
		public event EventHandler<Process>? ProcessStarted;
		public ProcessWatcher()
		{
			WQL_EVENT_QUERY = new("SELECT * FROM Win32_ProcessStartTrace");
		}

		public void StartWatch()
		{
			_watcher = new(WQL_EVENT_QUERY);
			_watcher.EventArrived += Watcher_EventArrived;
			_watcher.Start();
		}

		public void StopWatch()
		{
			_watcher?.Stop();
		}

		private void Watcher_EventArrived(object sender, EventArrivedEventArgs e)
		{
			int processId = (int)e.NewEvent.Properties["ProcessId"].Value;
			Process[] processes = Process.GetProcesses();
			if (processes.Any(p => p.Id == processId))
			{
				Process process = Process.GetProcessById(processId);
				ProcessStarted?.Invoke(null, process);
			}
		}
	}
}
