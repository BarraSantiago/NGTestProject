using UnityEngine;

namespace Interactables
{
    [RequireComponent(typeof(Rigidbody))]
    public class InteractableMoveableObject : MonoBehaviour, IInteractable
    {
        [Header("Movement Settings")]
        [SerializeField] private string objectName = "Object";
        [SerializeField] private float pushForce = 5f;
        [SerializeField] private float interactionRange = 2f;
        [SerializeField] private bool canPickUp = false;
        [SerializeField] private float holdDistance = 2f;
        [SerializeField] private float holdHeight = 1f;

        private Rigidbody rb;
        private bool isBeingHeld;
        private Transform holder;
        private Vector3 originalGravityScale;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
        }

        public string GetInteractionPrompt()
        {
            if (canPickUp)
            {
                return isBeingHeld ? $"Drop {objectName}" : $"Pick up {objectName}";
            }
            return $"Push {objectName}";
        }

        public void Interact(GameObject interactor)
        {
            if (canPickUp)
            {
                if (isBeingHeld)
                {
                    Drop();
                }
                else
                {
                    PickUp(interactor.transform);
                }
            }
            else
            {
                Push(interactor.transform);
            }
        }

        public bool CanInteract(GameObject interactor)
        {
            float distance = Vector3.Distance(transform.position, interactor.transform.position);
            return distance <= interactionRange;
        }

        public Transform GetTransform()
        {
            return transform;
        }

        private void PickUp(Transform interactor)
        {
            isBeingHeld = true;
            holder = interactor;
            rb.useGravity = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;

            Debug.Log($"Picked up {objectName}");
        }

        private void Drop()
        {
            isBeingHeld = false;
            holder = null;
            rb.useGravity = true;
            rb.isKinematic = false;

            Debug.Log($"Dropped {objectName}");
        }

        private void Push(Transform interactor)
        {
            Vector3 pushDirection = (transform.position - interactor.position).normalized;
            pushDirection.y = 0; // Keep push horizontal
            rb.AddForce(pushDirection * pushForce, ForceMode.Impulse);

            Debug.Log($"Pushed {objectName}");
        }

        private void Update()
        {
            if (isBeingHeld && holder)
            {
                Vector3 targetPosition = holder.position + holder.forward * holdDistance + Vector3.up * holdHeight;
                transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * 10f);
                transform.rotation = Quaternion.Lerp(transform.rotation, holder.rotation, Time.deltaTime * 5f);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, interactionRange);

            if (canPickUp)
            {
                Gizmos.color = Color.cyan;
                Vector3 holdPos = transform.position + transform.forward * holdDistance + Vector3.up * holdHeight;
                Gizmos.DrawWireSphere(holdPos, 0.3f);
                Gizmos.DrawLine(transform.position, holdPos);
            }
        }
    }
}