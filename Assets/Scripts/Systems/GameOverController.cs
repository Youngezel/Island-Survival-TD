using Game.Buildings;
using Game.Economy;
using Game.Waves;
using UnityEngine;

namespace Game.Systems
{
    /// <summary>
    /// Minimal game-over trigger for the prototype: awards meta XP based on
    /// how far the run got, then freezes the game when the village is
    /// destroyed. Will be replaced by a proper game-over screen in the UI
    /// system (§7.10).
    /// </summary>
    public class GameOverController : MonoBehaviour
    {
        [SerializeField] private int _baseXP = 10;
        [SerializeField] private int _xpPerWave = 5;

        private void OnEnable()
        {
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

            Debug.Log($"GAME OVER - het dorpje is vernietigd. XP toegekend: {xpAwarded} (wave {wavesSurvived} bereikt).");
            Time.timeScale = 0f;
        }
    }
}
