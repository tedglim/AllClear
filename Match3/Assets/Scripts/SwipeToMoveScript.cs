using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwipeToMoveScript : MonoBehaviour
{
    private Vector2 startTouchPos, endTouchPos;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            startTouchPos = Input.GetTouch(0).position;
        }

        if(Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended)
        {
            endTouchPos = Input.GetTouch(0).position;

            if((endTouchPos.x < startTouchPos.x) && transform.position.x > -1.75f)
            {
                transform.position = new Vector2(transform.position.x - 1.75f, transform.position.y);
            }

            if((endTouchPos.x > startTouchPos.x) && transform.position.x < 1.75f)
            {
                transform.position = new Vector2(transform.position.x + 1.75f, transform.position.y);
            }
        }

    }
}
