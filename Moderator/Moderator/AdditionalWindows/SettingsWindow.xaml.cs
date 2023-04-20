using Library.Models;
using Ookii.Dialogs.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
		private readonly Settings SETTINGS;

		private List<string>? _specificWords;
		private List<string>? _forbiddenPrograms;

		public SettingsWindow(string pathToSettings)
		{
			InitializeComponent();
			PATH_TO_SETTINGS = pathToSettings;

			if (ManageSettings.GetSettings(PATH_TO_SETTINGS) is Settings settings)
			{
				SETTINGS = settings;
				Initialize();
			}
			else
				SETTINGS = new();
		}

		private void Initialize()
		{
			TextBox_PathStatisticsReport.Text = SETTINGS.PathStatisticsReport;
			TextBox_PathModeratingReport.Text = SETTINGS.PathModeratingReport;

			_specificWords = SETTINGS.SpecificWords;
			_forbiddenPrograms = SETTINGS.ForbiddenPrograms;

			ListBox_Words.ItemsSource = _specificWords;
			ListBox_Programs.ItemsSource = _forbiddenPrograms;

			CheckBox_Moderating.IsChecked = SETTINGS.PerformModeration;
			CheckBox_Statistics.IsChecked = SETTINGS.GatheringStatistic;
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
				case "StatisticsReport":
					TextBox_PathStatisticsReport.Text = ChoosingPathDirectory();
					break;
				case "ModeratingReport":
					TextBox_PathModeratingReport.Text = ChoosingPathDirectory();
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

			if (_specificWords is null || _specificWords.Contains(word)) return;
			_specificWords.Add(word);
			ListBox_Words.Items.Refresh();
		}


		private void AddProgram()
		{
			string program = ChoosingPathExeFile();
			if (string.IsNullOrEmpty(program)) return;

			if (_forbiddenPrograms is null || _forbiddenPrograms.Contains(program)) return;
			_forbiddenPrograms.Add(program);
			ListBox_Programs.Items.Refresh();
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

			var word = (string)ListBox_Words.SelectedItem;
			_specificWords?.Remove(word);

			ListBox_Words.Items.Refresh();
		}


		private void RemoveProgram()
		{
			if (ListBox_Programs.SelectedIndex == -1) return;

			var program = (string)ListBox_Programs.SelectedItem;
			_forbiddenPrograms?.Remove(program);

			ListBox_Programs.Items.Refresh();
		}


		private void Btn_SaveSettings_Click(object sender, RoutedEventArgs e)
		{
			if (!CheckBox_Statistics.IsChecked.HasValue || !CheckBox_Moderating.IsChecked.HasValue || 
				(!CheckBox_Statistics.IsChecked.Value && !CheckBox_Moderating.IsChecked.Value))
			{
				MessageBox.Show("You must select at least one checkbox.");
				return;
			}

			if (!Directory.Exists(TextBox_PathStatisticsReport.Text))
			{
				MessageBox.Show("Choose path to report.");
				return;
			}

			if (!Directory.Exists(TextBox_PathModeratingReport.Text))
			{
				MessageBox.Show("Choose path to report on specific typed words.");
				return;
			}

			SETTINGS.PathStatisticsReport = TextBox_PathStatisticsReport.Text;
			SETTINGS.PathModeratingReport = TextBox_PathModeratingReport.Text;

			if (CheckBox_Statistics.IsChecked == true)
				SETTINGS.GatheringStatistic = true;
			else
				SETTINGS.GatheringStatistic = false;

			if (CheckBox_Moderating.IsChecked == true)
			{
				if (ListBox_Words.Items.Count <= 0)
				{
					MessageBox.Show("You have marked moderation, but the word list is empty. Fill it out first.");
					return;
				}

				SETTINGS.SpecificWords = ListBox_Words.Items.Cast<string>().ToList();

				if (ListBox_Programs.Items.Count <= 0)
				{
					MessageBox.Show("You have marked moderation, but the program list is empty. Fill it out first.");
					return;
				}

				SETTINGS.ForbiddenPrograms = ListBox_Programs.Items.Cast<string>().ToList();

				SETTINGS.PerformModeration = true;
			}
			else
			{
				SETTINGS.PerformModeration = false;
			}

			try
			{
				ManageSettings.SaveSettings(SETTINGS, PATH_TO_SETTINGS);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}

			MessageBox.Show("The settings have been saved.");
		}


		
	}
}
