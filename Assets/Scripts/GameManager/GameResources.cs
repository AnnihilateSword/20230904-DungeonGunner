using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameResources : MonoBehaviour
{
    private static GameResources instance;
    public static GameResources Instance
    {
        get
        {
            if (instance == null)
            {
                // 这是一种巧妙的方式，集中我们需要的任何资源，分享以使它更容易访问
                instance = Resources.Load<GameResources>("GameResources");
            }
            return instance;
        }
    }

    #region Header DUNGEON
    [Space(10)]
    [Header("DUNGEON")]
    #endregion
    #region Tooltip
    [Tooltip("填充地牢房间节点类型列表")]
    #endregion
    public RoomNodeTypeListSO roomNodeTypeList;
}
