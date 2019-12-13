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
    private Text cyanDestroyedText;
    [SerializeField]
    private Text greenDestroyedText;
    [SerializeField]
    private Text orangeDestroyedText;
    [SerializeField]
    private Text pinkDestroyedText;
    [SerializeField]
    private Text redDestroyedText;
    [SerializeField]
    private Text violetDestroyedText;
    [SerializeField]
    private Text yellowDestroyedText;
    [SerializeField]
    private Text roundNumber;
    [SerializeField]
    private Text moveNumber;

//nonserialized
    private Color origColor;
    private ParticleSystem ps;

//serialized
    [SerializeField]
    private List<GameObject> roundCircles;
    [SerializeField]
    private ParticleSystem movesParticles;
    [SerializeField]
    private int numMovesTrigParticles;
    [SerializeField]
    private float posX;
    [SerializeField]
    private float posY;
    [SerializeField]
    private GameObject gameOverPanel;

    void Awake()
    {
        origColor = moveNumber.color;
        ps = Instantiate(movesParticles, new Vector2(posX, posY), Quaternion.identity);
        ps.Stop();
    }

    void Start()
    {
        GameEventsScript.clearGems.AddListener(UpdateDestroyedCountText);
        GameEventsScript.countRound.AddListener(UpdateRoundCountText);
        GameEventsScript.countMove.AddListener(UpdateMoveCountText);
        GameEventsScript.gameIsOver.AddListener(DisplayGameOverPanel);
    }

    private void UpdateDestroyedCountText(GameEventsScript.DestroyedGemsData data)
    {
        if(cyanDestroyedText != null)
        {
            cyanDestroyedText.text = data.cyanCleared.ToString();
        }
        if(greenDestroyedText != null)
        {
            greenDestroyedText.text = data.greenCleared.ToString();
        }
        if(orangeDestroyedText != null)
        {
            orangeDestroyedText.text = data.orangeCleared.ToString();
        }
        if(pinkDestroyedText != null)
        {
            pinkDestroyedText.text = data.pinkCleared.ToString();
        }
        if(redDestroyedText != null)
        {
            redDestroyedText.text = data.redCleared.ToString();
        }
        if(violetDestroyedText != null)
        {
            violetDestroyedText.text = data.violetCleared.ToString();
        }
        if(yellowDestroyedText != null)
        {
            yellowDestroyedText.text = data.yellowCleared.ToString();
        }
    }

    private void UpdateRoundCountText(GameEventsScript.CountRoundsData data)
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

    private void UpdateMoveCountText(GameEventsScript.CountMoveData data)
    {
        if(data.currMove <= numMovesTrigParticles)
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

    private void DisplayGameOverPanel(GameEventsScript.GameOverData data)
    {
        gameOverPanel.SetActive(true);
        Text title = gameOverPanel.transform.Find("Title").GetComponent<Text>();

        if (data.isWin)
        {
            title.text = "VICTORY";
            gameOverPanel.transform.Find("WinImg").gameObject.SetActive(true);
        } else 
        {
            title.text = "DEFEAT";
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
