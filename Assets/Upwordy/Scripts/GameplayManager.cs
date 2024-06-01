using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using Unity.Collections.LowLevel.Unsafe;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public enum GameplayState { 
    PreGame
    ,LoadMap
    ,DrawPieces
    ,WaitInput
    ,SomethingMoving
    ,CheckBoard
    ,SpaanNewPieces
}


public class GameplayManager : Singleton<GameplayManager>
{
    [SerializeField] private GameplayState _gameplayState;
    public GameplayState gameplayState
    {
        get { return _gameplayState; }
        set {
            _gameplayState = value;

        }
    }




    public const int MIN_WORD_LENGTH = 2;
    static public int MAP_SIZE;
    static public Cell[,] CELLS;
    static public List<Piece> PIECES;

    static public Transform START_POS;
    static public Transform CELL_ANCHOR;
    static public Transform HAND_PIECES_ANCHOR;
    static public Transform MOVING_PIECES_ANCHOR;
    static public List<Piece> PIECES_IN_HAND;

    static public int TURNS_COUNT;
    static public int PIECES_COUNT_PER_ROUND;

    static public List<Piece> PIECES_ADDED_THIS_ROUND;

    static public Dictionary<string, int> WORDS_THIS_TURN;
    static public Dictionary<string, int> WORDS_THIS_GAME;

    public static Dictionary<char, int> LIST_OF_PIECES;

    [Header("Set in Inspector")]
    public GameObject startPosition;
    public GameObject cellAnchor;
    public GameObject handPieceAnchor;
    public GameObject movingPiecesAnchor;
    public GameObject exchangeAnchor;

    public Cell cellPrefab;
    public Piece pieceLetterPrefab;

    /*
    public int mapSize = 8;
    public int turnCounts = 8;
    public int picsInRoundCount = 8;
    */
    public float cellSize = 0;
    public Vector2 cellPosOffset = new Vector2(0, 0);

    public RectTransform rectTr_cellCAnch;
    public RectTransform rectTr_handAnch;
    public RectTransform rectTr_exchangeAnch;

    [Header("Scores Value")]
    public int scorePerNewTile = 2;
    public int scorePerOldTile = 1;
    public int scorePerStackedTiles = 1;
    public int scorePerRemovingTile = -1;

    public static int SCORE_PER_NEW_TILE;
    public static int SCORE_PER_OLD_TILE;
    public static int SCORE_PER_STACK_TILE;
    public static int SCORE_PER_REMOVING_TILE;

    public static int TOTAL_SCORE= 0;
    public static int SCORE_THIS_ROUND = 0;

    public int scoreThisRound = 0;
    public int crntTurnCount = 1;


    public static bool IS_ANOTHER_TURN = false;

    //    public List<string> wordsThisTurn;
    //    public List<string> wordsThisGame;

    public void Awake()
    {

        TOTAL_SCORE = 0;
        gameplayState = GameplayState.PreGame;

        MAP_SIZE = GameManager.Instance.mapSize;
        CELLS = new Cell[MAP_SIZE, MAP_SIZE];
        PIECES = new List<Piece>();

        START_POS = startPosition.transform;
        CELL_ANCHOR = cellAnchor.transform;
        HAND_PIECES_ANCHOR = handPieceAnchor.transform;
        MOVING_PIECES_ANCHOR = movingPiecesAnchor.transform;


        PIECES_IN_HAND = new List<Piece>();
        LIST_OF_PIECES = new Dictionary<char, int>(GameManager.Instance.LIST_OF_PIECES);


        TURNS_COUNT = GameManager.Instance.turnCounts;
        PIECES_COUNT_PER_ROUND = GameManager.Instance.picsInRoundCount;

        PIECES_ADDED_THIS_ROUND = new List<Piece>();

        WORDS_THIS_TURN = new Dictionary<string, int>();
        WORDS_THIS_GAME = new Dictionary<string, int>();

        SCORE_PER_NEW_TILE = scorePerNewTile;
        SCORE_PER_OLD_TILE = scorePerOldTile;
        SCORE_PER_STACK_TILE = scorePerStackedTiles;
        SCORE_PER_REMOVING_TILE = scorePerRemovingTile;

        rectTr_cellCAnch = cellAnchor.GetComponent<RectTransform>();
        rectTr_handAnch = handPieceAnchor.GetComponent<RectTransform>();
        rectTr_exchangeAnch = exchangeAnchor.GetComponent<RectTransform>();


        GameManager.OnScreenSizeChanged += Resizing;

        CalculateCellSize();
        CreateCellGrid();
        DrawPiecesForTurn(GameManager.Instance.picsInBegining);
    }

