using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundScript : MonoBehaviour
{
    private bool firstCall;
    private float initPosY;
    private int totalGems;
    private int currentGems;
    private bool wasPinged;

    private int cyan;
    private int green;
    private int orange;
    private int pink;
    private int red;
    private int violet;
    private int yellow;



    [SerializeField]
    private float paddingX;
    private bool FXOn;
    private int prevGemmCount;
    [SerializeField]
    private GameObject background;
    [SerializeField]
    private float distance;
    [SerializeField]
    private float FXDuration;
    private float currFXTime;

    // Start is called before the first frame update
    void Start()
    {
        GameEventsScript.clearGems.AddListener(SetupBackgroundScroll);
        GameEventsScript.gameIsOver.AddListener(SetBackgroundToFinalPos);
        firstCall = true;
        FXOn = false;
        currFXTime = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        if(FXOn)
        {
            if(currFXTime/FXDuration >= 1)
            {
                currFXTime = 0;
                FXOn = false;
                prevGemmCount = currentGems;
                return;
            }
            if(wasPinged)
            {
                wasPinged = false;
                currFXTime = 0f;
            }
            if(currentGems == 0)
            {
                background.transform.position = new Vector3(paddingX, Mathf.Lerp(background.transform.position.y, distance + initPosY, currFXTime/FXDuration), 0);
            } else 
            {
                background.transform.position = new Vector3(paddingX, Mathf.Lerp(background.transform.position.y, (1f - (float)currentGems/(float)totalGems) * distance + initPosY, currFXTime/FXDuration), 0);
            }
            currFXTime += Time.deltaTime;
        }
    }

    private void SetupBackgroundScroll(GameEventsScript.DestroyedGemsData data)
    {
        if(firstCall)
        {
            totalGems = data.cyanCleared + data.greenCleared + data.orangeCleared + data.pinkCleared + data.redCleared + data.violetCleared + data.yellowCleared;
            initPosY = background.transform.position.y;
            firstCall = false;
        } else
        {
            if(data.cyanCleared <= 0)
            {
                cyan = 0;
            } else 
            {
                cyan = data.cyanCleared;
            }
            if(data.greenCleared <= 0)
            {
                green = 0;
            } else 
            {
                green = data.greenCleared;
            }
            if(data.orangeCleared <= 0)
            {
                orange = 0;
            } else 
            {
                orange = data.orangeCleared;
            }
            if(data.pinkCleared <= 0)
            {
                pink = 0;
            } else 
            {
                pink = data.pinkCleared;
            }
            if(data.redCleared <= 0)
            {
                red = 0;
            } else
            {
                red = data.redCleared;
            }
            if(data.violetCleared <= 0)
            {
                violet = 0;
            } else 
            {
                violet = data.violetCleared;
            }
            if(data.yellowCleared <= 0)
            {
                yellow = 0;
            } else 
            {
                yellow = data.yellowCleared;
            }
            currentGems = cyan + green + orange + pink + red + violet + yellow;
            FXOn = true;
            wasPinged = true;
            Debug.Log("IS PINGED");
            // currFXTime = 0;
        }
    }

    private void SetBackgroundToFinalPos(GameEventsScript.GameOverData data)
    {
        if (data.isWin)
        {
            background.transform.position = new Vector3(paddingX, initPosY + distance, 0);
        }
    }
}
