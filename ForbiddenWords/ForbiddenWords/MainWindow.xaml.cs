using Microsoft.Win32;
using Ookii.Dialogs.Wpf;
using System;
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
		public MainWindow()
		{
			InitializeComponent();
			RadioButton_Checked(null, null);
		}
		#endregion


		#region Additional Methods
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


		#region Events
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
		#endregion


	}
}