    public void Update() {
    }

    public void GameStateCheck() {
    }

    public void CalculateCellSize() {
        RectTransform fieldRect = cellAnchor.GetComponent<RectTransform>();
        cellSize = Mathf.Min(fieldRect.rect.width, fieldRect.rect.height) / MAP_SIZE;
        cellPosOffset = new Vector2(
            -(cellSize * MAP_SIZE) / 2 + cellSize/2,
            (cellSize * MAP_SIZE) / 2 - cellSize/2
            );
    }

    public void CreateCellGrid() {
        gameplayState = GameplayState.LoadMap;

        for (int j = 0; j < MAP_SIZE; j++) {
            for (int i = 0; i < MAP_SIZE; i++) {
                SpawnCell(i, j);
            }
        }
    }

    public void SpawnCell(int i, int j)
    {
        Cell newCell = Instantiate<Cell>(cellPrefab);
        newCell.transform.SetParent(CELL_ANCHOR);
        // to do add cell offset
        newCell.SetCell(i, j, cellSize, cellPosOffset);
        CELLS[i, j] = newCell;
    }

    public void DrawPiecesForTurn(int count) {
        gameplayState = GameplayState.DrawPieces;
        for (int i = 0; i < count; i++) {

//        Vector2 crntPos = new Vector2(0, 0);
        Vector2 crntPos = new Vector2((-((float)count) / 2 + i)*cellSize, 0);
            DrawPiece(crntPos);
        }
    }

    public void DrawPiece() {
        SpawnPieces();
    }

    public void DrawPiece(Vector2 piecePos)
    {
        SpawnPieces(piecePos);
    }

    public void SpawnPieces() {
        Vector2 relPosition = GetRelativePosInHand(new Vector2(0, 0));

        Piece newPiece = Instantiate<Piece>(pieceLetterPrefab);
        newPiece.transform.SetParent(HAND_PIECES_ANCHOR);
        newPiece.SetPiece(cellSize, relPosition, GET_CHAR_PIECE());
        PIECES_IN_HAND.Add(newPiece);
        PIECES.Add(newPiece);
    }

    public void SpawnPieces(Vector2 piecePos)
    {
        Vector2 relPosition = GetRelativePosInHand(piecePos);

        Piece newPiece = Instantiate<Piece>(pieceLetterPrefab);
        newPiece.transform.SetParent(HAND_PIECES_ANCHOR);
        newPiece.SetPiece(cellSize, relPosition, GET_CHAR_PIECE());
        PIECES_IN_HAND.Add(newPiece);
        PIECES.Add(newPiece);
    }

    public void SpawnPieceFromExchange() {
        DrawPiece();
    }

