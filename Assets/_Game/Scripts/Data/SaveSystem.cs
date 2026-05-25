using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace IslandHarvest.Game
{
    public static class SaveSystem
    {
        public const int CurrentSaveVersion = 1;

        public static void SaveData<T>(T data, string fileName)
        {
            if (data is GameData gameData)
                gameData.saveVersion = CurrentSaveVersion;

            string filePath = GetJsonPath(fileName);
            string json = JsonUtility.ToJson(data, prettyPrint: true);

            File.WriteAllText(filePath, json);
        }

        public static T LoadData<T>(string fileName)
        {
            string jsonPath = GetJsonPath(fileName);
            if (File.Exists(jsonPath))
                return LoadJson<T>(jsonPath);

            // Migrate legacy binary saves from template.
            string legacyPath = GetLegacyDatPath(fileName);
            if (File.Exists(legacyPath))
            {
                T migrated = LoadLegacyBinary<T>(legacyPath);
                if (migrated != null)
                {
                    SaveData(migrated, fileName);
                    Debug.Log($"Migrated save '{fileName}' from .dat to .json");
                }

                return migrated;
            }

            return default;
        }

        public static string GetLatestSaveFileName()
        {
            var jsonFiles = Directory.GetFiles(Application.persistentDataPath, "*.json");
            var latestJson = jsonFiles
                .OrderByDescending(File.GetLastWriteTime)
                .Select(Path.GetFileNameWithoutExtension)
                .FirstOrDefault();

            if (!string.IsNullOrEmpty(latestJson))
                return latestJson;

            var datFiles = Directory.GetFiles(Application.persistentDataPath, "*.dat");
            return datFiles
                .OrderByDescending(File.GetLastWriteTime)
                .Select(Path.GetFileNameWithoutExtension)
                .FirstOrDefault();
        }

        public static bool SaveFileExists(string fileName)
        {
            return File.Exists(GetJsonPath(fileName)) || File.Exists(GetLegacyDatPath(fileName));
        }

        private static string GetJsonPath(string fileName) =>
            Path.Combine(Application.persistentDataPath, fileName + ".json");

        private static string GetLegacyDatPath(string fileName) =>
            Path.Combine(Application.persistentDataPath, fileName + ".dat");

        private static T LoadJson<T>(string filePath)
        {
            try
            {
                string json = File.ReadAllText(filePath);
                return JsonUtility.FromJson<T>(json);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to load JSON save: {filePath}\n{ex}");
                return default;
            }
        }

        private static T LoadLegacyBinary<T>(string filePath)
        {
            try
            {
#pragma warning disable SYSLIB0011
                var formatter = new BinaryFormatter();
#pragma warning restore SYSLIB0011
                using var stream = new FileStream(filePath, FileMode.Open);
                return (T)formatter.Deserialize(stream);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to load legacy save: {filePath}\n{ex}");
                return default;
            }
        }
    }
}
