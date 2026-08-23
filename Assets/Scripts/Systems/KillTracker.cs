using UnityEngine;

namespace Game.Systems
{
    /// <summary>
    /// Counts enemies defeated during the current run, shown on the game
    /// over screen. Resets naturally with the scene, like CoinWallet.
    /// </summary>
    public class KillTracker : MonoBehaviour
    {
        public static KillTracker Instance { get; private set; }

        public int Kills { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        public void RegisterKill()
        {
            Kills++;
        }
    }
}
