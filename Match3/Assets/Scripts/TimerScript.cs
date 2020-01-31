using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimerScript : MonoBehaviour
{
    float timer;
    bool stopTime;

    // Start is called before the first frame update
    void Start()
    {
        timer = 0;
        stopTime = false;
        GameEventsScript.getTime.AddListener(GetTime);
        GameEventsScript.updateTime.Invoke(new GameEventsScript.TimeData(timer));
    }

    // Update is called once per frame
    void Update()
    {
        if(stopTime)
        {
            return;
        }
        timer += Time.deltaTime;
        GameEventsScript.updateTime.Invoke(new GameEventsScript.TimeData(timer));
    }

    void GetTime()
    {
        if(timer>=999.99f)
        {
            timer = 999.99f;
        }
        GameEventsScript.setTime.Invoke(new GameEventsScript.TimeData(timer));
        stopTime = true;
    }
}
