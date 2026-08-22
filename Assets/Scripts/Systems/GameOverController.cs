using System;
using Game.Buildings;
using Game.Economy;
using Game.Waves;
using UnityEngine;

namespace Game.Systems
{
    /// <summary>
    /// Awards meta XP based on how far the run got, then freezes the game
    /// when the village is destroyed. Fires OnGameOverResolved once the XP
    /// award is final, so UI (GameOverScreenUI) can display the result
    /// without racing the award itself.
    /// </summary>
    public class GameOverController : MonoBehaviour
    {
        public static event Action<int, int> OnGameOverResolved;
        public static bool IsGameOver { get; private set; }

        [SerializeField] private int _baseXP = 10;
        [SerializeField] private int _xpPerWave = 5;

        private void OnEnable()
        {
            IsGameOver = false;
            Village.OnVillageDestroyed += HandleGameOver;
        }

        private void OnDisable()
        {
            Village.OnVillageDestroyed -= HandleGameOver;
        }

        private void HandleGameOver()
        {
            int wavesSurvived = WaveManager.Instance != null ? WaveManager.Instance.CurrentWave : 0;
            int xpAwarded = _baseXP + _xpPerWave * wavesSurvived;
            if (XPWallet.Instance != null)
            {
                XPWallet.Instance.AddXP(xpAwarded);
            }

            IsGameOver = true;
            Time.timeScale = 0f;

            OnGameOverResolved?.Invoke(xpAwarded, wavesSurvived);
        }
    }
}
