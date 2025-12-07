using System;
using System.Collections;
using System.Collections.Generic;
using InventoryDir.Items;
using UnityEngine;

namespace PlayerDir
{
    public class PlayerStats : MonoBehaviour
    {
        [Header("Base Stats")]
        public float maxHealth = 100f;
        public float currentHealth = 100f;
        public float maxStamina = 100f;
        public float currentStamina = 100f;
        public float baseSpeed = 5f;
        public float baseJumpForce = 5f;
        public float baseGravity = 1f;

        [Header("Current Modifiers")]
        public float currentSpeed;
        public float currentJumpForce;
        public float currentGravity;
        public Vector3 baseScale;

        public Action OnStatsChanged;
        private List<ActiveEffect> activeEffects = new();

        private void Awake()
        {
            currentSpeed = baseSpeed;
            currentJumpForce = baseJumpForce;
            currentGravity = baseGravity;
            baseScale = transform.localScale;
        }

        public void ApplyEffect(ItemEffect effect)
        {
            switch (effect.type)
            {
                case EffectType.HealthRestore:
                    currentHealth = Mathf.Min(currentHealth + effect.value, maxHealth);
                    Debug.Log($"Health restored by {effect.value}. Current: {currentHealth}/{maxHealth}");
                    break;

                case EffectType.HealthBoost:
                    if (effect.duration > 0)
                    {
                        maxHealth += effect.value;
                        currentHealth += effect.value;
                        StartTimedEffect(effect);
                        Debug.Log($"Max health boosted by {effect.value} for {effect.duration}s");
                    }

                    break;

                case EffectType.SpeedBoost:
                    if (effect.duration > 0)
                    {
                        currentSpeed += effect.value;
                        StartTimedEffect(effect);
                        Debug.Log($"Speed boosted by {effect.value} for {effect.duration}s. Current: {currentSpeed}");
                    }

                    break;

                case EffectType.JumpBoost:
                    if (effect.duration > 0)
                    {
                        currentJumpForce += effect.value;
                        StartTimedEffect(effect);
                        Debug.Log(
                            $"Jump boosted by {effect.value} for {effect.duration}s. Current: {currentJumpForce}");
                    }

                    break;

                case EffectType.StaminaRestore:
                    currentStamina = Mathf.Min(currentStamina + effect.value, maxStamina);
                    Debug.Log($"Stamina restored by {effect.value}. Current: {currentStamina}/{maxStamina}");
                    break;

                case EffectType.StaminaBoost:
                    if (effect.duration > 0)
                    {
                        maxStamina += effect.value;
                        currentStamina += effect.value;
                        StartTimedEffect(effect);
                        Debug.Log($"Max stamina boosted by {effect.value} for {effect.duration}s");
                    }

                    break;

                case EffectType.ScaleIncrease:
                    if (effect.duration > 0)
                    {
                        transform.localScale = baseScale * (1 + effect.value);
                        StartTimedEffect(effect);
                        Debug.Log($"Scale increased by {effect.value * 100}% for {effect.duration}s");
                    }

                    break;

                case EffectType.ScaleDecrease:
                    if (effect.duration > 0)
                    {
                        transform.localScale = baseScale * (1 - effect.value);
                        StartTimedEffect(effect);
                        Debug.Log($"Scale decreased by {effect.value * 100}% for {effect.duration}s");
                    }

                    break;

                case EffectType.GravityReduction:
                    if (effect.duration > 0)
                    {
                        currentGravity -= effect.value;
                        StartTimedEffect(effect);
                        Debug.Log(
                            $"Gravity reduced by {effect.value} for {effect.duration}s. Current: {currentGravity}");
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            OnStatsChanged?.Invoke();
        }

        private void StartTimedEffect(ItemEffect effect)
        {
            ActiveEffect active = new()
            {
                effect = effect,
                remainingTime = effect.duration
            };
            activeEffects.Add(active);
            StartCoroutine(TimedEffectCoroutine(active));
        }

        private IEnumerator TimedEffectCoroutine(ActiveEffect active)
        {
            yield return new WaitForSeconds(active.effect.duration);

            RemoveEffect(active.effect);
            activeEffects.Remove(active);
        }

        private void RemoveEffect(ItemEffect effect)
        {
            switch (effect.type)
            {
                case EffectType.HealthBoost:
                    maxHealth -= effect.value;
                    currentHealth = Mathf.Min(currentHealth, maxHealth);
                    Debug.Log($"Health boost expired. Max health: {maxHealth}");
                    break;

                case EffectType.SpeedBoost:
                    currentSpeed -= effect.value;
                    Debug.Log($"Speed boost expired. Current speed: {currentSpeed}");
                    break;

                case EffectType.JumpBoost:
                    currentJumpForce -= effect.value;
                    Debug.Log($"Jump boost expired. Current jump: {currentJumpForce}");
                    break;

                case EffectType.StaminaBoost:
                    maxStamina -= effect.value;
                    currentStamina = Mathf.Min(currentStamina, maxStamina);
                    Debug.Log($"Stamina boost expired. Max stamina: {maxStamina}");
                    break;

                case EffectType.ScaleIncrease:
                case EffectType.ScaleDecrease:
                    transform.localScale = baseScale;
                    Debug.Log("Scale effect expired");
                    break;

                case EffectType.GravityReduction:
                    currentGravity += effect.value;
                    Debug.Log($"Gravity effect expired. Current gravity: {currentGravity}");
                    break;
            }
            
            OnStatsChanged?.Invoke();
        }

        private class ActiveEffect
        {
            public ItemEffect effect;
            public float remainingTime;
        }
    }
}