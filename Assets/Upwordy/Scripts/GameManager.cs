using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using YG;

public enum GameState { 
    PreGame
        ,MainMenu
        ,Gameplay
        ,GameOver
        ,FadeInOut
}

public class GameManager : Soliton<GameManager>
{
    private GameState _gameState;
    public GameState gameState {
        get { return _gameState; }
        set { _gameState = value; }
    }

    public bool IS_MOBILE;
    public bool SIMULATE_MOBILE;

    public int scoreCount;

    [Header("Gameplay Settings")]

    public int picsInBegining = 8;
    public int mapSize = 8;
    public int turnCounts = 8;
    public int picsInRoundCount = 8;

    [Header("Pieces List")]
    [SerializeField] private TextAsset letterListText;
    public Dictionary<char, int> LIST_OF_PIECES;

    [Header("WordList")]
    public HashSet<string> LIST_OF_WORDS;

    [SerializeField] public TextAsset wordListText;
    public int numToParseBeforeYield = 10000;
    public int wordLengthMin = 3;
    public int wordLengthMax = 7;

    public int currLine = 0;
    public int totalLines;
    public int wordCount;

    public string[] lines;

    [Header("Game Settings")]

    [SerializeField] private static float _soundVolume = 1;

    [Header("Ad things")]
    public float nextTimeAdRequest = 180;
    public float updateTimeAdRequestTime = 180;

    [Header("Set Dynamically")]
    public bool isLoading = true;

    [Header("Screen Size")]
    public Vector2Int screenSize;

    [Header("High")]

    public int highScore = 0;

    
    public static float soundVolume {
        get { return _soundVolume; }
        set {
            _soundVolume = Mathf.Clamp01(value);
            OnSoundVolumeChanged(_soundVolume);
        }
    }

    public static event Action<float> OnSoundVolumeChanged;
    public static event Action<Vector2Int> OnScreenSizeChanged;

    public override void Awake()
    {
        gameState = GameState.PreGame;
        isLoading = true;

        base.Awake();
        IS_MOBILE = false
            || SIMULATE_MOBILE;


        LIST_OF_PIECES = new Dictionary<char, int>();
        LIST_OF_WORDS = new HashSet<string>();
        InitLetterParse();
        InitWordsParse();

        screenSize = new Vector2Int(Screen.width, Screen.height);

        OnScreenSizeChanged += Foo;

        gameState = GameState.MainMenu;
    }

    #region SceneManagement

    public static void LoadMainMenuScene()
    {
        SceneManager.LoadScene("MainMenuScene", LoadSceneMode.Single);
    }

    public static void LoadGamePlayScene()
    {
        SceneManager.LoadScene("GameplayScene", LoadSceneMode.Single);
    }

    #endregion SceneManagement

    #region DictionaryOfWords

    public void InitLetterParse() {
        lines = letterListText.text.Split('\n');
        totalLines = lines.Length;

        string[] wordLine = new string[2];
        for (currLine = 0; currLine < totalLines; currLine++) {
            wordLine = lines[currLine].Split('\t');
            char ch = (wordLine[0].ToUpperInvariant())[0];
            int count = int.Parse(wordLine[1]);
//            Debug.Log(ch + " count: " + count);
            LIST_OF_PIECES.Add(ch, count);
        }
    }

    public void InitWordsParse()
    {
        lines = wordListText.text.Split('\n');
        totalLines = lines.Length;

        StartCoroutine(ParseWordsLines());
    }

    public IEnumerator ParseWordsLines() {
        string word;
        for (currLine = 0; currLine < totalLines; currLine++)
        {
            word = lines[currLine];
            if (
                word.Length >= wordLengthMin 
                && word.Length <= wordLengthMax 
                && !LIST_OF_WORDS.Contains(word)
                )
            {
                LIST_OF_WORDS.Add(word);
            }


            if (currLine % numToParseBeforeYield == 0)
            {
                wordCount = LIST_OF_WORDS.Count;
                yield return null;
            }

        }
        isLoading = false;
        MainSceneUIManager.END_LOADING();
        wordCount = LIST_OF_WORDS.Count;
        YandexGame.GameReadyAPI();

        //end parse
    }

    #endregion DictionaryOfWords

    void Update()
    {
        CheckScreenSize();
    }

    void CheckScreenSize() {
        Vector2Int newScreenSize = new Vector2Int(Screen.width, Screen.height);
        if (screenSize != newScreenSize) {
            OnScreenSizeChanged(newScreenSize);
            screenSize = newScreenSize;
        }
    }

    private void Foo(Vector2Int newSize) { 
    }

    #region saves 
    public void CheckHighScore(int newScore) {
        if (newScore > highScore) {
            highScore = newScore;
            Save();
        }
    }

    private void OnEnable() => YandexGame.GetDataEvent += GetLoad;
    private void OnDisable() => YandexGame.GetDataEvent -= GetLoad;

    public void Save()
    {
        YandexGame.savesData.highScore = highScore;
        YandexGame.SaveProgress();
    }

    public void GetLoad() {
        highScore = YandexGame.savesData.highScore;
        MainSceneUIManager.Instance.UpdateHighScore(highScore);
    }

        #endregion saves
}
