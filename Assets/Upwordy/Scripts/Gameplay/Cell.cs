using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;

public class Cell : MonoBehaviour
{
    public Vector2Int gridPos;

    public GameObject textCountGO;
    public Transform pieceContainer;
    public TextMeshProUGUI textCount;

    protected RectTransform rectTransform;

    private Piece crntPiece;
    private Piece prevPiece;
    [SerializeField] private int _piecesCount = 0;

    private int piecesCount
    {
        get { return _piecesCount; }
        set {
            if (value > 0)
            {
                textCountGO.SetActive(true);
                textCount.text = value.ToString();
            }
            else 
            {
                textCountGO.SetActive(false);
            }
            _piecesCount = value;
        }
    }

    public void Awake() {
        prevPiece = null;
        crntPiece = null;

        rectTransform = GetComponent<RectTransform>();
    }

    public void SetCell(int x, int y, float cellSize, Vector2 cellPosOffset) {
        this.gridPos.x = x;
        this.gridPos.y = y;

        gameObject.name = "Cell_" + gridPos.x.ToString("D2") + "_x_" + gridPos.y.ToString("D2");

        SetCellPosition(gridPos.x, gridPos.y, cellSize, cellPosOffset);
    }

    public void SetCellPosition(int x, int y, float cellSize, Vector2 cellPosOffset) {
//        Debug.Log(gameObject.name + "  " + x * cellSize + "  " + -y * cellSize);
        rectTransform.localPosition = new Vector3(x * cellSize + cellPosOffset.x, -y * cellSize + cellPosOffset.y);
        rectTransform.sizeDelta = new Vector2(cellSize, cellSize);
    }

    public void Resizing(float cellSize, Vector2 cellPosOffset) {
        rectTransform.sizeDelta = new Vector2(cellSize, cellSize);
        rectTransform.localPosition = new Vector3(gridPos.x * cellSize + cellPosOffset.x, -gridPos.y * cellSize + cellPosOffset.y);
    }

    public void AddPiece(Piece piece)
    {
        prevPiece = crntPiece;
        crntPiece = piece;
        piecesCount += 1;
    }

    public Piece LookLastPiece() { 
        return crntPiece;
    }

    public Piece LookPrevPiece() {
        return prevPiece;
    }

    public Piece BackLastPiece()
    {
        Piece temp = crntPiece;
        crntPiece = prevPiece;
        prevPiece = null;
        piecesCount -=1;
        return temp;
    }

    public int PieceCount() {
        return piecesCount;
    }

}

