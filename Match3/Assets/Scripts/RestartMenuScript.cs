using System.Net.Mime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RestartMenuScript : MonoBehaviour
{
    [SerializeField]
    private GameObject ScoreTable;
    [SerializeField]
    private GameObject ScoreTableTemplate;
    private List<HighScoreEntry> highScoreEntryList;
    private List<GameObject> highScoreEntryGObjList;
    private int highScoreLimit = 10;
    public Text yourScore;

    private class HighScores {
        public List<HighScoreEntry> highScoreEntryList;
    }

    [System.Serializable]
    private class HighScoreEntry
    {
        public int gemsDestroyed;
        public float time;
    }

    void Awake()
    {
        PlayStatsScript.Time = 60.0f;
        PlayStatsScript.GemsCleared = 30;
    }

    void Start()
    {
        ShowScoreMenu();
    }

    public void Restart()
    {
        SceneManager.LoadSceneAsync(2);
    }

    public void GetInstructions()
    {
        SceneManager.LoadSceneAsync(0);
    }

    public void Quit()
    {
        Application.Quit();
    }

    private void ShowScoreMenu()
    {
        yourScore.text = "Gems Cleared: " + PlayStatsScript.GemsCleared.ToString() + "\n" + "Your Time: " + PlayStatsScript.Time.ToString("F");
        AddHighScoreEntry(PlayStatsScript.GemsCleared, PlayStatsScript.Time);
        string jsonString = PlayerPrefs.GetString("ScoreTable");
        HighScores highscores = JsonUtility.FromJson<HighScores>(jsonString);
        for (int i = 0; i < highscores.highScoreEntryList.Count; i++)
        {
            for (int j = i+1; j < highscores.highScoreEntryList.Count; j++)
            {
                if(highscores.highScoreEntryList[j].gemsDestroyed > highscores.highScoreEntryList[i].gemsDestroyed)
                {
                    HighScoreEntry currScore = highscores.highScoreEntryList[i];
                    highscores.highScoreEntryList[i] = highscores.highScoreEntryList[j];
                    highscores.highScoreEntryList[j] = currScore;
                }
                if (highscores.highScoreEntryList[j].gemsDestroyed == highscores.highScoreEntryList[i].gemsDestroyed)
                {
                    if(highscores.highScoreEntryList[j].time < highscores.highScoreEntryList[i].time)
                    {
                        HighScoreEntry currScore = highscores.highScoreEntryList[i];
                        highscores.highScoreEntryList[i] = highscores.highScoreEntryList[j];
                        highscores.highScoreEntryList[j] = currScore; 
                    }
                }
            }
        }

        highScoreEntryGObjList = new List<GameObject>();
        if (highscores.highScoreEntryList.Count >= highScoreLimit)
        {
            for (int i = 0; i < highScoreLimit; i++)
            {
                CreateHighScoreEntryGObj(highscores.highScoreEntryList[i], ScoreTable, highScoreEntryGObjList);
            }
        } else 
        {
            foreach (HighScoreEntry highScoreEntry in highscores.highScoreEntryList)
            {
                CreateHighScoreEntryGObj(highScoreEntry, ScoreTable, highScoreEntryGObjList);
            }
        }

    }

    //make and spawn highscore data on screen
    private void CreateHighScoreEntryGObj(HighScoreEntry highScoreEntry, GameObject container, List<GameObject> gObjList)
    {
        float height = 40;
        GameObject entryGObj = (GameObject)Instantiate(ScoreTableTemplate, container.transform);
        RectTransform entryRectTransform = entryGObj.GetComponent<RectTransform>();
        entryRectTransform.anchoredPosition = new Vector2(0, -height * gObjList.Count);
        entryGObj.SetActive(true);

        int rank = gObjList.Count + 1;
        string rankString = rank.ToString("D");
        string gemString = highScoreEntry.gemsDestroyed.ToString();
        string timeString = highScoreEntry.time.ToString("F");

        Transform rankTransform = entryGObj.transform.Find("Rank");
        rankTransform.GetComponent<Text>().text = rankString;

        Transform gemsTransform = entryGObj.transform.Find("Gems");
        gemsTransform.GetComponent<Text>().text = gemString;

        Transform timeTransform = entryGObj.transform.Find("Time");
        timeTransform.GetComponent<Text>().text = timeString;

        gObjList.Add(entryGObj);
    }

    private void AddHighScoreEntry (int gemsDestroyed, float time)
    {
        //Create Entry
        HighScoreEntry highScoreEntry = new HighScoreEntry {
            gemsDestroyed = gemsDestroyed,
            time = time
        };

        if (!PlayerPrefs.HasKey("ScoreTable"))
        {
            //INIT LIST ON FIRST PLAY
            highScoreEntryList = new List<HighScoreEntry>(){};
            HighScores hs = new HighScores
            {
                highScoreEntryList = highScoreEntryList
            };
            string js = JsonUtility.ToJson(hs);
            PlayerPrefs.SetString("ScoreTable", js);
            PlayerPrefs.Save();
        }
        
        //load Saved HighScores
        string jsonString = PlayerPrefs.GetString("ScoreTable");
        HighScores highscores = JsonUtility.FromJson<HighScores>(jsonString);
        
        //add entry to highscores
        highscores.highScoreEntryList.Add(highScoreEntry);

        //save updated highscores
        string json = JsonUtility.ToJson(highscores);
        PlayerPrefs.SetString("ScoreTable", json);
        PlayerPrefs.Save();
    }
}
