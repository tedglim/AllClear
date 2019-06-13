using System.Net.NetworkInformation;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoardManagerScript : MonoBehaviour
{
    private struct Gem {
        public GameObject gemGObj;
        public GameObject gemGridObj;
        public int matchCountHoriz;
        public int matchCountVert;
        public int dropDist;
        public bool groupedHoriz;
        public bool groupedVert;
        public int xLoc;
        public int yLoc;
        public bool destroyed;
    }

    public int boardDimX = 6;
    public int boardDimY = 5;
    public int boardYDropOffset = 5;
    private Gem[,] gemGridLayout;

    public List<GameObject> gemOptions;
    private Gem leftGem;
    private Gem downGem;
    private int initHorizCount;
    private int initVertCount;
    private int notUsed = -1;

    public float fallTimeInterval = .01f;
    public float fallPercentIncrease = .05f;

    private Ray touchPos;
    public float underlayAlpha = .25f;
    public float overlayAlpha = .75f;
    private Gem gemClone;
    public float moveSpeed = 25.0f;
    private Vector2Int prevActiveTouchPos;

    public float rotationTimeInterval = .000001f;
    public float rotatePercentIncrease = 1.0f / 3.0f;
    private Vector3 rotationAngle;
    private bool isRotating = false;
    private bool gridLocked = false;
    private bool isShifting = false;

    private List<Gem> gemListToDestroy = new List<Gem>();
    private bool rainCheck = false;

    public int textTime = 5;
    public int countdown = 10; 
    public Text text1;
    public Text text2;
    private bool isStarting;
    private int shiftCount = 0;
    private GameManagerScript gameManagerScript;
    private float inGameTimer = 0.0f;
    private bool inGameTimerOn = true;


    // Start is called before the first frame update
    void Awake()
    {
        GameObject gameManagerGObj = GameObject.Find("GameManager"); 
        gameManagerScript = gameManagerGObj.GetComponent<GameManagerScript>();
        StartCoroutine(IntroScene());
    }

    IEnumerator IntroScene()
    {
        isStarting = true;
        int showTextTime = textTime;
        text1.text = "";
        text2.text = "";
        while (showTextTime > 0)
        {
            text1.text = "YOU GET\n 10 SECONDS\n TO PLAN...";
            if (showTextTime <= 2)
            {
                text2.text = "USE THEM WISELY!";
            }
            showTextTime--;
            yield return new WaitForSecondsRealtime(1);
        }
        text1.text = "";
        text2.text = "";

        InitBoard();
        MoveGemsDown();
    }

    IEnumerator StartCountdown()
    {
        text1.fontSize = 48;
        int countdownTime = countdown;
        while (countdownTime >=0)
        {
            if (countdownTime == 0)
            {
                text1.text = "GO!";
                gridLocked = false;
                isShifting = false;

            } else 
            {            
                text1.text = countdownTime.ToString();
            }
            countdownTime--;
            yield return new WaitForSecondsRealtime(1);
        }
        isStarting = false;
        text1.text = inGameTimer.ToString();
    }

    private void InitBoard()
    {
        gemGridLayout = new Gem[boardDimX, boardDimY];
        GameObject randGem;
        for (int y = 0; y < boardDimY; y++)
        {
            for (int x = 0; x < boardDimX; x++)
            {
                //List possible gem options
                List<GameObject> availableGems = new List<GameObject>();    
                availableGems.AddRange(gemOptions);
                while (true)
                {
                    //initialization for while loop
                    randGem = availableGems[UnityEngine.Random.Range(0, availableGems.Count)];
                    initHorizCount = 1;
                    initVertCount = 1;
                    //if not in far left column
                    if(x > 0)
                    {
                        leftGem = gemGridLayout[x - 1, y];
                        //if rand gem is same as 1 to left
                        if (randGem.Equals(leftGem.gemGObj))
                        {
                            //repick if left already has 2 in a row
                            if (leftGem.matchCountHoriz > 1)
                            {
                                availableGems.Remove(randGem);
                                continue;
                            //set both prev and current to have horiz count 2
                            } else 
                            {
                                leftGem.matchCountHoriz++;
                                initHorizCount++;
                            }
                        }
                    }
                    //if not in bottom row
                    if(y > 0)
                    {
                        downGem = gemGridLayout[x, y - 1];
                        //if rand is same as 1 below
                        if (randGem.Equals(downGem.gemGObj))
                        {
                            //repick if down already has 2 in a row
                            if (downGem.matchCountVert > 1)
                            {
                                availableGems.Remove(randGem);
                                continue;
                            //set both 1 below and current to have vert count 2
                            } else
                            {
                                downGem.matchCountVert++;
                                initVertCount++;
                            }
                        }
                    }
                    break;
                }
                //make the gem and place it in grid
                MakeNewGem(randGem, x ,y);
            }
        }
    }

    //place Gem GObj into grid.
    private void MakeNewGem(GameObject gemGObj, int x, int y)
    {
        GameObject currentGem = (GameObject)Instantiate(gemGObj, new Vector2((float)x, (float)y + (float)boardYDropOffset), Quaternion.identity);
        currentGem.transform.parent = transform;
        gemGridLayout[x, y] = new Gem {
            gemGObj = gemGObj,
            gemGridObj = currentGem,
            matchCountHoriz = initHorizCount,
            matchCountVert = initVertCount,
            dropDist = boardYDropOffset,
            groupedHoriz = false,
            groupedVert = false,
            xLoc = x,
            yLoc = y,
            destroyed = false,
        };
    }

    //wrapper for moving gems down into position on screen
    private void MoveGemsDown()
    {
        gridLocked = true;
        isShifting = true;
        for(int y = 0; y < boardDimY; y++)
        {
            for(int x = 0; x < boardDimX; x++)
            {
                if(gemGridLayout[x, y].dropDist > 0)
                {
                    //set start and end pos before coroutine
                    Vector2 startPos = gemGridLayout[x, y].gemGridObj.transform.position;
                    Vector2 endPos = new Vector2(gemGridLayout[x, y].gemGridObj.transform.position.x, gemGridLayout[x, y].gemGridObj.transform.position.y - gemGridLayout[x, y].dropDist);
                    StartCoroutine(MoveGemsDownEnum(gemGridLayout[x, y], startPos, endPos));
                    //reset drop distance and matches
                    gemGridLayout[x, y].matchCountHoriz = notUsed;
                    gemGridLayout[x, y].matchCountVert = notUsed;
                    gemGridLayout[x, y].dropDist = 0;
                }
            }
        }
    }

    //moves Gems down at specified lerp rate
    IEnumerator MoveGemsDownEnum(Gem gem, Vector2 start, Vector2 end)
    {
        float fallPercent = 0.0f;
        while(fallPercent <= 1.0f)
        {
            gem.gemGridObj.transform.position = Vector2.Lerp(start, end, fallPercent);
            fallPercent += fallPercentIncrease;
            yield return new WaitForSeconds(fallTimeInterval);
        }
        gem.gemGridObj.transform.position = end;

        if (shiftCount < 1)
        {
            StartCoroutine(StartCountdown());
            shiftCount++;
        } else {
            gridLocked = false;
            isShifting = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!isStarting && !gameManagerScript.isGameOver)
        {
            if (inGameTimerOn)
            {
                inGameTimer += Time.deltaTime;
                text1.text = (inGameTimer).ToString("F");
            }
        }
        if(isStarting || gridLocked || isShifting || gameManagerScript.isGameOver)
        {
            return;
        }

        //On initial touch of screen
        if(Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            touchPos = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
            // don't bother if finger is way off.
            if (Mathf.RoundToInt(touchPos.origin.y) < boardDimY + 1 && Mathf.RoundToInt(touchPos.origin.y) > -2)
            {
                DisplayGemClone(touchPos.origin);
            }
        //if finger is moving on screen
        } else if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved)
        {
            touchPos = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
            gemClone.gemGridObj.transform.Translate((touchPos.origin - gemClone.gemGridObj.transform.position) * Time.deltaTime * moveSpeed);
            ShowGemMovement(touchPos.origin);
        //if finger is off
        } else if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended)
        {
            if(isRotating)
            {
                rainCheck = true;
                touchPos = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
            } else 
            {
                touchPos = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
                DropGem();
                StartCoroutine(MatchGems());
            }
        }
        //raincheck version
        if(rainCheck)
        {
            if(!isRotating)
            {
                DropGem();
                StartCoroutine(MatchGems());
                rainCheck = false;
            }
        }
    }

    private void DisplayGemClone(Vector2 touchPos)
    {
        int touchPosX = GetPosOnAxis(touchPos.x, boardDimX);
        int touchPosY = GetPosOnAxis(touchPos.y, boardDimY);
        //get Gem in grid; change its alpha
        Gem selectedGem = gemGridLayout[touchPosX, touchPosY];
        selectedGem.gemGridObj.GetComponent<SpriteRenderer>().color = ChangeGemAlpha(selectedGem, underlayAlpha);
        //create Gem Clone at same location
        MakeGemClone(selectedGem, touchPosX, touchPosY);
        prevActiveTouchPos = new Vector2Int(touchPosX, touchPosY);
    }

    private int GetPosOnAxis(float main, int size)
    {
        int coordinate = Mathf.RoundToInt(main);
        if (coordinate < 0)
        {
            coordinate = 0;
        }
        if (coordinate > size - 1)
        {
            coordinate = size - 1;
        }
        return coordinate;
    }

    //change alpha for animation purposes
    private Color ChangeGemAlpha(Gem gem, float aVal)
    {
        Color gemColor = gem.gemGridObj.GetComponent<SpriteRenderer>().color;
        gemColor.a = aVal;
        return gemColor;
    }

    //show currently held gem
    private void MakeGemClone (Gem origGem, int x, int y)
    {
        gemClone = new Gem {
            gemGObj = origGem.gemGObj,
            gemGridObj = (GameObject)Instantiate(origGem.gemGridObj, new Vector2(x, y), Quaternion.identity),
            matchCountHoriz = 0,
            matchCountVert = 0,
            dropDist = 0,
            groupedHoriz = false,
            groupedVert = false,
            xLoc = 0,
            yLoc = 0,
            destroyed = false,
        };
        gemClone.gemGridObj.GetComponent<SpriteRenderer>().color = ChangeGemAlpha(gemClone, overlayAlpha);
    }

    //wrapper for gem swap coroutine
    private void ShowGemMovement(Vector2 touchPos)
    {
        int touchPosX = GetPosOnAxis(touchPos.x, boardDimX);
        int touchPosY = GetPosOnAxis(touchPos.y, boardDimY);
    // Debug.Log("Current finger pos: [" + touchPosX + ", " + touchPosY + "]");
        //Updates gem movement when finger moves to new cell and as fast as the rotation happens.
        if ((prevActiveTouchPos.x != touchPosX || prevActiveTouchPos.y != touchPosY) && isRotating == false)
        {
            if(touchPosX - prevActiveTouchPos.x > 0)
            {
                touchPosX = prevActiveTouchPos.x + 1;
            } else if (touchPosX - prevActiveTouchPos.x < 0)
            {
                touchPosX = prevActiveTouchPos.x - 1;
            } else if (touchPosY - prevActiveTouchPos.y > 0)
            {
                touchPosY = prevActiveTouchPos.y + 1;
            } else if (touchPosY - prevActiveTouchPos.y < 0)
            {
                touchPosY = prevActiveTouchPos.y - 1;
            }
            StartCoroutine(ShowGemMovementEnum(touchPosX, touchPosY));
        } 
    }

    IEnumerator ShowGemMovementEnum(int currTouchPosX, int currTouchPosY)
    {
        //inits
    // Debug.Log("Prev Finger Pos: [" + prevActiveTouchPos.x + ", " + prevActiveTouchPos.y + "]");
        isRotating = true;
        float rotatePercent = 0.0f;
        rotationAngle = new Vector3(0, 0, 180.0f);
        GameObject gemRotator = new GameObject();
        gemRotator.transform.position = new Vector2 ((float)prevActiveTouchPos.x - (float)(prevActiveTouchPos.x - currTouchPosX)/2.0f, (float)prevActiveTouchPos.y - (float)(prevActiveTouchPos.y - currTouchPosY)/2.0f);

        // update involved gems's parent to gem Rotator
        gemGridLayout[prevActiveTouchPos.x, prevActiveTouchPos.y].gemGridObj.transform.parent = gemGridLayout[currTouchPosX, currTouchPosY].gemGridObj.transform.parent = null;
        gemGridLayout[prevActiveTouchPos.x, prevActiveTouchPos.y].gemGridObj.transform.parent = gemGridLayout[currTouchPosX, currTouchPosY].gemGridObj.transform.parent = gemRotator.transform;

        //rotate to desired positions
        while(rotatePercent <= 1.0f)
        {
            gemRotator.transform.eulerAngles = Vector3.Lerp(Vector3.zero, rotationAngle, rotatePercent);
            rotatePercent += rotatePercentIncrease;
            yield return new WaitForSeconds(rotationTimeInterval);
        }

        //finalize rotation and movements
        gemRotator.transform.eulerAngles = rotationAngle;

        //reparent/unparent appropriately
        gemGridLayout[prevActiveTouchPos.x, prevActiveTouchPos.y].gemGridObj.transform.parent = gemGridLayout[currTouchPosX, currTouchPosY].gemGridObj.transform.parent = null;
        gemGridLayout[prevActiveTouchPos.x, prevActiveTouchPos.y].gemGridObj.transform.parent = gemGridLayout[currTouchPosX, currTouchPosY].gemGridObj.transform.parent = transform;

        //swap old gem and new gem in grid
        Gem tempGem = gemGridLayout[prevActiveTouchPos.x, prevActiveTouchPos.y];
        gemGridLayout[prevActiveTouchPos.x, prevActiveTouchPos.y] = gemGridLayout[currTouchPosX, currTouchPosY];
        gemGridLayout[currTouchPosX, currTouchPosY] = tempGem;

        //update touch position
        prevActiveTouchPos.x = currTouchPosX;
        prevActiveTouchPos.y = currTouchPosY;

        //Cleanup
        Destroy(gemRotator);
        isRotating = false;
    }

    private void DropGem()
    {
        gemGridLayout[prevActiveTouchPos.x, prevActiveTouchPos.y].gemGridObj.GetComponent<SpriteRenderer>().color = ChangeGemAlpha(gemGridLayout[prevActiveTouchPos.x, prevActiveTouchPos.y], 1.0f);
        if (gemClone.gemGridObj != null)
        {
            Destroy(gemClone.gemGridObj);
        }
    }

    //actually match gems together and pop them
    IEnumerator MatchGems()
    {
        gridLocked = true;
        ResetBoardForMatching();
        gemListToDestroy = new List<Gem>();
        //Horizontal check first
        for (int y = 0; y < boardDimY; y++)
        {
            for (int x = 0; x < boardDimX - 1; x++)
            {
                if(!gemGridLayout[x,y].groupedHoriz)
                {
                    List<Gem> gemGroupList = new List<Gem>();
                    Queue gemQ = new Queue();
                    gemQ.Enqueue(gemGridLayout[x,y]);
                    //Q each gem in grid in succession, if DQ'd gem is same as Orig, add the gem on the right to the Q
                    while (gemQ.Count != 0)
                    {
                        Gem gemLFM = (Gem)gemQ.Dequeue();
                        if (gemLFM.gemGObj == gemGridLayout[x,y].gemGObj)
                        {
                            gemGroupList.Add(gemGridLayout[gemLFM.xLoc, gemLFM.yLoc]);
                            //Look right of current gem
                            if (gemLFM.xLoc < boardDimX - 1)
                            {
                                gemQ.Enqueue(gemGridLayout[gemLFM.xLoc + 1, gemLFM.yLoc]);
                            }
                        }
                    }
                    //Mark gems for Destruction if 3 gems are grouped Horizontally
                    for (int i = 0; i < gemGroupList.Count; i++ )
                    {
                        gemGridLayout[gemGroupList[i].xLoc, gemGroupList[i].yLoc].groupedHoriz = true;
                        //need 3 to be deleted
                        if (gemGroupList.Count >= 3) {
                            gemListToDestroy.Add(gemGroupList[i]);
                        }
                    }
                }
            }
        }

        //Vertical check next
        for (int y = 0; y < boardDimY - 1; y++)
        {
            for (int x = 0; x < boardDimX; x++)
            {
                if(!gemGridLayout[x,y].groupedVert)
                {
                    List<Gem> gemGroupList = new List<Gem>();
                    Queue gemQ = new Queue();
                    gemQ.Enqueue(gemGridLayout[x,y]);
                    //Q each gem in grid in succession, if DQ'd gem is same as Orig, add the gem on the right to the Q
                    while (gemQ.Count != 0)
                    {
                        Gem gemLFM = (Gem)gemQ.Dequeue();
                        if (gemLFM.gemGObj == gemGridLayout[x,y].gemGObj)
                        {
                            gemGroupList.Add(gemGridLayout[gemLFM.xLoc, gemLFM.yLoc]);
                            //Look Up of current gem
                            if (gemLFM.yLoc < boardDimY - 1)
                            {
                                gemQ.Enqueue(gemGridLayout[gemLFM.xLoc, gemLFM.yLoc + 1]);
                            }
                        }
                    }
                    //Mark gems for Destruction if 3 gems are grouped Vertically
                    for (int i = 0; i < gemGroupList.Count; i++ )
                    {
                        gemGridLayout[gemGroupList[i].xLoc, gemGroupList[i].yLoc].groupedVert = true;
                        //need 3 to be matched
                        if (gemGroupList.Count >= 3) {
                            gemListToDestroy.Add(gemGroupList[i]);
                        }
                    }
                }
            }
        }

        //Destroy matched gems and check how many were destroyed
        int count = 0;
        for (int i = 0; i < gemListToDestroy.Count; i++)
        {
            int a = gemListToDestroy[i].xLoc;
            int b = gemListToDestroy[i].yLoc;
            if (!gemGridLayout[a, b].destroyed)
            {
                gemGridLayout[a, b].destroyed = true;
                Destroy(gemListToDestroy[i].gemGridObj);
                count++;
            }
        }
        if (count == 0)
        {
            yield return new WaitForSeconds(.01f);
            gridLocked = false;
        } else {
            //Rule 00
            inGameTimerOn = false;
            yield return new WaitForSeconds(1.0f);
            //give count and ingame timer
            gameManagerScript.GameOver(inGameTimer, count);
            //
            MoveLeftoverGemsDown();
            MoveNewGemsDown();
        }
    }

    private void MoveLeftoverGemsDown()
    {
        for (int y = 1; y < boardDimY; y++)
        {
            for (int x = 0; x < boardDimX; x++)
            {
                //if not a gem that was blown up
                if (!gemGridLayout[x, y].destroyed)
                {
                    int dropDistance = 0;
                    for (int i = 1; i <= y; i++)
                    {
                        if (gemGridLayout[x, y - i].destroyed)
                        {
                            dropDistance++;
                        }
                    }
                    if (dropDistance > 0)
                    {
                        gemGridLayout[x, y].dropDist = dropDistance;
                        Gem temp = gemGridLayout[x, y - dropDistance];
                        gemGridLayout[x, y - dropDistance] = gemGridLayout[x, y];
                        gemGridLayout[x, y] = temp;
                    }
                }
            }
        }
    }

    private void MoveNewGemsDown()
    {
        for (int y = 0; y < boardDimY; y++)
        {
            for (int x = 0; x < boardDimX; x++)
            {
                if (gemGridLayout[x,y].destroyed)
                {
                    GameObject newGem = gemOptions[UnityEngine.Random.Range(0, gemOptions.Count)];
                    MakeNewGem(newGem, x, y);
                }
            }
        }
        MoveGemsDown();
        ResetBoardForMatching();
        StartCoroutine(RepeatMatchGems());
    }

    private void ResetBoardForMatching()
    {
        for (int y =0; y < boardDimY; y++)
        {
            for (int x =0; x < boardDimX; x++)
            {
                gemGridLayout[x,y].dropDist = 0;
                gemGridLayout[x,y].groupedHoriz = false;
                gemGridLayout[x,y].groupedVert = false;
                gemGridLayout[x,y].xLoc = x;
                gemGridLayout[x,y].yLoc = y;
                gemGridLayout[x,y].destroyed = false;                
            }
        }
    }

    private void checkBoardStatusHelper()
    {
        for (int y = 0; y < boardDimY; y++)
        {
            for (int x = 0; x < boardDimX; x++)
            {
                Debug.Log("Coordinates: [" + x + ", " + y + "]");
                Debug.Log("[DropDistance, xLoc, yLoc]: [" + gemGridLayout[x, y].dropDist + ", " + gemGridLayout[x,y].xLoc + ", " + gemGridLayout[x, y].yLoc + "]");
                Debug.Log("[groupedH, groupedV, Destroyed]: [" + gemGridLayout[x, y].groupedHoriz.ToString() + ", " + gemGridLayout[x,y].groupedVert.ToString() + ", " + gemGridLayout[x, y].destroyed.ToString() + "]");
                Debug.Log("Color: " + gemGridLayout[x, y].gemGObj);
            }
        }
    }

    IEnumerator RepeatMatchGems()
    {
        yield return new WaitUntil(() => !isShifting);
        StartCoroutine(MatchGems());
    }
}