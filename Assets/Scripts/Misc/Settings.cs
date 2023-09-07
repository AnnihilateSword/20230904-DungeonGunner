using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Settings
{
    #region ROOM SETTINGS

    // 从一个房间通向儿童走廊的最大数量。—— 最多应该是3个，但不建议这样做，因为这样会导致地下城建筑失败，因为房间更有可能不适合在一起
    public const int maxChildCorridors = 3;

    #endregion
}
