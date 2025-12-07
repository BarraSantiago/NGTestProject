using System.Collections;
using Interactables;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Dialogue
{
    public class DialogueCameraController : MonoBehaviour
    {
        [Header("Camera References")] 
        [SerializeField] private CinemachineVirtualCamera dialogueCamera;
        [SerializeField] private CinemachineVirtualCamera mainCamera;

        [Header("Camera Settings")] 
        [SerializeField] private float dialogueFOV = 40f;
        [SerializeField] private float normalFOV = 60f;
        [SerializeField] private float transitionSpeed = 2f;
        [SerializeField] private Vector3 cameraOffset = new(0.5f, 0.3f, 2f);

        [Header("Post Processing")] 
        [SerializeField] private Volume postProcessVolume;
        [SerializeField] private float targetVignetteIntensity = 0.4f;
        [SerializeField] private float vignetteTransitionSpeed = 3f;

        [Header("Zoom Effect (Choose One)")]
        [SerializeField] private bool useFOVZoom = true;
        [SerializeField] private bool usePaniniProjection = false;
        [SerializeField] private float paniniDistance = 0.3f;

        private Vignette vignette;
        private PaniniProjection paniniProjection;
        private Transform currentNPCTarget;
        private bool isInDialogue;
        private bool isSubscribed = false;
        private Coroutine transitionCoroutine;

        private void Awake()
        {
            if (postProcessVolume && postProcessVolume.profile)
            {
                postProcessVolume.profile.TryGet(out vignette);
                postProcessVolume.profile.TryGet(out paniniProjection);
            }

            if (!dialogueCamera)
            {
                CreateDialogueCamera();
            }

            if (dialogueCamera)
            {
                dialogueCamera.Priority = 0;
            }
        }

        private void Start()
        {
            SubscribeToEvents();
        }

        private void OnEnable()
        {
            if (DialogueManager.Instance)
            {
                SubscribeToEvents();
            }
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();
        }

        private void SubscribeToEvents()
        {
            if (isSubscribed || !DialogueManager.Instance) return;

            DialogueManager.Instance.OnDialogueStart += OnDialogueStart;
            DialogueManager.Instance.OnDialogueEnd += OnDialogueEnd;

            isSubscribed = true;
        }

        private void UnsubscribeFromEvents()
        {
            if (!isSubscribed || !DialogueManager.Instance) return;

            DialogueManager.Instance.OnDialogueStart -= OnDialogueStart;
            DialogueManager.Instance.OnDialogueEnd -= OnDialogueEnd;

            isSubscribed = false;
        }

        private void OnDialogueStart(string speakerName, string firstLine)
        {
            GameObject npc = GameObject.Find(speakerName);
            if (!npc)
            {
                // Try to find by tag or component
                InteractableNPC[] npcs = FindObjectsByType<InteractableNPC>(FindObjectsSortMode.None);
                foreach (var n in npcs)
                {
                    if (n.name.Contains(speakerName) || n.GetComponent<InteractableNPC>())
                    {
                        npc = n.gameObject;
                        break;
                    }
                }
            }

            if (npc)
            {
                FocusOnNPC(npc.transform);
            }
        }

        private void OnDialogueEnd()
        {
            ReturnToNormalView();
        }

        public void FocusOnNPC(Transform npcTransform)
        {
            if (!npcTransform) return;

            currentNPCTarget = npcTransform;
            isInDialogue = true;

            if (transitionCoroutine != null)
            {
                StopCoroutine(transitionCoroutine);
            }

            transitionCoroutine = StartCoroutine(TransitionToDialogue());
        }

        public void ReturnToNormalView()
        {
            isInDialogue = false;
            currentNPCTarget = null;

            if (transitionCoroutine != null)
            {
                StopCoroutine(transitionCoroutine);
            }

            transitionCoroutine = StartCoroutine(TransitionToNormal());
        }

        private IEnumerator TransitionToDialogue()
        {
            if (!dialogueCamera) yield break;
        
            // Disable player movement
            DisablePlayerMovement();
        
            // Position dialogue camera
            Vector3 targetPosition = currentNPCTarget.position +
                                     currentNPCTarget.right * cameraOffset.x +
                                     currentNPCTarget.up * cameraOffset.y +
                                     currentNPCTarget.forward * cameraOffset.z;
        
            dialogueCamera.transform.position = targetPosition;
            dialogueCamera.LookAt = currentNPCTarget;
            dialogueCamera.Follow = currentNPCTarget;
        
            // Switch to dialogue camera
            dialogueCamera.Priority = 20;
            if (mainCamera) mainCamera.Priority = 10;
        
            float elapsed = 0f;
            float duration = 1f / transitionSpeed;
        
            float startVignette = vignette ? vignette.intensity.value : 0f;
            float startFOV = normalFOV; // Use normalFOV as starting point
            if (dialogueCamera) startFOV = dialogueCamera.m_Lens.FieldOfView;
            float startPanini = paniniProjection ? paniniProjection.distance.value : 0f;
        
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0, 1, elapsed / duration);
        
                // Vignette effect
                if (vignette)
                {
                    vignette.active = true;
                    vignette.intensity.value = Mathf.Lerp(startVignette, targetVignetteIntensity, t);
                }
        
                // FOV zoom
                if (useFOVZoom && dialogueCamera)
                {
                    dialogueCamera.m_Lens.FieldOfView = Mathf.Lerp(startFOV, dialogueFOV, t);
                }
        
                // Panini projection
                if (usePaniniProjection && paniniProjection)
                {
                    paniniProjection.distance.value = Mathf.Lerp(startPanini, paniniDistance, t);
                }
        
                yield return null;
            }
        
            // Ensure final values
            if (vignette) vignette.intensity.value = targetVignetteIntensity;
            if (useFOVZoom && dialogueCamera) dialogueCamera.m_Lens.FieldOfView = dialogueFOV;
            if (usePaniniProjection && paniniProjection)
                paniniProjection.distance.value = paniniDistance;
        }
        
        
        
        private void DisablePlayerMovement()
        {
            var playerInputs = FindFirstObjectByType<StarterAssets.StarterAssetsInputs>();
            if (playerInputs)
            {
                playerInputs.move = Vector2.zero;
                playerInputs.enabled = false;
            }
        }
        
        private void EnablePlayerMovement()
        {
            var playerInputs = FindFirstObjectByType<StarterAssets.StarterAssetsInputs>();
            if (playerInputs)
            {
                playerInputs.enabled = true;
            }
        }

        private IEnumerator TransitionToNormal()
        {
            float elapsed = 0f;
            float duration = 1f / transitionSpeed;
        
            float startVignette = vignette ? vignette.intensity.value : 0f;
            float startFOV = dialogueFOV; // Start from dialogue FOV
            if (dialogueCamera) startFOV = dialogueCamera.m_Lens.FieldOfView;
            float startPanini = paniniProjection ? paniniProjection.distance.value : 0f;
        
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0, 1, elapsed / duration);
        
                // Vignette effect
                if (vignette)
                {
                    vignette.intensity.value = Mathf.Lerp(startVignette, 0f, t);
                }
        
                // FOV zoom
                if (useFOVZoom && dialogueCamera)
                {
                    dialogueCamera.m_Lens.FieldOfView = Mathf.Lerp(startFOV, normalFOV, t);
                }
        
                // Panini projection
                if (usePaniniProjection && paniniProjection)
                {
                    paniniProjection.distance.value = Mathf.Lerp(startPanini, 0f, t);
                }
        
                yield return null;
            }
        
            // Ensure final values are set
            if (vignette)
            {
                vignette.intensity.value = 0f;
                vignette.active = false;
            }
        
            if (useFOVZoom && dialogueCamera)
            {
                dialogueCamera.m_Lens.FieldOfView = normalFOV;
            }
        
            if (usePaniniProjection && paniniProjection)
            {
                paniniProjection.distance.value = 0f;
            }
        
            // Switch back to main camera AFTER effects are done
            if (dialogueCamera) dialogueCamera.Priority = 0;
            if (mainCamera) mainCamera.Priority = 20;
        
            // Clear references
            dialogueCamera.LookAt = null;
            dialogueCamera.Follow = null;
        
            // Re-enable player movement after transition
            EnablePlayerMovement();
        }
        
        private void CreateDialogueCamera()
        {
            GameObject camObj = new GameObject("DialogueCamera");
            camObj.transform.SetParent(transform);
            dialogueCamera = camObj.AddComponent<CinemachineVirtualCamera>();
            dialogueCamera.m_Lens.FieldOfView = normalFOV;
            dialogueCamera.Priority = 0;
        }
    }
}