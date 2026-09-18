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

        /// <summary>
        /// Set by the main menu's Tutorial button right before loading the
        /// game scene, read by TutorialController on arrival. Deliberately
        /// NOT persisted via PlayerPrefs - it's a one-shot signal for the
        /// very next scene load, not a standing preference, and gets reset
        /// to false the moment either a tutorial or a normal run begins.
        /// </summary>
        public static bool IsTutorial { get; set; }
    }
}
