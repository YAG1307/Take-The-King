using UnityEngine;

public class ChessBoard : MonoBehaviour
{
    private const int Width = 5;
    private const int Height = 5;
    public float TileSize = 1f;

    public Vector2 boardOffset = new Vector2(0f, 1.5f);

    private SpriteRenderer whiteTilePrefab;
    private SpriteRenderer blackTilePrefab;

    private void Start()
    {
        whiteTilePrefab = Resources.Load<SpriteRenderer>("SquareWhite");
        blackTilePrefab = Resources.Load<SpriteRenderer>("SquareBlack");

        if (whiteTilePrefab == null || blackTilePrefab == null)
        {
            return;
        }

        GenerateGrid();
    }

    private void GenerateGrid()
    {
        Vector2 startPos = getStartPos();

        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {

                Vector3 pos = new Vector3(startPos.x + (x * TileSize), startPos.y + (y * TileSize), 0f);

                SpriteRenderer prefabToInstantiate = ((x + y) % 2 == 0) ? blackTilePrefab : whiteTilePrefab;
                SpriteRenderer tile = Instantiate(prefabToInstantiate, pos, Quaternion.identity, transform);

                tile.sortingOrder = 0;
            }
        }
    }

    public Vector2 getStartPos()
    {
        return new Vector2(
            -(Width * TileSize) / 2f + (TileSize / 2f),
            -(Height * TileSize) / 2f + (TileSize / 2f) + boardOffset.y
        );
    }

    public Vector3 GetWorldPos(int x, int y)
    {
        Vector2 startPos = getStartPos();
        return new Vector3(startPos.x + (x * TileSize), startPos.y + (y * TileSize), 0f);
    }
}