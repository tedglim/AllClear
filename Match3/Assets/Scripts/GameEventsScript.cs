using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameEventsScript : MonoBehaviour
{
    //Event Declarations
    public static UnityEvent menuListOnOff = new UnityEvent();
    public static UnityEvent undoOnOff = new UnityEvent();
    public static GemsDestroyedEvent clearGems = new GemsDestroyedEvent();
    public static CountRoundEvent countRound = new CountRoundEvent();
    public static CountRoundV1Event countRoundV1 = new CountRoundV1Event();
    public static CountMoveEvent countMove = new CountMoveEvent();
    public static GameOverEvent gameIsOver = new GameOverEvent();

    //Even Class Declarations
    public class GemsDestroyedEvent: UnityEvent<DestroyedGemsData>{}
    public class CountRoundEvent: UnityEvent<CountRoundsData>{}
    public class CountRoundV1Event: UnityEvent<CountRoundsV1Data>{}
    public class CountMoveEvent: UnityEvent<CountMoveData>{}
    public class GameOverEvent: UnityEvent<GameOverData>{}

    //Event Classes
    public class DestroyedGemsData {
        public int cyanCleared;
        public int greenCleared;
        public int orangeCleared;
        public int pinkCleared;
        public int redCleared;
        public int violetCleared;
        public int yellowCleared;
        
        public DestroyedGemsData(int cyan, int green, int orange, int pink, int red, int violet, int yellow)
        {
            this.cyanCleared = cyan;
            this.greenCleared = green;
            this.orangeCleared = orange;
            this.pinkCleared = pink;
            this.redCleared = red;
            this.violetCleared = violet;
            this.yellowCleared = yellow;
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

    public class CountRoundsV1Data {
        public int currRound;
        
        public CountRoundsV1Data(int currRound)
        {
            this.currRound = currRound;
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

    public class GameOverData {

        public bool isWin;

        public GameOverData(bool isWin)
        {
            this.isWin = isWin;
        }
    }

}
