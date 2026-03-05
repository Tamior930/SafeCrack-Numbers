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
}
