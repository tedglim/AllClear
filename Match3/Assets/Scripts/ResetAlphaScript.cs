using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ResetAlphaScript : MonoBehaviour
{
    private Image image;
    private Color origColor;

    //inits
    void Awake()
    {
        image = this.GetComponent<Image>();
        origColor = image.color;
    }

    //UNDO Button full saturation
    public void changeAlphaFull()
    {
        Color tempColor = image.color;
        tempColor.a = 1f;
        image.color = tempColor;
        GameEventsScript.undoOnOff.Invoke();
    }

    //UNDO Button half transparent
    public void changeAlphaHalf()
    {
        if (Input.touchCount < 1)
        {
            image.color = origColor;
            GameEventsScript.undoOnOff.Invoke();            
        } else 
        {
            Touch touch = Input.GetTouch(0);
            if(touch.phase != TouchPhase.Ended)
            {
                image.color = origColor;
                GameEventsScript.undoOnOff.Invoke();
            }
        }
    }
}
