using System.Collections;
using System.Collections.Generic;
using Game.Combat;
using Game.Data;
using Game.Enemies;
using Game.Grid;
using Game.Systems;
using UnityEngine;

namespace Game.Buildings
{
    /// <summary>
    /// Automatically fires at the nearest enemy in range, at a fixed fire
    /// rate, applying whatever effects are active from this building type's
    /// committed run-only upgrade path (see RunUpgradeManager). Shared by
    /// the village and every turret - the village never has an
    /// UpgradeSaveKey, so it just falls back to its plain base stats.
    /// </summary>
    [RequireComponent(typeof(Targeting))]
    public class Shooter : MonoBehaviour
    {
        private struct ActiveEffects
        {
            public int DamageBonus;
            public float RangeBonus;
            public float FireRateBonus;
            public bool SpreadShot;
            public bool SequentialDoubleShot;
            public bool MultiTargetShot;
            public int PierceCount;
            public float FireDamagePerSecond;
            public float SplashRadiusBonus;
        }

        [SerializeField] private Projectile _projectilePrefab;
        [SerializeField] private Transform _headTransform;
        [SerializeField] private SpriteRenderer _headRenderer;
        [SerializeField] private Sprite[] _fireFrames;
        [SerializeField] private float _fireFrameDuration = 0.05f;

        private Targeting _targeting;
        private BuildingData _data;
        private float _cooldown;
        private Sprite _headIdleSprite;
        private Coroutine _fireAnimationCoroutine;

        public BuildingData Data => _data;

        /// <summary>How many tiers (0-3) of this building type's committed run-only path are active.</summary>
        public int RunUpgradeTier => RunUpgradeManager.Instance != null && _data != null ? RunUpgradeManager.Instance.GetTier(_data.UpgradeSaveKey) : 0;

        public bool HasCommittedPath => RunUpgradeManager.Instance != null && _data != null && RunUpgradeManager.Instance.HasCommittedPath(_data.UpgradeSaveKey);

        public bool IsPathACommitted => RunUpgradeManager.Instance != null && _data != null && RunUpgradeManager.Instance.IsPathA(_data.UpgradeSaveKey);

        /// <summary>Current damage per shot, including any active run-only path bonus.</summary>
        public int CurrentDamage => _data.Damage + ComputeActiveEffects().DamageBonus;

        /// <summary>Current range in hex tiles, including any active run-only path bonus.</summary>
        public float CurrentRange => _data.Range + ComputeActiveEffects().RangeBonus;

        /// <summary>Current fire rate in shots/sec, including any active run-only path bonus.</summary>
        public float CurrentFireRate => Mathf.Max(0.05f, _data.FireRate + ComputeActiveEffects().FireRateBonus);

        private void Awake()
        {
            _targeting = GetComponent<Targeting>();
            if (_headRenderer != null)
            {
                _headIdleSprite = _headRenderer.sprite;
            }
        }

        public void Initialize(BuildingData data)
        {
            _data = data;
        }

        private ActiveEffects ComputeActiveEffects()
        {
            var effects = new ActiveEffects();
            if (_data == null || RunUpgradeManager.Instance == null || string.IsNullOrEmpty(_data.UpgradeSaveKey))
            {
                return effects;
            }

            if (!RunUpgradeManager.Instance.HasCommittedPath(_data.UpgradeSaveKey))
            {
                return effects;
            }

            bool isPathA = RunUpgradeManager.Instance.IsPathA(_data.UpgradeSaveKey);
            UpgradePath path = isPathA ? _data.PathA : _data.PathB;
            int tier = RunUpgradeManager.Instance.GetTier(_data.UpgradeSaveKey);

            for (int i = 0; i < tier && path != null && i < path.Nodes.Length; i++)
            {
                UpgradeNode node = path.Nodes[i];
                switch (node.Effect)
                {
                    case UpgradeEffect.Damage: effects.DamageBonus += Mathf.RoundToInt(node.Value); break;
                    case UpgradeEffect.Range: effects.RangeBonus += node.Value; break;
                    case UpgradeEffect.FireRate: effects.FireRateBonus += node.Value; break;
                    case UpgradeEffect.SpreadShot: effects.SpreadShot = true; break;
                    case UpgradeEffect.SequentialDoubleShot: effects.SequentialDoubleShot = true; break;
                    case UpgradeEffect.MultiTargetShot: effects.MultiTargetShot = true; break;
                    case UpgradeEffect.PiercingShot: effects.PierceCount += Mathf.RoundToInt(node.Value); break;
                    case UpgradeEffect.FireDamage: effects.FireDamagePerSecond += node.Value; break;
                    case UpgradeEffect.SplashDamage: effects.SplashRadiusBonus += node.Value; break;
                }
            }

            return effects;
        }

