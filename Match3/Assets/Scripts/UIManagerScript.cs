using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
// using GameEventsScript;

public class UIManagerScript : MonoBehaviour
{
//serialized values
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
    private float bonusTimeDuration;
    [SerializeField]
    private Text timer;
    [SerializeField]
    private Text moveNumber;
    [SerializeField]
    private ParticleSystem movesParticles;
    [SerializeField]
    private int numMovesTrigParticlesOld;
    [SerializeField]
    private float posX;
    [SerializeField]
    private float posY;
    [SerializeField]
    private GameObject gameOverPanelContainer;
    [SerializeField]
    private Text roundNumber;
    [SerializeField]
    private List<GameObject> roundCircles;
    [SerializeField]
    private GameObject gameOverPanel;

//nonserialized values
    private GameObject cyanDestroyedText;
    private GameObject cyanCheck;
    private int currCyanNum;
    private int cyanNum;
    private GameObject greenDestroyedText;
    private GameObject greenCheck;
    private int currGreenNum;
    private int greenNum;
    private GameObject orangeDestroyedText;
    private GameObject orangeCheck;
    private int currOrangeNum;
    private int orangeNum;
    private GameObject pinkDestroyedText;
    private GameObject pinkCheck;
    private int currPinkNum;
    private int pinkNum;
    private GameObject redDestroyedText;
    private GameObject redCheck;
    private int currRedNum;
    private int redNum;
    private GameObject violetDestroyedText;
    private GameObject violetCheck;
    private int currVioletNum;
    private int violetNum;
    private GameObject yellowDestroyedText;
    private GameObject yellowCheck;
    private int currYellowNum;
    private int yellowNum;

    private bool isBonusFXOn;
    private int bonusAmt;
    private float bonusTime;

    private Color origColor;
    private ParticleSystem ps;

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
        isBonusFXOn = false;
        if (cyanGObj != null)
        {
            cyanDestroyedText = cyanGObj.transform.Find("Text").gameObject;
            cyanCheck = cyanGObj.transform.Find("CheckMark").gameObject;
            cyanCheck.SetActive(false);
        }
        if (greenGObj != null)
        {
            greenDestroyedText = greenGObj.transform.Find("Text").gameObject;
            greenCheck = greenGObj.transform.Find("CheckMark").gameObject;
            greenCheck.SetActive(false);
        }
        if (orangeGObj != null)
        {
            orangeDestroyedText = orangeGObj.transform.Find("Text").gameObject;
            orangeCheck = orangeGObj.transform.Find("CheckMark").gameObject;
            orangeCheck.SetActive(false);
        }
        if (pinkGObj != null)
        {
            pinkDestroyedText = pinkGObj.transform.Find("Text").gameObject;
            pinkCheck = pinkGObj.transform.Find("CheckMark").gameObject;
            pinkCheck.SetActive(false);
        }
        if (redGObj != null)
        {
            redDestroyedText = redGObj.transform.Find("Text").gameObject;
            redCheck = redGObj.transform.Find("CheckMark").gameObject;
            redCheck.SetActive(false);        
        }
        if (violetGObj != null)
        {
            violetDestroyedText = violetGObj.transform.Find("Text").gameObject;
            violetCheck = violetGObj.transform.Find("CheckMark").gameObject;
            violetCheck.SetActive(false);        
        }
        if (yellowGObj != null)
        {
            yellowDestroyedText = yellowGObj.transform.Find("Text").gameObject;
            yellowCheck = yellowGObj.transform.Find("CheckMark").gameObject;
            yellowCheck.SetActive(false);
        }

