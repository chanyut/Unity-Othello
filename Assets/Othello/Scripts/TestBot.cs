using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TestBot : Bot {


    public override string BotName
    {
        get
        {
            return "Test";
        }
    }

    public override void OnEnemyTurn(int enemyRow, int enemyColumn)
    {
    }

    private int player;
    public override void OnGameStart(int playerIndex)
    {
        player = playerIndex;
        /// board available!
    }

    public override void PlayTurn(out int row, out int column)
    {
        List<Vector2> marks = new List<Vector2>();
        for(int r=0; r<board.NumberOfRows; r++){
            for(int c=0; c<board.NumberOfColumns; c++){
                if (!board.CanPutMarkAt(player, r, c)) continue;
                marks.Add(new Vector2(c * 100, r * 100)); 
            }
        }


        Vector2 result = marks[Random.Range(0, marks.Count)];
        row = Mathf.FloorToInt(result.y) / 100;
        column = Mathf.FloorToInt(result.x) / 100;
    }
}
