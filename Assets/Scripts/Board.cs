using UnityEngine;
using UnityEngine.Tilemaps;

public class Board : MonoBehaviour
{
    public Tilemap tilemap { get; private set;}
    public Tile tileUnknown;
    public Tile tileEmpty;
    public Tile tileExploded;
    public Tile tileMine;
    public Tile tileFlag;
    public Tile tileNumber1;
    public Tile tileNumber2;
    public Tile tileNumber3;
    public Tile tileNumber4;
    public Tile tileNumber5;
    public Tile tileNumber6;
    public Tile tileNumber7;
    public Tile tileNumber8;


    private void Awake()
    {
        tilemap = GetComponent<Tilemap>();
    }

    public void Draw(Cell[,] state)
    {
        int x = state.GetLength(0);
        int y = state.GetLength(1);

        for (int i = 0; i < x; i++)
        {
            for (int j = 0; j < y; j++)
            {
                Cell cell = state[i, j];
                tilemap.SetTile(cell.position, GetTile(cell));
            }
        }
    } 

    private Tile GetTile(Cell cell)
    {
        if(cell.clicked) {
            return GetClickedTile(cell);
        } else if (cell.flagged) {
            return tileFlag;
        } else {
            return tileUnknown;
        }
    }

    private Tile GetClickedTile(Cell cell)
    {
        switch(cell.type)
        {
            case Cell.Type.Empty: return tileEmpty;
            case Cell.Type.Bomb: return tileMine;
            case Cell.Type.Number: return GetNumberTile(cell);
            default: return null;
        }
    }

    private Tile GetNumberTile(Cell cell)
    {
        switch(cell.value)
        {
            case 1: return tileNumber1;
            case 2: return tileNumber2;
            case 3: return tileNumber3;
            case 4: return tileNumber4;
            case 5: return tileNumber5;
            case 6: return tileNumber6;
            case 7: return tileNumber7;
            case 8: return tileNumber8;
            default: return null;
        }
    }
}
