using System;
using System.Collections;
using Game.Enemies;
using UnityEngine;

namespace Game.Waves
{
    /// <summary>
    /// Spawns enemies in escalating waves. Enemy count grows per wave, and
    /// progressively tougher prefabs unlock from _enemyPrefabPool (ordered
    /// weakest to strongest) as waves go on. Both formulas are simple,
    /// tunable placeholders, not hand-authored per-wave data.
    /// </summary>
    public class WaveManager : MonoBehaviour
    {
        public static WaveManager Instance { get; private set; }

        public static event Action<int> OnWaveStarted;
        public static event Action<int> OnWaveCleared;

        [SerializeField] private GameObject[] _enemyPrefabPool;
        [SerializeField] private Transform[] _spawnPoints;
        [SerializeField] private float _timeBetweenSpawns = 0.5f;
        [SerializeField] private float _firstWaveDelay = 2f;

        public int CurrentWave { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            StartCoroutine(BeginFirstWaveAfterDelay());
        }

        private IEnumerator BeginFirstWaveAfterDelay()
        {
            yield return new WaitForSeconds(_firstWaveDelay);
            StartNextWave();
        }

        public void StartNextWave()
        {
            CurrentWave++;
            OnWaveStarted?.Invoke(CurrentWave);
            StartCoroutine(SpawnWave());
        }

        private IEnumerator SpawnWave()
        {
            int wavesSinceStart = CurrentWave - 1;
            int enemyCount = 3 + wavesSinceStart * 2 + (wavesSinceStart * wavesSinceStart) / 3;
            int unlockedTypes = Mathf.Clamp(1 + wavesSinceStart / 2, 1, _enemyPrefabPool.Length);

            // Enemies trickle in one at a time early on; later waves spawn
            // several at once per tick so the pressure keeps escalating even
            // as the per-wave enemy count grows more slowly than the burst size.
            int burstSize = Mathf.Clamp(1 + wavesSinceStart / 4, 1, 5);

            int spawned = 0;
            while (spawned < enemyCount)
            {
                int thisBurst = Mathf.Min(burstSize, enemyCount - spawned);
                for (int i = 0; i < thisBurst; i++)
                {
                    SpawnEnemy(unlockedTypes);
                }

                spawned += thisBurst;
                yield return new WaitForSeconds(_timeBetweenSpawns);
            }

            yield return new WaitUntil(() => Enemy.ActiveEnemies.Count == 0);

            OnWaveCleared?.Invoke(CurrentWave);
        }

        private void SpawnEnemy(int unlockedTypes)
        {
            if (_enemyPrefabPool.Length == 0 || _spawnPoints.Length == 0)
            {
                return;
            }

            GameObject prefab = _enemyPrefabPool[UnityEngine.Random.Range(0, unlockedTypes)];
            Transform spawnPoint = _spawnPoints[UnityEngine.Random.Range(0, _spawnPoints.Length)];
            Instantiate(prefab, spawnPoint.position, Quaternion.identity);
        }
    }
}
