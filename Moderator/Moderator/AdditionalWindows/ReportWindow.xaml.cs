using Library.Models;
using System.Windows;

namespace Moderator.AdditionalWindows
{
	/// <summary>
	/// Interaction logic for ReportWindow.xaml
	/// </summary>
	public partial class ReportWindow : Window
	{
		private readonly Settings SETTINGS;
		private StatisticsReport? _statisticsReport;
		private ModeratingReport? _moderatingReport;
		public ReportWindow(Settings settings)
		{
			InitializeComponent();
			SETTINGS = settings;
			ReadAllData();
			SetDataGrids();
		}

		private void ReadAllData()
		{
			string? statistics = SETTINGS.PathStatisticsReport;

			if (statistics is not null)
				_statisticsReport = ManageReports.GetStatisticsReport(statistics);

			string? moderating = SETTINGS.PathModeratingReport;

			if (moderating is not null)
				_moderatingReport = ManageReports.GetModeratingReport(moderating);
		}

		private void SetDataGrids()
		{
			DataGrid_PressedKeys.ItemsSource = null;
			DataGrid_PressedKeys.ItemsSource = _statisticsReport?.PressedKeys;

			DataGrid_LaunchedPrograms.ItemsSource = null;
			DataGrid_LaunchedPrograms.ItemsSource = _statisticsReport?.LaunchedPrograms;

			DataGrid_TypedWords.ItemsSource = null;
			DataGrid_TypedWords.ItemsSource = _moderatingReport?.TypedWords;

			DataGrid_LaunchedForbiddenPrograms.ItemsSource = null;
			DataGrid_LaunchedForbiddenPrograms.ItemsSource = _moderatingReport?.LaunchedForbiddenPrograms;
		}

		private void Btn_UpdateData_Click(object sender, RoutedEventArgs e)
		{
			ReadAllData();
			SetDataGrids();
		}
	}
}
