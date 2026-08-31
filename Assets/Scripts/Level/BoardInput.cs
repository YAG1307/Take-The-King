using UnityEngine;

public class BoardInput : MonoBehaviour
{
    private ChessBoard chessBoard;
    private BoardManager boardManager;

    void Start()
    {
        chessBoard = FindAnyObjectByType<ChessBoard>();
        boardManager = FindAnyObjectByType<BoardManager>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0f;

            Vector2Int gridPos = WorldToGrid(mouseWorldPos);

            if (gridPos.x >= 0 && gridPos.x < 5 && gridPos.y >= 0 && gridPos.y < 5)
            {
                boardManager.TileClicked(gridPos);
            }
        }
    }

    private Vector2Int WorldToGrid(Vector3 worldPos)
    {
        Vector2 startPos = chessBoard.getStartPos();

        int x = Mathf.RoundToInt((worldPos.x - startPos.x) / chessBoard.TileSize);
        int y = Mathf.RoundToInt((worldPos.y - startPos.y) / chessBoard.TileSize);

        return new Vector2Int(x, y);
    }
}