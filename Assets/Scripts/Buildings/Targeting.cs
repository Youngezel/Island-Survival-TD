using Game.Enemies;
using UnityEngine;

namespace Game.Buildings
{
    /// <summary>
    /// Finds the nearest active enemy within range of this GameObject.
    /// Shared by the village and every turret.
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
    }
}
