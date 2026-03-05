using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SafeGameManager : MonoBehaviour
{
    private const int MaxAttempts = 5;
    private const float FeedbackDelay = 1.0f;

    private static readonly string[] SequenceDisplays =
    {
        "2  ->  4  ->  7  ->  11  ->  16  ->  ?",
        "2  ->  6  ->  7  ->  21  ->  22  ->  ?",
        "1  ->  2  ->  3  ->  5  ->  8  ->  ?",
        "1  ->  4  ->  9  ->  16  ->  25  ->  ?"
    };

    private static readonly int[] CorrectAnswers = { 22, 66, 13, 36 };

    private int _currentLevel;
    private int _attemptsLeft;

    [Header("Texte")]
    [SerializeField] private TMP_Text levelLabel;
    [SerializeField] private TMP_Text sequenceText;
    [SerializeField] private TMP_Text attemptsText;
    [SerializeField] private TMP_Text feedbackText;

    [Header("Eingabe")]
    [SerializeField] private TMP_InputField answerInput;
    [SerializeField] private Button submitButton;

    [Header("Screens")]
    [SerializeField] private GameObject gameScreen;
    [SerializeField] private GameObject winScreen;

    private void Start()
    {
        answerInput.onSubmit.AddListener(_ => TrySubmit());
        submitButton.onClick.AddListener(TrySubmit);
        ResetToStart();
    }

    public void ResetToStart()
    {
        _currentLevel = 0;
        _attemptsLeft = MaxAttempts;

        gameScreen.SetActive(true);
        winScreen.SetActive(false);

        RefreshUI();
        SetFeedback("", Color.white);
        FocusInput();
    }

    private void TrySubmit()
    {
        string raw = answerInput.text.Trim();

        if (string.IsNullOrEmpty(raw))
        {
            SetFeedback("Bitte eine Zahl eingeben.", Color.yellow);
            return;
        }

        if (!int.TryParse(raw, out int guess))
        {
            SetFeedback("Nur Zahlen erlaubt.", Color.yellow);
            answerInput.text = string.Empty;
            FocusInput();
            return;
        }

        if (guess == CorrectAnswers[_currentLevel])
            HandleCorrect();
        else
            HandleWrong();
    }

    private void RefreshUI()
    {
        levelLabel.text   = $"Reihe {_currentLevel + 1} / {CorrectAnswers.Length}";
        sequenceText.text = SequenceDisplays[_currentLevel];
        attemptsText.text = $"Versuche: {_attemptsLeft} / {MaxAttempts}";
        answerInput.text  = string.Empty;
    }

    private void FocusInput()
    {
        answerInput.Select();
        answerInput.ActivateInputField();
    }

    private void SetFeedback(string msg, Color color)
    {
        feedbackText.text  = msg;
        feedbackText.color = color;
    }
}
