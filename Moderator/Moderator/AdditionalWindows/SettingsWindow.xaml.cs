using Library.Models;
using Ookii.Dialogs.Wpf;
using System;
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

			_settings = ManageSettings.GetSettings(PATH_TO_SETTINGS);
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
					TextBox_PathToReport.Text = ChoosingPath();
					break;
				case "ReportTypedWords":
					TextBox_PathToReportOnSpecificTypedWords.Text = ChoosingPath();
					break;
			}
		}


		private static string ChoosingPath()
		{
			VistaFolderBrowserDialog dialog = new();

			bool? res = dialog.ShowDialog() ?? throw new Exception("Path was empty or null.");
			if (res is false) return "";

			return dialog.SelectedPath;
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

		private void AddProgram()
		{
			throw new NotImplementedException();
		}

		private void AddWord()
		{
			throw new NotImplementedException();
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

		private void RemoveProgram()
		{
			throw new NotImplementedException();
		}

		private void RemoveWord()
		{
			throw new NotImplementedException();
		}


		private void Btn_SaveSettings_Click(object sender, RoutedEventArgs e)
		{

		}
	}
}
