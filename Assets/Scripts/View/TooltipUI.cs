using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Text;
using InventoryDir.Items;

namespace View
{
    public class TooltipUI : MonoBehaviour
    {
        public Image iconImage;
        public TMP_Text titleText;
        public TMP_Text descriptionText;
        public TMP_Text effectsText;
        public CanvasGroup group;

        public static float ShowDelay = 0.1f;

        private RectTransform rectTransform;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();

            if (!group) group = GetComponent<CanvasGroup>();
            Hide();
        }

        public void Show(string title, string description, Sprite icon, Vector2 position, ItemData itemData = null)
        {
            if (titleText) titleText.text = title;
            if (descriptionText) descriptionText.text = description;
            if (iconImage)
            {
                iconImage.sprite = icon;
                iconImage.enabled = icon;
            }

            if (effectsText)
            {
                effectsText.text = GetEffectsText(itemData);
                effectsText.gameObject.SetActive(!string.IsNullOrEmpty(effectsText.text));
            }

            transform.position = position;
            if (group)
            {
                group.alpha = 1;
                group.blocksRaycasts = false;
            }

            gameObject.SetActive(true);
        }

        private string GetEffectsText(ItemData itemData)
        {
            if (!itemData) return "";

            StringBuilder sb = new();

            if (itemData.isConsumable && itemData.effects != null && itemData.effects.Length > 0)
            {
                sb.AppendLine("<color=yellow>Effects:</color>");
                foreach (ItemEffect effect in itemData.effects)
                {
                    sb.AppendLine(FormatEffect(effect));
                }
            }
            else if (itemData.isEquippable)
            {
                sb.AppendLine("<color=cyan>Equippable Item</color>");
                // Add equipment stats here when you implement them
            }

            return sb.ToString().TrimEnd('\n', '\r');
        }

        private string FormatEffect(ItemEffect effect)
        {
            string effectName = GetEffectName(effect.type);
            string valueText = FormatValue(effect.type, effect.value);
            string durationText = effect.duration > 0 ? $" ({effect.duration}s)" : "";

            return $"• {effectName}: {valueText}{durationText}";
        }

        private string GetEffectName(EffectType type)
        {
            return type switch
            {
                EffectType.HealthRestore => "Health",
                EffectType.HealthBoost => "Max Health Boost",
                EffectType.SpeedBoost => "Speed Boost",
                EffectType.JumpBoost => "Jump Boost",
                EffectType.StaminaRestore => "Stamina",
                EffectType.StaminaBoost => "Max Stamina Boost",
                EffectType.ScaleIncrease => "Size Increase",
                EffectType.ScaleDecrease => "Size Decrease",
                EffectType.GravityReduction => "Gravity Reduction",
                _ => type.ToString()
            };
        }

        private string FormatValue(EffectType type, float value)
        {
            return type switch
            {
                EffectType.ScaleIncrease or EffectType.ScaleDecrease => $"{value * 100:F0}%",
                EffectType.SpeedBoost => $"+{value:F1}",
                EffectType.JumpBoost => $"+{value:F1}",
                EffectType.GravityReduction => $"-{value:F1}",
                _ => $"+{value:F0}"
            };
        }

        public void UpdatePosition(Vector2 position)
        {
            if (!rectTransform) return;

            rectTransform.position = position;
            rectTransform.pivot = new Vector2(0, 1);
        }

        public void Hide()
        {
            if (group)
            {
                group.alpha = 0;
                group.blocksRaycasts = false;
            }

            gameObject.SetActive(false);
        }
    }
}