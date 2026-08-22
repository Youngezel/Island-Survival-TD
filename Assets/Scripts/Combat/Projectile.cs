using Game.Enemies;
using UnityEngine;

namespace Game.Combat
{
    /// <summary>
    /// Travels toward the enemy it was fired at and deals damage on arrival.
    /// If the target dies mid-flight, it keeps flying toward the target's
    /// last known position instead of snapping or vanishing. Splash damage
    /// hits every living enemy within radius of the impact point.
    /// </summary>
    public class Projectile : MonoBehaviour
    {
        private Enemy _target;
        private Vector3 _lastKnownTargetPosition;
        private float _speed;
        private int _damage;
        private bool _splash;
        private float _splashRadiusWorldUnits;

        public void Initialize(Enemy target, float speed, int damage, bool splash, float splashRadiusWorldUnits)
        {
            _target = target;
            _lastKnownTargetPosition = target.transform.position;
            _speed = speed;
            _damage = damage;
            _splash = splash;
            _splashRadiusWorldUnits = splashRadiusWorldUnits;
        }

        private void Update()
        {
            if (_target != null && !_target.IsDead)
            {
                _lastKnownTargetPosition = _target.transform.position;
            }

            Vector3 toTarget = _lastKnownTargetPosition - transform.position;
            float step = _speed * Time.deltaTime;

            if (toTarget.magnitude <= step)
            {
                transform.position = _lastKnownTargetPosition;
                Hit();
                return;
            }

            transform.position += toTarget.normalized * step;
        }

        private void Hit()
        {
            if (_splash)
            {
                float radiusSqr = _splashRadiusWorldUnits * _splashRadiusWorldUnits;
                foreach (Enemy enemy in Enemy.ActiveEnemies.ToArray())
                {
                    if (enemy == null || enemy.IsDead)
                    {
                        continue;
                    }

                    if ((enemy.transform.position - transform.position).sqrMagnitude <= radiusSqr)
                    {
                        enemy.GetComponent<Health>().TakeDamage(_damage);
                    }
                }
            }
            else if (_target != null && !_target.IsDead)
            {
                _target.GetComponent<Health>().TakeDamage(_damage);
            }

            Destroy(gameObject);
        }
    }
}
