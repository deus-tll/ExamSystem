using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.Models
{
	internal static class LogWriter
	{
		public static void WriteLog(string path, string log)
		{
			using StreamWriter writer = new(path, true);
			writer.WriteLine(log);
		}
	}
}
