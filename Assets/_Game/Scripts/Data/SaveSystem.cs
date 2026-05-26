using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace IslandHarvest.Game
{
    public static class SaveSystem
    {
        /// <summary>
        /// Schema version for on-disk save structures (NOT the cloud save revision counter).
        /// </summary>
        public const int CurrentSaveVersion = 2;

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
            if (SaveFileExists(WorldSceneIds.ProfileFileName))
                return WorldSceneIds.HomeIsland;

            var jsonFiles = Directory.GetFiles(Application.persistentDataPath, "*.json");
            var latestJson = jsonFiles
                .OrderByDescending(File.GetLastWriteTime)
                .Select(Path.GetFileNameWithoutExtension)
                .FirstOrDefault(name => IsPlayableSceneSave(name));

            if (!string.IsNullOrEmpty(latestJson))
                return latestJson;

            var datFiles = Directory.GetFiles(Application.persistentDataPath, "*.dat");
            return datFiles
                .OrderByDescending(File.GetLastWriteTime)
                .Select(Path.GetFileNameWithoutExtension)
                .FirstOrDefault(name => IsPlayableSceneSave(name));
        }

        public static bool HasPlayerProfile() => SaveFileExists(WorldSceneIds.ProfileFileName);

        public static bool IsSceneInBuildSettings(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName))
                return false;

            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                string path = SceneUtility.GetScenePathByBuildIndex(i);
                if (string.IsNullOrEmpty(path))
                    continue;

                if (Path.GetFileNameWithoutExtension(path) == sceneName)
                    return true;
            }

            return false;
        }

        private static bool IsPlayableSceneSave(string fileName) =>
            !string.IsNullOrEmpty(fileName)
            && SaveFileExists(fileName)
            && IsSceneInBuildSettings(fileName);

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
