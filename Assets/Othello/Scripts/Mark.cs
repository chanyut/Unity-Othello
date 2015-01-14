using UnityEngine;
using System.Collections;

public class Mark : MonoBehaviour 
{
    public int PlayerIndex;
    public GameObject Visual;

    public Material BlackMarkMaterial;
    public Material WhiteMarkMaterial;

    public void RefreshVisual()
    {
        if (PlayerIndex == GamePlay.PLAYER_INDEX_P1)
        {
            Visual.renderer.material = BlackMarkMaterial;    
        }
        else if (PlayerIndex == GamePlay.PLAYER_INDEX_P2)
        {
            Visual.renderer.material = WhiteMarkMaterial;
        }
    }

    public void Flip()
    {
        if (PlayerIndex == GamePlay.PLAYER_INDEX_P1)
        {
            PlayerIndex = GamePlay.PLAYER_INDEX_P2;
        }
        else if (PlayerIndex == GamePlay.PLAYER_INDEX_P2)
        {
            PlayerIndex = GamePlay.PLAYER_INDEX_P1;
        }
        RefreshVisual();
    }
}
