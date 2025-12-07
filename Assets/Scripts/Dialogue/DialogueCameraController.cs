using System.Collections;
using Interactables;
using PlayerDir;
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
        private Player _player;

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
            _player = FindFirstObjectByType<Player>();
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
            InteractableNPC npc = InteractionDetector.CurrentNPC;
            if (!npc)
            {
                InteractableNPC[] npcs = FindObjectsByType<InteractableNPC>(FindObjectsSortMode.None);
                foreach (InteractableNPC n in npcs)
                {
                    if (n.name.Contains(speakerName) || n.GetComponent<InteractableNPC>())
                    {
                        npc = n;
                        break;
                    }
                }
            }
        
            if (npc)
            {
                FocusOnNPC(npc.cameraLookTarget);
            }
            else
            {
                Debug.LogWarning($"Could not find NPC '{speakerName}' for camera focus!");
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
            if (!mainCamera || !currentNPCTarget) yield break;
        
            // Disable player movement
            DisablePlayerMovement();
        
            // Simply set the main camera to look at the NPC
            mainCamera.LookAt = currentNPCTarget;
        
            yield return null;
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
            if (!mainCamera) yield break;
        
            // Clear the look target
            mainCamera.LookAt = _player.LookAtTarget;
        
            // Re-enable player movement
            EnablePlayerMovement();
        
            yield return null;
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