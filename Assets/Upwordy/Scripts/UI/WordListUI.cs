using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WordListUI : MonoBehaviour
{
    [Header("Set in Inspector")]
    [SerializeField] private TextMeshProUGUI text_Word;
    [SerializeField] private TextMeshProUGUI text_Score;

    [Header("Set Dynamically")]
    [SerializeField] private string word;
    [SerializeField] private int score;

    public void SetWordListUi(string word, int score) { 
        this.word = word;
        this.score = score;

        text_Word.text = word;
        text_Score.text = score.ToString();
    }
}
