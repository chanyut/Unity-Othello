using UnityEngine;
using System.Collections;
using System;
using ProgrammerBird;

namespace ProgrammerBird {


    public static class BitBoard {

        public static UInt64 Mask(int row, int column) {
            int pk = (row << 3 | column);
            return ((UInt64)1) << pk;
        }

        public static bool GetValue(UInt64 board, UInt64 mask) {
            return (board & mask) == mask;
        }

        public static void SetValue(ref UInt64 board, UInt64 mask, bool value) {
            if (value) {
                board = (board | mask);
            }
            else {
                board = (board & ~mask);
            }
        }
    }

    public class GameContext {
        public int rows = 8;
        public int columns = 8;
    }

    public class GameBoard {
        private UInt64 _hasColorBoard;
        private UInt64 _colorBoard;

        public const int CellEmpty = 0;
        public const int CellBlack = 1;
        public const int CellWhite = 2;

        public GameBoard() {
            _hasColorBoard = 0;
            _colorBoard = 0;
        }

        public void WriteData(out UInt64 a, out UInt64 b) {
            a = _hasColorBoard;
            b = _colorBoard;
        }

        public void RestoreData(UInt64 a, UInt64 b) {
            _hasColorBoard = a;
            _colorBoard = b;
        }

        public int GetCell(int row, int column)
        {
            UInt64 mask = BitBoard.Mask(row, column);
            if (BitBoard.GetValue(_hasColorBoard, mask))
            {
                if (BitBoard.GetValue(_colorBoard, mask))
                {
                    return CellWhite;
                }
                else
                {
                    return CellBlack;
                }
            }
            else
            {
                return CellEmpty;
            }
        }

        public void SetCell(int row, int column, int cellValue) {
            UInt64 mask = BitBoard.Mask(row, column);
            if (cellValue == CellEmpty) {
                BitBoard.SetValue(ref _hasColorBoard, mask, false);
                BitBoard.SetValue(ref _colorBoard, mask, false);
            }
            else if (cellValue == CellBlack)
            {
                BitBoard.SetValue(ref _hasColorBoard, mask, true);
                BitBoard.SetValue(ref _colorBoard, mask, false);
            }
            else if (cellValue == CellWhite)
            {
                BitBoard.SetValue(ref _hasColorBoard, mask, true);
                BitBoard.SetValue(ref _colorBoard, mask, true);
            }
        }

        /// <summary>
        /// +1 for every white
        /// -1 for every black
        /// </summary>
        /// <returns></returns>
        public int GetBoardScore(GameContext context) {

            int totalScore = 0;
            int totalExtraScore = 0;

            bool hasEmptyCell = false;
            int edgeRows = context.rows - 1;
            int edgeColumns = context.columns - 1;
            for (int r=0; r<context.rows; r++){
                for(int c=0; c<context.columns; c++){
                    UInt64 mask = BitBoard.Mask(r, c);
                    if (!BitBoard.GetValue(_hasColorBoard, mask)) {
                        hasEmptyCell = true;
                        continue;
                    }
                    int baseScore = 1;
                    int extraScore = 0;
                    if (r == 0 || r == edgeRows) {
                        extraScore += 5;
                    }
                    if (c == 0 || c == edgeColumns) {
                        extraScore += 5;
                    }
                    if (BitBoard.GetValue(_colorBoard, mask))
                    {
                        totalExtraScore += extraScore;
                        totalScore += baseScore;
                    }
                    else
                    {
                        totalExtraScore -= extraScore;
                        totalScore -= baseScore;
                    }
                }
            }

            if (hasEmptyCell) {
                return totalScore + totalExtraScore;
            }
            return totalScore;
        }


        public bool GetNeighbour(GameContext context, int direction, ref int row, ref int column) 
        {

            switch (direction) {
                case 0: {
                    row -= 1;
                    break;
                }
                case 1: {
                    row += 1;
                    break;
                }
                case 2: {
                    column -= 1;
                    break;
                }
                case 3: {
                    column += 1;
                    break;
                }

                case 4: {
                    row -= 1;
                    column -= 1;
                    break;
                }
                case 5: {
                    row -= 1;
                    column += 1;
                    break;
                }
                case 6: {
                    row += 1;
                    column -= 1;
                    break;
                }
                case 7: {
                    row += 1;
                    column += 1;
                    break;
                }
            }

            if (row < 0) return false;
            if (column < 0) return false;

            if (row >= context.rows) return false;
            if (column >= context.columns) return false;

            return true;
        }

