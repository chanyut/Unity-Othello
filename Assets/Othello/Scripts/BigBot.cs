using UnityEngine;

using System.Collections;
using System.Collections.Generic;

using ChanyutLeecharoen;

public class BigBot : Bot {
    private LeeAI mAI;

    public string DebugLogicalBoard;

    public override string BotName {
        get {
            return "Othello King";
        }
    }

    public override void OnEnemyTurn(int enemyRow, int enemyColumn) {
        
    }

    private int player;
    public override void OnGameStart(int playerIndex) {
        player = playerIndex;

        mAI = new LeeAI();
        mAI.Initial(board, playerIndex, LeeAI.AILevelEnum.Weakest);
    }

    public override void PlayTurn(out int row, out int column) {

        int r = 0;
        int c = 0;

        mAI.UpdateLogcialBoard(board);
        if (mAI.GetTheBestMove(out r, out c)) {
            row = r;
            column = c;
        }
        else {
            row = int.MinValue;
            column = int.MinValue;
        }
    }
}
