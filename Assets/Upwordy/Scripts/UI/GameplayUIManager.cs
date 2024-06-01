using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class GameplayUIManager : Singleton<GameplayUIManager>
{
    [Header("Buttons")]
    public Button button_Settings;
    public Button button_Info;
    public Button button_ShowScore;

    public Button button_CloseSettingsCanvas;
    public Button button_CloseInfoCanvas;
    public Button button_CloseScoreCanvas;
    public Button button_ClosePopUp;

    public Button button_Draw;
    public Button button_CheckTurn;
    public Button button_EndTurn;
    public Button button_ToMainMenu;
    public Button button_SkipTurn;

    public Button button_ReturnToHand;

    public Button button_ToMainMenuGameOve;
    public Button button_AnotherTurnGameOver;

    [Header("Canvases")]
    public GameObject canvasGO_Settings;
    public GameObject canvasGO_Info;
    public GameObject canvasGO_Score;
    public GameObject canvasGO_PopUp;

    public GameObject canvasGO_ScoreListContent;

    public GameObject canvasGO_GameOverGO;

    [Header("Sliders")]
    public Slider slider_SoundVolume;

    [Header("Textes")]
    public TextMeshProUGUI text_SoundVolume;
    public TextMeshProUGUI text_Score;

    public TextMeshProUGUI text_PopUpText;

    public TextMeshProUGUI text_ScoreGameOver;

    [Header("Prefabs")]
    public WordListUI wordListPrefab;

    [Header("Times")]
    public float timerPopUpStart = -1;
    public float timerPopUpWait = 5;

    [Header("UI Audio Clip")]
    public AudioClip audioClipUI;
    public AudioSource audioSourceUI;

    public void Awake()
    {
        audioSourceUI = GetComponent<AudioSource>();

        canvasGO_Settings.SetActive(false);
        canvasGO_Info.SetActive(false);
        canvasGO_Score.SetActive(false);
        canvasGO_PopUp.SetActive(false);
        canvasGO_GameOverGO.SetActive(false);

        button_Settings.onClick.AddListener(() =>
        {
            audioSourceUI.PlayOneShot(audioClipUI);
            canvasGO_Settings.SetActive(true);
        });
        button_Info.onClick.AddListener(() =>
        {
            audioSourceUI.PlayOneShot(audioClipUI);
            canvasGO_Info.SetActive(true);
        });

        button_ShowScore.onClick.AddListener(() =>
        {
            audioSourceUI.PlayOneShot(audioClipUI);
            canvasGO_Score.SetActive(true);
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
        button_CloseScoreCanvas.onClick.AddListener(() =>
        {
            audioSourceUI.PlayOneShot(audioClipUI);
            canvasGO_Score.SetActive(false);
        });
        button_ClosePopUp.onClick.AddListener(() =>
        {
            audioSourceUI.PlayOneShot(audioClipUI);
            canvasGO_PopUp.SetActive(false);
        });


        button_Draw.onClick.AddListener(() =>
        {
            audioSourceUI.PlayOneShot(audioClipUI);
            GameplayManager.Instance.DrawPiece();
        });
        button_CheckTurn.onClick.AddListener(() =>
        {
            audioSourceUI.PlayOneShot(audioClipUI);
            GameplayManager.CHECK_TURN();
        });
        button_EndTurn.onClick.AddListener(() =>
        {
            audioSourceUI.PlayOneShot(audioClipUI);
            GameplayManager.END_TURN();
        });
        button_ReturnToHand.onClick.AddListener(() =>
        {
            audioSourceUI.PlayOneShot(audioClipUI);
            GameplayManager.RETURN_PIECES_TO_HAND();
        });


        button_ToMainMenu.onClick.AddListener(() => {
            audioSourceUI.PlayOneShot(audioClipUI);
            GameManager.LoadMainMenuScene();
        });
        button_SkipTurn.onClick.AddListener(() =>
        {
            audioSourceUI.PlayOneShot(audioClipUI);
            GameplayManager.SKIP_TURN();
        });

        button_ToMainMenuGameOve.onClick.AddListener(() => 
        {
            audioSourceUI.PlayOneShot(audioClipUI);
            GameManager.LoadMainMenuScene();
        });
        button_AnotherTurnGameOver.onClick.AddListener(() =>
        {
            audioSourceUI.PlayOneShot(audioClipUI);
            GameplayManager.IS_ANOTHER_TURN = true;
            canvasGO_GameOverGO.SetActive(false);
        });


        /*
         * Slider
         */

        slider_SoundVolume.onValueChanged.AddListener((value) =>
        {
            GameManager.soundVolume = value;
            text_SoundVolume.text = ((int)(value * 100)).ToString();
        });

        UPDATE_SCORE(0);
    }


    public static void SHOW_ERROR_POPUP (string errorText) {
        Instance.ShowErrorPopUp(errorText);
    }

    private void ShowErrorPopUp(string errorText)
    {
        canvasGO_PopUp.SetActive(true);
        text_PopUpText.text = errorText;
        timerPopUpStart = Time.time;
    }

    public static void UPDATE_SCORE(int endScore)
    {
        string txt = "Ñ÷¸ò: " + endScore;
        Instance.text_Score.text = txt;
    }

    public void AddWordToScoreList(string word, int score) {
        WordListUI crntWordList = Instantiate(wordListPrefab, canvasGO_ScoreListContent.transform);
        crntWordList.SetWordListUi(word, score);
    }

    public static void ADD_WORD_TO_SCORE_LIST(string word, int score) {
        Instance.AddWordToScoreList(word, score);
    }

    public void ClosePopUp() {
        if (timerPopUpStart == -1) return;
        if (timerPopUpStart + timerPopUpWait < Time.time) {
            canvasGO_PopUp.SetActive(false);
            timerPopUpStart = -1;
        }
    }

    public void GameOver(int score) {
        canvasGO_GameOverGO.SetActive(true);

        string txt = "Ñ÷¸ò: " + score;
        text_ScoreGameOver.text = txt;
    }

    public void Update() {
        ClosePopUp();
    }
}
