using UnityEngine;

namespace Game.Combat
{
    /// <summary>
    /// Ticks damage over time on whatever it's attached to. Applied by a
    /// projectile fired from a turret with the fire-damage upgrade active;
    /// re-applying refreshes the duration and keeps the strongest DPS rather
    /// than stacking multiple burns.
    /// </summary>
    public class Burning : MonoBehaviour
    {
        private const float Duration = 3f;

        private Health _health;
        private float _damagePerSecond;
        private float _remaining;
        private float _accumulatedDamage;

        public void Apply(Health health, float damagePerSecond)
        {
            _health = health;
            _damagePerSecond = Mathf.Max(_damagePerSecond, damagePerSecond);
            _remaining = Duration;
        }

        private void Update()
        {
            if (_health == null || _health.IsDead)
            {
                Destroy(this);
                return;
            }

            _remaining -= Time.deltaTime;
            _accumulatedDamage += _damagePerSecond * Time.deltaTime;

            int whole = Mathf.FloorToInt(_accumulatedDamage);
            if (whole > 0)
            {
                _accumulatedDamage -= whole;
                _health.TakeDamage(whole);
            }

            if (_remaining <= 0f)
            {
                Destroy(this);
            }
        }
    }
}
