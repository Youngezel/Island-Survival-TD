using Game.Buildings;
using UnityEngine;

namespace Game.Systems
{
    /// <summary>
    /// Minimal game-over trigger for the prototype: freezes the game when the
    /// village is destroyed. Will be replaced by a proper game-over screen in
    /// the UI system (§7.10).
    /// </summary>
    public class GameOverController : MonoBehaviour
    {
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
            Debug.Log("GAME OVER - het dorpje is vernietigd.");
            Time.timeScale = 0f;
        }
    }
}
