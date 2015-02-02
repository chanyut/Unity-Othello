using UnityEngine;
using System.Collections;

public class GamePlay : MonoBehaviour 
{
    public const int PLAYER_INDEX_P1 = 1;
    public const int PLAYER_INDEX_P2 = 2;

    public Camera GameCamera;
    public Board GameBoard;
    public int CurrentPlayerIndex;

    public Bot BotBlack;
    public Bot BotWhite;

    void Start()
    {
        CurrentPlayerIndex = PLAYER_INDEX_P1;
        StartGame();

        StartCoroutine(StartBotGame(BotBlack, BotWhite));
    }


    IEnumerator StartBotGame(Bot botA, Bot botB) {

        string playerAName = "Human1";
        string playerBName = "Human2";
        if (botA) 
        {
            playerAName = botA.gameObject.name;
            botA.board = GameBoard;
            botA.OnGameStart(PLAYER_INDEX_P1);
        }
        if (botB)
        {
            playerBName = botB.gameObject.name;
            botB.board = GameBoard;
            botB.OnGameStart(PLAYER_INDEX_P2);
        }

        int row = 0;
        int column = 0;
        while (true) {

            bool gameover = true;
            if (GameBoard.CanPutMark(PLAYER_INDEX_P1))
            {
                gameover = false;

                if (botA)
                {
                    Debug.Log(string.Format("{0} Turn", playerAName));
                    
                    botA.PlayTurn(out row, out column);
                    if (!GameBoard.PutMark(PLAYER_INDEX_P1, row, column))
                    {
                        Debug.Log(string.Format("{0} Foul! {1},{2}", playerAName, row, column));
                        yield break;
                    }

                }
                else {
                    while (true)
                    {
                        Debug.Log(string.Format("{0} Turn", playerAName));

                        yield return StartCoroutine(WaitForInput());
                        row = lastInputRow;
                        column = lastInputColumn;
                        if (!GameBoard.PutMark(PLAYER_INDEX_P1, row, column)) continue;
                        break;
                    }
                }

                yield return new WaitForSeconds(1);


                if (botB) {
                    botB.OnEnemyTurn(row, column);
                }
            }

            if (GameBoard.CanPutMark(PLAYER_INDEX_P2))
            {
                gameover = false;

                if (botB)
                {
                    Debug.Log(string.Format("{0} Turn", playerBName));

                    botB.PlayTurn(out row, out column);
                    if (!GameBoard.PutMark(PLAYER_INDEX_P2, row, column))
                    {
                        Debug.Log(string.Format("{0} Foul! {1},{2}", playerBName, row, column));
                        yield break;
                    }
                }
                else {

                    while (true)
                    {
                        Debug.Log(string.Format("{0} Turn", playerBName));

                        yield return StartCoroutine(WaitForInput());

                        row = lastInputRow;
                        column = lastInputColumn;
                        if (!GameBoard.PutMark(PLAYER_INDEX_P2, row, column)) continue;
                        break;
                    }
                }

                yield return new WaitForSeconds(1);

                if (botA)
                {
                    botA.OnEnemyTurn(row, column);
                }
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

        Debug.Log(string.Format("{0} score: {1}", playerAName, p1Score));
        Debug.Log(string.Format("{0} score: {1}", playerBName, p2Score));
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


    private int lastInputRow;
    private int lastInputColumn;

    IEnumerator WaitForInput() {

        while (true) {

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

                        lastInputRow = row;
                        lastInputColumn = col;
                        yield break;
                    }
                }

            }
            yield return null;
        }
    }

}
