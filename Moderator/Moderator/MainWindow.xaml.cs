using Library.Models;
using Moderator.AdditionalWindows;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Moderator
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private const string COMMAND_SETTINGS = "Settings";
		private const string COMMAND_MONITORING = "Monitoring";
		private const string COMMAND_REPORT = "Report";

		private const string PATH_TO_SETTINGS = "settings.json";
		private const string STATISTICS_REPORT = "statistics_report.json";
		private const string MODERATING_REPORT = "moderating_report.json";
		private const string PATH_TO_LOG_ERRORS = "log_errors.txt";

		private ManageMonitoring? _manageMonitoring;

		public MainWindow()
		{
			InitializeComponent();
			Initialize();
		}

		#region Additional Methods
		private void Initialize()
		{
			TextBlock_Settings.Text = "This mode opens a dialog box where you will be prompted to select the settings that are convenient for you to use the application.";
			TextBlock_Monitoring.Text = "This mode starts monitoring (moderation and\\or statistical collection of data) user actions, hides the main window from the desktop and from the lower taskbar, and can only be accessed from the Task Manager.";
			TextBlock_Report.Text = "This mode opens a dialog box where you can view all the collected statistics and actions while moderation.";

			Btn_Settings.Content = COMMAND_SETTINGS;
			Btn_Monitoring.Content = COMMAND_MONITORING;
			Btn_Report.Content = COMMAND_REPORT;
		}
		#endregion

		#region Events MainWindow
		private void Command_Click(object sender, RoutedEventArgs e)
		{
			Button button = (Button)sender;
			string? command = button.Content.ToString();
			if (command is null) return;

			switch (command)
			{
				case COMMAND_SETTINGS:
					SettingsCommand();
					break;
				case COMMAND_MONITORING:
					MonitoringCommand();
					break;
				case COMMAND_REPORT:
					ReportCommand();
					break;
			}
		}
		#endregion


		#region Commands
		private static void SettingsCommand()
		{
			SettingsWindow settingsWindow = new(PATH_TO_SETTINGS);
			settingsWindow.ShowDialog();
		}


		private void MonitoringCommand()
		{
			Settings? settings = ManageSettings.GetSettings(PATH_TO_SETTINGS);
			if (settings is null)
			{
				MessageBox.Show("Something went wrong while reading or there is no a settings file.");
				return;
			}

			_manageMonitoring = new ManageMonitoring(PATH_TO_LOG_ERRORS, settings);
			_manageMonitoring.StartMonitoring();
			//Visibility = Visibility.Hidden;
		}


		private static void ReportCommand()
		{
			Settings? settings = ManageSettings.GetSettings(PATH_TO_SETTINGS);
			if (settings is null)
			{
				MessageBox.Show("Something went wrong while reading or there is no such a settings file.");
				return;
			}

			if (settings.PathStatisticsReport is not null)
				settings.PathStatisticsReport += STATISTICS_REPORT;

			if (settings.PathModeratingReport is not null)
				settings.PathModeratingReport += MODERATING_REPORT;

			ReportWindow reportWindow = new(settings);
			reportWindow.ShowDialog();
		}
		#endregion

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			_manageMonitoring?.StopMonitoring();
		}
	}
}
