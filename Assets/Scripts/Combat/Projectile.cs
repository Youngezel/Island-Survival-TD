using System;
using System.Collections.Generic;
using Game.Enemies;
using UnityEngine;

namespace Game.Combat
{
    /// <summary>
    /// Travels toward the point it was fired at and deals damage on arrival.
    /// Fired by turrets at an enemy (homing on it while it's alive, with
    /// optional splash, piercing follow-through, and a fire-damage burn) or
    /// by enemies at a fixed point - the village, a building, or a hex tile -
    /// via a generic onHit callback so this one class doesn't need to know
    /// about every target type. A spread-shot pellet instead flies to a
    /// fixed point off to one side and hits whichever enemy is nearest that
    /// point on arrival, since it isn't homing on anything in particular.
    /// </summary>
    public class Projectile : MonoBehaviour
    {
        // Fixed visual size for a non-splash hit (turret / long-range
        // turret) - the mortar's splash hits instead use the real splash
        // radius so the effect matches which enemies actually took damage.
        private const float SmallImpactRadius = 0.35f;

        [SerializeField] private ImpactEffect _impactEffectPrefab;
        [SerializeField] private FireVfx _fireVfxPrefab;

        private Enemy _target;
        private Vector3 _lastKnownTargetPosition;
        private float _speed;
        private int _damage;
        private bool _splash;
        private float _splashRadiusWorldUnits;
        private int _pierceCount;
        private float _fireDamagePerSecond;
        private float _arrivalHitRadius;
        private Action<int> _onHit;
        private Vector3 _lastMoveDirection = Vector3.right;

        public void Initialize(Enemy target, float speed, int damage, bool splash, float splashRadiusWorldUnits, int pierceCount = 0, float fireDamagePerSecond = 0f)
        {
            _target = target;
            _lastKnownTargetPosition = target.transform.position;
            _speed = speed;
            _damage = damage;
            _splash = splash;
            _splashRadiusWorldUnits = splashRadiusWorldUnits;
            _pierceCount = pierceCount;
            _fireDamagePerSecond = fireDamagePerSecond;
        }

        /// <summary>Fired at a fixed world point (the target doesn't move); onHit applies the damage however that target type needs.</summary>
        public void InitializeAtFixedTarget(Vector3 worldPosition, float speed, int damage, Action<int> onHit)
        {
            _lastKnownTargetPosition = worldPosition;
            _speed = speed;
            _damage = damage;
            _onHit = onHit;
        }

        /// <summary>A spread-shot pellet: flies to a fixed point off to one side of the real target and damages whichever enemy is nearest that point on arrival, if any.</summary>
        public void InitializeAtPoint(Vector3 worldPosition, float speed, int damage, float arrivalHitRadius)
        {
            _lastKnownTargetPosition = worldPosition;
            _speed = speed;
            _damage = damage;
            _arrivalHitRadius = arrivalHitRadius;
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

            _lastMoveDirection = toTarget.normalized;
            transform.position += _lastMoveDirection * step;
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
                        DamageEnemy(enemy);
                    }
                }

                SpawnImpactEffect(_splashRadiusWorldUnits);
            }
            else if (_onHit != null)
            {
                _onHit(_damage);
            }
            else if (_target != null && !_target.IsDead)
            {
                DamageEnemy(_target);
                if (_pierceCount > 0)
                {
                    ApplyPierce(_target);
                }

                SpawnImpactEffect(SmallImpactRadius);
            }
            else if (_arrivalHitRadius > 0f)
            {
                Enemy nearest = FindNearestWithin(transform.position, _arrivalHitRadius, null);
                if (nearest != null)
                {
                    DamageEnemy(nearest);
                    SpawnImpactEffect(SmallImpactRadius);
                }
            }

            Destroy(gameObject);
        }

        /// <summary>Spawns the shared impact-VFX prefab at this projectile's current position, sized to the given world-space radius.</summary>
        private void SpawnImpactEffect(float radius)
        {
            if (_impactEffectPrefab == null)
            {
                return;
            }

            ImpactEffect effect = Instantiate(_impactEffectPrefab, transform.position, Quaternion.identity);
            effect.SetRadius(radius);
        }

        /// <summary>Sweeps forward from the impact point in the shot's travel direction, damaging up to _pierceCount additional enemies it passes near.</summary>
        private void ApplyPierce(Enemy alreadyHit)
        {
            var hitSet = new HashSet<Enemy> { alreadyHit };
            const float stepDistance = 1f;
            const float hitRadius = 0.5f;
            Vector3 point = transform.position;

            for (int i = 0; i < _pierceCount; i++)
            {
                point += _lastMoveDirection * stepDistance;
                Enemy next = FindNearestWithin(point, hitRadius, hitSet);
                if (next == null)
                {
                    break;
                }

                DamageEnemy(next);
                hitSet.Add(next);
            }
        }

        private Enemy FindNearestWithin(Vector3 point, float radius, HashSet<Enemy> exclude)
        {
            Enemy nearest = null;
            float nearestSqr = radius * radius;

            foreach (Enemy enemy in Enemy.ActiveEnemies)
            {
                if (enemy == null || enemy.IsDead || (exclude != null && exclude.Contains(enemy)))
                {
                    continue;
                }

                float sqrDistance = (enemy.transform.position - point).sqrMagnitude;
                if (sqrDistance <= nearestSqr)
                {
                    nearestSqr = sqrDistance;
                    nearest = enemy;
                }
            }

            return nearest;
        }

        private void DamageEnemy(Enemy enemy)
        {
            Health health = enemy.GetComponent<Health>();
            health.TakeDamage(_damage);

            if (_fireDamagePerSecond > 0f && !health.IsDead)
            {
                Burning burning = enemy.GetComponent<Burning>();
                if (burning == null)
                {
                    burning = enemy.gameObject.AddComponent<Burning>();
                }

                burning.Apply(health, _fireDamagePerSecond, _fireVfxPrefab);
            }
        }
    }
}
