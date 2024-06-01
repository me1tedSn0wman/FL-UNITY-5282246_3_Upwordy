using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;

public enum eFSState
{
    Idle,
    Pre,
    Active,
    Post
}

public class FloatingScoreUI : MonoBehaviour
{
    [Header("Set Dynamically")]
    public eFSState state = eFSState.Idle;

    [SerializeField] private int _score = 0;
    public string scoreString;


    public float timeStart = -1f;
    public float timeDuration = 1f;

    private RectTransform rectTrans;
    private TextMeshProUGUI textMP;

    public int score
    {
        get { return _score; }
        set { 
            _score = value;
            scoreString = _score.ToString("N0");
            textMP.text = scoreString;
        }
    }

    public List<Vector2> pts;

    public void Awake()
    {
        textMP = GetComponent<TextMeshProUGUI>();
        rectTrans = GetComponent<RectTransform>();
    }
    public void Init(List<Vector2> ePts, float eTimeS = 0, float eTimeD = 1)
    {
        rectTrans.anchoredPosition = Vector2.zero;
        pts = new List<Vector2>(ePts);

        if (ePts.Count == 1)
        { // If there's only one point
            // ...then just go there.
            transform.position = ePts[0];
            return;
        }

        // If eTimeS is the default, just start at the current time
        if (eTimeS == 0) eTimeS = Time.time;
        timeStart = eTimeS;
        timeDuration = eTimeD;
        state = eFSState.Pre; // Set it to the pre state, ready to start moving
    }

    void Update()
    {
        if (state == eFSState.Idle) return;

        float u = (Time.time - timeStart) / timeDuration;

        if (u < 0)
        { // If u<0, then we shouldn't move yet.
            state = eFSState.Pre;
            textMP.enabled = false; // Hide the score initially
        }
        else
        {
            if (u >= 1) { 
                state = eFSState.Idle;
            }
            else {
                state = eFSState.Active;
                textMP.enabled = true;
            }

            Vector2 pos = Utils.Util.Bezier(u, pts);
            rectTrans.anchorMin = rectTrans.anchorMax = pos;
        }
    }

}
