using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicTransparencyScript : MonoBehaviour
{
    [SerializeField]
    private GameObject arrow01;
    private SpriteRenderer arrow01SR;
    private Color arrow01StartColor;
    private Color arrow01TargetColor;

    [SerializeField]
    private GameObject arrow02;
    private SpriteRenderer arrow02SR;
    private Color arrow02StartColor;
    private Color arrow02TargetColor;

    [SerializeField]
    private GameObject arrow03;
    private SpriteRenderer arrow03SR;
    private Color arrow03StartColor;
    private Color arrow03TargetColor;

    [SerializeField]
    private GameObject pointer01;
    private SpriteRenderer pointer01SR;
    private Color pointer01StartColor;
    private Color pointer01TargetColor;

    [SerializeField]
    private GameObject pointer02;
    private float pointer02StartPosY;
    private float pointer02TargetPosY;

    [SerializeField]
    private GameObject pointer03;
    private float pointer03StartPosY;
    private float pointer03TargetPosY;

    private bool transparencySwitch;
    private bool bounceSwitch;
    private float currTimeTransparencyFX;
    [SerializeField]
    private float transparencyDeltaSpeed;
    private float currTimeBounceFX;
    [SerializeField]
    private float distance;
    [SerializeField]
    private float upDownDeltaSpeed;

    [SerializeField]
    private GameObject highlightBoxGoal;
    [SerializeField]
    private GameObject highlightBoxMoves;

    // Start is called before the first frame update
    void Start()
    {
        GameEventsScript.tutorialEvent01.AddListener(tutorialTransition01);
        GameEventsScript.tutorialEvent01dot5.AddListener(tutorialTransition01dot5);
        GameEventsScript.tutorialEvent02.AddListener(tutorialTransition02);
        GameEventsScript.tutorialEvent02dot5.AddListener(tutorialTransition02dot5);
        GameEventsScript.tutorialEvent03.AddListener(tutorialTransition03);
        GameEventsScript.tutorialEvent03dot5.AddListener(tutorialTransition03dot5);
        GameEventsScript.tutorialEvent04.AddListener(tutorialTransition04);
        GameEventsScript.tutorialEvent04dot5.AddListener(tutorialTransition04dot5);
        GameEventsScript.tutorialEvent05.AddListener(tutorialTransition05);
        GameEventsScript.tutorialEvent05dot5.AddListener(tutorialTransition05dot5);
        GameEventsScript.tutorialEvent05dot51.AddListener(tutorialTransition05dot51);
        GameEventsScript.tutorialEvent06.AddListener(tutorialTransition06);

        arrow01SR = arrow01.GetComponent<SpriteRenderer>();
        arrow01StartColor = arrow01SR.color;
        Color c = arrow01StartColor;
        c.a = .5f;
        arrow01TargetColor = c;
        transparencySwitch = false;
        arrow01.SetActive(false);

        arrow02SR = arrow02.GetComponent<SpriteRenderer>();
        arrow02StartColor = arrow02SR.color;
        Color e = arrow02StartColor;
        e.a = .5f;
        arrow02TargetColor = e;
        arrow02.SetActive(false);

        arrow03SR = arrow03.GetComponent<SpriteRenderer>();
        arrow03StartColor = arrow03SR.color;
        Color f = arrow03StartColor;
        f.a = .5f;
        arrow03TargetColor = e;
        arrow03.SetActive(false);

        pointer01SR = pointer01.GetComponent<SpriteRenderer>();
        pointer01StartColor = pointer01SR.color;
        Color d = pointer01StartColor;
        d.a = 1f;
        pointer01TargetColor = d;

        pointer02StartPosY = pointer02.transform.position.y;
        pointer02TargetPosY = pointer02StartPosY + distance;
        bounceSwitch = false;
        pointer02.SetActive(false);

        pointer03StartPosY = pointer03.transform.position.y;
        pointer03TargetPosY = pointer03StartPosY + distance;
        pointer03.SetActive(false);

        highlightBoxGoal.SetActive(false);
        highlightBoxMoves.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        doTransparencyFX(arrow01, arrow01SR, arrow01StartColor, arrow01TargetColor);
        doTransparencyFX(arrow02, arrow02SR, arrow02StartColor, arrow02TargetColor);
        doTransparencyFX(arrow03, arrow03SR, arrow03StartColor, arrow03TargetColor);
        doTransparencyFX(pointer01, pointer01SR, pointer01StartColor, pointer01TargetColor);

        doBounceFX(pointer02, pointer02StartPosY, pointer02TargetPosY);
        doBounceFX(pointer03, pointer03StartPosY, pointer03TargetPosY);
    }

    private void doTransparencyFX(GameObject arrow, SpriteRenderer spriteRenderer, Color startColor, Color endColor)
    {
        if(arrow != null)
        {
            if(arrow.activeInHierarchy)
            {
                if(currTimeTransparencyFX/(transparencyDeltaSpeed/2) >= 1)
                {
                    transparencySwitch = !transparencySwitch;
                    currTimeTransparencyFX = 0f;
                }
                if(!transparencySwitch)
                {
                    spriteRenderer.color = Color.Lerp(startColor, endColor, (currTimeTransparencyFX/(transparencyDeltaSpeed/2f)));
                } else
                {
                    spriteRenderer.color = Color.Lerp(endColor, startColor, (currTimeTransparencyFX/(transparencyDeltaSpeed/2f)));
                }
                currTimeTransparencyFX += Time.deltaTime;
            }
        }
    }

    private void doBounceFX(GameObject pointer, float startPos, float targetPos)
    {
        if(pointer != null)
        {
            if(pointer.activeInHierarchy)
            {
                if(currTimeBounceFX/(upDownDeltaSpeed/2) >= 1)
                {
                    bounceSwitch = !bounceSwitch;
                    currTimeBounceFX = 0f;
                }
                if(!bounceSwitch)
                {
                    pointer.transform.position = new Vector3(pointer.transform.position.x, Mathf.Lerp(startPos, targetPos, currTimeBounceFX/(upDownDeltaSpeed/2f)), pointer.transform.position.z);
                } else
                {
                    pointer.transform.position = new Vector3(pointer.transform.position.x, Mathf.Lerp(targetPos, startPos, currTimeBounceFX/(upDownDeltaSpeed/2f)), pointer.transform.position.z);
                }
                currTimeBounceFX += Time.deltaTime;
            }
        }        
    }

    private void tutorialTransition01()
    {
        arrow01.SetActive(true);
        pointer01.SetActive(false);
        pointer02.SetActive(true);
    }

    private void tutorialTransition01dot5()
    {
        if(pointer02.activeInHierarchy)
        {
            pointer02.SetActive(false);

        } else
        {
            pointer02.SetActive(true);
        }
    }

    private void tutorialTransition02()
    {
        arrow01.SetActive(false);
        pointer01.SetActive(true);
        pointer02.SetActive(false);
    }

    private void tutorialTransition02dot5()
    {
        pointer01.SetActive(false);
    }

    private void tutorialTransition03()
    {
        pointer01.SetActive(true);
        highlightBoxGoal.SetActive(true);
    }

    private void tutorialTransition03dot5()
    {
        highlightBoxGoal.SetActive(false);
        highlightBoxMoves.SetActive(true);
    }

    private void tutorialTransition04()
    {
        highlightBoxMoves.SetActive(false);
        pointer01.SetActive(false);
        arrow02.SetActive(true);
        pointer03.SetActive(true);
    }

    private void tutorialTransition04dot5()
    {
        if(pointer03.activeInHierarchy)
        {
            pointer03.SetActive(false);

        } else
        {
            pointer03.SetActive(true);
        }
    }

    private void tutorialTransition05()
    {
        arrow02.SetActive(false); 
        pointer03.SetActive(false);
    }

    private void tutorialTransition05dot5()
    {
        pointer01.SetActive(true);
    }

    private void tutorialTransition05dot51()
    {
        pointer01.SetActive(false);
        arrow03.SetActive(true);
    }

    private void tutorialTransition06()
    {
        if(arrow03.activeInHierarchy)
        {
            arrow03.SetActive(false);
        } else 
        {
            arrow03.SetActive(true);
        }
    }
}
