using System.Net.Mime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManagerScript : MonoBehaviour
{
    public bool isGameOver = false;
    private GameObject restartMenu;
    private GameObject highScoreTableContainer;
    private GameObject highScoreTableTemplate;
    private List<HighScoreEntry> highScoreEntryList;
    private List<GameObject> highScoreEntryGObjList;
    private int highScoreLimit = 10;
    public Text yourScore;



    void Awake()
    {
        restartMenu = GameObject.Find("Canvas/RestartMenu");
        highScoreTableContainer = GameObject.Find("Canvas/RestartMenu/HighScoreTableContainer");
        highScoreTableTemplate = GameObject.Find("Canvas/RestartMenu/HighScoreTableContainer/HighScoreTableTemplate");
        restartMenu.SetActive(false);
        highScoreTableTemplate.SetActive(false);
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

    public void GameOver(float inGameTimer, int gemsDestroyed)
    {
        if (!isGameOver)
        {
            isGameOver = true;
            restartMenu.SetActive(true);
            yourScore.text = "Your Time: " + inGameTimer.ToString("F") + "\n" + "Gems Cleared: " + gemsDestroyed.ToString();

            AddHighScoreEntry(inGameTimer, gemsDestroyed);
            string jsonString = PlayerPrefs.GetString("highScoreTable");
            HighScores highscores = JsonUtility.FromJson<HighScores>(jsonString);

            for (int i = 0; i < highscores.highScoreEntryList.Count; i++)
            {
                for (int j = i+1; j < highscores.highScoreEntryList.Count; j++)
                {
                    if(highscores.highScoreEntryList[j].gemsDestroyed > highscores.highScoreEntryList[i].gemsDestroyed)
                    {
                        HighScoreEntry tmp = highscores.highScoreEntryList[i];
                        highscores.highScoreEntryList[i] = highscores.highScoreEntryList[j];
                        highscores.highScoreEntryList[j] = tmp;
                    }
                    if (highscores.highScoreEntryList[j].gemsDestroyed == highscores.highScoreEntryList[i].gemsDestroyed)
                    {
                        if(highscores.highScoreEntryList[j].time < highscores.highScoreEntryList[i].time)
                        {
                            HighScoreEntry tmp = highscores.highScoreEntryList[i];
                            highscores.highScoreEntryList[i] = highscores.highScoreEntryList[j];
                            highscores.highScoreEntryList[j] = tmp; 
                        }
                    }
                }
            }

            highScoreEntryGObjList = new List<GameObject>();
            if (highscores.highScoreEntryList.Count >= highScoreLimit)
            {
                for (int i = 0; i < highScoreLimit; i++)
                {
                    CreateHighScoreEntryGObj(highscores.highScoreEntryList[i], highScoreTableContainer, highScoreEntryGObjList);
                }
            } else 
            {
                foreach (HighScoreEntry highScoreEntry in highscores.highScoreEntryList)
                {
                    CreateHighScoreEntryGObj(highScoreEntry, highScoreTableContainer, highScoreEntryGObjList);
                }
            }
        }
    }

    //make and spawn highscore data on screen
    private void CreateHighScoreEntryGObj(HighScoreEntry highScoreEntry, GameObject container, List<GameObject> gObjList)
    {
        float height = 40;
        GameObject entryGObj = (GameObject)Instantiate(highScoreTableTemplate, container.transform);
        RectTransform entryRectTransform = entryGObj.GetComponent<RectTransform>();
        entryRectTransform.anchoredPosition = new Vector2(0, -height * gObjList.Count);
        entryGObj.SetActive(true);

        int rank = gObjList.Count + 1;
        string rankString = rank.ToString("D");
        string timeString = highScoreEntry.time.ToString("F"); // timer.Tostring(f)
        string gemString = highScoreEntry.gemsDestroyed.ToString(); // gemDestroyed.ToString

        Transform rankTransform = entryGObj.transform.Find("Rank");
        rankTransform.GetComponent<Text>().text = rankString;

        Transform timeTransform = entryGObj.transform.Find("Time");
        timeTransform.GetComponent<Text>().text = timeString;
                
        Transform gemsTransform = entryGObj.transform.Find("Gems");
        gemsTransform.GetComponent<Text>().text = gemString;     

        gObjList.Add(entryGObj);
    }

    private void AddHighScoreEntry (float time, int gemsDestroyed)
    {
        //Create Entry
        HighScoreEntry highScoreEntry = new HighScoreEntry {
            time = time,
            gemsDestroyed = gemsDestroyed,
        };

        if (!PlayerPrefs.HasKey("highScoreTable"))
        {
            //INIT LIST ON FIRST PLAY
            highScoreEntryList = new List<HighScoreEntry>()
            {
                new HighScoreEntry {time = 100.0f, gemsDestroyed = 1}
            };
            HighScores hs = new HighScores{highScoreEntryList = highScoreEntryList};
            string js = JsonUtility.ToJson(hs);
            PlayerPrefs.SetString("highScoreTable", js);
            PlayerPrefs.Save();
        }
        //load Saved HighScores
        string jsonString = PlayerPrefs.GetString("highScoreTable");
        HighScores highscores = JsonUtility.FromJson<HighScores>(jsonString);

        //add entry to highscores
        highscores.highScoreEntryList.Add(highScoreEntry);

        //save updated highscores
        string json = JsonUtility.ToJson(highscores);
        PlayerPrefs.SetString("highScoreTable", json);
        PlayerPrefs.Save();
    }

    private class HighScores {
        public List<HighScoreEntry> highScoreEntryList;
    }

    [System.Serializable]
    private class HighScoreEntry
    {
        public float time;
        public int gemsDestroyed;
    }
}
