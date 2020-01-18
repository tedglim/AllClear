using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PlayStatsScript
{
    private static float time;
    private static int gemsCleared;

    public static int GemsCleared
    {
        get
        {
            return gemsCleared;
        }
        set
        {
            gemsCleared = value;
        }
    }

    public static float Time
    {
        get
        {
            return time;
        }
        set
        {
            time = value;
        }
    }
}
