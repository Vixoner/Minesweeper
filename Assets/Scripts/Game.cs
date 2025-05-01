using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Game : MonoBehaviour
{
    public int width = 10;
    public int height = 10;
    public int bombAmount = 20;
    public TMP_Text bombCounterText;
    public TMP_Text winText;
    private int remainingBombs;

    private Board board;
    private Cell[,] state;
    private bool alive;
    private bool gameStarted = false;

    public GameObject setupPanel;
    public TMP_InputField widthInput;
    public TMP_InputField heightInput;
    public Button startButton;

    private void Awake()
    {
        board = GetComponentInChildren<Board>();
    }

    private void Start() 
    {
        widthInput.text = width.ToString();
        heightInput.text = height.ToString();
        
        startButton.onClick.AddListener(StartGameFromSetup);
        
        setupPanel.SetActive(true);
        
        board.gameObject.SetActive(false);

        bombCounterText.gameObject.SetActive(false);
        winText.gameObject.SetActive(false);
    }

    public void StartGameFromSetup()
    {
        if (!int.TryParse(widthInput.text, out width))
        {
            width = 10;
        }
        
        if (!int.TryParse(heightInput.text, out height))
        {
            height = 10;
        }
        
        width = Mathf.Clamp(width, 5, 15);
        height = Mathf.Clamp(height, 5, 15);
        bombAmount = (width * height) / 8;

        setupPanel.SetActive(false);
        
        board.gameObject.SetActive(true);
        
        bombCounterText.gameObject.SetActive(true);

        gameStarted = true;
        NewGame();
    }

    private void NewGame()
    {
        board.ClearBoard();
        state = new Cell[width, height];
        alive = true;
        remainingBombs = bombAmount;
        UpdateBombCounter();
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
        if (Input.GetKeyDown(KeyCode.R))
        {
            setupPanel.SetActive(true);
        
            board.gameObject.SetActive(false);
            
            gameStarted = false;

            bombCounterText.gameObject.SetActive(false);
            winText.gameObject.SetActive(false);

            Debug.Log("Game restarted.");
        }

        if (gameStarted && alive)
        {
            if (Input.GetMouseButtonDown(1))
                Flag();
            else if (Input.GetMouseButtonDown(0))
                click();
        }
        
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

        if (!cell.flagged)
        {
            remainingBombs--;
        }
        else
        {
            remainingBombs++;
        }

        UpdateBombCounter();

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

        // Flooding
        if (cell.type == Cell.Type.Empty)
            ClickAround(cellPosition.x, cellPosition.y);

        // GG
        if (cell.type == Cell.Type.Bomb)
            Explode(cell);
            
        CheckForWin();

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

                if (state[i, j].flagged)
                {
                    remainingBombs++;
                    state[i, j].flagged = false;
                    UpdateBombCounter();
                }

                state[i, j].clicked = true;
                if (state[i, j].type == Cell.Type.Empty)
                    ClickAround(i, j);
            }
        }
    }

    private void Explode(Cell cell)
    {
        alive = false;
        cell.exploded = true;
        state[cell.position.x, cell.position.y] = cell;

        // Reveal all mines
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (state[i, j].type == Cell.Type.Bomb)
                {
                    state[i, j].clicked = true;
                }
            }
        }

        Debug.Log("Game Over!");

        Invoke("ShowSetupPanelDelayed", 2f);
    }

    private void ShowSetupPanelDelayed()
    {
        setupPanel.SetActive(true);
        
        board.gameObject.SetActive(false);
        
        gameStarted = false;

        bombCounterText.gameObject.SetActive(false);
        winText.gameObject.SetActive(false);
    }

    private void CheckForWin()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (state[i, j].type != Cell.Type.Bomb && !state[i, j].clicked)
                {
                    return;
                }
            }
        }

        alive = false;

        // Flag all mines
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (state[i, j].type == Cell.Type.Bomb)
                {
                    state[i, j].flagged = true;
                }
            }
        }

        remainingBombs = 0;
        UpdateBombCounter();

        Debug.Log("You win!");

        winText.gameObject.SetActive(true);

        Invoke("ShowSetupPanelDelayed", 5f);
    }

    public void UpdateBombCounter()
    {
        bombCounterText.text = "Mines: " + remainingBombs;
    }
}