    public Vector2Int TryGetGridPosition(Vector2 screenPoint) {
        Vector2Int gridPos = new Vector2Int(-1, -1);
        Vector3 localMapPos = rectTr_cellCAnch.InverseTransformPoint(screenPoint);

        if (true
            && localMapPos.x >= rectTr_cellCAnch.rect.xMin
            && localMapPos.x <= rectTr_cellCAnch.rect.xMax
            && localMapPos.y >= rectTr_cellCAnch.rect.yMin
            && localMapPos.y <= rectTr_cellCAnch.rect.yMax
            )
        {
            gridPos.x = Mathf.FloorToInt((localMapPos.x - rectTr_cellCAnch.rect.xMin) / cellSize);
            gridPos.y = Mathf.FloorToInt((rectTr_cellCAnch.rect.xMax - localMapPos.y) / cellSize);
        }

        /*
        Debug.Log(
            rectTr_cellCAnch.rect.xMin
            + "___" + rectTr_cellCAnch.rect.xMax
            + "___" + rectTr_cellCAnch.rect.yMin
            + "___" + rectTr_cellCAnch.rect.yMax
            + "___" + localMapPos.ToString()
            + "___" + gridPos.ToString()
            );
        */


        return gridPos;

        

        
        //        Debug.Log("cell_anch: " +rectCont + "   handCont: " + handCont);

    }

    public bool CheckEndPiecePosInHand(Vector2 screenPoint, out Vector2 relPosition) {
        Vector3 localHandPos = rectTr_handAnch.InverseTransformPoint(screenPoint);
        relPosition = Vector2.zero;
        if (true
            && localHandPos.x >= rectTr_handAnch.rect.xMin
            && localHandPos.x <= rectTr_handAnch.rect.xMax
            && localHandPos.y >= rectTr_handAnch.rect.yMin
            && localHandPos.y <= rectTr_handAnch.rect.yMax

        )
        {

            relPosition = GetRelativePosInHand(new Vector2(localHandPos.x, localHandPos.y));

//            Debug.Log(relPosition);
            return true;
        }
        return false;
    }

    public bool CheckExchangePosition(Vector2 screenPoint)
    {
        Vector3 localHandPos = rectTr_exchangeAnch.InverseTransformPoint(screenPoint);
        if (true
            && localHandPos.x >= rectTr_exchangeAnch.rect.xMin
            && localHandPos.x <= rectTr_exchangeAnch.rect.xMax
            && localHandPos.y >= rectTr_exchangeAnch.rect.yMin
            && localHandPos.y <= rectTr_exchangeAnch.rect.yMax

        )
        {
            return true;
        }
        return false;
    }


    public Vector2 GetRelativePosInHand(Vector2 localPos) { 
        return new Vector2(
                (localPos.x - rectTr_handAnch.rect.xMin) / (rectTr_handAnch.rect.xMax - rectTr_handAnch.rect.xMin),
                (localPos.y - rectTr_handAnch.rect.yMin) / (rectTr_handAnch.rect.yMax - rectTr_handAnch.rect.yMin)
                );
    }

    public static void AddPieceToListThisRound(Piece piece) {
        PIECES_ADDED_THIS_ROUND.Add(piece);
    }

    public static void RemovePieceFromListThisRound(Piece piece)
    {
        if(PIECES_ADDED_THIS_ROUND.Contains(piece))
            PIECES_ADDED_THIS_ROUND.Remove(piece);
    }


    public static int PIECES_COUNT() {
        int sum = 0;

        foreach (KeyValuePair<char,int> kvp in LIST_OF_PIECES) {
            sum += kvp.Value;
        }
        Debug.Log(sum);
        return sum;
    }

    public static char GET_CHAR_PIECE() {
        int pieceCount = PIECES_COUNT();
        if (pieceCount == 0)
        {
            GameplayUIManager.SHOW_ERROR_POPUP("Кончились Фишки");
//            Debug.Log("endOfPieces");
            return ' ';
        }
        int ind = Random.Range(0, PIECES_COUNT());
        int sum = 0;

        char crntChar = ' ';
        int crntCount=0;
        foreach(KeyValuePair<char, int> kvp in LIST_OF_PIECES) {

            crntChar= kvp.Key;
            crntCount = kvp.Value;
            sum += crntCount;
            if (ind < sum) {
                break;
            }
        }
        if (crntChar != ' ')
        {
            LIST_OF_PIECES.Remove(crntChar);
            LIST_OF_PIECES.Add(crntChar, crntCount - 1);
        }
        return crntChar;
    }

