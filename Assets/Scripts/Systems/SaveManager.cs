using System.IO;
using UnityEngine;

namespace Game.Systems
{
    /// <summary>
    /// Loads/saves SaveData as JSON in Application.persistentDataPath.
    /// DefaultExecutionOrder ensures Current is loaded before other
    /// components' Awake (e.g. XPWallet) try to read it.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance { get; private set; }

        public SaveData Current { get; private set; }

        private string SavePath => Path.Combine(Application.persistentDataPath, "save.json");

        private void Awake()
        {
            Instance = this;
            Load();
        }

        public void Load()
        {
            if (File.Exists(SavePath))
            {
                string json = File.ReadAllText(SavePath);
                Current = JsonUtility.FromJson<SaveData>(json);
            }
            else
            {
                Current = new SaveData();
            }
        }

        public void Save()
        {
            string json = JsonUtility.ToJson(Current, true);
            File.WriteAllText(SavePath, json);
        }

        /// <summary>How many tiers (0-3) of the given path are permanently unlocked for this building type.</summary>
        public int GetUnlockedTier(string key, bool pathA)
        {
            switch (key)
            {
                case "turret": return pathA ? Current.turretPathATier : Current.turretPathBTier;
                case "long_range_turret": return pathA ? Current.longRangeTurretPathATier : Current.longRangeTurretPathBTier;
                case "mortar": return pathA ? Current.mortarPathATier : Current.mortarPathBTier;
                default: return 0;
            }
        }

        public void SetUnlockedTier(string key, bool pathA, int tier)
        {
            switch (key)
            {
                case "turret":
                    if (pathA) Current.turretPathATier = tier; else Current.turretPathBTier = tier;
                    break;
                case "long_range_turret":
                    if (pathA) Current.longRangeTurretPathATier = tier; else Current.longRangeTurretPathBTier = tier;
                    break;
                case "mortar":
                    if (pathA) Current.mortarPathATier = tier; else Current.mortarPathBTier = tier;
                    break;
            }
        }
    }
}
