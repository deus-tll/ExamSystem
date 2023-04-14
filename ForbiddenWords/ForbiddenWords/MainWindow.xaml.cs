using Library.ManageForbiddenWords_Parts;
using Library.Models;
using Microsoft.Win32;
using Ookii.Dialogs.Wpf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace ForbiddenWords
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		#region Fields and Constructors
		private ManageForbiddenWords? _manageForbiddenWords;
		private short _totalCountOfDrives;
		private long _totalCountOfFiles;

		public MainWindow()
		{
			InitializeComponent();
			RadioButton_Checked(null, null);
			NonActiveState();
		}
		#endregion


		#region Additional Methods
		private ManageForbiddenWords CreateManageForbiddenWords()
		{
			if (!RadioButton_EnterWords.IsChecked.HasValue)
				throw new Exception("Error while trying to start.");

			if (!Directory.Exists(TextBox_PathToResults.Text))
				throw new Exception("The directory specified in the path to the results does not exist");

			if (RadioButton_EnterWords.IsChecked.Value)
				return new ManageForbiddenWords(TextBox_PathToResults.Text, ListBox_Words.Items.Cast<string>().ToList(), true);
			else
			{
				if (!File.Exists(TextBox_PathToWords.Text))
					throw new Exception("The file by the specified path to the forbidden words does not exist");

				return new ManageForbiddenWords(TextBox_PathToResults.Text, TextBox_PathToWords.Text, true);
			}
		}


		private void RadioButtonChecked()
		{
			if (Grid_EnterWords is null || Grid_LoadWords is null) return;

			bool flag;
			if (RadioButton_EnterWords.IsChecked is not null && (bool)RadioButton_EnterWords.IsChecked)
				flag = true;
			else
				flag = false;

			Grid_EnterWords.IsEnabled = flag;
			Grid_LoadWords.IsEnabled = !flag;
		}


		private void ChoosePathToFileWords()
		{
			OpenFileDialog dialog = new()
			{
				Filter = "Text files (*.txt)|*.txt",
				Title = "Choose a text file",
				CheckFileExists = true,
				CheckPathExists = true
			};

			string path = "";

			if (dialog.ShowDialog() == true)
				path = dialog.FileName;

			if (string.IsNullOrEmpty(path))
				throw new Exception("Path was empty or null.");

			TextBox_PathToWords.Text = path;
		}


		private void ChoosePathToResults()
		{
			VistaFolderBrowserDialog dialog = new();

			bool? res = dialog.ShowDialog() ?? throw new Exception("Path was empty or null.");
			if (res is false) return;

			TextBox_PathToResults.Text = dialog.SelectedPath;
		}
		#endregion


		#region MainWindow Events
		private void RadioButton_Checked(object? sender, RoutedEventArgs? e)
		{
			RadioButtonChecked();
		}


		private void Btn_AddWord_Click(object sender, RoutedEventArgs e)
		{
			string word = TextBox_EnterWord.Text;
			if (string.IsNullOrEmpty(word)) return;

			if(!ListBox_Words.Items.Contains(word))
				ListBox_Words.Items.Add(word);
        }


		private void Btn_RemoveWord_Click(object sender, RoutedEventArgs e)
		{
			if (ListBox_Words.SelectedIndex == -1) return;

			ListBox_Words.Items.Remove(ListBox_Words.SelectedItem);
		}


		private void ButtonChoosePath_Click(object sender, RoutedEventArgs e)
		{
			if (sender is not Button button) return;
			
			string? tag = button.Tag as string;
			if (string.IsNullOrEmpty(tag)) return;

			try
			{
				switch (tag)
				{
					case "Words":
						ChoosePathToFileWords();
						break;
					case "Results":
						ChoosePathToResults();
						break;
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}


		private void Btn_Command_Click(object sender, RoutedEventArgs e)
		{
			if (sender is not Button button) return;

			string? command = button.Content.ToString();
			if (string.IsNullOrEmpty(command)) return;

			switch (command)
			{
				case "Start":
					StartCommand();
					break;
				case "Stop":
					StopCommand();
					break;
				case "Pause":
					PauseCommand();
					break;
				case "Resume":
					ResumeCommand();
					break;
			}
		}


		private void DataGrid_InfectedFiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (DataGrid_InfectedFiles.SelectedItem is not InfectedFile infectedFile) return;

			DataGrid_WordsInFiles.ItemsSource = null;
			DataGrid_WordsInFiles.ItemsSource = infectedFile.ForbiddenWords;
		}
		#endregion



		#region Commands
		private void StartCommand()
		{
			_manageForbiddenWords = null;

			StartedState();

			try
			{
				_manageForbiddenWords = CreateManageForbiddenWords();
				_manageForbiddenWords.ExploringResumed += ManageForbiddenWords_ExploringResumed;
				_manageForbiddenWords.ProgressChanged += ManageForbiddenWords_ProgressChanged;
				_manageForbiddenWords.ErrorOccurred += ManageForbiddenWords_ErrorOccurred;
				_manageForbiddenWords.GetTotalCountOfDiscs += ManageForbiddenWords_GetTotalCountOfDiscs;
				_manageForbiddenWords.GetTotalCountOfFiles += ManageForbiddenWords_GetTotalCountOfFiles;
				_manageForbiddenWords.EstimateProgressChanged += ManageForbiddenWords_EstimateProgressChanged;
				_manageForbiddenWords.ExploringEnded += ManageForbiddenWords_ExploringEnded;

				Thread thread = new(_manageForbiddenWords.StartExploring);
				thread.Start();
			}
			catch (Exception ex)
			{
				NonActiveState();
				MessageBox.Show(ex.Message);
			}
		}


		private void StopCommand()
		{
			_manageForbiddenWords?.StopExploring();
		}


		private void PauseCommand()
		{
			PauseResumeState();
			_manageForbiddenWords?.PauseExploring();
		}


		private void ResumeCommand()
		{
			_manageForbiddenWords?.ResumeExploring();
		}
		#endregion


		#region ManageForbiddenWords Events
		private void ManageForbiddenWords_ExploringEnded(object? sender, EventArgs e)
		{
			Dispatcher.Invoke(() =>
			{
				NonActiveState();

				if (sender is not List<InfectedFile> infectedFiles) return;

				DataGrid_InfectedFiles.ItemsSource = infectedFiles;
			});
		}


		private void ManageForbiddenWords_EstimateProgressChanged(object? sender, bool flag)
		{
			Dispatcher.Invoke(() =>
			{
				ProgressBar_Estimate.IsIndeterminate = flag;
				if (!flag)
				{
					ProgressBar_Estimate.Value = 100;
					TextBlock_Estimate.Text = "Estimate ended.";
				}
				else
				{
					TextBlock_Estimate.Text = "Estimate...";
				}
			});
		}


		private void ManageForbiddenWords_GetTotalCountOfFiles(object? sender, long count)
		{
			Dispatcher.Invoke(() =>
			{
				_totalCountOfFiles = count;
			});
		}


		private void ManageForbiddenWords_GetTotalCountOfDiscs(object? sender, short count)
		{
			Dispatcher.Invoke(() =>
			{
				_totalCountOfDrives = count;
			});
		}


		private void ManageForbiddenWords_ErrorOccurred(object? sender, string error)
		{
			MessageBox.Show(error);
		}


		private void ManageForbiddenWords_ProgressChanged(object? sender, ProgressChanged progressChanged)
		{
			Dispatcher.Invoke(() =>
			{
				string? element = sender?.ToString();
				if (element is null) return;

				switch (element)
				{
					case "drive":
						GetProgressOnDisc(progressChanged);
						break;
					case "file":
						GetProgressOnFile(progressChanged);
						break;
				}
			});
		}


		private void GetProgressOnFile(ProgressChanged progressChanged)
		{
			if (!string.IsNullOrEmpty(progressChanged.FullName))
			{
				ListBox_LogOfWork.SelectedIndex = ListBox_LogOfWork.Items.Add($"Processed File - \"{progressChanged.FullName}\" ");
				ListBox_LogOfWork.ScrollIntoView(ListBox_LogOfWork.SelectedItem);
			}

			TextBlock_StatusCountFiles.Text = $"Progress on Files: {progressChanged.CurrentCountElements}/{_totalCountOfFiles}";
			ProgressBar_Files.Value = progressChanged.Progress;
		}


		private void GetProgressOnDisc(ProgressChanged progressChanged)
		{
			if (!string.IsNullOrEmpty(progressChanged.FullName))
			{
				ListBox_LogOfWork.SelectedIndex = ListBox_LogOfWork.Items.Add($"Processed Drive - \"{progressChanged.FullName}\" ");
				ListBox_LogOfWork.ScrollIntoView(ListBox_LogOfWork.SelectedItem);
			}

			TextBlock_StatusCountDiscs.Text = $"Progress on drives: {progressChanged.CurrentCountElements}/{_totalCountOfDrives}";
			ProgressBar_Discs.Value = progressChanged.Progress;
		}


		private void ManageForbiddenWords_ExploringResumed(object? sender, EventArgs e)
		{
			Dispatcher.Invoke(() =>
			{
				PauseResumeState(false);
			});
		}
		#endregion


		#region State
		private void NonActiveState()
		{
			bool flag = true;

			Btn_AddWord.IsEnabled = flag;
			Btn_RemoveWord.IsEnabled = flag;
			Btn_Start.IsEnabled = flag;
			Btn_Stop.IsEnabled = !flag;
			Btn_Pause.IsEnabled = !flag;
			Btn_Resume.IsEnabled = !flag;

			TextBlock_Estimate.Text = "";
			ProgressBar_Estimate.Value = 0;
			ProgressBar_Estimate.IsIndeterminate = false;
		}


		private void StartedState()
		{
			bool flag = true;

			Btn_AddWord.IsEnabled = !flag;
			Btn_RemoveWord.IsEnabled = !flag;
			Btn_Start.IsEnabled = !flag;
			Btn_Stop.IsEnabled = flag;
			Btn_Pause.IsEnabled = flag;

			TextBlock_StatusCountDiscs.Text = "Progress on Drives:";
			TextBlock_StatusCountFiles.Text = "Progress on Files:";
			ProgressBar_Discs.Value = 0;
			ProgressBar_Files.Value = 0;

			ListBox_LogOfWork.Items.Clear();
		}


		private void PauseResumeState(bool flag = true)
		{
			Btn_Pause.IsEnabled = !flag;
			Btn_Resume.IsEnabled = flag;
		}
		#endregion
	}
}
