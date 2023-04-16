using Library.Models;
using Newtonsoft.Json;
using Ookii.Dialogs.Wpf;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Moderator.AdditionalWindows
{
	/// <summary>
	/// Interaction logic for SettingsWindow.xaml
	/// </summary>
	public partial class SettingsWindow : Window
	{
		private readonly string PATH_TO_SETTINGS;
		private readonly Settings _settings;
		public SettingsWindow(string pathToSettings)
		{
			InitializeComponent();
			PATH_TO_SETTINGS = pathToSettings;

			if (ManageSettings.GetSettings(PATH_TO_SETTINGS) is Settings settings)
			{
				_settings = settings;
				Initialize();
			}
			else
				_settings = new();
		}

		private void Initialize()
		{
			TextBox_PathToReport.Text = _settings.PathToReport;
			TextBox_PathToReportOnSpecificTypedWords.Text = _settings.PathToReportOnSpecificTypedWords;

			ListBox_Words.ItemsSource = _settings.SpecificWords;
			ListBox_Programs.ItemsSource = _settings.ForbiddenPrograms;

			CheckBox_Moderating.IsChecked = _settings.PerformModeration;
			CheckBox_Statistics.IsChecked = _settings.GatheringStatistic;
		}

		private void ButtonChoosePath_Click(object sender, RoutedEventArgs e)
		{
			if (sender is not Button button) return;

			string? tag = button.Tag as string;
			if (string.IsNullOrEmpty(tag)) return;

			try
			{
				DefineChoosing(tag);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
			
		}


		private void DefineChoosing(string tag)
		{
			switch (tag)
			{
				case "MainReport":
					TextBox_PathToReport.Text = ChoosingPathDirectory();
					break;
				case "ReportTypedWords":
					TextBox_PathToReportOnSpecificTypedWords.Text = ChoosingPathDirectory();
					break;
			}
		}


		private static string ChoosingPathDirectory()
		{
			VistaFolderBrowserDialog dialog = new();

			bool? res = dialog.ShowDialog() ?? throw new Exception("Path was empty or null.");
			if (res is false) return "";

			return dialog.SelectedPath;
		}


		private static string ChoosingPathExeFile()
		{
			VistaOpenFileDialog dialog = new()
			{
				Filter = "Execution file (*.exe)|*.exe",
				Title = "Choose an .exe file",
				CheckFileExists = true,
				CheckPathExists = true,
			};

			string path = "";

			if (dialog.ShowDialog() == true)
				path = dialog.FileName;

			return path;
		}


		private void Btn_AddElement_Click(object sender, RoutedEventArgs e)
		{
			if (sender is not Button button) return;

			string? tag = button.Tag as string;
			if (string.IsNullOrEmpty(tag)) return;

			try
			{
				DefineAdding(tag);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}

		private void DefineAdding(string tag)
		{
			switch (tag)
			{
				case "AddWord":
					AddWord();
					break;
				case "AddProgram":
					AddProgram();
					break;
			}
		}


		private void AddWord()
		{
			string word = TextBox_EnterWord.Text;
			if (string.IsNullOrEmpty(word)) return;

			if (!ListBox_Words.Items.Contains(word))
				ListBox_Words.Items.Add(word);
		}


		private void AddProgram()
		{
			string program = ChoosingPathExeFile();
			if (string.IsNullOrEmpty(program)) return;

			if (!ListBox_Programs.Items.Contains(program))
				ListBox_Programs.Items.Add(program);
		}



		private void Btn_RemoveElement_Click(object sender, RoutedEventArgs e)
		{
			if (sender is not Button button) return;

			string? tag = button.Tag as string;
			if (string.IsNullOrEmpty(tag)) return;

			try
			{
				DefineRemoving(tag);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}

		private void DefineRemoving(string tag)
		{
			switch (tag)
			{
				case "RemoveWord":
					RemoveWord();
					break;
				case "RemoveProgram":
					RemoveProgram();
					break;
			}
		}


		private void RemoveWord()
		{
			if (ListBox_Words.SelectedIndex == -1) return;

			ListBox_Words.Items.Remove(ListBox_Words.SelectedItem);
		}


		private void RemoveProgram()
		{
			if (ListBox_Programs.SelectedIndex == -1) return;

			ListBox_Programs.Items.Remove(ListBox_Programs.SelectedItem);
		}


		private void Btn_SaveSettings_Click(object sender, RoutedEventArgs e)
		{
			if (!CheckBox_Statistics.IsChecked.HasValue || !CheckBox_Moderating.IsChecked.HasValue || 
				(!CheckBox_Statistics.IsChecked.Value && !CheckBox_Moderating.IsChecked.Value))
			{
				MessageBox.Show("You must select at least one checkbox.");
				return;
			}

			if (!Directory.Exists(TextBox_PathToReport.Text))
			{
				MessageBox.Show("Choose path to report.");
				return;
			}

			if (!Directory.Exists(TextBox_PathToReportOnSpecificTypedWords.Text))
			{
				MessageBox.Show("Choose path to report on specific typed words.");
				return;
			}

			_settings.PathToReport = TextBox_PathToReport.Text;
			_settings.PathToReportOnSpecificTypedWords = TextBox_PathToReportOnSpecificTypedWords.Text;

			if (CheckBox_Statistics.IsChecked == true)
				_settings.GatheringStatistic = true;
			else
				_settings.GatheringStatistic = false;

			if (CheckBox_Moderating.IsChecked == true)
			{
				if (ListBox_Words.Items.Count <= 0)
				{
					MessageBox.Show("You have marked moderation, but the word list is empty. Fill it out first.");
					return;
				}

				_settings.SpecificWords = ListBox_Words.Items.Cast<string>().ToList();

				if (ListBox_Programs.Items.Count <= 0)
				{
					MessageBox.Show("You have marked moderation, but the program list is empty. Fill it out first.");
					return;
				}

				_settings.ForbiddenPrograms = ListBox_Programs.Items.Cast<string>().ToList();

				_settings.PerformModeration = true;
			}
			else
			{
				_settings.PerformModeration = false;
			}

			try
			{
				ManageSettings.SaveSettings(_settings, PATH_TO_SETTINGS);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}

			MessageBox.Show("The settings have been saved.");
		}


		
	}
}
