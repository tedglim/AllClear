using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
// using GameEventsScript;

public class UIManagerScript : MonoBehaviour
{
//serialized Texts
    [SerializeField]
    private GameObject cyanGObj;
    [SerializeField]
    private GameObject greenGObj;
    [SerializeField]
    private GameObject orangeGObj;
    [SerializeField]
    private GameObject pinkGObj;
    [SerializeField]
    private GameObject redGObj;
    [SerializeField]
    private GameObject violetGObj;
    [SerializeField]
    private GameObject yellowGObj;
    [SerializeField]
    private Text roundNumber;
    [SerializeField]
    private Text moveNumber;
    [SerializeField]
    private Text timer;

//nonserialized
    private Text cyanDestroyedText;
    private GameObject cyanCheck;
    private Text greenDestroyedText;
    private GameObject greenCheck;
    private Text orangeDestroyedText;
    private GameObject orangeCheck;
    private Text pinkDestroyedText;
    private GameObject pinkCheck;
    private Text redDestroyedText;
    private GameObject redCheck;
    private Text violetDestroyedText;
    private GameObject violetCheck;
    private Text yellowDestroyedText;
    private GameObject yellowCheck;
    private Color origColor;
    private ParticleSystem ps;

//serialized
    [SerializeField]
    private List<GameObject> roundCircles;
    [SerializeField]
    private ParticleSystem movesParticles;
    // [SerializeField]
    // private int numMovesTrigParticlesOld;
    [SerializeField]
    private float posX;
    [SerializeField]
    private float posY;
    [SerializeField]
    private GameObject gameOverPanel;
    [SerializeField]
    private GameObject gameOverPanelContainer;

    //setup particle effect
    void Awake()
    {
        origColor = moveNumber.color;
        ps = Instantiate(movesParticles, new Vector2(posX, posY), Quaternion.identity);
        ps.Stop();
    }

    //setup color ui reqs + event listeners
    void Start()
    {
        if (cyanGObj != null)
        {
            cyanDestroyedText = cyanGObj.transform.Find("Text").GetComponent<Text>();
            cyanCheck = cyanGObj.transform.Find("CheckMark").gameObject;
            cyanCheck.SetActive(false);
        }
        if (greenGObj != null)
        {
            greenDestroyedText = greenGObj.transform.Find("Text").GetComponent<Text>();
            greenCheck = greenGObj.transform.Find("CheckMark").gameObject;
            greenCheck.SetActive(false);
        }
        if (orangeGObj != null)
        {
            orangeDestroyedText = orangeGObj.transform.Find("Text").GetComponent<Text>();
            orangeCheck = orangeGObj.transform.Find("CheckMark").gameObject;
            orangeCheck.SetActive(false);
        }
        if (pinkGObj != null)
        {
            pinkDestroyedText = pinkGObj.transform.Find("Text").GetComponent<Text>();
            pinkCheck = pinkGObj.transform.Find("CheckMark").gameObject;
            pinkCheck.SetActive(false);
        }
        if (redGObj != null)
        {
            redDestroyedText = redGObj.transform.Find("Text").GetComponent<Text>();
            redCheck = redGObj.transform.Find("CheckMark").gameObject;
            redCheck.SetActive(false);        
        }
        if (violetGObj != null)
        {
            violetDestroyedText = violetGObj.transform.Find("Text").GetComponent<Text>();
            violetCheck = violetGObj.transform.Find("CheckMark").gameObject;
            violetCheck.SetActive(false);        
        }
        if (yellowGObj != null)
        {
            yellowDestroyedText = yellowGObj.transform.Find("Text").GetComponent<Text>();
            yellowCheck = yellowGObj.transform.Find("CheckMark").gameObject;
            yellowCheck.SetActive(false);
        }

        GameEventsScript.clearGems.AddListener(UpdateDestroyedCountText);
        // GameEventsScript.countMoveOld.AddListener(UpdateMoveCountText);
        // GameEventsScript.countRoundOld.AddListener(UpdateRoundCountText);
        // GameEventsScript.gameIsOverOld.AddListener(DisplayGameOverPanel);
        GameEventsScript.updateTime.AddListener(UpdateTimeText);        
        GameEventsScript.countRoundV1.AddListener(UpdateRoundCountTextForV1);
        GameEventsScript.gameIsOverV1.AddListener(DisplayGameOverPanelV1);
    }

