using UnityEngine;
using System.Collections;

public class GamePlay : MonoBehaviour 
{
    public const int PLAYER_INDEX_P1 = 1;
    public const int PLAYER_INDEX_P2 = 2;

    public Camera GameCamera;
    public Board GameBoard;
    public int CurrentPlayerIndex;

    public Bot BotBig;
    public Bot BotBird;

    void Start()
    {
        CurrentPlayerIndex = PLAYER_INDEX_P1;
        StartGame();

        StartCoroutine(StartBotGame(BotBig, BotBird));
    }


    IEnumerator StartBotGame(Bot botA, Bot botB) {

        botA.board = GameBoard;
        botB.board = GameBoard;

        botA.OnGameStart(PLAYER_INDEX_P1);
        botB.OnGameStart(PLAYER_INDEX_P2);

        int row = 0;
        int column = 0;
        while (true) {

            bool gameover = true;
            if (GameBoard.CanPutMark(PLAYER_INDEX_P1))
            {
                gameover = false;
                botA.PlayTurn(out row, out column);
                if (!GameBoard.PutMark(PLAYER_INDEX_P1, row, column))
                {
                    Debug.Log(string.Format("{0} Foul! {1},{2}", botA.gameObject.name, row, column));
                    yield break;
                }
                yield return new WaitForSeconds(1);
                botB.OnEnemyTurn(row, column);
            }

            if (GameBoard.CanPutMark(PLAYER_INDEX_P2)) 
            {
                gameover = false;
                botB.PlayTurn(out row, out column);
                if (!GameBoard.PutMark(PLAYER_INDEX_P2, row, column))
                {
                    Debug.Log(string.Format("{0} Foul! {1},{2}", botB.gameObject.name, row, column));
                    yield break;
                }
                yield return new WaitForSeconds(1);
                botA.OnEnemyTurn(row, column);
            }

            if (gameover) break;
        }

        int p1Score = 0;
        int p2Score = 0;
        for (int r = 0; r < GameBoard.NumberOfRows; r++)
        {
            for (int c = 0; c < GameBoard.NumberOfColumns; c++)
            {
                Mark m = GameBoard.Grids[c, r];
                if (m == null) {
                    continue;
                }
                if (m.PlayerIndex == PLAYER_INDEX_P1) p1Score++;
                if (m.PlayerIndex == PLAYER_INDEX_P2) p2Score++;
            }
        }

        Debug.Log(string.Format("{0} score: {1}", botA.gameObject.name, p1Score));
        Debug.Log(string.Format("{0} score: {1}", botB.gameObject.name, p2Score));
    }

    void StartGame()
    {
        int row = GameBoard.NumberOfRows;
        int col = GameBoard.NumberOfColumns;

        GameBoard.PutMark(PLAYER_INDEX_P1, (row / 2) - 1, (col / 2) - 1, force: true);
        GameBoard.PutMark(PLAYER_INDEX_P2, (row / 2) - 1, (col / 2), force: true);
        GameBoard.PutMark(PLAYER_INDEX_P2, (row / 2), (col / 2) - 1, force: true);
        GameBoard.PutMark(PLAYER_INDEX_P1, (row / 2), (col / 2), force: true);
    }

    public void SwitchPlayerTurn()
    {
        if (CurrentPlayerIndex == PLAYER_INDEX_P1)
        {
            CurrentPlayerIndex = PLAYER_INDEX_P2;
        }
        else if (CurrentPlayerIndex == PLAYER_INDEX_P2)
        {
            CurrentPlayerIndex = PLAYER_INDEX_P1;
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray inputRay = GameCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;
            if (Physics.Raycast(inputRay, out hitInfo, GameCamera.farClipPlane))
            {
                if (hitInfo.collider.gameObject.tag == "Board")
                {
                    Debug.Log("hit board at point: " + hitInfo.point);
                    Vector3 point = hitInfo.point;
                    Vector2 cellSize = GameBoard.GetBoardCellSizeInWorldCoord();
                    int row = Mathf.FloorToInt(point.z / cellSize.y);
                    int col = Mathf.FloorToInt(point.x / cellSize.x);
                    GameBoard.PutMark(CurrentPlayerIndex, row, col);
                    SwitchPlayerTurn();
                }
            }
        }
    }
}
