using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorChangingScript : MonoBehaviour
{
    private Text FXText;
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

    void Awake()
    {
        GameEventsScript.startAllClearFX.AddListener(StartAllClearFX);
        GameEventsScript.startBonusFX.AddListener(StartBonusFX);
        GameEventsScript.gameIsOver.AddListener(TurnOffFX);
    }

    void Start()
    {
        FXText = GetComponent<Text>();
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
            FXText.color = Color.Lerp (FXText.color, allClearColorList[colorIndex], allClearFXTime/allClearFXDuration);
            if (allClearFXTime < (modAllClearFXDuration/2))
            {
                currFXTextSize = (int) Mathf.Lerp(fxTextNorm, fxTextLarge, growthTime/(modAllClearFXDuration/2));
                FXText.fontSize = currFXTextSize;
                growthTime += Time.deltaTime;
            } else if (allClearFXTime >= (modAllClearFXDuration/2) && allClearFXTime < modAllClearFXDuration)
            {
                currFXTextSize = (int) Mathf.Lerp(fxTextLarge, fxTextNorm, shrinkTime/(modAllClearFXDuration/2));
                FXText.fontSize = currFXTextSize;
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
            if(bonusFXTime/bonusFXDuration >= 1f)
            {
                bonusFXOn = false;
                bonusFXTime = 0f;
                bonusFXDelay = 0f;
                GameEventsScript.endBonusFX.Invoke();
                return;
            }

            for (int i = 0; i < goalTextList.Count; i++)
            {
                currGoalTextSize = (int) Mathf.Lerp(goalTextLarge, goalTextNorm, bonusFXTime/bonusFXDuration);
                goalTextList[i].fontSize = currGoalTextSize;
                Text goalText = goalTextList[i];
                goalText.color = Color.Lerp(goalText.color, Color.white, bonusFXTime/bonusFXDuration);
            }
            bonusFXDelay += Time.deltaTime;
            if (bonusFXDelay >= bonusFXDelayDuration)
            {
                bonusFXTime += Time.deltaTime;
            }
        }
    }


    //set text to init color, init text
    void StartAllClearFX()
    {
        Color c = Color.white;
        c.a = 1;
        FXText.color = c;
        FXText.text = "ALL CLEAR";
        allClearFXOn = true;
    }

    //turn off All Clear FX
    void turnOffAllClearFX()
    {
        allClearFXOn = false;
        allClearFXTime = 0f;
        Color c = FXText.color;
        c.a = 0;
        FXText.color = c;
        growthTime = 0;
        shrinkTime = 0;
    }

    //set init goal text to large, red text
    void StartBonusFX()
    {
        Color c = Color.red;
        for (int i = 0; i < goalTextList.Count; i++)
        {
            goalTextList[i].color = c;
            goalTextList[i].fontSize = (int) goalTextLarge;
        }
        bonusFXOn = true;
    }

    //title
    void TurnOffFX(GameEventsScript.GameOverData data)
    {
        allClearFXOn = false;
        bonusFXOn = false;
        Color c = FXText.color;
        c.a = 0;
        FXText.color = c;
    }
}
