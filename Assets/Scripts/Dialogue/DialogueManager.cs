using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dialogue
{
    public class DialogueManager : MonoBehaviour
    {
        public static DialogueManager Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private float textSpeed = 0.05f;
        [SerializeField] private bool autoAdvance = false;
        [SerializeField] private float autoAdvanceDelay = 2f;

        private Queue<string> dialogueQueue = new();
        private string currentSpeakerName;
        private string currentLine; // Store the line being typed
        private Coroutine typewriterCoroutine;
        private bool isTyping;
        private bool dialogueActive;

        public event Action<string, string> OnDialogueStart;
        public event Action<string> OnDialogueLineChanged;
        public event Action OnDialogueEnd;
        public event Action OnTypingComplete;

        public bool IsActive => dialogueActive;
        public bool IsTyping => isTyping;

        private void Awake()
        {
            if (Instance && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        public void StartDialogue(string speakerName, string[] lines)
        {
            if (lines == null || lines.Length == 0)
            {
                Debug.LogWarning("No dialogue lines provided!");
                return;
            }

            StopAllCoroutines();
            dialogueQueue.Clear();

            currentSpeakerName = speakerName;
            dialogueActive = true;

            foreach (string line in lines)
            {
                if (!string.IsNullOrEmpty(line))
                {
                    dialogueQueue.Enqueue(line);
                }
            }

            OnDialogueStart?.Invoke(currentSpeakerName, lines[0]);
            DisplayNextLine();
        }

        public void StartDialogue(string speakerName, string singleLine)
        {
            StartDialogue(speakerName, new[] { singleLine });
        }

        public void DisplayNextLine()
        {
            if (!dialogueActive) return;

            if (isTyping)
            {
                CompleteTyping();
                return;
            }

            if (dialogueQueue.Count == 0)
            {
                EndDialogue();
                return;
            }

            currentLine = dialogueQueue.Dequeue(); // Store current line
            typewriterCoroutine = StartCoroutine(TypeLine(currentLine));
        }

        private IEnumerator TypeLine(string line)
        {
            isTyping = true;
            string currentText = "";
            OnDialogueLineChanged?.Invoke(currentText);

            foreach (char c in line)
            {
                currentText += c;
                OnDialogueLineChanged?.Invoke(currentText);
                yield return new WaitForSeconds(textSpeed);
            }

            isTyping = false;
            OnTypingComplete?.Invoke();

            if (autoAdvance && dialogueQueue.Count > 0)
            {
                yield return new WaitForSeconds(autoAdvanceDelay);
                DisplayNextLine();
            }
        }

        private void CompleteTyping()
        {
            if (typewriterCoroutine != null)
            {
                StopCoroutine(typewriterCoroutine);
                typewriterCoroutine = null;
            }

            // Show the full current line that was being typed
            if (!string.IsNullOrEmpty(currentLine))
            {
                OnDialogueLineChanged?.Invoke(currentLine);
            }

            isTyping = false;
            OnTypingComplete?.Invoke();
        }

        public void EndDialogue()
        {
            if (!dialogueActive) return;

            StopAllCoroutines();
            dialogueQueue.Clear();
            dialogueActive = false;
            isTyping = false;
            currentSpeakerName = null;
            currentLine = null;

            OnDialogueEnd?.Invoke();
        }

        public void SetTextSpeed(float speed)
        {
            textSpeed = Mathf.Clamp(speed, 0.01f, 0.2f);
        }

        public void ToggleAutoAdvance(bool enabled)
        {
            autoAdvance = enabled;
        }
    }
}