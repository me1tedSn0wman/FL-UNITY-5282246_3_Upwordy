using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class MainSceneUIManager : Singleton<MainSceneUIManager>
{
    [Header("Buttons")]

    public Button button_Settings;
    public Button button_Info;
    public Button button_StartGame;

    public Button button_CloseSettingsCanvas;
    public Button button_CloseInfoCanvas;

    [Header("Canvases")]
    public GameObject canvasGO_Settings;
    public GameObject canvasGO_Info;

    [Header("Sliders")]

    public Slider slider_SoundVolume;
    public Slider slider_MapSize;
    public Slider slider_TurnsCount;
    public Slider slider_PicsPerRoundCount;

    [Header("Textes")]
    public TextMeshProUGUI text_SoundVolume;
    public TextMeshProUGUI text_MapSize;
    public TextMeshProUGUI text_TurnsCount;
    public TextMeshProUGUI text_PicsPerRoundCount;

    public TextMeshProUGUI text_HighScore;

    [Header("GO")]
    public float rotatingSpeed = 3f;
    public bool isLoading=true;
    public GameObject loadingScreenGO;
    public GameObject loadingImageGO;

    [Header("UI Audio Clip")]

    public AudioClip audioClipUI;
    public AudioSource audioSourceUI;

    public void Awake()
    {
        audioSourceUI = GetComponent<AudioSource>();

        canvasGO_Settings.SetActive(false);
        canvasGO_Info.SetActive(false);
        isLoading = GameManager.Instance.isLoading;
        loadingScreenGO.SetActive(isLoading);
        /*
         * buttons
         */

        button_Settings.onClick.AddListener(() => {
            audioSourceUI.PlayOneShot(audioClipUI);
            canvasGO_Settings.SetActive(true);
        });
        button_Info.onClick.AddListener(() => {
            audioSourceUI.PlayOneShot(audioClipUI);
            canvasGO_Info.SetActive(true);
        });
        button_StartGame.onClick.AddListener(() =>
        {
            audioSourceUI.PlayOneShot(audioClipUI);
            GameManager.LoadGamePlayScene();
        });


        button_CloseSettingsCanvas.onClick.AddListener(() =>
        {
            audioSourceUI.PlayOneShot(audioClipUI);
            canvasGO_Settings.SetActive(false);
        });

        button_CloseInfoCanvas.onClick.AddListener(() =>
        {
            audioSourceUI.PlayOneShot(audioClipUI);
            canvasGO_Info.SetActive(false);
        });

        /*
         * sliders 
         */

        slider_SoundVolume.onValueChanged.AddListener((value) =>
        {
            GameManager.soundVolume = value;
            text_SoundVolume.text = ((int)(value*100)).ToString();
        });

        slider_MapSize.onValueChanged.AddListener((value) =>
        {
            GameManager.Instance.mapSize = (int)value;
            text_MapSize.text = ((int)value).ToString();
        });
        slider_TurnsCount.onValueChanged.AddListener((value) => { 
            GameManager.Instance.turnCounts = (int)value;
            text_TurnsCount.text = ((int)value).ToString();
        });
        slider_PicsPerRoundCount.onValueChanged.AddListener((value) => {
            GameManager.Instance.picsInRoundCount = (int)value;
            text_PicsPerRoundCount.text = ((int)value).ToString();
        });

        UpdateHighScore(GameManager.Instance.highScore);
        init();
    }

    public static void END_LOADING() {
        Instance.isLoading = false;
        Instance.loadingScreenGO.SetActive(false);
    }

    private void Update()
    {
        if (isLoading == true) {
            loadingImageGO.transform.Rotate(new Vector3(0, 0, rotatingSpeed), Space.Self);
        }
    }

    private void init() {
        slider_MapSize.value = GameManager.Instance.mapSize;
        text_MapSize.text = (GameManager.Instance.mapSize).ToString();

        slider_TurnsCount.value = GameManager.Instance.turnCounts;
        text_TurnsCount.text = (GameManager.Instance.turnCounts).ToString();

        slider_PicsPerRoundCount.value = GameManager.Instance.picsInRoundCount;
        text_PicsPerRoundCount.text = (GameManager.Instance.picsInRoundCount).ToString();
    }

    public void UpdateHighScore(int highScore) {
        string txt = "Рекорд: \n" + highScore.ToString();
        text_HighScore.text = txt;
    }
}
