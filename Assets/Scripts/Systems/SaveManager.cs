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

        public int GetUpgradeLevel(string key)
        {
            switch (key)
            {
                case "turret": return Current.turretUpgradeLevel;
                case "long_range_turret": return Current.longRangeTurretUpgradeLevel;
                case "mortar": return Current.mortarUpgradeLevel;
                default: return 0;
            }
        }

        public void SetUpgradeLevel(string key, int level)
        {
            switch (key)
            {
                case "turret":
                    Current.turretUpgradeLevel = level;
                    break;
                case "long_range_turret":
                    Current.longRangeTurretUpgradeLevel = level;
                    break;
                case "mortar":
                    Current.mortarUpgradeLevel = level;
                    break;
            }
        }
    }
}
