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
		public static Settings? GetSettings(string path)
		{
			if (!File.Exists(path)) return null;

			string json = File.ReadAllText(path);

			Settings? settings = JsonConvert.DeserializeObject<Settings>(json);
			if (settings is null) return null;

			return settings;
		}

		public static void SaveSettings(Settings settings, string path)
		{
			JsonSerializer serializer = new();
			using StreamWriter sw = new(path);
			using JsonWriter writer = new JsonTextWriter(sw);
			serializer.Serialize(writer, settings);
		}
	}
}