    public static bool CHECK_TURN() {


        int crntRoundPiecesCount = PIECES_ADDED_THIS_ROUND.Count;
        if (crntRoundPiecesCount == 0) {
            GameplayUIManager.SHOW_ERROR_POPUP("Для окончания хода нужно добавить хотя бы одну фишку");
//            Debug.Log("Nothing Was Added");
            return false;
        }

        WORDS_THIS_TURN = new Dictionary<string, int>();

        bool sameRow = true;
        bool sameColumn = true;
        int crntRoundRow = PIECES_ADDED_THIS_ROUND[0].gridPos.x;
        int crntRoundColumn = PIECES_ADDED_THIS_ROUND[0].gridPos.y;

        int minX = crntRoundRow;
        int minY = crntRoundColumn;

        int newPiecesCount=0;
        int oldPiecesCount = 0;
        bool existsLockedLetter = false;

        for (int i = 1; i < PIECES_ADDED_THIS_ROUND.Count; i++) {
            if (PIECES_ADDED_THIS_ROUND[i].gridPos.x != crntRoundRow) sameColumn = false;
            if (PIECES_ADDED_THIS_ROUND[i].gridPos.y != crntRoundColumn) sameRow = false;
            if (!sameRow && !sameColumn) {
                GameplayUIManager.SHOW_ERROR_POPUP("Новые фишки должны располагаться на одной строке или колонне");
//                Debug.Log("Not same column/row");
                return false;
            }

            minX = Mathf.Min(minX, PIECES_ADDED_THIS_ROUND[i].gridPos.x);
            minY = Mathf.Min(minY, PIECES_ADDED_THIS_ROUND[i].gridPos.y);
        }

        /*
        Debug.Log(
            "sameRow__" + sameRow
            + "sameColumn__" + sameColumn
            + "minX__" + minX
            + "minY__" + minY
            + "crntRoundRow__" + crntRoundRow
            + "crntRoundColumn__" + crntRoundColumn
            );
        */

        if (sameRow) {
            string main_word = "";
            int mainWordScore = 0;
            /*going right*/
            for (int i=0; minX+i<MAP_SIZE;i++) {

                Cell lookCell = CELLS[minX + i, crntRoundColumn];
                Piece lookPiece = lookCell.LookLastPiece();
                Piece lookPrevPiece = lookCell.LookPrevPiece();

                if (lookPiece == null && newPiecesCount < PIECES_ADDED_THIS_ROUND.Count) {
                    GameplayUIManager.SHOW_ERROR_POPUP("В добавленных словах есть пробелы");
//                    Debug.Log("There is a gap");
                    return false;
                }
                if (lookPiece == null) {
                    break;
                }
                if (lookPiece != null) {
                    main_word += lookPiece.charForTextMesh;
                    if (lookPiece.pieceState == PieceState.AddedOnMap) {
                        newPiecesCount++;
                        mainWordScore += SCORE_PER_NEW_TILE + Mathf.Max(0, (lookCell.PieceCount() - 1)) * SCORE_PER_STACK_TILE;
                    }
                    if (lookPiece.pieceState == PieceState.AddedOnMap && lookPrevPiece != null) {
                        oldPiecesCount++;
                    }
                    if (lookPiece.pieceState == PieceState.Locked) {
                        mainWordScore += SCORE_PER_OLD_TILE + Mathf.Max(0, (lookCell.PieceCount() - 1)) * SCORE_PER_STACK_TILE;
                        existsLockedLetter = true;
                    }
                }
            }
            /*going left*/
            for (int i = 1; minX - i >= 0; i++) {
                Cell lookCell = CELLS[minX - i, crntRoundColumn];
                Piece lookPiece = lookCell.LookLastPiece();
                if (lookPiece == null)
                {
                    break;
                }
                if (lookPiece != null)
                {
                    main_word = lookPiece.charForTextMesh + main_word;
                    mainWordScore += SCORE_PER_OLD_TILE + Mathf.Max(0, (lookCell.PieceCount() - 1)) * SCORE_PER_STACK_TILE;
                    existsLockedLetter = true;
                }
            }
            /*check covering whole world */
            if (oldPiecesCount <= PIECES_ADDED_THIS_ROUND.Count && oldPiecesCount > 1 && !existsLockedLetter) {
                GameplayUIManager.SHOW_ERROR_POPUP("Нельзя покрывать полностью всё старое слово");
//                Debug.Log("can't cover whole word");
                return false;
            }
            if (main_word.Length >= MIN_WORD_LENGTH && !WORDS_THIS_TURN.ContainsKey(main_word) && !WORDS_THIS_GAME.ContainsKey(main_word))
            {
                WORDS_THIS_TURN.Add(main_word, mainWordScore);
            }


            for (int i = 0; i < PIECES_ADDED_THIS_ROUND.Count; i++) {
                string word = "";
                int wordScore = 0;
                Vector2Int crntGridPos = PIECES_ADDED_THIS_ROUND[i].gridPos;
                for (int j = 0; crntGridPos.y + j < MAP_SIZE; j++) {
                    Cell lookCell = CELLS[crntGridPos.x, crntGridPos.y + j];
                    Piece lookPiece = lookCell.LookLastPiece();

                    if (lookPiece == null) break;

                    word += lookPiece.charForTextMesh;
                    if (lookPiece.pieceState == PieceState.AddedOnMap)
                        wordScore += SCORE_PER_NEW_TILE + Mathf.Max(0, (lookCell.PieceCount() - 1)) * SCORE_PER_STACK_TILE;
                    else if (lookPiece.pieceState == PieceState.Locked)
                        wordScore += SCORE_PER_OLD_TILE + Mathf.Max(0, (lookCell.PieceCount() - 1)) * SCORE_PER_STACK_TILE;
                }
                for (int j = 1; crntGridPos.y - j >= 0; j++)
                {
                    Cell lookCell = CELLS[crntGridPos.x, crntGridPos.y - j];
                    Piece lookPiece = lookCell.LookLastPiece();

                    if (lookPiece == null) break;
                    word = lookPiece.charForTextMesh + word;

                    if (lookPiece.pieceState == PieceState.AddedOnMap)
                        wordScore += SCORE_PER_NEW_TILE + Mathf.Max(0, (lookCell.PieceCount() - 1)) * SCORE_PER_STACK_TILE;
                    else if (lookPiece.pieceState == PieceState.Locked)
                        wordScore += SCORE_PER_OLD_TILE + Mathf.Max(0, (lookCell.PieceCount() - 1)) * SCORE_PER_STACK_TILE;

                }
                if (word.Length >= MIN_WORD_LENGTH && !WORDS_THIS_TURN.ContainsKey(word) && !WORDS_THIS_GAME.ContainsKey(word))
                {
                    WORDS_THIS_TURN.Add(word, wordScore);
                }
            }



        }

        if (sameColumn)
        {
            string main_word = "";
            int mainWordScore = 0;
            for (int i = 0; minY + i < MAP_SIZE; i++)
            {
                Cell lookCell = CELLS[crntRoundRow, minY+i];
                Piece lookPiece = lookCell.LookLastPiece();
                Piece lookPrevPiece = lookCell.LookPrevPiece();

                if (lookPiece == null && newPiecesCount < PIECES_ADDED_THIS_ROUND.Count)
                {
                    GameplayUIManager.SHOW_ERROR_POPUP("В добавленных словах есть пробелы");
//                    Debug.Log("There is a gap");
                    return false;
                }
                if (lookPiece == null)
                {
                    break;
                }
                if (lookPiece != null)
                {
                    main_word += lookPiece.charForTextMesh;
                    if (lookPiece.pieceState == PieceState.AddedOnMap)
                    {
                        newPiecesCount++;
                        mainWordScore += SCORE_PER_NEW_TILE + Mathf.Max(0, (lookCell.PieceCount() - 1)) * SCORE_PER_STACK_TILE;
                    }
                    if (lookPiece.pieceState == PieceState.AddedOnMap && lookPrevPiece != null)
                    {
                        oldPiecesCount++;
                    }
                    if (lookPiece.pieceState == PieceState.Locked)
                    {
                        mainWordScore += SCORE_PER_OLD_TILE + Mathf.Max(0, (lookCell.PieceCount() - 1)) * SCORE_PER_STACK_TILE;
                        existsLockedLetter = true;
                    }
                }


            }

            for (int i = 1; minY - i >= 0; i++)
            {
                Cell lookCell = CELLS[crntRoundRow, minY - i];
                Piece lookPiece = lookCell.LookLastPiece();
                if (lookPiece == null)
                {
                    break;
                }
                if (lookPiece != null)
                {
                    main_word = lookPiece.charForTextMesh + main_word;
                    mainWordScore += SCORE_PER_OLD_TILE + Mathf.Max(0, (lookCell.PieceCount() - 1)) * SCORE_PER_STACK_TILE;
                    existsLockedLetter = true;
                }
            }
            if (oldPiecesCount <= PIECES_ADDED_THIS_ROUND.Count && oldPiecesCount > 1 && !existsLockedLetter)
            {
                GameplayUIManager.SHOW_ERROR_POPUP("Нельзя покрывать полностью всё старое слово");
//                Debug.Log("can't cover whole word");
                return false;
            }

            if (main_word.Length >= MIN_WORD_LENGTH && !WORDS_THIS_TURN.ContainsKey(main_word) && !WORDS_THIS_GAME.ContainsKey(main_word))
            {
                WORDS_THIS_TURN.Add(main_word, mainWordScore);
            }


            for (int i = 0; i < PIECES_ADDED_THIS_ROUND.Count; i++)
            {
                string word = "";
                int wordScore = 0;
                Vector2Int crntGridPos = PIECES_ADDED_THIS_ROUND[i].gridPos;
                for (int j = 0; crntGridPos.x + j < MAP_SIZE; j++)
                {
                    Cell lookCell = CELLS[crntGridPos.x + j, crntGridPos.y];
                    Piece lookPiece = lookCell.LookLastPiece();
                    if (lookPiece == null) break;
                    word += lookPiece.charForTextMesh;

                    if (lookPiece.pieceState == PieceState.AddedOnMap)
                        wordScore += SCORE_PER_NEW_TILE + Mathf.Max(0, (lookCell.PieceCount() - 1)) * SCORE_PER_STACK_TILE;
                    else if (lookPiece.pieceState == PieceState.Locked)
                        wordScore += SCORE_PER_OLD_TILE + Mathf.Max(0, (lookCell.PieceCount() - 1)) * SCORE_PER_STACK_TILE;
                }
                for (int j = 1; crntGridPos.x - j >= 0; j++)
                {
                    Cell lookCell = CELLS[crntGridPos.x - j, crntGridPos.y];
                    Piece lookPiece = lookCell.LookLastPiece();
                    if (lookPiece == null) break;
                    word = lookPiece.charForTextMesh + word;
                }
                if (word.Length >= MIN_WORD_LENGTH && !WORDS_THIS_TURN.ContainsKey(word) && !WORDS_THIS_GAME.ContainsKey(main_word))
                {
                    WORDS_THIS_TURN.Add(word, wordScore);
                }
            }
        }


        List<string> listOfNonExistsWords = new List<string>();

        if (WORDS_THIS_TURN.Count == 0) {
            GameplayUIManager.SHOW_ERROR_POPUP("На поле нет новых слов");
//            Debug.Log("There are no words");
            return false;
        }

        foreach (KeyValuePair<string,int> kvp in WORDS_THIS_TURN) {

            if (!GameManager.Instance.LIST_OF_WORDS.Contains(kvp.Key))
                listOfNonExistsWords.Add(kvp.Key);
        }

        if (listOfNonExistsWords.Count > 0) {
            string text = "Я не знаю таких слов:";
            for (int i = 0; i < listOfNonExistsWords.Count; i++) {
                text += " " + listOfNonExistsWords[i];
            }
            Debug.Log(text);
            GameplayUIManager.SHOW_ERROR_POPUP(text);
            return false;
        }

        return true;

    }