        private void Update()
        {
            if (_data == null || HexGridManager.Instance == null)
            {
                return;
            }

            _cooldown -= Time.deltaTime;

            ActiveEffects effects = ComputeActiveEffects();
            float rangeWorldUnits = (_data.Range + effects.RangeBonus) * HexGridManager.Instance.HexStepWorldDistance;
            float fireRate = Mathf.Max(0.05f, _data.FireRate + effects.FireRateBonus);

            // Found every frame (not just when off cooldown) so the head can
            // keep tracking the nearest enemy continuously between shots.
            Enemy aimTarget = _targeting.FindNearestEnemyInRange(rangeWorldUnits);
            RotateHeadToward(aimTarget);

            if (_cooldown > 0f)
            {
                return;
            }

            if (effects.MultiTargetShot)
            {
                List<Enemy> targets = _targeting.FindNearestEnemiesInRange(rangeWorldUnits, 2);
                if (targets.Count == 0)
                {
                    return;
                }

                foreach (Enemy target in targets)
                {
                    FireAt(target, effects);
                }

                _cooldown = 1f / fireRate;
                return;
            }

            if (aimTarget == null)
            {
                return;
            }

            if (effects.SequentialDoubleShot)
            {
                StartCoroutine(FireSequential(aimTarget, effects, rangeWorldUnits));
            }
            else if (effects.SpreadShot)
            {
                FireSpread(aimTarget, effects);
            }
            else
            {
                FireAt(aimTarget, effects);
            }

            _cooldown = 1f / fireRate;
        }

        /// <summary>Rotates the head sprite to face the target; the head art is drawn pointing "up" as its 0-degree reference.</summary>
        private void RotateHeadToward(Enemy target)
        {
            if (_headTransform == null || target == null)
            {
                return;
            }

            Vector3 direction = target.transform.position - transform.position;
            if (direction.sqrMagnitude < 0.0001f)
            {
                return;
            }

            float angle = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;
            _headTransform.rotation = Quaternion.Euler(0f, 0f, -angle);
        }

        private void PlayFireAnimation()
        {
            if (_headRenderer == null || _fireFrames == null || _fireFrames.Length == 0)
            {
                return;
            }

            if (_fireAnimationCoroutine != null)
            {
                StopCoroutine(_fireAnimationCoroutine);
            }

            _fireAnimationCoroutine = StartCoroutine(FireAnimationRoutine());
        }

        private IEnumerator FireAnimationRoutine()
        {
            foreach (Sprite frame in _fireFrames)
            {
                _headRenderer.sprite = frame;
                yield return new WaitForSeconds(_fireFrameDuration);
            }

            _headRenderer.sprite = _headIdleSprite;
            _fireAnimationCoroutine = null;
        }

        private void FireAt(Enemy target, ActiveEffects effects)
        {
            if (_projectilePrefab == null || target == null)
            {
                return;
            }

            Projectile projectile = Instantiate(_projectilePrefab, transform.position, Quaternion.identity);
            float splashRadiusWorldUnits = (_data.SplashRadius + effects.SplashRadiusBonus) * HexGridManager.Instance.HexStepWorldDistance;
            bool splash = _data.Splash || effects.SplashRadiusBonus > 0f;
            projectile.Initialize(target, _data.ProjectileSpeed, _data.Damage + effects.DamageBonus, splash, splashRadiusWorldUnits, effects.PierceCount, effects.FireDamagePerSecond);
            PlayFireAnimation();
        }

        /// <summary>Fires the real homing shot at the target plus two straight pellets angled off to either side.</summary>
        private void FireSpread(Enemy target, ActiveEffects effects)
        {
            FireAt(target, effects);

            if (_projectilePrefab == null)
            {
                return;
            }

            Vector3 toTarget = target.transform.position - transform.position;
            float distance = toTarget.magnitude;
            Vector3 direction = toTarget.normalized;
            float hitRadius = 0.5f * HexGridManager.Instance.HexStepWorldDistance;

            float[] spreadAngles = { -18f, 18f };
            foreach (float angle in spreadAngles)
            {
                Vector3 spreadDirection = Quaternion.Euler(0f, 0f, angle) * direction;
                Vector3 endPoint = transform.position + spreadDirection * distance;
                Projectile pellet = Instantiate(_projectilePrefab, transform.position, Quaternion.identity);
                pellet.InitializeAtPoint(endPoint, _data.ProjectileSpeed, _data.Damage + effects.DamageBonus, hitRadius);
            }
        }

        /// <summary>Fires at the target, then fires again shortly after at whatever's nearest (usually the same target, unless it died).</summary>
        private IEnumerator FireSequential(Enemy firstTarget, ActiveEffects effects, float rangeWorldUnits)
        {
            FireAt(firstTarget, effects);
            yield return new WaitForSeconds(0.15f);

            Enemy secondTarget = firstTarget != null && !firstTarget.IsDead ? firstTarget : _targeting.FindNearestEnemyInRange(rangeWorldUnits);
            if (secondTarget != null)
            {
                FireAt(secondTarget, effects);
            }
        }
    }
}
