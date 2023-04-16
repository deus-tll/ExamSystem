using Moderator.AdditionalWindows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Moderator
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private const string COMMAND_SETTINGS = "Settings";
		private const string COMMAND_MODERATING = "Moderating";
		private const string COMMAND_REPORT = "Report";
		private const string PATH_TO_SETTINGS = "settings.json";

		public MainWindow()
		{
			InitializeComponent();
			Initialize();
		}

		#region Additional Methods
		private void Initialize()
		{
			TextBlock_Settings.Text = "This mode opens a dialog box where you will be prompted to select the settings that are convenient for you to use the application.";
			TextBlock_Moderating.Text = "This mode starts moderation and statistical collection of data about user actions, hides the main window from the desktop and from the lower taskbar, and can only be accessed from the Task Manager.";
			TextBlock_Report.Text = "This mode opens a dialog box where you can view the collected moderation statistics.";

			Btn_Settings.Content = COMMAND_SETTINGS;
			Btn_Moderating.Content = COMMAND_MODERATING;
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
				case COMMAND_MODERATING:
					ModeratingCommand();
					break;
				case COMMAND_REPORT:
					ReportCommand();
					break;
			}
		}
		#endregion


		#region Commands
		private void SettingsCommand()
		{
			SettingsWindow settingsWindow = new(PATH_TO_SETTINGS);
			settingsWindow.ShowDialog();
		}


		private void ModeratingCommand()
		{

		}


		private void ReportCommand()
		{
			ReportWindow reportWindow = new();
			reportWindow.ShowDialog();
		}
		#endregion
	}
}