        public bool TryPlace(GameContext context, int row, int column, int colorCell) {
            UInt64 mask = BitBoard.Mask(row, column);
            if (BitBoard.GetValue(_hasColorBoard, mask)) return false;

            bool canPlace = false;
            for (int direction = 0; direction < 8; direction++) {
                int neighbourRow = row;
                int neighbourColumn = column;
                if (!GetNeighbour(context, direction, ref neighbourRow, ref neighbourColumn)) continue;

                int neighbourCell = GetCell(neighbourRow, neighbourColumn);
                if (neighbourCell == CellEmpty) continue;
                if (neighbourCell == colorCell) continue; // friendly units


                int cursorRow = neighbourRow;
                int cursorColumn = neighbourColumn;
                bool success = false;
                while (true) {
                    if (!GetNeighbour(context, direction, ref cursorRow, ref cursorColumn)) break;

                    UInt64 cursorMask = BitBoard.Mask(cursorRow, cursorColumn);
                    int cursorCell = GetCell(cursorRow, cursorColumn);
                    if (cursorCell == CellEmpty) break;
                    if (cursorCell != colorCell) continue;

                    /// found closing 
                    success = true;
                    break;
                }

                if (success) {
                    canPlace = true;

                    cursorRow = neighbourRow;
                    cursorColumn = neighbourColumn;
                    SetCell(cursorRow, cursorColumn, colorCell);

                    while (true)
                    {
                        if (!GetNeighbour(context, direction, ref cursorRow, ref cursorColumn)) break;

                        UInt64 cursorMask = BitBoard.Mask(cursorRow, cursorColumn);
                        int cursorCell = GetCell(cursorRow, cursorColumn);
                        if (cursorCell == CellEmpty) break;
                        if (cursorCell != colorCell) {
                            SetCell(cursorRow, cursorColumn, colorCell);
                            continue;
                        }
                        break;
                    }

                }
            }

            if (canPlace) {
                SetCell(row, column, colorCell);
                return true;
            }
            return false;
        }

        public void Log(GameContext context) {


            string im = "";
            for (int c = 0; c < context.columns; c++)
            {
                for (int r = context.rows - 1; r>=0 ; r--)
                {
                    int cell = GetCell(r, c);
                    if (cell == GameBoard.CellBlack)
                    {
                        im += "x";
                    }
                    else if (cell == GameBoard.CellWhite)
                    {
                        im += "o";
                    }
                    else
                    {
                        im += ".";
                    }
                }

                im += "\n";
            }

            Debug.Log(im);
        }

    }


    public static class AI {

        public static int PlayTurn(GameContext context, GameBoard board, int colorCell, int level, out int outRow, out int outColumn)
        {

            if (level <= 0)
            {
                outRow = 0;
                outColumn = 0;
                return board.GetBoardScore(context);
            }

            UInt64 saveA;
            UInt64 saveB;
            board.WriteData(out saveA, out saveB);

            int bestRow = 0;
            int bestColumn = 0;
            int bestScore, enemyCell;
            if (colorCell == GameBoard.CellWhite)
            {
                bestScore = int.MinValue;
                enemyCell = GameBoard.CellBlack;
            }
            else
            {
                bestScore = int.MaxValue;
                enemyCell = GameBoard.CellWhite;
            }

            bool canPlay = false;
            for (int r = 0; r < context.rows; r++)
            {
                for (int c = 0; c < context.columns; c++)
                {
                    if (!board.TryPlace(context, r, c, colorCell)) continue;

                    canPlay = true;
                    int tmpRow, tmpColumn;
                    int score = PlayTurn(context, board, enemyCell, level - 1, out tmpRow, out tmpColumn);
                    if (colorCell == GameBoard.CellWhite)
                    {
                        if (bestScore < score)
                        {
                            bestScore = score;
                            bestRow = r;
                            bestColumn = c;
                        }
                    }
                    else
                    {
                        /// colorCell = CellBlack
                        if (bestScore > score)
                        {
                            bestScore = score;
                            bestRow = r;
                            bestColumn = c;
                        }
                    }

                    // restore
                    board.RestoreData(saveA, saveB);
                }
            }

            if (!canPlay)
            {
                outRow = 0;
                outColumn = 0;
                return board.GetBoardScore(context);
            }

            outRow = bestRow;
            outColumn = bestColumn;
            return bestScore;
        }
    }
}


public class BirdBot : Bot {

    public bool debug = false;
    private int _player;
    private GameContext _context;
    private GameBoard _gameBoard;

    public int cleverness = 5;
    public override void OnGameStart(int playerIndex)
    {

        _gameBoard = new GameBoard();
        _context = new GameContext();
        _context.rows = Mathf.Min(8, board.NumberOfRows);
        _context.columns = Mathf.Min(8, board.NumberOfColumns);

        _player = playerIndex;

        base.OnGameStart(playerIndex);
    }

    private void ReloadGameBoard() {

        for (int r = 0; r < _context.rows; r++)
        {
            for (int c = 0; c < _context.columns; c++)
            {

                Mark mark = board.Grids[r, c];
                if (mark == null) 
                {
                    _gameBoard.SetCell(r, c, GameBoard.CellEmpty);
                }
                else if (mark.PlayerIndex == GamePlay.PLAYER_INDEX_P1)
                {
                    _gameBoard.SetCell(r, c, GameBoard.CellBlack);
                }
                else if (mark.PlayerIndex == GamePlay.PLAYER_INDEX_P2)
                {
                    _gameBoard.SetCell(r, c, GameBoard.CellWhite);
                }
            }
        }
    }

    public override void PlayTurn(out int row, out int column)
    {
        ReloadGameBoard();
        if(debug) _gameBoard.Log(_context);

        int colorCell = (_player == GamePlay.PLAYER_INDEX_P1) ? GameBoard.CellBlack : GameBoard.CellWhite;
        int score = AI.PlayTurn(_context, _gameBoard, colorCell, cleverness, out row, out column);

        if (colorCell == GameBoard.CellBlack)
        {
            score = -score;
        }
        Debug.Log(string.Format("{0} confident: {1}", gameObject.name, score));
    }

}
