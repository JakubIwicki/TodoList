using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;

namespace TodoList
{
    [Serializable]
    internal class UserSettings
    {
        /// <summary>
        /// File name for settings
        /// </summary>
        private static string fileName => "tlJI.json";

        /// <summary>
        /// Path to program folder
        /// </summary>
        private static string folderPath =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TodoListJI");

        /// <summary>
        /// Full path to settings file
        /// </summary>
        internal static string filePath => Path.Combine(folderPath, fileName);


        private static UserSettings? _instance;
        private static readonly object _lock = new();
        internal UserSettingsData? Data { get; set; }


        [Serializable]
        internal class UserSettingsData
        {
            [JsonPropertyName("LastSelectedFilterIndex")]
            public int LastSelectedFilterIndex { get; set; }

            [JsonPropertyName("LastSelectedOverdueToDate")]
            public DateTime? LastSelectedOverdueToDate { get; set; }

            [JsonPropertyName("DatabaseFilePath")]
            public string DatabaseFilePath { get; set; } = "myTasks.db";
        }


        /// <summary>
        /// Init for singleton -> creates directories if not exists, loads settings if exists
        /// </summary>
        private UserSettings()
        {
            if (!File.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            if (!File.Exists(filePath))
            {
                File.Create(filePath).Close();
                Data = new();
            }
            else
                Load();
        }

        /// <summary>
        /// Main instance of UserSettings
        /// </summary>
        public static UserSettings Instance
        {
            get
            {
                if (_instance != null)
                    return _instance;

                lock (_lock)
                {
                    _instance ??= new UserSettings();
                    return _instance;
                }
            }
        }

        public void Save()
        {
            try
            {
                var json = JsonSerializer.Serialize(Data);

                if (!string.IsNullOrEmpty(json))
                {
                    File.WriteAllText(filePath, json);
                }

            }
            catch
            {
            }
        }

        public void Load()
        {
            if (!File.Exists(filePath))
                return;

            try
            {
                var json = File.ReadAllText(filePath);
                Data = JsonSerializer.Deserialize<UserSettingsData>(json);
            }
            catch
            {
                Data = new();
            }
        }
    }
}
