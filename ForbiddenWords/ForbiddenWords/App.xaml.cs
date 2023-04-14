using Library.ManageForbiddenWords_Parts;
using System.Threading;
using System.Windows;

namespace ForbiddenWords
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		private static Mutex? mutex;

		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);

			mutex = new Mutex(true, "MyMutex_ForbiddenWordsApplication", out bool createdNew);

			if (!createdNew)
			{
				Current.Shutdown();
				return;
			}

			try
			{
				if (e.Args.Length > 0)
				{
					ManageForbiddenWords _manageForbiddenWords = new(e.Args[0], e.Args[1], false);
					_manageForbiddenWords.ErrorOccurred += ManageForbiddenWords_ErrorOccurred; ;

					Thread thread = new(_manageForbiddenWords.StartExploring);
					thread.Start();

					Current.Shutdown();
				}
				else
				{
					MainWindow = new MainWindow();
					MainWindow.Show();
				}
			}
			finally { mutex?.ReleaseMutex(); }
		}

		private void ManageForbiddenWords_ErrorOccurred(object? sender, string error)
		{
			MessageBox.Show(error);
		}
	}
}
