using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Dialogue : MonoBehaviour
{
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public Button nextButton;
    public GameObject tintOverlay;
    public float textSpeed = 0.07f;
    public float sideDialogueDuration = 5f;

    [TextArea(2, 4)] public List<string> globalIntroDialoguePool;
    [TextArea(2, 4)] public List<string> globalLossDialoguePool;
    [TextArea(2, 4)] public List<string> globalRetryDialoguePool;

    private List<string> currentLines = new List<string>();
    private int currentLineIndex;
    private Coroutine typingCoroutine;
    private Coroutine autoDismissCoroutine;
    private bool isTyping;
    private bool isTutorialMode;
    private Action onDialogueComplete;

    private void Awake()
    {
        if (nextButton != null)
        {
            nextButton.onClick.AddListener(OnNextButtonClicked);
        }
    }

    public void SetDialogueText(string line) => PlaySideDialogue(line);

    public void PlayTutorialSequence(List<string> lines, Action callback = null)
    {
        if (lines == null || lines.Count == 0)
        {
            callback?.Invoke();
            return;
        }

        isTutorialMode = true;
        if (tintOverlay != null) tintOverlay.SetActive(true);
        if (nextButton != null) nextButton.gameObject.SetActive(true);

        currentLines = lines;
        currentLineIndex = 0;
        onDialogueComplete = callback;

        if (dialoguePanel != null) dialoguePanel.SetActive(true);
        DisplayCurrentLine();
    }

    public void PlaySideDialogue(string line, Action callback = null)
    {
        if (string.IsNullOrEmpty(line))
        {
            callback?.Invoke();
            return;
        }

        isTutorialMode = false;
        if (tintOverlay != null) tintOverlay.SetActive(false);
        if (nextButton != null) nextButton.gameObject.SetActive(false);

        currentLines = new List<string> { line };
        currentLineIndex = 0;
        onDialogueComplete = callback;

        if (dialoguePanel != null) dialoguePanel.SetActive(true);
        DisplayCurrentLine();
    }

    public void ShowRandomIntro(Action callback = null) => ShowRandomFromList(globalIntroDialoguePool, callback);
    public void ShowRandomLoss(Action callback = null) => ShowRandomFromList(globalLossDialoguePool, callback);
    public void ShowRandomRetry(Action callback = null) => ShowRandomFromList(globalRetryDialoguePool, callback);

    private void ShowRandomFromList(List<string> lines, Action callback = null)
    {
        if (lines == null || lines.Count == 0)
        {
            callback?.Invoke();
            return;
        }

        string chosen = lines[UnityEngine.Random.Range(0, lines.Count)];
        PlaySideDialogue(chosen, callback);
    }

    private void DisplayCurrentLine()
    {
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        if (autoDismissCoroutine != null) StopCoroutine(autoDismissCoroutine);

        typingCoroutine = StartCoroutine(TypeLineRoutine(currentLines[currentLineIndex]));
    }

    private IEnumerator TypeLineRoutine(string line)
    {
        isTyping = true;
        if (dialogueText != null) dialogueText.text = "";

        int charCounter = 0;

        foreach (char c in line.ToCharArray())
        {
            if (dialogueText != null) dialogueText.text += c;

            if (charCounter % 4 == 0 && !char.IsWhiteSpace(c))
            {
                SFX.Instance?.PlayDialogue();
            }
            charCounter++;

            yield return new WaitForSecondsRealtime(textSpeed);
        }

        isTyping = false;

        if (!isTutorialMode)
        {
            autoDismissCoroutine = StartCoroutine(AutoDismissRoutine());
        }
    }

    private IEnumerator AutoDismissRoutine()
    {
        yield return new WaitForSecondsRealtime(sideDialogueDuration);
        EndDialogue();
    }

    public void OnNextButtonClicked()
    {
        SFX.Instance?.PlayButtonClick();

        if (!isTutorialMode) return;

        if (isTyping)
        {
            StopCoroutine(typingCoroutine);
            if (dialogueText != null) dialogueText.text = currentLines[currentLineIndex];
            isTyping = false;
            return;
        }

        currentLineIndex++;
        if (currentLineIndex < currentLines.Count)
        {
            DisplayCurrentLine();
        }
        else
        {
            EndDialogue();
        }
    }

    private void EndDialogue()
    {
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (tintOverlay != null) tintOverlay.SetActive(false);
        onDialogueComplete?.Invoke();
    }
}