using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ResetAlphaScript : MonoBehaviour
{
    Image image;
    Color origColor;
    // Start is called before the first frame update
    void Awake()
    {
        image = this.GetComponent<Image>();
        origColor = image.color;
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void changeAlphaFull()
    {
        Color tempColor = image.color;
        tempColor.a = 1f;
        image.color = tempColor;
        GameEventsScript.undoOnOff.Invoke();
        // Debug.Log("FULL");
    }

    public void changeAlphaHalf()
    {
        image.color = origColor;
        GameEventsScript.undoOnOff.Invoke();
        // Debug.Log("HALF");
    }
}
