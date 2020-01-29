using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorChangingScript : MonoBehaviour
{
    [SerializeField]
    private Text AllClearFXText;
    [SerializeField]
    private Text BonusFXText;
    [SerializeField]
    private List<Text> goalTextList;

    private bool allClearFXOn;
    private float allClearFXTime;
    [SerializeField]
    private float allClearFXDuration;
    [SerializeField]
    private float allClearFXTimeExt;
    private float modAllClearFXDuration;
    [SerializeField]
    private float fxTextLarge;
    [SerializeField]
    private float fxTextNorm;
    private int currFXTextSize;
    private float growthTime;
    private float shrinkTime;
    private float allClearFXColorTime;
    [SerializeField]
    private float allClearFXColorSpeed;
    [SerializeField]
    Color[] allClearColorList;
    private int colorIndex;

//bonus vars
    private bool bonusFXOn;
    private float bonusFXTime;
    [SerializeField]
    private float bonusFXDuration;
    private float bonusFXDelay;
    [SerializeField]
    private float bonusFXDelayDuration;
    [SerializeField]
    private float goalTextLarge;
    [SerializeField]
    private float goalTextNorm;
    private int currGoalTextSize;
    private float bonusFXColorTime;
    [SerializeField]
    private float bonusFXColorSpeed;

    void Awake()
    {
        GameEventsScript.startAllClearFX.AddListener(StartAllClearFX);
        GameEventsScript.startBonusFX.AddListener(StartBonusFX);
        GameEventsScript.gameIsOver.AddListener(TurnOffFX);
    }

    void Start()
    {
        // FXText = GetComponent<Text>();
        allClearFXOn = false;
        allClearFXTime = 0;
        allClearFXColorTime = 0;
        modAllClearFXDuration = allClearFXDuration - allClearFXTimeExt;
        colorIndex = 0;
        growthTime = 0;
        shrinkTime = 0;
        bonusFXOn = false;
        bonusFXTime = 0;
        bonusFXDelay = 0;
    }

    // Update is called once per frame
    void Update()
    {
        //allClearFX Controls
        if (allClearFXOn)
        {
            AllClearFXText.color = Color.Lerp (AllClearFXText.color, allClearColorList[colorIndex], allClearFXTime/allClearFXDuration);
            if (allClearFXTime < (modAllClearFXDuration/2))
            {
                currFXTextSize = (int) Mathf.Lerp(fxTextNorm, fxTextLarge, growthTime/(modAllClearFXDuration/2));
                AllClearFXText.fontSize = currFXTextSize;
                growthTime += Time.deltaTime;
            } else if (allClearFXTime >= (modAllClearFXDuration/2) && allClearFXTime < modAllClearFXDuration)
            {
                currFXTextSize = (int) Mathf.Lerp(fxTextLarge, fxTextNorm, shrinkTime/(modAllClearFXDuration/2));
                AllClearFXText.fontSize = currFXTextSize;
                shrinkTime += Time.deltaTime;
            }
            if (allClearFXTime/allClearFXDuration >= 1f)
            {
                turnOffAllClearFX();
                GameEventsScript.endAllClearFX.Invoke();
                return;
            }
            
            allClearFXTime += Time.deltaTime;
            allClearFXColorTime += Time.deltaTime;
            if(allClearFXColorTime > allClearFXColorSpeed)
            {
                if (colorIndex < allClearColorList.Length - 1)
                {
                    colorIndex++;
                } else
                {
                    colorIndex = 0;
                }
                allClearFXColorTime = 0;
            }
        }

        //Bonus FX controls
        if(bonusFXOn)
        {
            //reset bonus vars
            if(bonusFXTime/bonusFXDuration >= 1f)
            {
                bonusFXOn = false;
                bonusFXTime = 0f;
                bonusFXDelay = 0f;
                BonusFXText.text = "";
                GameEventsScript.endBonusFX.Invoke();
                return;
            }

            //loop through all goal texts
            for (int i = 0; i < goalTextList.Count; i++)
            {
                currGoalTextSize = (int) Mathf.Lerp(goalTextLarge, goalTextNorm, bonusFXTime/bonusFXDuration);
                goalTextList[i].fontSize = currGoalTextSize;
                Text goalText = goalTextList[i];
                goalText.color = Color.Lerp(goalText.color, Color.white, bonusFXTime/bonusFXDuration);
            }

            //change bonus FX text color
            BonusFXText.color = Color.Lerp (BonusFXText.color, allClearColorList[colorIndex], (bonusFXDelay+bonusFXTime)/bonusFXDuration);
            bonusFXColorTime += Time.deltaTime;
            if(bonusFXColorTime > bonusFXColorSpeed)
            {
                if (colorIndex < allClearColorList.Length - 1)
                {
                    colorIndex++;
                } else
                {
                    colorIndex = 0;
                }
                bonusFXColorTime = 0;
            }

            //bonus time controls
            if (bonusFXDelay >= bonusFXDelayDuration)
            {
                bonusFXTime += Time.deltaTime;
            } else
            {
                bonusFXDelay += Time.deltaTime;
            }
        }
    }


    //set text to init color, init text
    void StartAllClearFX()
    {
        Color c = Color.white;
        c.a = 1;
        AllClearFXText.color = c;
        AllClearFXText.text = "ALL CLEAR";
        allClearFXOn = true;
    }

    //turn off All Clear FX
    void turnOffAllClearFX()
    {
        allClearFXOn = false;
        allClearFXTime = 0f;
        AllClearFXText.text = "";
        growthTime = 0;
        shrinkTime = 0;
    }

    //set init goal text to large, red text
    void StartBonusFX(GameEventsScript.DestroyedGemsData data)
    {
        Color c = Color.red;
        for (int i = 0; i < goalTextList.Count; i++)
        {
            goalTextList[i].color = c;
            goalTextList[i].fontSize = (int) goalTextLarge;
        }
        Color w = Color.white;
        w.a = 1;
        BonusFXText.color = w;
        BonusFXText.text = "BONUS";
        bonusFXOn = true;

    }

    //title
    void TurnOffFX(GameEventsScript.GameOverData data)
    {
        allClearFXOn = false;
        bonusFXOn = false;
        Color c = AllClearFXText.color;
        c.a = 0;
        AllClearFXText.color = c;
    }
}
