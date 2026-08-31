using System.Collections.Generic;
using UnityEngine;

public class BoardHighlight : MonoBehaviour
{
    public GameObject moveDot;
    public GameObject captureRing;
    public List<GameObject> activeHighlights = new List<GameObject>();

    private ChessBoard chessBoard;
    private BoardManager boardManager;

    void Start()
    {
        chessBoard = FindAnyObjectByType<ChessBoard>();
        boardManager = FindAnyObjectByType<BoardManager>();
    }

    public void HighlightedMoves(List<Vector2Int> moves, Piece selectedPiece)
    {
        ClearHighlights();

        foreach (Vector2Int move in moves)
        {
            Vector3 worldPos = chessBoard.GetWorldPos(move.x, move.y);
            Piece targetPiece = boardManager.GetPieceAt(move);

            GameObject highlightObj;

            if (targetPiece != null && targetPiece.color != selectedPiece.color)
            {
                highlightObj = Instantiate(captureRing, new Vector3(worldPos.x, worldPos.y, -0.3f), Quaternion.identity);
                SpriteRenderer sr = highlightObj.GetComponent<SpriteRenderer>();
                if (sr != null) sr.sortingOrder = 15;
            }
            else
            {
                highlightObj = Instantiate(moveDot, new Vector3(worldPos.x, worldPos.y, -0.1f), Quaternion.identity);
                SpriteRenderer sr = highlightObj.GetComponent<SpriteRenderer>();
                if (sr != null) sr.sortingOrder = 5;
            }

            activeHighlights.Add(highlightObj);
        }
    }

    public void ClearHighlights()
    {
        foreach (GameObject dot in activeHighlights)
        {
            Destroy(dot);
        }
        activeHighlights.Clear();
    }
}