    public static bool TRY_GET_GRID_PIECE(Vector2Int gridPos, out Piece oldPiece) {
        oldPiece = CELLS[gridPos.x, gridPos.y].LookLastPiece();
        if (oldPiece == null) return false;
        return true;
    }

    public static void RETURN_PIECES_TO_HAND() {
        for (int i = 0; i < PIECES_ADDED_THIS_ROUND.Count; i++) {
            PIECES_ADDED_THIS_ROUND[i].MoveToHand();
        }
        PIECES_ADDED_THIS_ROUND.Clear();
    }

    public static void SKIP_TURN() {
        RETURN_PIECES_TO_HAND();
        int addScore = 0;
        addScore -= Mathf.Max(0, PIECES_IN_HAND.Count - PIECES_COUNT_PER_ROUND);
        TOTAL_SCORE += addScore;
        GameplayUIManager.UPDATE_SCORE(TOTAL_SCORE);
        if (Instance.crntTurnCount < TURNS_COUNT || IS_ANOTHER_TURN)
        {
            Instance.DrawPiecesForTurn(PIECES_COUNT_PER_ROUND);
            Instance.crntTurnCount++;
        }
        else {
            GameplayUIManager.Instance.GameOver(TOTAL_SCORE);
            GameManager.Instance.CheckHighScore(TOTAL_SCORE);
        }
    }