    //updates destroyed gem count
    private void UpdateDestroyedCountText(GameEventsScript.DestroyedGemsData data)
    {
        if(cyanDestroyedText != null)
        {
            cyanDestroyedText.text = data.cyanCleared.ToString();
            if(data.cyanCleared == 0)
            {
                cyanDestroyedText.text = "";
                cyanCheck.SetActive(true);
            }
        }
        if(greenDestroyedText != null)
        {
            greenDestroyedText.text = data.greenCleared.ToString();
            if(data.greenCleared == 0)
            {
                greenDestroyedText.text = "";
                greenCheck.SetActive(true);
            }
        }
        if(orangeDestroyedText != null)
        {
            orangeDestroyedText.text = data.orangeCleared.ToString();
            if(data.orangeCleared == 0)
            {
                orangeDestroyedText.text = "";
                orangeCheck.SetActive(true);
            }
        }
        if(pinkDestroyedText != null)
        {
            pinkDestroyedText.text = data.pinkCleared.ToString();
            if(data.pinkCleared == 0)
            {
                pinkDestroyedText.text = "";
                pinkCheck.SetActive(true);
            }
        }
        if(redDestroyedText != null)
        {
            redDestroyedText.text = data.redCleared.ToString();
            if(data.redCleared == 0)
            {
                redDestroyedText.text = "";
                redCheck.SetActive(true);
            }
        }
        if(violetDestroyedText != null)
        {
            violetDestroyedText.text = data.violetCleared.ToString();
            if(data.violetCleared == 0)
            {
                violetDestroyedText.text = "";
                violetCheck.SetActive(true);        
            }
        }
        if(yellowDestroyedText != null)
        {
            yellowDestroyedText.text = data.yellowCleared.ToString();
            if(data.yellowCleared == 0)
            {
                yellowDestroyedText.text = "";
                yellowCheck.SetActive(true);
            }
        }
    }

    // private void UpdateMoveCountText(GameEventsScript.CountMoveData data)
    // {
    //     if(data.currMove <= numMovesTrigParticlesOld)
    //     {
    //         moveNumber.color = Color.red;
    //         if (!ps.isPlaying)
    //         {
    //             ps.Play();
    //         }
    //     } else
    //     {
    //         moveNumber.color = origColor;
    //         ps.Stop();
    //     }
    //     moveNumber.text = data.currMove.ToString();
    // }

    // //update rounds for alpha version
    // private void UpdateRoundCountText(GameEventsScript.CountRoundsData data)
    // {
    //     if (data.currRound <= data.totalRounds)
    //     {
    //         roundCircles[data.currRound-1].GetComponent<SpriteRenderer>().color = Color.white;
    //     } else
    //     {
    //         return;
    //     }
    //     if(data.currRound == data.totalRounds)
    //     {
    //         roundNumber.text = "FINAL ROUND";
    //     } else 
    //     {
    //         roundNumber.text = "ROUND " + data.currRound.ToString();
    //     }
    // }

    // //display game over for alpha version
    // private void DisplayGameOverPanel(GameEventsScript.GameOverData data)
    // {
    //     gameOverPanel.SetActive(true);
    //     Text title = gameOverPanel.transform.Find("Title").GetComponent<Text>();

    //     if (data.isWin)
    //     {
    //         title.text = "VICTORY";
    //         gameOverPanel.transform.Find("WinImg").gameObject.SetActive(true);
    //     } else 
    //     {
    //         title.text = "TRY AGAIN";
    //         gameOverPanel.transform.Find("LoseImg").gameObject.SetActive(true);
    //     }
    // }

    //update time UI for Revised Alpha
    private void UpdateTimeText(GameEventsScript.TimeData data)
    {
        int timerSeconds;
        if (data.time > 999)
        {
            timerSeconds = 999;
        } else
        {
            timerSeconds = (int)(data.time);
        }

        timer.text = timerSeconds.ToString();
    }

    //update moves Left for Revised Alpha
    private void UpdateRoundCountTextForV1(GameEventsScript.CountRoundsV1Data data)
    {
        moveNumber.text = data.currRound.ToString();
    }

    //display game over for Revised Alpha
    private void DisplayGameOverPanelV1(GameEventsScript.GameOverDataV1 data)
    {
        gameOverPanelContainer.SetActive(true);
        GameEventsScript.sendStats.Invoke(new GameEventsScript.GameOverDataV1(data.isWin, data.movesTaken, data.timer));
    }

    public void Restart()
    {
        SceneManager.LoadSceneAsync(4);
    }

    public void MainMenu()
    {
        SceneManager.LoadSceneAsync(0);
    }
}
