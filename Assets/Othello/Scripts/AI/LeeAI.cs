//#define VERBOSE

using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;


namespace ChanyutLeecharoen {

    public class LeeAI {

        public enum AILevelEnum { 
            Weakest,
            Weak
        }

        private struct Vector2i {
            public int X;
            public int Y;

            public Vector2i(int x, int y) {
                X = x; 
                Y = y;
            }
        }

        private int PlayerIndex;
        private int[,] mLogicalBoard;
        private int[,] mBoardValueMatrix; // row, col
        private Vector2i mLogicalBoardSize;
        private AILevelEnum mAILevel;

        public void Initial(Board board, int playerIndex, AILevelEnum aiLevel) {
            mAILevel = aiLevel;
            PlayerIndex = playerIndex;
            mLogicalBoardSize = new Vector2i(board.NumberOfColumns, board.NumberOfRows);
            mLogicalBoard = new int[mLogicalBoardSize.X, mLogicalBoardSize.Y];
            for (int r = 0; r < mLogicalBoardSize.Y; r++) {
                for (int c = 0; c < mLogicalBoardSize.X; c++) {
                    mLogicalBoard[r, c] = -1;
                }
            }
            CreateBoardValueMatrix();
        }

        void CreateBoardValueMatrix() {
            int[,] m = new int[8, 8];

            m[0, 0] = 100;
            m[0, 1] = -1;
            m[0, 2] = 5;
            m[0, 3] = 2;
            
            m[1, 0] = -1;
            m[1, 1] = -20; //-10
            m[1, 2] = 1;
            m[1, 3] = 1;
            
            m[2, 0] = 5;
            m[2, 1] = 1;
            m[2, 2] = 1;
            m[2, 3] = 1;

            m[3, 0] = 2;
            m[3, 1] = 1;
            m[3, 2] = 1;
            m[3, 3] = 0;
            
            // build bottom rigth quarter
            for (int r = 0; r < 4; r++) {
                for (int c = 0; c < 4; c++) {
                    m[r, c + 4] = m[r, 3 - c];
                }    
            }

            // build top left quarter
            for (int c = 0; c < 4; c++) {
                for (int r = 0; r < 4; r++) {
                    m[r + 4, c] = m[c, 3 - r];
                }
            }

            // build top right quarter
            for (int r = 4; r < 8; r++) {
                for (int c = 0; c < 4; c++) {
                    m[r, c + 4] = m[r, 3 - c];
                }
            }

#if VERBOSE
            string b = "";
            for (int r = 7; r >= 0; r--) {
                for (int c = 0; c < 8; c++) {
                    b += "\t" + m[r, c];
                }
                b += "\n";
            }
            Debug.Log(b);
#endif

            mBoardValueMatrix = m;
        }

        public void UpdateLogcialBoard(Board board) { 
            UpdateLogcialBoard(board, mLogicalBoard);
        }

        public void UpdateLogcialBoard(Board board, int[,] logicalBoard) {
            for (int r = 0; r < mLogicalBoardSize.Y; r++) {
                for (int c = 0; c < mLogicalBoardSize.X; c++) {
                    if (board.Grids[r, c] != null) {
                        logicalBoard[r, c] = board.Grids[r, c].PlayerIndex;
                    }
                    else {
                        logicalBoard[r, c] = -1;
                    }
                }
            }

#if VERBOSE
            Debug.Log(GetLogicalBoardAsString());
#endif
        }

        public int[,] CopyCurrentLogicBoard(int[,] original) { 
            int[,] copied = new int[mLogicalBoardSize.X, mLogicalBoardSize.Y];
            for (int r = 0; r < mLogicalBoardSize.Y; r++) {
                for (int c = 0; c < mLogicalBoardSize.X; c++) {
                    copied[r, c] = original[r, c];
                }
            }

            return copied;
        }

        public string GetLogicalBoardAsString(int[,] logicalBoard) {
            string b = "";
            for (int r = mLogicalBoardSize.Y - 1; r >= 0; r--) {
                for (int c = 0; c < mLogicalBoardSize.X; c++) {
                    b += "\t" + (logicalBoard[r, c] < 0 ? 0 : logicalBoard[r, c]);
                }
                b += "\n";
            }
            return b;
        }

        bool GetTheBestMoveAndScore(int[,] logicalBoard, out int row, out int col, out int score) {
            List<Vector2i> l = GetAllPossibleMovesFor(logicalBoard, PlayerIndex);
            if (l.Count > 0) {
                int selectedIndex = 0;
                int maxScore = int.MinValue;

                for (int i = 0; i < l.Count; i++) {
                    int r = l[i].Y;
                    int c = l[i].X;
                    int s = mBoardValueMatrix[r, c];
                    if (s >= maxScore) {
                        selectedIndex = i;
                        maxScore = s;
                    }
                }

                row = l[selectedIndex].Y;
                col = l[selectedIndex].X;
                score = maxScore;
                return true;
            }

            // Unable to find any move
            row = 0;
            col = 0;
            score = int.MinValue;
            return false;
        }

