using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicTransparencyScript : MonoBehaviour
{
    [SerializeField]
    private GameObject arrow;
    private SpriteRenderer arrowSR;
    private Color arrowStartColor;
    private Color arrowTargetColor;
    private bool arrowSwitch;

    [SerializeField]
    private GameObject pointer01;
    private SpriteRenderer pointer01SR;
    private Color pointer01StartColor;
    private Color pointer01TargetColor;
    private bool pointer01Switch;

    [SerializeField]
    private GameObject pointer02;
    private float pointerStartPosY;
    private float pointerTargetPosY;
    [SerializeField]
    private float distance;
    private bool pointerSwitch;

    private float currTimeArrow;
    private float currTimePointer01;
    private float currTimePointer02;
    [SerializeField]
    private float transparencyDeltaSpeed;
    [SerializeField]
    private float upDownDeltaSpeed;

    // Start is called before the first frame update
    void Start()
    {
        GameEventsScript.tutorialEvent01.AddListener(tutorialTransition01);

        arrowSR = arrow.GetComponent<SpriteRenderer>();
        arrowStartColor = arrowSR.color;
        Color c = arrowStartColor;
        c.a = .5f;
        arrowTargetColor = c;
        arrowSwitch = false;
        arrow.SetActive(false);

        pointer01SR = pointer01.GetComponent<SpriteRenderer>();
        pointer01StartColor = pointer01SR.color;
        Color d = pointer01StartColor;
        d.a = 1f;
        pointer01TargetColor = d;
        pointer01Switch = false;

        pointerStartPosY = pointer02.transform.position.y;
        pointerTargetPosY = pointerStartPosY + distance;
        pointerSwitch = false;
        pointer02.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if(arrow != null)
        {
            if(arrow.activeInHierarchy)
            {
                if(currTimeArrow/(transparencyDeltaSpeed/2) >= 1)
                {
                    arrowSwitch = !arrowSwitch;
                    currTimeArrow = 0f;
                }
                if(!arrowSwitch)
                {
                    arrowSR.color = Color.Lerp(arrowStartColor, arrowTargetColor, (currTimeArrow/(transparencyDeltaSpeed/2f)));
                } else
                {
                    arrowSR.color = Color.Lerp(arrowTargetColor, arrowStartColor, (currTimeArrow/(transparencyDeltaSpeed/2f)));
                }
                currTimeArrow += Time.deltaTime;
            }
        }

        if(pointer01 != null)
        {
            if(pointer01.activeInHierarchy)
            {
                if(currTimePointer01/(transparencyDeltaSpeed/2) >= 1)
                {
                    pointer01Switch = !pointer01Switch;
                    currTimePointer01 = 0f;
                }
                if(!pointer01Switch)
                {
                    pointer01SR.color = Color.Lerp(pointer01StartColor, pointer01TargetColor, (currTimePointer01/(transparencyDeltaSpeed/2f)));
                } else
                {
                    pointer01SR.color = Color.Lerp(pointer01TargetColor, pointer01StartColor, (currTimePointer01/(transparencyDeltaSpeed/2f)));
                }
                currTimePointer01 += Time.deltaTime;
            }
        }

        if(pointer02 != null)
        {
            if(pointer02.activeInHierarchy)
            {
                if(currTimePointer02/(upDownDeltaSpeed/2) >= 1)
                {
                    pointerSwitch = !pointerSwitch;
                    currTimePointer02 = 0f;
                }
                if(!pointerSwitch)
                {
                    pointer02.transform.position = new Vector3(pointer02.transform.position.x, Mathf.Lerp(pointerStartPosY, pointerTargetPosY, currTimePointer02/(upDownDeltaSpeed/2f)), pointer02.transform.position.z);
                } else
                {
                    pointer02.transform.position = new Vector3(pointer02.transform.position.x, Mathf.Lerp(pointerTargetPosY, pointerStartPosY, currTimePointer02/(upDownDeltaSpeed/2f)), pointer02.transform.position.z);
                }
                currTimePointer02 += Time.deltaTime;
            }
        }
    }

    private void tutorialTransition01()
    {
        arrow.SetActive(true);
        pointer01.SetActive(false);
        pointer02.SetActive(true);
    }
}
