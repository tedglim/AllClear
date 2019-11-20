using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameEventsScript : MonoBehaviour
{

    public static UnityEvent releaseHold = new UnityEvent();
    public static UnityEvent gameIsOver = new UnityEvent();
    public static GemClearEvent clearGems = new GemClearEvent();
    //release hold,
    //count gems deleted

    public class GemClearEvent: UnityEvent<GemClearData>{}
    public class GemClearData{
        public int gemsCleared;
        public GemClearData(int gemsCleared)
        {
            this.gemsCleared = gemsCleared;
        }
    }
}
