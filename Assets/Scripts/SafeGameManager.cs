using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SafeGameManager : MonoBehaviour
{
    private const int MaxAttempts = 3;
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
    private bool _inputLocked;

    // Runtime UI references (built in code)
    private TMP_Text _levelLabel;
    private TMP_Text _sequenceText;
    private TMP_Text _attemptsText;
    private TMP_Text _feedbackText;
    private TMP_InputField _answerInput;
    private Button _submitButton;
    private GameObject _gameScreen;
    private GameObject _winScreen;

    // Color palette
    private static readonly Color BgColor    = new Color(0.07f, 0.09f, 0.14f);
    private static readonly Color PanelColor = new Color(0.11f, 0.14f, 0.22f);
    private static readonly Color AccentBlue = new Color(0.25f, 0.60f, 1.00f);
    private static readonly Color AccentGreen= new Color(0.20f, 0.85f, 0.45f);
    private static readonly Color TextWhite  = new Color(0.95f, 0.95f, 0.98f);
    private static readonly Color TextDim    = new Color(0.65f, 0.70f, 0.80f);

    private void Awake() => BuildUI();
    private void Start()  => ResetToStart();

    private void BuildUI()
    {
        if (FindFirstObjectByType<EventSystem>() == null)
        {
            var esGO = new GameObject("EventSystem");
            esGO.AddComponent<EventSystem>();
            esGO.AddComponent<StandaloneInputModule>();
        }

        var canvasGO = new GameObject("Canvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();

        var bg = MakeImage(canvasGO, "Background", BgColor);
        StretchFull(bg.GetComponent<RectTransform>());

        _gameScreen = MakeImage(canvasGO, "GameScreen", PanelColor);
        var gsRT = _gameScreen.GetComponent<RectTransform>();
        Center(gsRT, new Vector2(740, 560));
        AddOutline(_gameScreen, AccentBlue);

        var title = MakeText(_gameScreen, "Title", "[ SAFE CRACK: NUMBERS ]", 34, FontStyles.Bold, AccentBlue);
        PinTop(title, gsRT, offsetY: -48, height: 50);
        title.alignment = TextAlignmentOptions.Center;
    }

    public void ResetToStart()
    {
        _currentLevel = 0;
        _attemptsLeft = MaxAttempts;
        _inputLocked  = false;

        _gameScreen.SetActive(true);
        _winScreen.SetActive(false);

        RefreshUI();
        ClearFeedback();
        FocusInput();
    }

    private void TrySubmit()
    {
        if (_inputLocked) return;

        string raw = (_answerInput.text ?? "").Trim();

        if (string.IsNullOrEmpty(raw))
        {
            ShowFeedback("Bitte eine Zahl eingeben.", FeedbackKind.Warning);
            return;
        }

        if (!int.TryParse(raw, out int guess) || guess < 0)
        {
            ShowFeedback("Nur positive ganze Zahlen erlaubt.", FeedbackKind.Warning);
            _answerInput.text = string.Empty;
            FocusInput();
            return;
        }

        if (guess == CorrectAnswers[_currentLevel])
            HandleCorrect();
        else
            HandleWrong();
    }

    private void HandleCorrect()
    {
        _inputLocked = true;
        ShowFeedback("Richtig!", FeedbackKind.Success);
        _currentLevel++;

        if (_currentLevel >= CorrectAnswers.Length)
            StartCoroutine(ShowWinScreen());
        else
        {
            _attemptsLeft = MaxAttempts;
            StartCoroutine(NextLevel());
        }
    }

    private void HandleWrong()
    {
        _attemptsLeft--;
        _answerInput.text = string.Empty;

        if (_attemptsLeft <= 0)
        {
            _inputLocked = true;
            ShowFeedback("Alle Versuche aufgebraucht! Safe gesperrt.", FeedbackKind.Error);
            StartCoroutine(HardReset());
        }
        else
        {
            string v = _attemptsLeft == 1 ? "Versuch" : "Versuche";
            ShowFeedback($"Falsch! Noch {_attemptsLeft} {v} uebrig.", FeedbackKind.Error);
            _attemptsText.text = $"Versuche noch: {_attemptsLeft} / {MaxAttempts}";
            FocusInput();
        }
    }

    private IEnumerator NextLevel()
    {
        yield return new WaitForSeconds(FeedbackDelay);
        _inputLocked = false;
        RefreshUI();
        ClearFeedback();
        FocusInput();
    }

    private IEnumerator ShowWinScreen()
    {
        yield return new WaitForSeconds(FeedbackDelay);
        _gameScreen.SetActive(false);
        _winScreen.SetActive(true);
    }

    private IEnumerator HardReset()
    {
        yield return new WaitForSeconds(2.2f);
        ResetToStart();
    }

    private void RefreshUI()
    {
        _levelLabel.text   = $"Reihe {_currentLevel + 1} von {CorrectAnswers.Length}";
        _sequenceText.text = SequenceDisplays[_currentLevel];
        _attemptsText.text = $"Versuche noch: {_attemptsLeft} / {MaxAttempts}";
        _answerInput.text  = string.Empty;
    }

    private void FocusInput()
    {
        _answerInput.Select();
        _answerInput.ActivateInputField();
    }

    private void ClearFeedback() => _feedbackText.text = string.Empty;

    private void ShowFeedback(string msg, FeedbackKind kind)
    {
        _feedbackText.text = msg;
        _feedbackText.color = kind switch
        {
            FeedbackKind.Success => new Color(0.2f, 0.90f, 0.45f),
            FeedbackKind.Warning => new Color(1.0f, 0.80f, 0.10f),
            FeedbackKind.Error   => new Color(0.95f, 0.25f, 0.20f),
            _                    => TextWhite
        };
    }

    private enum FeedbackKind { Success, Warning, Error }
}
