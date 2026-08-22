using System;
using Game.Enemies;
using UnityEngine;

namespace Game.Combat
{
    /// <summary>
    /// Travels toward the point it was fired at and deals damage on arrival.
    /// Fired by turrets at an enemy (homing on it while it's alive, with
    /// optional splash to nearby enemies) or by enemies at a fixed point -
    /// the village, a building, or a hex tile - via a generic onHit callback
    /// so this one class doesn't need to know about every target type.
    /// </summary>
    public class Projectile : MonoBehaviour
    {
        private Enemy _target;
        private Vector3 _lastKnownTargetPosition;
        private float _speed;
        private int _damage;
        private bool _splash;
        private float _splashRadiusWorldUnits;
        private Action<int> _onHit;

        public void Initialize(Enemy target, float speed, int damage, bool splash, float splashRadiusWorldUnits)
        {
            _target = target;
            _lastKnownTargetPosition = target.transform.position;
            _speed = speed;
            _damage = damage;
            _splash = splash;
            _splashRadiusWorldUnits = splashRadiusWorldUnits;
        }

        /// <summary>Fired at a fixed world point (the target doesn't move); onHit applies the damage however that target type needs.</summary>
        public void InitializeAtFixedTarget(Vector3 worldPosition, float speed, int damage, Action<int> onHit)
        {
            _lastKnownTargetPosition = worldPosition;
            _speed = speed;
            _damage = damage;
            _onHit = onHit;
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
            else if (_onHit != null)
            {
                _onHit(_damage);
            }
            else if (_target != null && !_target.IsDead)
            {
                _target.GetComponent<Health>().TakeDamage(_damage);
            }

            Destroy(gameObject);
        }
    }
}
