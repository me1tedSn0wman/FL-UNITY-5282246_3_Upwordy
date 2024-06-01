using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum PieceState { 
    JustCreated
    ,InHand
    ,IsSelected
    ,Moving
    ,Locked
    ,Free
    ,AddedOnMap
}

public class Piece : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    public Image image;

    public Color color_Free;
    public Color color_AddedOnMap;
    public Color color_Locked;

    [SerializeField] private PieceState _pieceState;
    public PieceState pieceState
    {
        get { return _pieceState; }
        set {
            switch (value) {
                case PieceState.AddedOnMap:
                    image.color = color_AddedOnMap;
                    break;
                case PieceState.Locked:
                    image.color = color_Locked;
                    break;
                default:
                    image.color = color_Free;
                    break;
            }
            _pieceState = value; 
        }
    }

    [SerializeField] private Vector2Int _gridPos;
    public Vector2Int gridPos
    {
        get { return _gridPos; }
        set { 
            MoveToGridPos(value);
            _gridPos = value;
        }
    }

    public Vector3 pos {
        set {
            timeStartMove = Time.time;
            pts = new List<Vector3>() { transform.position, value };
        }
    }
    private List<Vector3> pts = new List<Vector3>();
    public Vector3 posImmediate
    {
        set {
            transform.position = value;
        }
    }

    public Vector2 posHandRel;

    public float pieceSize;

    [Header("Set in Inspector")]
    public float moveTimeDuration = 1f;
    [SerializeField] private float timeStartMove = -1f;

    private RectTransform rectTransform;

    [Header("Letters")]
    public TextMeshProUGUI textMesh;

    private char _char;
    public char charForTextMesh {
        get { return _char; }
        set {
            _char = value;
            textMesh.text = _char.ToString();
        }
    }

    public string str {
        get { return _char.ToString(); }
        set { _char = value[0]; }
    }


    public void Awake()
    {
        _gridPos = new Vector2Int(-1, -1);
        image = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();
        pieceState = PieceState.JustCreated;
    }

    public void Update()
    {
        Move();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (pieceState == PieceState.Locked) return;
        pieceState = PieceState.IsSelected;
        Vector2 pointerPosition = eventData.position;
        this.posImmediate = pointerPosition;


        if (gridPos != new Vector2Int(-1, -1)) {
            Cell parentCell = GameplayManager.CELLS[gridPos.x, gridPos.y];

            transform.SetParent(GameplayManager.MOVING_PIECES_ANCHOR);
            gridPos = new Vector2Int(-1, -1);
            parentCell.BackLastPiece();
            GameplayManager.RemovePieceFromListThisRound(this);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (pieceState == PieceState.Locked) return;
        pieceState = PieceState.IsSelected;
        Vector2 pointerPosition = eventData.position;
        this.posImmediate = pointerPosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (pieceState == PieceState.Locked) return;
        Vector2 pointerPosition = eventData.position;
        Vector2Int newGridPos = GameplayManager.Instance.TryGetGridPosition(pointerPosition);
        Vector2 relPos = new Vector2(0.5f, 0.5f);
        if (newGridPos != new Vector2Int(-1, -1))
        {
            Piece oldPiece;
            if (GameplayManager.TRY_GET_GRID_PIECE(newGridPos, out oldPiece))
            {
                if (oldPiece.pieceState == PieceState.AddedOnMap)
                {
                    GameplayUIManager.SHOW_ERROR_POPUP("«десь уже есть фишка, положенна€ в этом раунде");
                    //                    Debug.Log("There already exists piece, added at this round");
                    MoveToHand();

                    return;
                }
                if (oldPiece.charForTextMesh == charForTextMesh)
                {
                    GameplayUIManager.SHOW_ERROR_POPUP("Ќельз€ ложить ложить фишку с буквой на такую же букву");
                    //                    Debug.Log("You cannot place same letter");
                    MoveToHand();
                    return;
                }
            }
            gridPos = newGridPos;
            //            Debug.Log(newGridPos.ToString());

        }
        else if (GameplayManager.Instance.CheckEndPiecePosInHand(pointerPosition, out relPos))
        {

            pieceState = PieceState.Free;
            transform.SetParent(GameplayManager.HAND_PIECES_ANCHOR.transform);
            rectTransform.anchorMin = relPos;
            rectTransform.anchorMax = relPos;
            rectTransform.anchoredPosition = Vector2.zero;
            //            rectTransform.anchoredPosition = relPos;
            //            Debug.Log("In Hand");
        }
        else if (GameplayManager.Instance.CheckExchangePosition(pointerPosition)) 
        {
            GameplayManager.Instance.RemovePiece(this);
            GameplayManager.Instance.SpawnPieceFromExchange();
            Destroy(this.gameObject);
        }
        else
        {
            MoveToHand();
            //          Debug.Log("Somewhere else");
        }

    }



    public virtual void SetPiece(float pieceSize, Vector2 relPiecePos, char letter) {
        gameObject.name = "Piece";
        charForTextMesh = letter;
        this.pieceSize = pieceSize;
        SetPiecePosition(pieceSize, relPiecePos);
    }

    public virtual void SetPiecePosition(float pieceSize, Vector2 relPiecePos)
    {
        rectTransform.sizeDelta = new Vector2(pieceSize, pieceSize);
        rectTransform.anchorMin = relPiecePos;
        rectTransform.anchorMax = relPiecePos;
        rectTransform.anchoredPosition = Vector2.zero;
        Vector3 posEnd = transform.position;
        posImmediate = GameplayManager.START_POS.position;
        pos = posEnd;

    }

    public void Resizing(float cellSize) {
        pieceSize = cellSize;
        rectTransform.sizeDelta = new Vector2(cellSize, cellSize);
    }

    public void Move()
    {
        if (timeStartMove == -1) return;
        float deltaTime = (Time.time - timeStartMove) / moveTimeDuration;
        deltaTime = Mathf.Clamp01(deltaTime);
        Vector3 tPos = Utils.Util.Bezier(deltaTime, pts);
        transform.position = tPos;

        if (deltaTime == 1)
        {
            timeStartMove = -1;
            EndMoving();
        }
    }

    public void MoveToGridPos(Vector2Int newGridPos) {
        if (newGridPos == new Vector2(-1, -1)) return;
        pos = GameplayManager.CELLS[newGridPos.x, newGridPos.y].transform.position;
        GameplayManager.PIECES_IN_HAND.Remove(this);
        pieceState = PieceState.Moving;
    }
    public void MoveToHand()
    {
        if (gridPos != new Vector2Int(-1, -1)) {
            Cell parentCell = GameplayManager.CELLS[gridPos.x, gridPos.y];

            parentCell.BackLastPiece();
        }

        gridPos = new Vector2Int(-1, -1);
        pos = GameplayManager.HAND_PIECES_ANCHOR.position;
        transform.SetParent(GameplayManager.MOVING_PIECES_ANCHOR);
        pieceState = PieceState.Moving;

    }

    public void EndMoving() {
        if (gridPos != new Vector2Int(-1, -1))
        {
            Cell parentCell = GameplayManager.CELLS[gridPos.x, gridPos.y];

            transform.SetParent(parentCell.pieceContainer);
            parentCell.AddPiece(this);
            GameplayManager.AddPieceToListThisRound(this);

            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = Vector2.zero;

            pieceState = PieceState.AddedOnMap;
        }
        else {
            transform.SetParent(GameplayManager.HAND_PIECES_ANCHOR.transform);
        }
    }

    public virtual void Destroy()
    {
        Destroy(this.gameObject);
    }
}
