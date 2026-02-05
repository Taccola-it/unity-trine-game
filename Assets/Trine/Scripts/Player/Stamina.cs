using UnityEngine;

namespace Trine.Player
{
    public class Stamina : MonoBehaviour
    {
        [Header("Stamina")]
        public float max = 100f;
        public float regenPerSec = 18f;
        public float regenDelay = 0.25f;

        [Header("Costs")]
        public float sprintPerSec = 15f;
        public float jumpCost = 18f;
        public float rollCost = 25f;

        public float Current { get; private set; }

        private float _regenBlockedUntil;

        private void Awake()
        {
            Current = max;
        }

        public bool Has(float amount) => Current >= amount;

        public bool TrySpend(float amount)
        {
            if (amount <= 0f) return true;
            if (Current < amount) return false;

            Current -= amount;
            _regenBlockedUntil = Time.time + regenDelay;
            return true;
        }

        public void SpendContinuous(float perSec)
        {
            if (perSec <= 0f) return;
            Current = Mathf.Max(0f, Current - perSec * Time.deltaTime);
            _regenBlockedUntil = Time.time + regenDelay;
        }

        private void Update()
        {
            if (Time.time < _regenBlockedUntil) return;
            if (Current >= max) return;

            Current = Mathf.Min(max, Current + regenPerSec * Time.deltaTime);
        }
    }
}
