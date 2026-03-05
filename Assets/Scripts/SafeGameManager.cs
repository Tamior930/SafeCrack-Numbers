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

        var divider = MakeImage(_gameScreen, "Divider", new Color(0.25f, 0.60f, 1f, 0.35f));
        var dvRT = divider.GetComponent<RectTransform>();
        dvRT.anchorMin = new Vector2(0.05f, 1f);
        dvRT.anchorMax = new Vector2(0.95f, 1f);
        dvRT.pivot = new Vector2(0.5f, 1f);
        dvRT.anchoredPosition = new Vector2(0, -75);
        dvRT.sizeDelta = new Vector2(0, 2);

        _levelLabel = MakeText(_gameScreen, "LevelLabel", "", 20, FontStyles.Normal, TextDim);
        PinTop(_levelLabel, gsRT, offsetY: -105, height: 32);
        _levelLabel.alignment = TextAlignmentOptions.Center;

        _sequenceText = MakeText(_gameScreen, "SequenceText", "", 32, FontStyles.Bold, TextWhite);
        var seqRT = _sequenceText.GetComponent<RectTransform>();
        seqRT.anchorMin = new Vector2(0.5f, 0.5f);
        seqRT.anchorMax = new Vector2(0.5f, 0.5f);
        seqRT.pivot = new Vector2(0.5f, 0.5f);
        seqRT.anchoredPosition = new Vector2(0, 80);
        seqRT.sizeDelta = new Vector2(680, 60);
        _sequenceText.alignment = TextAlignmentOptions.Center;

        _attemptsText = MakeText(_gameScreen, "AttemptsText", "", 20, FontStyles.Normal, TextDim);
        var attRT = _attemptsText.GetComponent<RectTransform>();
        attRT.anchorMin = new Vector2(0.5f, 0.5f);
        attRT.anchorMax = new Vector2(0.5f, 0.5f);
        attRT.pivot = new Vector2(0.5f, 0.5f);
        attRT.anchoredPosition = new Vector2(0, 20);
        attRT.sizeDelta = new Vector2(680, 34);
        _attemptsText.alignment = TextAlignmentOptions.Center;

        _answerInput = MakeInputField(_gameScreen, new Vector2(0, -55), new Vector2(280, 54));
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

    private static GameObject MakeImage(GameObject parent, string name, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        go.AddComponent<Image>().color = color;
        return go;
    }

    private static TMP_Text MakeText(GameObject parent, string name, string text,
                                     float size, FontStyles style, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text; tmp.fontSize = size; tmp.fontStyle = style;
        tmp.color = color; tmp.raycastTarget = false;
        return tmp;
    }

    private static TMP_InputField MakeInputField(GameObject parent, Vector2 pos, Vector2 size)
    {
        var container = new GameObject("AnswerInput");
        container.transform.SetParent(parent.transform, false);
        var cRT = container.AddComponent<RectTransform>();
        cRT.anchorMin = new Vector2(0.5f, 0.5f); cRT.anchorMax = new Vector2(0.5f, 0.5f);
        cRT.pivot = new Vector2(0.5f, 0.5f); cRT.anchoredPosition = pos; cRT.sizeDelta = size;
        container.AddComponent<Image>().color = new Color(0.06f, 0.08f, 0.14f);
        var ol = container.AddComponent<Outline>();
        ol.effectColor = new Color(0.3f, 0.6f, 1f, 0.6f); ol.effectDistance = new Vector2(1.5f, 1.5f);

        var area = new GameObject("Text Area");
        area.transform.SetParent(container.transform, false);
        var aRT = area.AddComponent<RectTransform>();
        aRT.anchorMin = Vector2.zero; aRT.anchorMax = Vector2.one;
        aRT.offsetMin = new Vector2(10, 6); aRT.offsetMax = new Vector2(-10, -6);
        area.AddComponent<RectMask2D>();

        var ph = new GameObject("Placeholder");
        ph.transform.SetParent(area.transform, false);
        var phRT = ph.AddComponent<RectTransform>();
        phRT.anchorMin = Vector2.zero; phRT.anchorMax = Vector2.one;
        phRT.offsetMin = Vector2.zero; phRT.offsetMax = Vector2.zero;
        var phT = ph.AddComponent<TextMeshProUGUI>();
        phT.text = "Zahl eingeben..."; phT.fontSize = 22;
        phT.color = new Color(0.4f, 0.45f, 0.55f);
        phT.alignment = TextAlignmentOptions.Center;

        var inputText = new GameObject("Text");
        inputText.transform.SetParent(area.transform, false);
        var itRT = inputText.AddComponent<RectTransform>();
        itRT.anchorMin = Vector2.zero; itRT.anchorMax = Vector2.one;
        itRT.offsetMin = Vector2.zero; itRT.offsetMax = Vector2.zero;
        var itT = inputText.AddComponent<TextMeshProUGUI>();
        itT.fontSize = 26; itT.color = new Color(0.95f, 0.95f, 0.98f);
        itT.alignment = TextAlignmentOptions.Center;

        var field = container.AddComponent<TMP_InputField>();
        field.textViewport = aRT; field.textComponent = itT; field.placeholder = phT;
        field.contentType = TMP_InputField.ContentType.IntegerNumber;
        field.characterLimit = 8;
        field.caretColor = new Color(0.25f, 0.65f, 1f);
        field.selectionColor = new Color(0.25f, 0.65f, 1f, 0.4f);
        return field;
    }

    private static void StretchFull(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
    }

    private static void Center(RectTransform rt, Vector2 size)
    {
        rt.anchorMin = new Vector2(0.5f, 0.5f); rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f); rt.anchoredPosition = Vector2.zero; rt.sizeDelta = size;
    }

    private static void PinTop(TMP_Text tmp, RectTransform parentRT, float offsetY, float height)
    {
        var rt = tmp.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 1f); rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 1f); rt.anchoredPosition = new Vector2(0, offsetY);
        rt.sizeDelta = new Vector2(parentRT.sizeDelta.x - 60, height);
    }

    private static void AddOutline(GameObject go, Color color)
    {
        var o = go.AddComponent<Outline>();
        o.effectColor = new Color(color.r, color.g, color.b, 0.8f);
        o.effectDistance = new Vector2(2, 2);
    }
}