        GameEventsScript.clearGems.AddListener(UpdateDestroyedCountText);
        GameEventsScript.startBonusFX.AddListener(StartBonusFX);
        GameEventsScript.updateTime.AddListener(UpdateTimeText);        
        GameEventsScript.countRound.AddListener(UpdateRoundCountTextForRA);
        GameEventsScript.gameIsOver.AddListener(DisplayGameOverPanelRA);
        GameEventsScript.countMoveOld.AddListener(UpdateMoveCountText);
        GameEventsScript.countRoundOld.AddListener(UpdateRoundCountText);
        GameEventsScript.gameIsOverOld.AddListener(DisplayGameOverPanel);
    }

    //Bonus FX number countdown spd
    void Update()
    {
        if (isBonusFXOn)
        {
            if(bonusTime/bonusTimeDuration >= 1)
            {
                return;
            }
            if(cyanDestroyedText != null && cyanNum > 0)
            {
                currCyanNum = (int) Mathf.Lerp(cyanNum + bonusAmt, cyanNum, bonusTime/bonusTimeDuration);
                Text cyanText = cyanDestroyedText.GetComponent<Text>();
                cyanText.text = currCyanNum.ToString();
            }
            if(greenDestroyedText != null && greenNum > 0)
            {
                currGreenNum = (int) Mathf.Lerp(greenNum + bonusAmt, greenNum, bonusTime/bonusTimeDuration);
                Text greenText = greenDestroyedText.GetComponent<Text>();
                greenText.text = currGreenNum.ToString();
            }
            if(orangeDestroyedText != null && orangeNum > 0)
            {
                currOrangeNum = (int) Mathf.Lerp(orangeNum + bonusAmt, orangeNum, bonusTime/bonusTimeDuration);
                Text orangeText = orangeDestroyedText.GetComponent<Text>();
                orangeText.text = currOrangeNum.ToString();
            }
            if(pinkDestroyedText != null && pinkNum > 0)
            {
                currPinkNum = (int) Mathf.Lerp(pinkNum + bonusAmt, pinkNum, bonusTime/bonusTimeDuration);
                Text pinkText = pinkDestroyedText.GetComponent<Text>();
                pinkText.text = currPinkNum.ToString();
            }
            if(redDestroyedText != null && redNum > 0)
            {
                currRedNum = (int) Mathf.Lerp(redNum + bonusAmt, redNum, bonusTime/bonusTimeDuration);
                Text redText = redDestroyedText.GetComponent<Text>();
                redText.text = currRedNum.ToString();
            }
            if(violetDestroyedText != null && violetNum > 0)
            {
                currVioletNum = (int) Mathf.Lerp(violetNum + bonusAmt, violetNum, bonusTime/bonusTimeDuration);
                Text violetText = violetDestroyedText.GetComponent<Text>();
                violetText.text = currVioletNum.ToString();
            }
            if(yellowDestroyedText != null && yellowNum > 0)
            {
                currYellowNum = (int) Mathf.Lerp(yellowNum + bonusAmt, yellowNum, bonusTime/bonusTimeDuration);
                Text yellowText = yellowDestroyedText.GetComponent<Text>();
                yellowText.text = currYellowNum.ToString();
            }
            bonusTime += Time.deltaTime;
        }
    }

    //updates destroyed gem count
    private void UpdateDestroyedCountText(GameEventsScript.DestroyedGemsData data)
    {
        if(cyanDestroyedText != null)
        {
            if(data.cyanCleared <= 0)
            {
                cyanDestroyedText.SetActive(false);
                cyanCheck.SetActive(true);
            } else
            {
                Text cyanText = cyanDestroyedText.GetComponent<Text>();
                cyanText.text = data.cyanCleared.ToString();
            }
        }
        if(greenDestroyedText != null)
        {
            if(data.greenCleared <= 0)
            {
                greenDestroyedText.SetActive(false);
                greenCheck.SetActive(true);
            } else
            {
                Text greenText = greenDestroyedText.GetComponent<Text>();
                greenText.text = data.greenCleared.ToString();
            }
        }
        if(orangeDestroyedText != null)
        {
            if(data.orangeCleared <= 0)
            {
                orangeDestroyedText.SetActive(false);
                orangeCheck.SetActive(true);
            } else
            {
                Text orangeText = orangeDestroyedText.GetComponent<Text>();
                orangeText.text = data.orangeCleared.ToString();
            }
        }
        if(pinkDestroyedText != null)
        {
            if(data.pinkCleared <= 0)
            {
                pinkDestroyedText.SetActive(false);
                pinkCheck.SetActive(true);
            } else
            {
                Text pinkText = pinkDestroyedText.GetComponent<Text>();
                pinkText.text = data.pinkCleared.ToString();
            }
        }
        if(redDestroyedText != null)
        {
            if(data.redCleared <= 0)
            {
                redDestroyedText.SetActive(false);
                redCheck.SetActive(true);
            } else
            {
                Text redText = redDestroyedText.GetComponent<Text>();
                redText.text = data.redCleared.ToString();
            }
        }
        if(violetDestroyedText != null)
        {
            if(data.violetCleared <= 0)
            {
                violetDestroyedText.SetActive(false);
                violetCheck.SetActive(true);
            } else
            {
                Text violetText = violetDestroyedText.GetComponent<Text>();
                violetText.text = data.violetCleared.ToString();
            }
        }
        if(yellowDestroyedText != null)
        {
            if(data.yellowCleared <= 0)
            {
                yellowDestroyedText.SetActive(false);
                yellowCheck.SetActive(true);
            } else
            {
                Text yellowText = yellowDestroyedText.GetComponent<Text>();
                yellowText.text = data.yellowCleared.ToString();
            }
        }
    }

    //bonus FX setup for Revised Alpha
    private void StartBonusFX(GameEventsScript.DestroyedGemsData data)
    {
        isBonusFXOn = true;
        cyanNum = data.cyanCleared;
        greenNum = data.cyanCleared;
        orangeNum = data.orangeCleared;
        pinkNum = data.pinkCleared;
        redNum = data.redCleared;
        violetNum = data.violetCleared;
        yellowNum = data.yellowCleared;
        bonusAmt = data.bonusAmt;
        bonusTime = 0f;
    }

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
    private void UpdateRoundCountTextForRA(GameEventsScript.CountRoundData data)
    {
        if(data.currRound <= numMovesTrigParticlesOld)
        {
            moveNumber.color = Color.red;
            if (!ps.isPlaying)
            {
                ps.Play();
            }
        } else
        {
            moveNumber.color = origColor;
            ps.Stop();
        }
        moveNumber.text = data.currRound.ToString();        
    }

    //display game over for Revised Alpha
    private void DisplayGameOverPanelRA(GameEventsScript.GameOverData data)
    {
        gameOverPanelContainer.SetActive(true);
        GameEventsScript.sendStats.Invoke(new GameEventsScript.GameOverData(data.difficulty, data.menuRestart, data.isWin, data.didAllClear, data.movesTaken, data.timer));
    }

    //update move count for alpha version
    private void UpdateMoveCountText(GameEventsScript.CountMoveOldData data)
    {
        if(data.currMove <= numMovesTrigParticlesOld)
        {
            moveNumber.color = Color.red;
            if (!ps.isPlaying)
            {
                ps.Play();
            }
        } else
        {
            moveNumber.color = origColor;
            ps.Stop();
        }
        moveNumber.text = data.currMove.ToString();
    }

    //update rounds for alpha version
    private void UpdateRoundCountText(GameEventsScript.CountRoundOldData data)
    {
        if (data.currRound <= data.totalRounds)
        {
            roundCircles[data.currRound-1].GetComponent<SpriteRenderer>().color = Color.white;
        } else
        {
            return;
        }
        if(data.currRound == data.totalRounds)
        {
            roundNumber.text = "FINAL ROUND";
        } else 
        {
            roundNumber.text = "ROUND " + data.currRound.ToString();
        }
    }

    //display game over for alpha version
    private void DisplayGameOverPanel(GameEventsScript.GameOverOldData data)
    {
        gameOverPanel.SetActive(true);
        Text title = gameOverPanel.transform.Find("Title").GetComponent<Text>();

        if (data.isWin)
        {
            title.text = "VICTORY";
            gameOverPanel.transform.Find("WinImg").gameObject.SetActive(true);
        } else 
        {
            title.text = "TRY AGAIN";
            gameOverPanel.transform.Find("LoseImg").gameObject.SetActive(true);
        }
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
