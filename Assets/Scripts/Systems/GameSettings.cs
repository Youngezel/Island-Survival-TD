using UnityEngine;

namespace Game.Systems
{
    /// <summary>
    /// Lightweight player preferences, persisted via PlayerPrefs since
    /// they're simple standalone toggles rather than run/meta progression
    /// (see SaveManager for that). Currently just whether the next wave
    /// starts automatically after picking a reward, or waits for the
    /// player to press the resume button.
    /// </summary>
    public static class GameSettings
    {
        private const string AutoStartNextWaveKey = "AutoStartNextWave";

        public static bool AutoStartNextWave
        {
            get => PlayerPrefs.GetInt(AutoStartNextWaveKey, 0) == 1;
            set
            {
                PlayerPrefs.SetInt(AutoStartNextWaveKey, value ? 1 : 0);
                PlayerPrefs.Save();
            }
        }
    }
}
