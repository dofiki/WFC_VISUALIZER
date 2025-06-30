using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public List<GameObject> allTilePrefabs;
    public int height = 10;
    public int width = 10;
    public float tileSize = 1f;

    private enum Direction { Top, Bottom, Left, Right }

    private List<GameObject>[,] grid;
    private bool[,] isCollapsed;

    private void Start()
     {
         InitializeGrid();
         CollapseOneRandomCell();

         while (!AllCollapsed())
         {
             CollapseNextCell();

         }
     }
 
    /*
    private void Start()
    {
        StartCoroutine(GenerateMapStepByStep());
    }

    private IEnumerator GenerateMapStepByStep()
    {
        InitializeGrid();
        CollapseOneRandomCell();
        yield return new WaitForSeconds(0.1f); // short delay for first tile

        while (!AllCollapsed())
        {
            CollapseNextCell();
            yield return new WaitForSeconds(0.1f); // delay between each tile spawn
        }
    }*/

    void InitializeGrid()
    {
        grid = new List<GameObject>[width, height];
        isCollapsed = new bool[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                grid[x, y] = new List<GameObject>(allTilePrefabs);
                isCollapsed[x, y] = false;
            }
        }
    }

    void CollapseOneRandomCell()
    {
        int x = Random.Range(0, width);
        int y = Random.Range(0, height);
        CollapseCell(x, y);
        PropagateConstraints(x, y);
    }

    void CollapseNextCell()
    {
        int minOptions = int.MaxValue;
        Vector2Int bestCell = new Vector2Int(-1, -1);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (!isCollapsed[x, y])
                {
                    int options = grid[x, y].Count;
                    if (options < minOptions && options > 0)
                    {
                        minOptions = options;
                        bestCell = new Vector2Int(x, y);
                    }
                }
            }
        }

        if (bestCell.x != -1)
        {
            CollapseCell(bestCell.x, bestCell.y);
            PropagateConstraints(bestCell.x, bestCell.y);
        }
    }

    void CollapseCell(int x, int y)
    {
        List<GameObject> options = grid[x, y];

        if (options.Count == 0)
        {
            Debug.LogWarning($"No options to collapse at ({x}, {y})");
            return;
        }

        GameObject selected = options[Random.Range(0, options.Count)];
        Vector3 position = new Vector3(x * tileSize, 0, y * tileSize);
        Instantiate(selected, position, selected.transform.rotation);

        grid[x, y] = new List<GameObject> { selected };
        isCollapsed[x, y] = true;
    }

    void PropagateConstraints(int StartX, int StartY)
    {
        Queue<Vector2Int> toCheck = new Queue<Vector2Int>();
        toCheck.Enqueue(new Vector2Int(StartX, StartY));

        while (toCheck.Count > 0) {
            Vector2Int current = toCheck.Dequeue();
            int x = current.x;
            int y = current.y;

            if (!isCollapsed[x, y]) return;

            GameObject selectedTile = grid[x, y][0];
            BaseTile tileComp = selectedTile.GetComponent<BaseTile>();

            FilterNeighbor(x, y + 1, tileComp.sockets.top, Direction.Bottom, toCheck);
            FilterNeighbor(x, y - 1, tileComp.sockets.bottom, Direction.Top, toCheck);
            FilterNeighbor(x - 1, y, tileComp.sockets.left, Direction.Right, toCheck);
            FilterNeighbor(x + 1, y, tileComp.sockets.right, Direction.Left, toCheck);
        }
    }

    void FilterNeighbor(int nx, int ny, List<SocketType> requiredMatch, Direction neighborSocketSide, Queue<Vector2Int> queue)
    {
        if (nx < 0 || nx >= width || ny < 0 || ny >= height) return;
        if (isCollapsed[nx, ny]) return;

 
        List<GameObject> possibleTiles = grid[nx, ny];
        int beforeCount = possibleTiles.Count;

        possibleTiles.RemoveAll(tile =>
        {
            var comp = tile.GetComponent<BaseTile>();
            List<SocketType> socket = GetSocketList(comp, neighborSocketSide);
            return !(socket.Count == requiredMatch.Count && !socket.Except(requiredMatch).Any());
        });

        if (possibleTiles.Count == 0) {
            return;
        }

        if (possibleTiles.Count < beforeCount) { 
            queue.Enqueue(new Vector2Int(nx, ny));
        }

        if (possibleTiles.Count == 1 && !isCollapsed[nx, ny])
        {
            isCollapsed[nx, ny] = true;
            Vector3 pos = new Vector3(nx * tileSize, 0, ny * tileSize);
            Instantiate(possibleTiles[0], pos, possibleTiles[0].transform.rotation);
            PropagateConstraints(nx, ny);
        }

        if (possibleTiles.Count == 0)
        {
            Debug.LogWarning($"Contradiction at ({nx}, {ny})! No valid tiles remain.");
        }
    }

    List<SocketType> GetSocketList(BaseTile tile, Direction dir)
    {
        switch (dir)
        {
            case Direction.Top: return tile.sockets.top;
            case Direction.Bottom: return tile.sockets.bottom;
            case Direction.Left: return tile.sockets.left;
            case Direction.Right: return tile.sockets.right;
            default: return new List<SocketType>();
        }
    }

    bool AllCollapsed()
    {
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                if (!isCollapsed[x, y])
                    return false;
        return true;
    }
}