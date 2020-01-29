using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameEventsScript : MonoBehaviour
{
    //Event Declarations
    public static UnityEvent menuListOnOff = new UnityEvent();
    public static UnityEvent undoOnOffOld = new UnityEvent();
    public static UnityEvent startAllClearFX = new UnityEvent();
    public static UnityEvent endAllClearFX = new UnityEvent();
    public static UnityEvent endBonusFX = new UnityEvent();
    public static GemsDestroyedEvent clearGems = new GemsDestroyedEvent();
    public static CountRoundEvent countRound = new CountRoundEvent();
    public static StartBonusEvent startBonusFX = new StartBonusEvent();
    public static UpdateTimeEvent updateTime = new UpdateTimeEvent();
    public static GetTimeEvent getTime = new GetTimeEvent();
    public static SetTimeEvent setTime = new SetTimeEvent();
    public static GameOverEvent gameIsOver = new GameOverEvent();
    public static SendStatsEvent sendStats = new SendStatsEvent();
    public static CountRoundOldEvent countRoundOld = new CountRoundOldEvent();
    public static CountMoveOldEvent countMoveOld = new CountMoveOldEvent();
    public static GameOverOldEvent gameIsOverOld = new GameOverOldEvent();

    //Event Class Declarations
    public class GemsDestroyedEvent: UnityEvent<DestroyedGemsData>{}

    public class CountRoundEvent: UnityEvent<CountRoundData>{}
    public class StartBonusEvent: UnityEvent<DestroyedGemsData>{}
    public class UpdateTimeEvent: UnityEvent<TimeData>{}
    public class GetTimeEvent: UnityEvent{}
    public class SetTimeEvent: UnityEvent<TimeData>{}
    public class GameOverEvent: UnityEvent<GameOverData>{}
    public class SendStatsEvent: UnityEvent<GameOverData>{}
    public class CountRoundOldEvent: UnityEvent<CountRoundOldData>{}
    public class CountMoveOldEvent: UnityEvent<CountMoveOldData>{}
    public class GameOverOldEvent: UnityEvent<GameOverOldData>{}

    //Event Classes
    public class DestroyedGemsData {
        public int cyanCleared;
        public int greenCleared;
        public int orangeCleared;
        public int pinkCleared;
        public int redCleared;
        public int violetCleared;
        public int yellowCleared;
        public bool bonusFXOn;
        public int bonusAmt;
        
        public DestroyedGemsData(int cyan, int green, int orange, int pink, int red, int violet, int yellow, bool bonusFXOn, int bonusAmt)
        {
            this.cyanCleared = cyan;
            this.greenCleared = green;
            this.orangeCleared = orange;
            this.pinkCleared = pink;
            this.redCleared = red;
            this.violetCleared = violet;
            this.yellowCleared = yellow;
            this.bonusFXOn = bonusFXOn;
            this.bonusAmt = bonusAmt;
        }
    }

    public class CountRoundData {
        public int currRound;
        public int totalRounds;
        
        public CountRoundData(int currRound, int totalRounds)
        {
            this.currRound = currRound;
            this.totalRounds = totalRounds;
        }
    }

    public class TimeData {

        public float time;

        public TimeData(float time)
        {
            this.time = time;
        }
    }

    public class GameOverData {
        public string difficulty;
        public bool menuRestart;
        public bool isWin;
        public bool didAllClear;
        public int movesTaken;
        public float timer;

        public GameOverData(string difficulty, bool menuRestart, bool isWin, bool didAllClear, int movesTaken, float timer)
        {
            this.difficulty = difficulty;
            this.menuRestart = menuRestart;
            this.isWin = isWin;
            this.didAllClear = didAllClear;
            this.movesTaken = movesTaken;
            this.timer = timer;
        }
    }

    public class CountRoundOldData {
        public int currRound;
        public int totalRounds;
        
        public CountRoundOldData(int currRound, int totalRounds)
        {
            this.currRound = currRound;
            this.totalRounds = totalRounds;
        }
    }

    public class CountMoveOldData {
        public int currMove;
        public int totalMoves;
        
        public CountMoveOldData(int currMove, int totalMoves)
        {
            this.currMove = currMove;
            this.totalMoves = totalMoves;
        }
    }

    public class GameOverOldData {

        public bool isWin;

        public GameOverOldData(bool isWin)
        {
            this.isWin = isWin;
        }
    }
}
