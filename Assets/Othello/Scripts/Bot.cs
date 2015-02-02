using UnityEngine;
using System.Collections;

public class Bot: MonoBehaviour {


    public Board board;

    public virtual string BotName {
        get {
            return "";
        }
    }

    public virtual void OnGameStart(int playerIndex) { 
    
    }
    public virtual void OnEnemyTurn(int enemyRow, int enemyColumn) { 
    }

    public virtual void PlayTurn(out int row, out int column) {
        row = 0;
        column = 0;
    }

}