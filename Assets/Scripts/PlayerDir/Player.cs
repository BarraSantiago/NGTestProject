using UnityEngine;

namespace PlayerDir
{
    [RequireComponent(typeof(PlayerStats))]
    public class Player : MonoBehaviour
    {
        public PlayerStats Stats => stats;
        
        private PlayerStats stats;

        private void Awake()
        {
            stats = GetComponent<PlayerStats>();
        }
    }
}