using UnityEngine;

namespace View
{
    public class ButtonAnimation : MonoBehaviour
    {
        private static readonly int Close = Animator.StringToHash("Close");
        private static readonly int Open = Animator.StringToHash("Open");
        [SerializeField] private Animator animator;

        private bool _open = true;

        public void PlayClickAnimation()
        {
            if (!animator) return;

            _open = !_open;
            animator.SetTrigger(_open ? Close : Open);
        }
    }
}