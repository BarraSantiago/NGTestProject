using UnityEngine;

namespace PlayerDir
{
    [RequireComponent(typeof(PlayerStats))]
    public class Player : MonoBehaviour
    {
        [SerializeField] public Transform LookAtTarget;
        public PlayerStats Stats => stats;
        
        private PlayerStats stats;

        private void Awake()
        {
            stats = GetComponent<PlayerStats>();
        }
    }
}