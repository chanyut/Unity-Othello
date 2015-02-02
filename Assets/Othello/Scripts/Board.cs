using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Board : MonoBehaviour
{
    public int NumberOfRows;
    public int NumberOfColumns;
    
    public Mark[,] Grids;
    public GameObject MarkPrefab;
    public Transform MarksContainer;

    private BoxCollider mBoardCollider;

    void Awake()
    {
        Grids = new Mark[NumberOfColumns, NumberOfRows];
        for (int r = 0; r < NumberOfRows; r++)
        {
            for (int c = 0; c < NumberOfColumns; c++)
            {
                Grids[c, r] = null;
            }
        }
        mBoardCollider = GetComponent<BoxCollider>();
    }

    public bool PutMark(int playerIndex, int row, int col, bool force=false)
    {
        if (Grids[row, col] != null)
        {
            return false;
        }

        Mark[] beatenMarks = null;
        if (!force)
        {
            beatenMarks = GetBeatenMarkIfPutMarkAt(playerIndex, row, col);
            if (beatenMarks.Length == 0)
            {
                return false;
            }
        }

        Vector2 cellSize = GetBoardCellSizeInWorldCoord();
        var newMarkGameObject = Instantiate(MarkPrefab, 
            new Vector3(col + 0.5f, 0, row + 0.5f), Quaternion.identity) as GameObject;
        newMarkGameObject.transform.parent = MarksContainer;
        newMarkGameObject.transform.localScale = new Vector3(cellSize.x, 2, cellSize.y);

        Mark newMark = newMarkGameObject.GetComponent<Mark>();
        newMark.PlayerIndex = playerIndex;
        newMark.RefreshVisual();

        Grids[row, col] = newMark;

        if (beatenMarks != null)
        {
            for (int i = 0; i < beatenMarks.Length; i++)
            {
                beatenMarks[i].Flip();
            }
        }
        return true;
    }

    private IEnumerator FlipAllBeatenMarks(Mark[] marks)
    {
        for (int i = 0; i < marks.Length; i++)
        {
            marks[i].Flip();
            yield return null;
            // yield return new WaitForSeconds(0.05f);
        }
    }

    public Vector2 GetBoardSizeInWorldCoord()
    {
        if (mBoardCollider == null)
        {
            mBoardCollider = GetComponent<BoxCollider>();
        }

        float w = mBoardCollider.size.x * transform.localScale.x;
        float h = mBoardCollider.size.z * transform.localScale.z;
        return new Vector2(w, h);
    }

    public Vector2 GetBoardCellSizeInWorldCoord()
    {
        Vector2 boardSize = GetBoardSizeInWorldCoord();
        Vector2 cellSize = new Vector2(boardSize.x / NumberOfColumns,
            boardSize.y / NumberOfRows);
        return cellSize;
    }

    void OnDrawGizmos()
    {
        Vector2 boardSize = GetBoardSizeInWorldCoord();
        Vector2 cellSize = GetBoardCellSizeInWorldCoord();

        // Horizontal lines
        for (int r = 0; r < NumberOfRows; r++)
        {
            Vector3 p1= new Vector3(transform.position.x, 0, r * cellSize.y);
            Vector3 p2 = new Vector3(transform.position.x + boardSize.x, 0, r * cellSize.y);
            Gizmos.DrawLine(p1, p2);
        }

        // Vertical lines
        for (int c = 0; c < NumberOfColumns; c++)
        {
            Vector3 p1 = new Vector3(transform.position.x + (c * cellSize.x), 0, transform.position.z);
            Vector3 p2 = new Vector3(transform.position.x + (c * cellSize.x), 0, transform.position.z + boardSize.y);
            Gizmos.DrawLine(p1, p2);
        }
    }

    public bool CanPutMarkAt(int currentPlayerIndex, int row, int col) {
        if (Grids[row, col] != null) return false;
        Mark[] marks = GetBeatenMarkIfPutMarkAt(currentPlayerIndex, row, col);
        if (marks.Length > 0) {
            return true;
        }
        return false;
    }

    public bool CanPutMark(int currentPlayerIndex) {
        for (int r = 0; r < NumberOfRows; r++)
        {
            for (int c = 0; c < NumberOfColumns; c++)
            {
                if (CanPutMarkAt(currentPlayerIndex, r, c)) return true;
            }
        }
        return false;
    }

    Mark[] GetBeatenMarkIfPutMarkAt(int currentPlayerIndex, int row, int col)
    {
        List<Mark> allMarks = new List<Mark>();
        List<Mark> temp = new List<Mark>();
        int r = 0;
        int c = 0;
        bool canClose = false;
        Mark m = null;

        // North
        r = row;
        c = col;
        r++;
        canClose = false;
        temp.Clear();
        while (true)
        {
            if (r >= NumberOfRows)
            {
                break;
            }

            m = Grids[r, c];
            if (m == null) 
            {
                break;
            }
            else if (m.PlayerIndex == currentPlayerIndex)
            {
                canClose = true;
                break;
            }
            else
            {
                //Debug.Log(string.Format("beat mark @ row:{0} col:{1}", r, c));
                temp.Add(m);
                r++;
            }
        }
        if (canClose) 
        {
            allMarks.AddRange(temp);   
        }

        // South
        r = row;
        c = col;
        r--;
        canClose = false;
        temp.Clear();
        while (true)
        {
            if (r < 0)
            {
                break;
            }
            
            m = Grids[r, c];
            if (m == null)
            {
                break;
            }
            else if (m.PlayerIndex == currentPlayerIndex)
            {
                canClose = true;
                break;
            }
            else
            {
                //Debug.Log(string.Format("beat mark @ row:{0} col:{1}", r, c));
                temp.Add(m);
                r--;
            }
        }
        if (canClose)
        {
            allMarks.AddRange(temp);
        }

        // East
        r = row;
        c = col;
        c++;
        canClose = false;
        temp.Clear();
        while (true)
        {
            if (c >= NumberOfColumns)
            {
                break;
            }
            
            m = Grids[r, c];
            if (m == null)
            {
                break;
            }
            else if (m.PlayerIndex == currentPlayerIndex)
            {
                canClose = true;
                break;
            }
            else
            {
                //Debug.Log(string.Format("beat mark @ row:{0} col:{1}", r, c));
                temp.Add(m);
                c++;
            }
        }
        if (canClose)
        {
            allMarks.AddRange(temp);
        }

        // West
        r = row;
        c = col;
        c--;
        canClose = false;
        temp.Clear();
        while (true)
        {
            if (c < 0)
            {
                break;
            }

            m = Grids[r, c];
            if (m == null)
            {
                break;
            }
            else if (m.PlayerIndex == currentPlayerIndex)
            {
                canClose = true;
                break;
            }
            else
            {
                //Debug.Log(string.Format("beat mark @ row:{0} col:{1}", r, c));
                temp.Add(m);
                c--;
            }
        }
        if (canClose)
        {
            allMarks.AddRange(temp);
        }

        // North-East
        r = row;
        c = col;
        r++;
        c++;
        canClose = false;
        temp.Clear();
        while (true)
        {
            if (r >= NumberOfRows || c >= NumberOfColumns)
            {
                break;
            }
            
            m = Grids[r, c];
            if (m == null)
            {
                break;
            }
            else if (m.PlayerIndex == currentPlayerIndex)
            {
                canClose = true;
                break;
            }
            else
            {
                //Debug.Log(string.Format("beat mark @ row:{0} col:{1}", r, c));
                temp.Add(m);
                r++;
                c++;
            }
        }
        if (canClose)
        {
            allMarks.AddRange(temp);
        }

        // North-West
        r = row;
        c = col;
        r++;
        c--;
        canClose = false;
        temp.Clear();
        while (true)
        {
            if (r >= NumberOfRows || c < 0)
            {
                break;
            }
            
            m = Grids[r, c];
            if (m == null)
            {
                break;
            }
            else if (m.PlayerIndex == currentPlayerIndex)
            {
                canClose = true;
                break;
            }
            else
            {
                //Debug.Log(string.Format("beat mark @ row:{0} col:{1}", r, c));
                temp.Add(m);
                r++;
                c--;
            }
        }
        if (canClose)
        {
            allMarks.AddRange(temp);
        }

        // South-East
        r = row;
        c = col;
        r--;
        c++;
        canClose = false;
        temp.Clear();
        while (true)
        {
            if (r < 0 || c >= NumberOfColumns)
            {
                break;
            }
            
            m = Grids[r, c];
            if (m == null)
            {
                break;
            }
            else if (m.PlayerIndex == currentPlayerIndex)
            {
                canClose = true;
                break;
            }
            else
            {
                //Debug.Log(string.Format("beat mark @ row:{0} col:{1}", r, c));
                temp.Add(m);
                r--;
                c++;
            }
        }
        if (canClose)
        {
            allMarks.AddRange(temp);
        }

        // South-West
        r = row;
        c = col;
        r--;
        c--;
        canClose = false;
        temp.Clear();
        while (true)
        {
            if (r < 0 || c < 0)
            {
                break;
            }

            m = Grids[r, c];
            if (m == null)
            {
                break;
            }
            else if (m.PlayerIndex == currentPlayerIndex)
            {
                canClose = true;
                break;
            }
            else
            {
                //Debug.Log(string.Format("beat mark @ row:{0} col:{1}", r, c));
                temp.Add(m);
                r--;
                c--;
            }
        }
        if (canClose)
        {
            allMarks.AddRange(temp);
        }

        return allMarks.ToArray();
    }
}

