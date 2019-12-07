using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
// using GameEventsScript;

public class UIManagerScript : MonoBehaviour
{
//serialized nums
    [SerializeField]
    private Text cyanDestroyedText;
    [SerializeField]
    private Text greenDestroyedText;
    [SerializeField]
    private Text redDestroyedText;
    [SerializeField]
    private Text roundNumber;
    [SerializeField]
    private Text moveNumber;
    [SerializeField]
    private GameObject gameOverPanel;
//nonserialized
    private int currRoundNum;

//serialized
    [SerializeField]
    private List<GameObject> roundCircles;

    void Awake()
    {
        GameEventsScript.clearGems.AddListener(UpdateDestroyedCountText);
        GameEventsScript.countRound.AddListener(UpdateRoundCountText);
        GameEventsScript.countMove.AddListener(UpdateMoveCountText);
        GameEventsScript.gameIsOver.AddListener(DisplayGameOverPanel);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void UpdateDestroyedCountText(GameEventsScript.DestroyedGemsData data)
    {
        cyanDestroyedText.text = data.cyanCleared.ToString();
        greenDestroyedText.text = data.greenCleared.ToString();
        redDestroyedText.text = data.redCleared.ToString();
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
            // Color c = Color.white;
        }
    }

    private void UpdateMoveCountText(GameEventsScript.CountMoveData data)
    {
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
