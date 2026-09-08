using System;
using System.Collections.Generic;
using Game.Enemies;
using UnityEngine;

namespace Game.Combat
{
    /// <summary>
    /// Travels toward the point it was fired at and deals damage on arrival.
    /// Fired by turrets at an enemy (homing on it while it's alive, with
    /// optional splash, piercing follow-through, chain-lightning ricochet,
    /// and a fire-damage burn) or by enemies at a fixed point - the village,
    /// a building, or a hex tile - via a generic onHit callback so this one
    /// class doesn't need to know about every target type. A spread-shot
    /// pellet instead flies to a fixed point off to one side and hits
    /// whichever enemy is nearest that point on arrival, since it isn't
    /// homing on anything in particular.
    /// </summary>
    public class Projectile : MonoBehaviour
    {
        // Fixed visual size for a non-splash hit (turret / long-range
        // turret) - the mortar's splash hits instead use the real splash
        // radius so the effect matches which enemies actually took damage.
        private const float SmallImpactRadius = 0.35f;

        [SerializeField] private ImpactEffect _impactEffectPrefab;
        [SerializeField] private FireVfx _fireVfxPrefab;
        [SerializeField] private SpriteRenderer _spriteRenderer;

        /// <summary>
        /// Set only on the Tesla Coil's projectile. When present, the "shot"
        /// is drawn as an instant lightning bolt from where it fired to
        /// where it hit (and one more per chain-lightning hop) instead of a
        /// small sprite flying there - the travel sprite still exists for
        /// consistency with the other turrets' Projectile, but resolves so
        /// fast (see Shooter's very high ProjectileSpeed for this building)
        /// that the lightning segments are what actually reads as the shot.
        /// </summary>
        [SerializeField] private LightningBolt _lightningBoltPrefab;

        // How far a chain-lightning bolt can jump to reach the next enemy -
        // deliberately generous (bigger than the pierce sweep's hit radius)
        // since a chain jump isn't constrained to the shot's travel direction.
        private const float ChainJumpRadius = 3f;

        private Enemy _target;
        private Vector3 _lastKnownTargetPosition;
        private float _speed;
        private int _damage;
        private bool _splash;
        private float _splashRadiusWorldUnits;
        private int _pierceCount;
        private float _fireDamagePerSecond;
        private float _arrivalHitRadius;
        private int _chainCount;
        private Action<int> _onHit;
        private Vector3 _lastMoveDirection = Vector3.right;
        private Vector3 _originPosition;

        public void Initialize(Enemy target, float speed, int damage, bool splash, float splashRadiusWorldUnits, int pierceCount = 0, float fireDamagePerSecond = 0f, int chainCount = 0)
        {
            _originPosition = transform.position;
            _target = target;
            _lastKnownTargetPosition = target.transform.position;
            _speed = speed;
            _damage = damage;
            _splash = splash;
            _splashRadiusWorldUnits = splashRadiusWorldUnits;
            _pierceCount = pierceCount;
            _fireDamagePerSecond = fireDamagePerSecond;
            _chainCount = chainCount;

            // The lightning bolt segments are the shot's whole visual - no
            // separate travel sprite flying alongside them.
            if (_lightningBoltPrefab != null && _spriteRenderer != null)
            {
                _spriteRenderer.enabled = false;
            }
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

        /// <summary>Overrides the default sprite - used by an upgraded turret to show that tier's projectile art instead of the base one. No-op if either is missing.</summary>
        public void SetSprite(Sprite sprite)
        {
            if (sprite != null && _spriteRenderer != null)
            {
                _spriteRenderer.sprite = sprite;
            }
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
                SpawnLightningBolt(_originPosition, transform.position);
                DamageEnemy(_target);
                if (_pierceCount > 0)
                {
                    ApplyPierce(_target);
                }

                if (_chainCount > 0)
                {
                    ApplyChain(_target);
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

        /// <summary>Ricochet/chain lightning: jumps to the nearest not-yet-hit enemy within ChainJumpRadius, up to _chainCount times, damaging each one it reaches - unlike ApplyPierce this isn't constrained to the shot's travel direction, so it can double back toward a cluster of enemies.</summary>
        private void ApplyChain(Enemy alreadyHit)
        {
            var hitSet = new HashSet<Enemy> { alreadyHit };
            Vector3 point = alreadyHit.transform.position;

            for (int i = 0; i < _chainCount; i++)
            {
                Enemy next = FindNearestWithin(point, ChainJumpRadius, hitSet);
                if (next == null)
                {
                    break;
                }

                SpawnLightningBolt(point, next.transform.position);
                DamageEnemy(next);
                hitSet.Add(next);
                point = next.transform.position;
            }
        }

        /// <summary>No-op unless this projectile has a lightning bolt prefab wired (only the Tesla Coil's does).</summary>
        private void SpawnLightningBolt(Vector3 from, Vector3 to)
        {
            if (_lightningBoltPrefab == null)
            {
                return;
            }

            LightningBolt bolt = Instantiate(_lightningBoltPrefab, Vector3.zero, Quaternion.identity);
            bolt.Play(from, to);
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