    public static void END_TURN() {
        bool check = CHECK_TURN();

        if (!check) {
//            Debug.Log("Something Wrong");
            return;
        }

        for (int i = 0; i < PIECES_ADDED_THIS_ROUND.Count; i++) {
            PIECES_ADDED_THIS_ROUND[i].pieceState = PieceState.Locked;
        }

        int addScore = 0;
        foreach (KeyValuePair<string, int> kvp in WORDS_THIS_TURN) {
            WORDS_THIS_GAME.Add(kvp.Key,kvp.Value);
            GameplayUIManager.ADD_WORD_TO_SCORE_LIST(kvp.Key, kvp.Value);
            addScore += kvp.Value;
        }
        addScore -= Mathf.Max(0, PIECES_IN_HAND.Count - GameManager.Instance.picsInBegining);

        SCORE_THIS_ROUND = addScore;
        TOTAL_SCORE += addScore;
        GameplayUIManager.UPDATE_SCORE(TOTAL_SCORE);
        if (Instance.crntTurnCount < TURNS_COUNT || IS_ANOTHER_TURN)
        {
            PIECES_ADDED_THIS_ROUND = new List<Piece>();
            WORDS_THIS_TURN = new Dictionary<string, int>();
            Instance.DrawPiecesForTurn(PIECES_COUNT_PER_ROUND);
            Instance.crntTurnCount++;
        }
        else {
            GameplayUIManager.Instance.GameOver(TOTAL_SCORE);
            GameManager.Instance.CheckHighScore(TOTAL_SCORE);
        }
    }

    public void Resizing(Vector2Int newScreenSize) {
        CalculateCellSize();

        for (int i = 0; i < MAP_SIZE; i++) {
            for (int j = 0; j < MAP_SIZE; j++)
            {
                CELLS[i, j].Resizing(cellSize, cellPosOffset);
            }
        }

        for (int i = 0; i < PIECES.Count; i++) {
            PIECES[i].Resizing(cellSize);
        }
    }

    public void RemovePiece(Piece piece) {
        PIECES.Remove(piece);
        PIECES_IN_HAND.Remove(piece);

        TOTAL_SCORE += SCORE_PER_REMOVING_TILE;
        GameplayUIManager.UPDATE_SCORE(TOTAL_SCORE);
    }


    public void OnDestroy()
    {
        GameManager.OnScreenSizeChanged -= Resizing;
    }


}
