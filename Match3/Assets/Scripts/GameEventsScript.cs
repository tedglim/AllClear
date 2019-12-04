using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameEventsScript : MonoBehaviour
{

    // public static UnityEvent releaseHold = new UnityEvent();
    // public static UnityEvent countMove = new UnityEvent();
    // public static UnityEvent countRound = new UnityEvent();
    public static UnityEvent gameIsOver = new UnityEvent();
    public static GemsDestroyedEvent clearGems = new GemsDestroyedEvent();
    public static CountRoundEvent countRound = new CountRoundEvent();
    public static CountMoveEvent countMove = new CountMoveEvent();
    //release hold,
    //count gems deleted

    public class GemsDestroyedEvent: UnityEvent<DestroyedGemsData>{}
    public class CountRoundEvent: UnityEvent<CountRoundsData>{}
    public class CountMoveEvent: UnityEvent<CountMoveData>{}

    public class DestroyedGemsData {
        public int cyanCleared;
        public int greenCleared;
        public int redCleared;
        
        public DestroyedGemsData(int cyan, int green, int red)
        {
            this.cyanCleared = cyan;
            this.greenCleared = green;
            this.redCleared = red;
        }
    }

    public class CountRoundsData {
        public int currRound;
        public int totalRounds;
        
        public CountRoundsData(int currRound, int totalRounds)
        {
            this.currRound = currRound;
            this.totalRounds = totalRounds;
        }
    }

    public class CountMoveData {
        public int currMove;
        public int totalMoves;
        
        public CountMoveData(int currMove, int totalMoves)
        {
            this.currMove = currMove;
            this.totalMoves = totalMoves;
        }
    }
    
}
