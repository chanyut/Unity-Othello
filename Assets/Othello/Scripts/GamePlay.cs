using UnityEngine;
using System.Collections;

public class GamePlay : MonoBehaviour 
{
    public const int PLAYER_INDEX_P1 = 1;
    public const int PLAYER_INDEX_P2 = 2;

    public Camera GameCamera;
    public Board GameBoard;
    public int CurrentPlayerIndex;

    void Start()
    {
        CurrentPlayerIndex = PLAYER_INDEX_P1;
        StartGame();
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
