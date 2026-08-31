using System.Collections.Generic;
using UnityEngine;

public enum PieceType { pawn, knight, bishop, rook, queen, king }
public enum PieceColor { white, black }

[System.Serializable]
public struct PieceSpawnData
{
    public PieceType type;
    public PieceColor color;
    public Vector2Int gridPosition;
}

public class Piece : MonoBehaviour
{
    public PieceType type;
    public PieceColor color;
    public Vector2Int boardPosition;

    public void SetGridPosition(Vector2Int newPos, Vector3 worldpos)
    {
        boardPosition = newPos;
        transform.position = new Vector3(worldpos.x, worldpos.y, -0.2f);
    }

    public List<Vector2Int> GetPossibleMoves(Piece[,] board)
    {
        List<Vector2Int> validMoves = new List<Vector2Int>();

        switch (type)
        {
            case PieceType.pawn:
                int forwardDir = (color == PieceColor.white) ? 1 : -1;

                Vector2Int forwardPos = boardPosition + new Vector2Int(0, forwardDir);
                if (IsOnBoard(forwardPos) && board[forwardPos.x, forwardPos.y] == null)
                {
                    validMoves.Add(forwardPos);
                }

                Vector2Int[] pawnAttacks = new Vector2Int[]
                {
                    new Vector2Int(-1, forwardDir),
                    new Vector2Int(1, forwardDir)
                };

                foreach (Vector2Int attackOffset in pawnAttacks)
                {
                    Vector2Int targetPos = boardPosition + attackOffset;
                    if (IsOnBoard(targetPos))
                    {
                        Piece targetPiece = board[targetPos.x, targetPos.y];
                        if (targetPiece != null && targetPiece.color != this.color)
                        {
                            validMoves.Add(targetPos);
                        }
                    }
                }
                break;

            case PieceType.rook:
                Vector2Int[] rookDirections = new Vector2Int[]
                {
                    new Vector2Int(1, 0), new Vector2Int(-1, 0),
                    new Vector2Int(0, 1), new Vector2Int(0, -1)
                };
                AddSlidingMoves(rookDirections, validMoves, board);
                break;

            case PieceType.bishop:
                Vector2Int[] bishopDirections = new Vector2Int[]
                {
                    new Vector2Int(1, 1),  new Vector2Int(1, -1),
                    new Vector2Int(-1, 1), new Vector2Int(-1, -1)
                };
                AddSlidingMoves(bishopDirections, validMoves, board);
                break;

            case PieceType.queen:
                Vector2Int[] queenDirections = new Vector2Int[]
                {
                    new Vector2Int(1, 0),  new Vector2Int(-1, 0),  new Vector2Int(0, 1),   new Vector2Int(0, -1),
                    new Vector2Int(1, 1),  new Vector2Int(1, -1),  new Vector2Int(-1, 1),  new Vector2Int(-1, -1)
                };
                AddSlidingMoves(queenDirections, validMoves, board);
                break;

            case PieceType.knight:
                Vector2Int[] knightOffsets = new Vector2Int[]
                {
                    new Vector2Int(1, 2),  new Vector2Int(2, 1),
                    new Vector2Int(2, -1), new Vector2Int(1, -2),
                    new Vector2Int(-1, -2),new Vector2Int(-2, -1),
                    new Vector2Int(-2, 1), new Vector2Int(-1, 2)
                };
                AddSingleStepMoves(knightOffsets, validMoves, board);
                break;

            case PieceType.king:
                Vector2Int[] kingOffsets = new Vector2Int[]
                {
                    new Vector2Int(1, 0),  new Vector2Int(-1, 0),  new Vector2Int(0, 1),   new Vector2Int(0, -1),
                    new Vector2Int(1, 1),  new Vector2Int(1, -1),  new Vector2Int(-1, 1),  new Vector2Int(-1, -1)
                };
                AddSingleStepMoves(kingOffsets, validMoves, board);
                break;
        }

        return validMoves;
    }

    private void AddSlidingMoves(Vector2Int[] directions, List<Vector2Int> validMoves, Piece[,] board)
    {
        foreach (Vector2Int dir in directions)
        {
            Vector2Int nextPos = boardPosition + dir;
            while (IsOnBoard(nextPos))
            {
                Piece pieceAtTile = board[nextPos.x, nextPos.y];
                if (pieceAtTile == null)
                {
                    validMoves.Add(nextPos);
                }
                else if (pieceAtTile.color != this.color)
                {
                    validMoves.Add(nextPos);
                    break;
                }
                else
                {
                    break;
                }
                nextPos += dir;
            }
        }
    }

    private void AddSingleStepMoves(Vector2Int[] offsets, List<Vector2Int> validMoves, Piece[,] board)
    {
        foreach (Vector2Int offset in offsets)
        {
            Vector2Int targetPos = boardPosition + offset;
            if (IsOnBoard(targetPos))
            {
                Piece pieceAtTile = board[targetPos.x, targetPos.y];

                if (pieceAtTile == null || pieceAtTile.color != this.color)
                {
                    validMoves.Add(targetPos);
                }
            }
        }
    }

    private bool IsOnBoard(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < 5 && pos.y >= 0 && pos.y < 5;
    }
}