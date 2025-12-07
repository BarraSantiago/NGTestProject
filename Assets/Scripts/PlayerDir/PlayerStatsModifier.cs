using UnityEngine;
using StarterAssets;

namespace PlayerDir
{
    [RequireComponent(typeof(PlayerStats))]
    [RequireComponent(typeof(ThirdPersonController))]
    public class PlayerStatsModifier : MonoBehaviour
    {
        private PlayerStats stats;
        private ThirdPersonController controller;

        private float baseSpeed;
        private float baseSprintSpeed;
        private float baseJumpHeight;
        private float baseGravity;

        private void Awake()
        {
            stats = GetComponent<PlayerStats>();
            controller = GetComponent<ThirdPersonController>();

            // Store base values
            baseSpeed = controller.MoveSpeed;
            baseSprintSpeed = controller.SprintSpeed;
            baseJumpHeight = controller.JumpHeight;
            baseGravity = controller.Gravity;
            
            stats.OnStatsChanged += ApplyStatModifiers;
        }

        private void ApplyStatModifiers()
        {
            // Speed modifier
            float speedMultiplier = stats.currentSpeed / stats.baseSpeed;
            controller.MoveSpeed = baseSpeed * speedMultiplier;
            controller.SprintSpeed = baseSprintSpeed * speedMultiplier;

            // Jump modifier
            float jumpMultiplier = stats.currentJumpForce / stats.baseJumpForce;
            controller.JumpHeight = baseJumpHeight * jumpMultiplier;

            // Gravity modifier
            float gravityMultiplier = stats.currentGravity / stats.baseGravity;
            controller.Gravity = baseGravity * gravityMultiplier;
        }

        public void ResetToBaseValues()
        {
            controller.MoveSpeed = baseSpeed;
            controller.SprintSpeed = baseSprintSpeed;
            controller.JumpHeight = baseJumpHeight;
            controller.Gravity = baseGravity;
        }
    }
}