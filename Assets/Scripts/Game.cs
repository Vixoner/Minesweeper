using UnityEngine;

public class Game : MonoBehaviour
{
    public int width = 16;
    public int height = 16;
    public int bombAmount = 50;

    private Board board;
    private Cell[,] state;

    private void Awake()
    {
        board = GetComponentInChildren<Board>();
    }

    private void Start() 
    {
        NewGame();
    }

    private void NewGame()
    {
        state = new Cell[width, height];
        GenerateCells();
        GenerateBombs();
        GenerateNumbers();
        Camera.main.transform.position = new Vector3(width / 2f, height / 2f, -10f);
        board.Draw(state);
    }

    private void GenerateCells()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                Cell cell = new Cell();
                cell.position = new Vector3Int(i, j, 0);
                cell.type = Cell.Type.Empty;
                state[i, j] = cell;
            }
        }
    }

    private void GenerateBombs() 
    {
        int x = 0;
        int y = 0;

        for (int i = 0; i < bombAmount; i++)
        {
            do {
                x = Random.Range(0, width);
                y = Random.Range(0, height);
            } while (state[x, y].type == Cell.Type.Bomb);
            
            state[x, y].type = Cell.Type.Bomb;
        }
    }

    private void GenerateNumbers()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                Cell cell = state[i, j];
                
                if (cell.type == Cell.Type.Bomb)
                    continue;

                cell.value = CountBombs(i, j);

                if (cell.value > 0)
                    cell.type = Cell.Type.Number;

                state[i, j] = cell;
            }
        }
    }

    private int CountBombs(int x, int y)
    {
        int result = 0;

        for (int i = x - 1; i <= x + 1; i++)
        {
            for (int j = y - 1; j <= y + 1; j++)
            {
                if ((i < 0 || i >= width) || (j < 0 || j >= height))
                    continue;

                if (state[i, j].type == Cell.Type.Bomb)
                    result++;
            }
        }

        return result;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(1))
            Flag();
        else if (Input.GetMouseButtonDown(0))
            click();
    }

    private void Flag()
    {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cellPosition = board.tilemap.WorldToCell(worldPosition);
        if ((cellPosition.x < 0 || cellPosition.x >= width) || (cellPosition.y < 0 || cellPosition.y >= height))
            return;

        Cell cell = state[cellPosition.x, cellPosition.y];

        if (cell.clicked)
            return;

        cell.flagged = !cell.flagged;
        state[cellPosition.x, cellPosition.y] = cell;
        board.Draw(state);
    }

    private void click()
    {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cellPosition = board.tilemap.WorldToCell(worldPosition);
        if ((cellPosition.x < 0 || cellPosition.x >= width) || (cellPosition.y < 0 || cellPosition.y >= height))
            return;

        Cell cell = state[cellPosition.x, cellPosition.y];

        if (cell.clicked || cell.flagged)
            return;

        cell.clicked = true;
        state[cellPosition.x, cellPosition.y] = cell;
        if (cell.type == Cell.Type.Empty && cell.value == 0)
            ClickAround(cellPosition.x, cellPosition.y);
        board.Draw(state);
    }

    private void ClickAround(int x, int y)
    {
        for (int i = x - 1; i <= x + 1; i++)
        {
            for (int j = y - 1; j <= y + 1; j++)
            {
                if ((i < 0 || i >= width) || (j < 0 || j >= height) || state[i, j].clicked == true)
                    continue;

                state[i, j].clicked = true;
                if (state[i, j].type == Cell.Type.Empty && state[i, j].value == 0)
                    ClickAround(i, j);
            }
        }
    }
}
