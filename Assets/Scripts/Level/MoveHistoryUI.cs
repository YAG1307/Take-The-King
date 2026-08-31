using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class MoveHistoryUI : MonoBehaviour
{
    public TextMeshProUGUI[] moveSlots;
    private List<string> moveHistory = new List<string>();

    public void RecordMove(PieceType piece, Vector2Int fromPos, Vector2Int toPos, bool isCapture)
    {
        if (moveHistory.Count >= 3) return;

        string moveNotation = FormatChessNotation(piece, fromPos, toPos, isCapture);
        moveHistory.Add(moveNotation);
       
        UpdateUI();
    }

    public void UndoLastMove()
    {
        if (moveHistory.Count > 0)
        {
            moveHistory.RemoveAt(moveHistory.Count - 1);
            UpdateUI();
        }
    }

    public void ResetHistory()
    {
        moveHistory.Clear();
        UpdateUI();
    }

    private void UpdateUI()
    {
        for (int i = 0; i < moveSlots.Length; i++)
        {
            if (i < moveHistory.Count)
            {
                moveSlots[i].text = moveHistory[i];
            }
            else
            {
                moveSlots[i].text = ""; 
            }
        }
    }

    private string FormatChessNotation(PieceType piece, Vector2Int from, Vector2Int to, bool isCapture)
    {
        char file = (char)('a' + to.x);
        int rank = to.y + 1;

        string piecePrefix = piece switch
        {
            PieceType.knight => "N",
            PieceType.bishop => "B",
            PieceType.rook => "R",
            PieceType.queen => "Q",
            PieceType.king => "K",
            _ => "" 
        };

        string captureSymbol = isCapture ? "x" : "";

        return $"{piecePrefix}{captureSymbol}{file}{rank}";
    }
}