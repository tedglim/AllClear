using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WinStatsScript : MonoBehaviour
{
//Serialized panels
    [SerializeField]
    private GameObject winPanel;
    [SerializeField]
    private GameObject losePanel;

//Serialized text
    [SerializeField]
    private Text yourTime;
    [SerializeField]
    private Text yourMoves;
    [SerializeField]
    private Text bestTime;
    [SerializeField]
    private Text avgTimeTxt;
    [SerializeField]
    private Text avgMovesTxt;
    [SerializeField]
    private Text winPct;

//non serialized values
    private List<WinTimeEntry> winTimeEntryList;
    private List<MovesTakenEntry> movesTakenEntryList;

//db classes
    private class WinTime {
        public List<WinTimeEntry> winTimeEntryList;
    }

    private class MovesTaken {
        public List<MovesTakenEntry> movesTakenEntryList;
    }

    [System.Serializable]
    private class WinTimeEntry
    {
        public float time;
    }

    [System.Serializable]
    private class MovesTakenEntry
    {
        public int movesTaken;
    }

    void Awake()
    {
        GameEventsScript.sendStats.AddListener(ShowStats);
    }

    private void ShowStats(GameEventsScript.GameOverData data)
    {
        //initialize
        if(!PlayerPrefs.HasKey(data.difficulty + "gamesPlayed"))
        {
            PlayerPrefs.SetInt(data.difficulty + "gamesPlayed", 0);
            PlayerPrefs.Save();
        }
        if(!PlayerPrefs.HasKey(data.difficulty + "gamesWon"))
        {
            PlayerPrefs.SetInt(data.difficulty + "gamesWon", 0);
            PlayerPrefs.Save();
        }

        //if game was won
        if(data.isWin)
        {
            winPanel.SetActive(true);
            yourTime.text = data.timer.ToString("F2");
            yourMoves.text = data.movesTaken.ToString();

            //record times and moves taken
            AddWinTimeEntry(data.difficulty, data.timer);
            AddMovesTakenEntry(data.difficulty, data.movesTaken);
            string jsonTimes = PlayerPrefs.GetString(data.difficulty + "winTimeTable");
            WinTime wintimes = JsonUtility.FromJson<WinTime>(jsonTimes);
            string jsonMoves = PlayerPrefs.GetString(data.difficulty + "movesTakenTable");
            MovesTaken movesTaken = JsonUtility.FromJson<MovesTaken>(jsonMoves);

            //sort times
            for (int i = 0; i < wintimes.winTimeEntryList.Count; i++)
            {
                for (int j = i + 1; j < wintimes.winTimeEntryList.Count; j++)
                {
                    if(wintimes.winTimeEntryList[j].time < wintimes.winTimeEntryList[i].time)
                    {
                        WinTimeEntry tmp = wintimes.winTimeEntryList[i];
                        wintimes.winTimeEntryList[i] = wintimes.winTimeEntryList[j];
                        wintimes.winTimeEntryList[j] = tmp;
                    }
                }
            }

            //sort moves taken
            for (int i = 0; i < movesTaken.movesTakenEntryList.Count; i++)
            {
                for (int j = i + 1; j < movesTaken.movesTakenEntryList.Count; j++)
                {
                    if(movesTaken.movesTakenEntryList[j].movesTaken < movesTaken.movesTakenEntryList[i].movesTaken)
                    {
                        MovesTakenEntry tmp = movesTaken.movesTakenEntryList[i];
                        movesTaken.movesTakenEntryList[i] = movesTaken.movesTakenEntryList[j];
                        movesTaken.movesTakenEntryList[j] = tmp;
                    }
                }
            }
            //get avg time
            float timeSum = 0;
            for (int i = 0; i < wintimes.winTimeEntryList.Count; i++)
            {
                timeSum += wintimes.winTimeEntryList[i].time;
            }
            float avgTime = timeSum/wintimes.winTimeEntryList.Count;
            
            //get avg moves
            int movesSum = 0;
            for (int i = 0; i < movesTaken.movesTakenEntryList.Count; i++)
            {
                movesSum += movesTaken.movesTakenEntryList[i].movesTaken;
            }
            float avgMoves = (float)movesSum/(float)movesTaken.movesTakenEntryList.Count;

            bestTime.text = wintimes.winTimeEntryList[0].time.ToString("F2");
            avgTimeTxt.text = avgTime.ToString("F2");
            avgMovesTxt.text = avgMoves.ToString("F2");

            int gamesWon = PlayerPrefs.GetInt(data.difficulty + "gamesWon");
            gamesWon += 1;
            int gamesPlayed = PlayerPrefs.GetInt(data.difficulty + "gamesPlayed");
            gamesPlayed += 1;
            winPct.text = ((float)gamesWon/(float)gamesPlayed*100.0f).ToString("F2") + "%";
            PlayerPrefs.SetInt(data.difficulty + "gamesWon", gamesWon);
            PlayerPrefs.SetInt(data.difficulty + "gamesPlayed", gamesPlayed);
            PlayerPrefs.Save();
        } else 
        {
            losePanel.SetActive(true);
            int gamesPlayed = PlayerPrefs.GetInt(data.difficulty + "gamesPlayed");
            gamesPlayed += 1;
            PlayerPrefs.SetInt(data.difficulty + "gamesPlayed", gamesPlayed);
            PlayerPrefs.Save();
        }
    }

    private void AddWinTimeEntry(string difficulty, float time)
    {
        //Create Entry
        WinTimeEntry winTimeEntry = new WinTimeEntry {
            time = time
        };

        if (!PlayerPrefs.HasKey(difficulty + "winTimeTable"))
        {
            //INIT LIST ON FIRST PLAY
            winTimeEntryList = new List<WinTimeEntry>(){};
            WinTime times = new WinTime
            {
                winTimeEntryList = winTimeEntryList
            };
            string jsTimes = JsonUtility.ToJson(times);
            PlayerPrefs.SetString(difficulty + "winTimeTable", jsTimes);
            PlayerPrefs.Save();
        }

        //load Saved HighScores
        string jsonLoadTimes = PlayerPrefs.GetString(difficulty + "winTimeTable");
        WinTime loadedTimes = JsonUtility.FromJson<WinTime>(jsonLoadTimes);
        
        //add entry to highscores
        loadedTimes.winTimeEntryList.Add(winTimeEntry);

        //save updated highscores
        string jsonSaveTimes = JsonUtility.ToJson(loadedTimes);
        PlayerPrefs.SetString(difficulty + "winTimeTable", jsonSaveTimes);
        PlayerPrefs.Save();
    }

    private void AddMovesTakenEntry (string difficulty, int movesTaken)
    {
        MovesTakenEntry movesTakenEntry = new MovesTakenEntry
        {
            movesTaken = movesTaken
        };

        if (!PlayerPrefs.HasKey(difficulty + "movesTakenTable"))
        {
            //INIT LIST ON FIRST PLAY
            movesTakenEntryList = new List<MovesTakenEntry>(){};
            MovesTaken moves = new MovesTaken
            {
                movesTakenEntryList = movesTakenEntryList
            };
            string jsMoves = JsonUtility.ToJson(moves);
            PlayerPrefs.SetString(difficulty + "movesTakenTable", jsMoves);
            PlayerPrefs.Save();
        }

        //load Saved HighScores
        string jsonLoadMoves = PlayerPrefs.GetString(difficulty + "movesTakenTable");
        MovesTaken loadedMoves = JsonUtility.FromJson<MovesTaken>(jsonLoadMoves);
        
        //add entry to highscores
        loadedMoves.movesTakenEntryList.Add(movesTakenEntry);

        //save updated highscores
        string jsonSaveMoves = JsonUtility.ToJson(loadedMoves);
        PlayerPrefs.SetString(difficulty + "movesTakenTable", jsonSaveMoves);
        PlayerPrefs.Save();
    }
}
