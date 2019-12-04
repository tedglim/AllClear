using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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
        if (data.currRound > data.totalRounds)
        {
            roundCircles[data.currRound-1].GetComponent<SpriteRenderer>().color = Color.white;
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

    // private void UpdateRoundCountText(GameEventsScript.CountRoundEvent data)
    // {
    //     // roundNumber.text = data.currRound.ToString();
    // }
}
