using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace View
{
    public class TooltipUI : MonoBehaviour
    {
        public TMP_Text titleText;
        public TMP_Text descriptionText;
        public Image iconImage;
        public CanvasGroup group;

        private void Awake()
        {
            if (!group) group = GetComponent<CanvasGroup>();
            Hide();
        }

        public void Show(string title, string description, Sprite icon, Vector2 position)
        {
            if (titleText) titleText.text = title;
            if (descriptionText) descriptionText.text = description;
            if (iconImage)
            {
                iconImage.sprite = icon;
                iconImage.enabled = icon;
            }

            transform.position = position;
            if (group)
            {
                group.alpha = 1;
                group.blocksRaycasts = false;
            }

            gameObject.SetActive(true);
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