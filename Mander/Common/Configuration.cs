using System;
using System.IO;
using Newtonsoft.Json;

namespace Manderville.Common {
	/// <summary>
	/// Bot Configuration
	/// </summary>
	public class Configuration {
		[JsonIgnore]
		/// <summary> The location and name of your bot's configuration file </summary>
		public static string FileName { get; private set; } = "config/configuration.json";
		/// <summary> Ids of users who will have owner access to the bot </summary>
		public ulong[] Owners { get; set; }

        public string Prefix { get; set; }
		/// <summary> Your bot's login token. </summary>
		public string Token { get; set; } = "";

		public static void EnsureExists() {
			string file = Path.Combine(AppContext.BaseDirectory, FileName);
			if (!File.Exists(file)) {
				string path = Path.GetDirectoryName(file); // Create config directory if doesn't exist
				if (!Directory.Exists(path))
					Directory.CreateDirectory(path);

				var config = new Configuration(); // Create a new configuration object

				Console.WriteLine("Please enter your token: "); // Read bot token from console
				string token = Console.ReadLine();

				config.Token = token;
				config.SaveJson();

			}
		}

		/// <summary> Save the configuration to the path specified in FileName </summary>
		public void SaveJson() {
			string file = Path.Combine(AppContext.BaseDirectory, FileName);
			File.WriteAllText(file, ToJson());
		}

		/// <summary> Load the configuration from the path specified in FileName  </summary>
		/// <returns>Configuration</returns>
		public static Configuration Load() {
			string file = Path.Combine(AppContext.BaseDirectory, FileName);
			return JsonConvert.DeserializeObject<Configuration>(File.ReadAllText(file));
		}

        

		/// <summary> Convert the configuration to a json string </summary>
		public string ToJson()
			=> JsonConvert.SerializeObject(this, Formatting.Indented);

	}
}