        public bool GetTheBestMove(out int row, out int col) {
            if (GetTheBestMove_ByDecissionTree(out row, out col)) {
                return true;
            }
            else {
                int score = 0;
                return GetTheBestMoveAndScore(mLogicalBoard, out row, out col, out score);
            }

            //int score = 0;
            //return GetTheBestMoveAndScore(mLogicalBoard, out row, out col, out score);
        }

        bool GetTheBestMove_ByDecissionTree(out int row, out int col) {
            DecissionTree tree = CreateDecissionTree(1);
            List<DecissionTreeNode> oppnNodes = new List<DecissionTreeNode>();
            for (int i = 0; i < tree.Root.Children.Count; i++) {
                for (int j = 0; j < tree.Root.Children[i].Children.Count; j++) {
                    oppnNodes.Add(tree.Root.Children[i].Children[j]);
                }
            }

            int minScore = int.MaxValue;
            int index = -1;
            for (int i = 0; i < oppnNodes.Count; i++) {
                int score = 0;
                int r = 0;
                int c = 0;
                if (GetTheBestMoveAndScore(oppnNodes[i].LogicBoard, out r, out c, out score)) {
                    if (score < minScore) {
                        minScore = score;
                        index = i;
                    }
                }
            }

            if (index == -1) {
                row = 0;
                col = 0;
                return false;
            }

            DecissionTreeNode playerAdventageNode = oppnNodes[index].Parent;
            int[,] desiredLogicalBoard = playerAdventageNode.LogicBoard;

#if VERBOSE
            Debug.Log(GetLogicalBoardAsString(mLogicalBoard) + "\n\n" + GetLogicalBoardAsString(desiredLogicalBoard));
#endif
            for (int r = 0; r < mLogicalBoardSize.Y; r++) {
                for (int c = 0; c < mLogicalBoardSize.X; c++) {
                    if (mLogicalBoard[r, c] != desiredLogicalBoard[r, c]) {
                        row = r;
                        col = c;
                        return true;
                    }
                }
            }

            row = 0;
            col = 0;
            return false;
        }

        List<Vector2i> GetAllPossibleMovesFor(int[,] logicalBoard, int playerIndex) {
            List<Vector2i> l = new List<Vector2i>();
            for (int r = 0; r < mLogicalBoardSize.Y; r++) {
                for (int c = 0; c < mLogicalBoardSize.X; c++) {
                    var m = GetBeatenMarkIfPutMarkAt(logicalBoard, playerIndex, r, c);
                    if (m.Count > 0) {
                        l.Add(new Vector2i(c, r));
                    }
                }
            }
            return l;
        }

        DecissionTree CreateDecissionTree(int maxLevel) {
            DecissionTree tree = new DecissionTree();
            DecissionTreeNode root = new DecissionTreeNode();
            root.Parent = null;
            root.LogicBoard = CopyCurrentLogicBoard(mLogicalBoard);
            root.PlayerIndexTurn = GetOppositePlayerIndex(PlayerIndex);

            DecissionTreeNode currentNode = root;
            List<DecissionTreeNode> allNodesInLevel = new List<DecissionTreeNode>();
            allNodesInLevel.Add(root);
            for (int l = 0; l < maxLevel * 2; l++) {
                List<DecissionTreeNode> allChildrenNodesofThisLevel = new List<DecissionTreeNode>();
                for (int n_index = 0; n_index < allNodesInLevel.Count; n_index++) {
                    AppendAllChildrenForNode(allNodesInLevel[n_index]);
                    allChildrenNodesofThisLevel.AddRange(allNodesInLevel[n_index].Children);
                }
                if (allChildrenNodesofThisLevel.Count == 0) {
                    break;
                }
                allNodesInLevel = allChildrenNodesofThisLevel;
            }

            tree.Root = root;
            return tree;
        }

        void AppendAllChildrenForNode(DecissionTreeNode node) {
            node.Children = new List<DecissionTreeNode>();
            int p_index = GetOppositePlayerIndex(node.PlayerIndexTurn);
            var l = GetAllPossibleMovesFor(node.LogicBoard, p_index);

            for (int i = 0; i < l.Count; i++) {
                int r = l[i].Y;
                int c = l[i].X;

                DecissionTreeNode n = new DecissionTreeNode();
                n.Parent = node;
                n.LogicBoard = CopyCurrentLogicBoard(node.LogicBoard);
                n.LogicBoard[r, c] = p_index;
                n.PlayerIndexTurn = p_index;

                node.Children.Add(n);
            }
        }

        int GetOppositePlayerIndex(int playerIndex) {
            int oppositePlayerIndex = 0;
            if (playerIndex == GamePlay.PLAYER_INDEX_P1) oppositePlayerIndex = GamePlay.PLAYER_INDEX_P2;
            else if (playerIndex == GamePlay.PLAYER_INDEX_P2) oppositePlayerIndex = GamePlay.PLAYER_INDEX_P1;
            return oppositePlayerIndex;
        }

