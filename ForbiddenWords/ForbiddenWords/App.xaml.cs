using System.Windows;

namespace ForbiddenWords
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);

			if (e.Args.Length > 0)
			{

				Current.Shutdown();
			}
			else
			{
				MainWindow = new MainWindow();
				MainWindow.Show();
			}
		}
	}
}
