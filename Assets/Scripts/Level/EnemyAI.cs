using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    private BoardManager boardManager;

    public void Setup(BoardManager manager)
    {
        boardManager = manager;
    }

    public (Piece piece, Vector2Int move) GetBestMove(Piece[,] board)
    {
        int currentLevel = PlayerPrefs.GetInt("SelectedLevel", 1);

        if (currentLevel == 20)
        {
            for (int x = 0; x < 5; x++)
            {
                for (int y = 0; y < 5; y++)
                {
                    Piece p = board[x, y];
                    if (p != null && p.color == PieceColor.black && p.type == PieceType.queen)
                    {
                        List<Vector2Int> queenMoves = boardManager.GetLegalMoves(p);
                        Vector2Int targetMove = new Vector2Int(3, 1);

                        if (queenMoves.Contains(targetMove))
                        {
                            return (p, targetMove);
                        }
                    }
                }
            }
        }

        Piece bestPiece = null;
        Vector2Int bestMove = Vector2Int.zero;
        int highestScore = -999;

        List<Piece> blackPieces = GetBlackPieces(board);

        foreach (Piece p in blackPieces)
        {
            List<Vector2Int> legalMoves = boardManager.GetLegalMoves(p);

            foreach (Vector2Int move in legalMoves)
            {
                int score = EvaluateMove(p, move, board);

                if (p.type == PieceType.king && boardManager.IsSqrAttacked(p.boardPosition, PieceColor.white))
                {
                    score += 50;
                }

                if (score > highestScore)
                {
                    highestScore = score;
                    bestPiece = p;
                    bestMove = move;
                }
            }
        }

        return (bestPiece, bestMove);
    }

    private int EvaluateMove(Piece piece, Vector2Int targetPos, Piece[,] board)
    {
        int score = 0;
        Piece targetPiece = board[targetPos.x, targetPos.y];

        if (targetPiece != null && targetPiece.color == PieceColor.white)
        {
            score += targetPiece.type switch
            {
                PieceType.king => 999,
                PieceType.queen => 90,
                PieceType.rook => 50,
                PieceType.bishop => 30,
                PieceType.knight => 30,
                PieceType.pawn => 10,
                _ => 0
            };
        }

        if (boardManager.IsSqrAttacked(targetPos, PieceColor.white)) score -= 40;
        score += (4 - targetPos.y);

        return score;
    }

    private List<Piece> GetBlackPieces(Piece[,] board)
    {
        List<Piece> pieces = new List<Piece>();
        for (int x = 0; x < 5; x++)
        {
            for (int y = 0; y < 5; y++)
            {
                if (board[x, y] != null && board[x, y].color == PieceColor.black)
                    pieces.Add(board[x, y]);
            }
        }
        return pieces;
    }
}