        List<Vector2i> GetBeatenMarkIfPutMarkAt(int[,] logicalBoard, int currentPlayerIndex, int row, int col) {
            List<Vector2i> allMarks = new List<Vector2i>();
            List<Vector2i> temp = new List<Vector2i>();
            int r = 0;
            int c = 0;
            bool canClose = false;
            int m = -1;

            if (logicalBoard[row, col] != -1) {
                return allMarks;
            }

            // North
            r = row;
            c = col;
            r++;
            canClose = false;
            temp.Clear();
            while (true) {
                if (r >= mLogicalBoardSize.Y) {
                    break;
                }

                m = logicalBoard[r, c];
                if (m == -1) {
                    break;
                }
                else if (m == currentPlayerIndex) {
                    canClose = true;
                    break;
                }
                else {
                    temp.Add(new Vector2i(c, r));
                    r++;
                }
            }
            if (canClose) {
                allMarks.AddRange(temp);
            }

            // South
            r = row;
            c = col;
            r--;
            canClose = false;
            temp.Clear();
            m = -1;
            while (true) {
                if (r < 0) {
                    break;
                }

                m = logicalBoard[r, c];
                if (m == -1) {
                    break;
                }
                else if (m == currentPlayerIndex) {
                    canClose = true;
                    break;
                }
                else {
                    temp.Add(new Vector2i(c, r));
                    r--;
                }
            }
            if (canClose) {
                allMarks.AddRange(temp);
            }

            // East
            r = row;
            c = col;
            c++;
            canClose = false;
            temp.Clear();
            while (true) {
                if (c >= mLogicalBoardSize.X) {
                    break;
                }

                m = logicalBoard[r, c];
                if (m == -1) {
                    break;
                }
                else if (m == currentPlayerIndex) {
                    canClose = true;
                    break;
                }
                else {
                    temp.Add(new Vector2i(c, r));
                    c++;
                }
            }
            if (canClose) {
                allMarks.AddRange(temp);
            }

            // West
            r = row;
            c = col;
            c--;
            canClose = false;
            temp.Clear();
            m = -1;
            while (true) {
                if (c < 0) {
                    break;
                }

                m = logicalBoard[r, c];
                if (m == -1) {
                    break;
                }
                else if (m == currentPlayerIndex) {
                    canClose = true;
                    break;
                }
                else {
                    temp.Add(new Vector2i(c, r));
                    c--;
                }
            }
            if (canClose) {
                allMarks.AddRange(temp);
            }

            // North-East
            r = row;
            c = col;
            r++;
            c++;
            canClose = false;
            temp.Clear();
            m = -1;
            while (true) {
                if (r >= mLogicalBoardSize.Y || c >= mLogicalBoardSize.X) {
                    break;
                }

                m = logicalBoard[r, c];
                if (m == -1) {
                    break;
                }
                else if (m == currentPlayerIndex) {
                    canClose = true;
                    break;
                }
                else {
                    temp.Add(new Vector2i(c, r));
                    r++;
                    c++;
                }
            }
            if (canClose) {
                allMarks.AddRange(temp);
            }

            // North-West
            r = row;
            c = col;
            r++;
            c--;
            canClose = false;
            temp.Clear();
            while (true) {
                if (r >= mLogicalBoardSize.Y || c < 0) {
                    break;
                }

                m = logicalBoard[r, c];
                if (m == -1) {
                    break;
                }
                else if (m == currentPlayerIndex) {
                    canClose = true;
                    break;
                }
                else {
                    temp.Add(new Vector2i(c, r));
                    r++;
                    c--;
                }
            }
            if (canClose) {
                allMarks.AddRange(temp);
            }

            // South-East
            r = row;
            c = col;
            r--;
            c++;
            canClose = false;
            temp.Clear();
            m = -1;
            while (true) {
                if (r < 0 || c >= mLogicalBoardSize.X) {
                    break;
                }

                m = logicalBoard[r, c];
                if (m == -1) {
                    break;
                }
                else if (m == currentPlayerIndex) {
                    canClose = true;
                    break;
                }
                else {
                    temp.Add(new Vector2i(c, r));
                    r--;
                    c++;
                }
            }
            if (canClose) {
                allMarks.AddRange(temp);
            }

            // South-West
            r = row;
            c = col;
            r--;
            c--;
            canClose = false;
            temp.Clear();
            while (true) {
                if (r < 0 || c < 0) {
                    break;
                }

                m = logicalBoard[r, c];
                if (m == -1) {
                    break;
                }
                else if (m == currentPlayerIndex) {
                    canClose = true;
                    break;
                }
                else {
                    temp.Add(new Vector2i(c, r));
                    r--;
                    c--;
                }
            }
            if (canClose) {
                allMarks.AddRange(temp);
            }

            return allMarks;
        }
    }

    public class DecissionTree {
        public DecissionTreeNode Root;
    }

    public class DecissionTreeNode {
        public DecissionTreeNode Parent;
        public List<DecissionTreeNode> Children;

        public int PlayerIndexTurn;
        public int[,] LogicBoard;
    }
}