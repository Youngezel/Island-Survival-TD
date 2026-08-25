using System.Collections.Generic;
using Game.Enemies;
using UnityEngine;

namespace Game.Buildings
{
    /// <summary>
    /// Finds the nearest active enemy (or enemies) within range of this
    /// GameObject. Shared by the village and every turret.
    /// </summary>
    public class Targeting : MonoBehaviour
    {
        public Enemy FindNearestEnemyInRange(float rangeWorldUnits)
        {
            Enemy nearest = null;
            float nearestSqrDistance = float.MaxValue;
            float rangeSqr = rangeWorldUnits * rangeWorldUnits;

            foreach (Enemy enemy in Enemy.ActiveEnemies)
            {
                if (enemy == null || enemy.IsDead)
                {
                    continue;
                }

                float sqrDistance = (enemy.transform.position - transform.position).sqrMagnitude;
                if (sqrDistance <= rangeSqr && sqrDistance < nearestSqrDistance)
                {
                    nearestSqrDistance = sqrDistance;
                    nearest = enemy;
                }
            }

            return nearest;
        }

        /// <summary>Up to <paramref name="count"/> distinct nearest enemies in range, nearest first - for the multi-target upgrade, which fires at several enemies at once instead of one.</summary>
        public List<Enemy> FindNearestEnemiesInRange(float rangeWorldUnits, int count)
        {
            var candidates = new List<(Enemy enemy, float sqrDistance)>();
            float rangeSqr = rangeWorldUnits * rangeWorldUnits;

            foreach (Enemy enemy in Enemy.ActiveEnemies)
            {
                if (enemy == null || enemy.IsDead)
                {
                    continue;
                }

                float sqrDistance = (enemy.transform.position - transform.position).sqrMagnitude;
                if (sqrDistance <= rangeSqr)
                {
                    candidates.Add((enemy, sqrDistance));
                }
            }

            candidates.Sort((a, b) => a.sqrDistance.CompareTo(b.sqrDistance));

            var result = new List<Enemy>(count);
            for (int i = 0; i < candidates.Count && result.Count < count; i++)
            {
                result.Add(candidates[i].enemy);
            }

            return result;
        }
    }
}
