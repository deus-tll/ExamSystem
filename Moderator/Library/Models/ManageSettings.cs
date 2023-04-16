using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.Models
{
	public static class ManageSettings
	{
		public static Settings GetSettings(string path)
		{
			if (!File.Exists(path)) return new();

			string json = File.ReadAllText(path);

			Settings? settings = JsonConvert.DeserializeObject<Settings>(json);
			if (settings is null) return new();

			return settings;
		}
	}
}
