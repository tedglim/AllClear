using System.Net.NetworkInformation;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TutorialScript : MonoBehaviour
{
    private struct Gemm
    {
        public GameObject gemGObj;
        public int dropDist;
        public int gridXLoc;
        public int gridYLoc;
    }
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
    public int dropOffset;
    private Gem[,] gemGridLayout;
    private Gemm[,] gemmGridLayout;
    public int minDots = 3;
    private int redCount;
    private int blueCount;
    private int yellowCount;
    private int greenCount;


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
    public float rotatePercentIncrease;
    private Vector3 rotationAngle;
    private bool isRotating = false;
    private bool gridLocked = false;
    private bool isShifting = false;

    private List<Gem> gemListToDestroy = new List<Gem>();
    private bool rainCheck = false;

    public Text text3;
    public Text text4;
    public Toggle timerToggle;
    private bool toggleOn;
    private bool isStarting;
    private int shiftCount = 0;
    // private GameManagerScript gameManagerScript;
    private float inGameTimer = 0.0f;
    private bool inGameTimerOn = true;
    
    private bool isFirstDrop;
    [SerializeField]
    //moves grid up/down screen; "+" = down, "-" = up
    private float gridScreenOffset;
    private Gemm gemmClone;
    private bool isGemmSelected;
    private bool canGemmFollowMe;
    private bool wantGemmDrop;
    private bool isGemmCloneAlive;


    // declare inits and get script references
    void Awake()
    {
        GameEventsScript.gameIsOver.AddListener(GameOver);
    }

    private void GameOver()
    {
        SceneManager.LoadSceneAsync(3);
    }

    void Start()
    {
        isFirstDrop = true;
        StartCoroutine(SetupTutorialBoard());
    }

    //Wrapper for creating initial board
    IEnumerator SetupTutorialBoard()
    {
        yield return new WaitForSecondsRealtime(1.0f);

        MakeGemmsInGrid();
        // checkGridState();
        MoveGemmsDown();
        // checkGridState();
        isFirstDrop = false;
    }

    //Create Gemms in Grid
    private void MakeGemmsInGrid()
    {
        gemmGridLayout = new Gemm[boardDimX, boardDimY];
        for (int y = 0; y < boardDimY; y++)
        {
            for (int x = 0; x < boardDimX; x++)
            {
                int z = x + 2;
                GameObject gemm;

                //if rows 3/6, shift gem to the left by 1 color
                if (y % gemOptions.Count == gemOptions.Count - 1)
                {
                    gemm = gemOptions[z%gemOptions.Count];
                } else 
                {
                    gemm = gemOptions[x%gemOptions.Count];
                }
                MakeGemm(gemm, x, y);
            }
        }
    }

    //Instantiate Gemm
    private void MakeGemm(GameObject gemGObj, int x, int y)
    {
        GameObject currentGem = (GameObject)Instantiate(gemGObj, new Vector2((float)x, (float)y + (float)dropOffset), Quaternion.identity);
        currentGem.transform.parent = transform;
        gemmGridLayout[x, y] = new Gemm {
            gemGObj = currentGem,
            dropDist = dropOffset,
            gridXLoc = x,
            gridYLoc = y,
        };
    }

    //Wrapper to move Gemms down to be in the screen
    private void MoveGemmsDown()
    {
        for(int y = 0; y < boardDimY; y++)
        {
            for(int x = 0; x < boardDimX; x++)
            {
                if(gemmGridLayout[x, y].dropDist > 0)
                {
                    Vector2 startPos = gemmGridLayout[x, y].gemGObj.transform.position;
                    Vector2 endPos = new Vector2(gemmGridLayout[x, y].gemGObj.transform.position.x, gemmGridLayout[x, y].gemGObj.transform.position.y - gemmGridLayout[x, y].dropDist);
                    StartCoroutine(MoveGemsDownEnum(gemmGridLayout[x, y], startPos, endPos));
                    gemmGridLayout[x, y].dropDist = 0;
                }
            }
        }
    }

    //animate move Gemms down
    IEnumerator MoveGemsDownEnum(Gemm gemm, Vector2 start, Vector2 end)
    {
        float fallPercent = 0.0f;
        while(fallPercent <= 1.0f)
        {
            gemm.gemGObj.transform.position = Vector2.Lerp(start, end, fallPercent);
            fallPercent += fallPercentIncrease;
            yield return new WaitForSeconds(fallTimeInterval);
        }
        gemm.gemGObj.transform.position = end;
    }

    //Debug function for diff attributes
    private void checkGridState()
    {
        for (int y = 0; y < boardDimY; y++)
        {
            for (int x = 0; x < boardDimX; x++)
            {
                Debug.Log("Coordinates: [" + x + ", " + y + "]" + "\n" + 
                "[DropDistance, gridX, gridY]: [" + gemmGridLayout[x, y].dropDist + ", " + gemmGridLayout[x,y].gridXLoc + ", " + gemmGridLayout[x, y].gridYLoc + "]");
                Debug.Log("Color: " + gemmGridLayout[x, y].gemGObj + "\n" +
                "World Space: " + gemmGridLayout[x, y].gemGObj.transform.position);
            }
        }
    }

    void Update()
    {
        if (isFirstDrop)
        {
            return;
        }
        if (Input.GetMouseButtonDown(0))
        {
            touchPos = Camera.main.ScreenPointToRay(Input.mousePosition);
            
            if (Mathf.RoundToInt(touchPos.origin.x) < boardDimX && Mathf.RoundToInt(touchPos.origin.x) > -1 && Mathf.RoundToInt(touchPos.origin.y) < boardDimY && Mathf.RoundToInt(touchPos.origin.y) > -1)
            {
                Debug.Log("Touch Pos: " + touchPos.origin);
                // checkGridState();
                isGemmSelected = true;
                DisplayGemmClone(touchPos.origin);
            }
        // }
        } else if (Input.GetMouseButton(0))
        {
            touchPos = Camera.main.ScreenPointToRay(Input.mousePosition);
            // touchPos.origin = new Vector3(touchPos.origin.x, touchPos.origin.y + gridScreenOffset, touchPos.origin.z);

            if (Mathf.RoundToInt(touchPos.origin.x) < boardDimX && Mathf.RoundToInt(touchPos.origin.x) > -1 && Mathf.RoundToInt(touchPos.origin.y) < boardDimY && Mathf.RoundToInt(touchPos.origin.y) > -1)
            {
                canGemmFollowMe = true;
            } 
        // } 
        } else if (Input.GetMouseButtonUp(0))
        {
            wantGemmDrop = true;
            isGemmSelected = canGemmFollowMe = false;
        }
    }

    void FixedUpdate()
    {
        if (isFirstDrop)
        {
            return;
        }
        if(isGemmSelected && canGemmFollowMe)
        {
            if(isGemmCloneAlive)
            {
                gemmClone.gemGObj.transform.Translate((touchPos.origin - gemmClone.gemGObj.transform.position) * Time.deltaTime * moveSpeed);
                ShowGemmMovement(touchPos.origin);
            }
        }
        if (wantGemmDrop)
        {
            DropGemm();
            // checkGridState();
        }
    }

    //Wrapper for showing gemm clone
    private void DisplayGemmClone(Vector2 touchPos)
    {
        Debug.Log("X before: " + touchPos.x);
        int gridXPos = GetPosOnGrid(touchPos.x, boardDimX);
        Debug.Log("X after: " + gridXPos);
        Debug.Log("Y before: " + touchPos.y);
        int gridYPos = GetPosOnGrid(touchPos.y, boardDimY);
        Debug.Log("Y after: " + gridYPos);
        prevActiveTouchPos = new Vector2Int(gridXPos, gridYPos);

        //get Gem in grid
        Gemm selectedGem = gemmGridLayout[gridXPos, gridYPos];
        //create Gem Clone at same location
        MakeGemmClone(selectedGem, gridXPos, gridYPos);
        //change its alpha
        selectedGem.gemGObj.GetComponent<SpriteRenderer>().color = ChangeGemmAlpha(selectedGem, underlayAlpha);
    }

    private int GetPosOnGrid(float main, int size)
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

    //makes gemm clone to display movement
    private void MakeGemmClone (Gemm origGem, int x, int y)
    {
        // Debug.Log("place pos to make gemm: " + x + ", " + y);
        gemmClone = new Gemm {
            gemGObj = (GameObject)Instantiate(origGem.gemGObj, new Vector2(x, y), Quaternion.identity),
            dropDist = 0,
            gridXLoc = x,
            gridYLoc = y,
        };
        gemmClone.gemGObj.GetComponent<SpriteRenderer>().color = ChangeGemmAlpha(gemmClone, overlayAlpha);
        isGemmCloneAlive = true;
    }

    //adjusts gemm transparency
    private Color ChangeGemmAlpha(Gemm gem, float aVal)
    {
        Color gemColor = gem.gemGObj.GetComponent<SpriteRenderer>().color;
        gemColor.a = aVal;
        return gemColor;
    }

    //wrapper for gem swap coroutine
    private void ShowGemmMovement(Vector2 touchPos)
    {
        int gridXPos = GetPosOnGrid(touchPos.x, boardDimX);
        int gridYPos = GetPosOnGrid(touchPos.y, boardDimY);

        //Updates gem movement when finger moves to new cell and as fast as the rotation happens.
        if ((prevActiveTouchPos.x != gridXPos || prevActiveTouchPos.y != gridYPos) && !isRotating)
        {
            if(gridXPos - prevActiveTouchPos.x > 0)
            {
                gridXPos = prevActiveTouchPos.x + 1;
                StartCoroutine(ShowGemmMovementEnum(gridXPos, prevActiveTouchPos.y));
            } else if (gridXPos - prevActiveTouchPos.x < 0)
            {
                gridXPos = prevActiveTouchPos.x - 1;
                StartCoroutine(ShowGemmMovementEnum(gridXPos, prevActiveTouchPos.y));
            } else if (gridYPos - prevActiveTouchPos.y > 0)
            {
                gridYPos = prevActiveTouchPos.y + 1;
                StartCoroutine(ShowGemmMovementEnum(prevActiveTouchPos.x, gridYPos));
            } else if (gridYPos - prevActiveTouchPos.y < 0)
            {
                gridYPos = prevActiveTouchPos.y - 1;
                StartCoroutine(ShowGemmMovementEnum(prevActiveTouchPos.x, gridYPos));
            }
        } 
    }

    IEnumerator ShowGemmMovementEnum(int currTouchPosX, int currTouchPosY)
    {
        //inits
        isRotating = true;
        float rotatePercent = 0.0f;
        rotationAngle = new Vector3(0, 0, 180.0f);
        GameObject gemRotator = new GameObject();
        gemRotator.transform.position = new Vector2 ((float)prevActiveTouchPos.x - (float)(prevActiveTouchPos.x - currTouchPosX)/2.0f, (float)prevActiveTouchPos.y - (float)(prevActiveTouchPos.y - currTouchPosY)/2.0f);

        // update involved gems's parent to gem Rotator
        gemmGridLayout[prevActiveTouchPos.x, prevActiveTouchPos.y].gemGObj.transform.Rotate(0.0f, 0.0f, 180.0f, Space.Self);        
        gemmGridLayout[currTouchPosX, currTouchPosY].gemGObj.transform.Rotate(0.0f, 0.0f, 180.0f, Space.Self);
        gemmGridLayout[prevActiveTouchPos.x, prevActiveTouchPos.y].gemGObj.transform.parent = gemmGridLayout[currTouchPosX, currTouchPosY].gemGObj.transform.parent = null;
        gemmGridLayout[prevActiveTouchPos.x, prevActiveTouchPos.y].gemGObj.transform.parent = gemmGridLayout[currTouchPosX, currTouchPosY].gemGObj.transform.parent = gemRotator.transform;

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
        gemmGridLayout[prevActiveTouchPos.x, prevActiveTouchPos.y].gemGObj.transform.parent = gemmGridLayout[currTouchPosX, currTouchPosY].gemGObj.transform.parent = null;
        gemmGridLayout[prevActiveTouchPos.x, prevActiveTouchPos.y].gemGObj.transform.parent = gemmGridLayout[currTouchPosX, currTouchPosY].gemGObj.transform.parent = transform;

        //swap old gem and new gem in grid
        Gemm tempGem = gemmGridLayout[prevActiveTouchPos.x, prevActiveTouchPos.y];
        gemmGridLayout[prevActiveTouchPos.x, prevActiveTouchPos.y] = gemmGridLayout[currTouchPosX, currTouchPosY];
        gemmGridLayout[currTouchPosX, currTouchPosY] = tempGem;

        //update touch position
        prevActiveTouchPos.x = currTouchPosX;
        prevActiveTouchPos.y = currTouchPosY;

        //Cleanup
        Destroy(gemRotator);
        isRotating = false;
    }

    //ends player turn
    private void DropGemm()
    {
        gemmGridLayout[prevActiveTouchPos.x, prevActiveTouchPos.y].gemGObj.GetComponent<SpriteRenderer>().color = ChangeGemmAlpha(gemmGridLayout[prevActiveTouchPos.x, prevActiveTouchPos.y], 1.0f);
        if (gemmClone.gemGObj != null)
        {
            Destroy(gemmClone.gemGObj);
        }
        wantGemmDrop = false;
        isGemmCloneAlive = false;
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
                count++;
            }
        }
        if (count == 0)
        {
            // inGameTimerOn = true;
            yield return new WaitForSeconds(0.1f);
            gridLocked = false;
        } else {
            // float tempInGameTimer = inGameTimer;
            // inGameTimerOn=false;
            yield return new WaitForSeconds(1.0f);
            for (int i = 0; i < gemListToDestroy.Count; i++)
            {
                int a = gemListToDestroy[i].xLoc;
                int b = gemListToDestroy[i].yLoc;
                if (gemGridLayout[a, b].destroyed)
                {
                    Destroy(gemListToDestroy[i].gemGridObj);
                }
            }
            //Rule 00
            int gemTotal = boardDimX * boardDimY;
            if (count == gemTotal)
            {
                text4.text = "ALL-CLEAR";
                yield return new WaitForSeconds(2.0f);
                text4.text = "";
            } else
            {
                yield return new WaitForSeconds(2.0f);                
            }
            //give count and ingame timer
            PlayStatsScript.GemsCleared = count;
            // PlayStatsScript.Time = tempInGameTimer;
            GameEventsScript.gameIsOver.Invoke();
            // gameManagerScript.GameOver(tempInGameTimer, count);
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
                    // MakeNewGem(newGem, x, y);
                }
            }
        }
        // MoveGemsDown();
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

    private void ClearBoard()
    {
        for (int y =0; y < boardDimY; y++)
        {
            for (int x =0; x < boardDimX; x++)
            {
                Destroy(gemGridLayout[x,y].gemGridObj);               
            }
        }
        redCount = 0;
        // yellowCount = 0;
        blueCount = 0;
        greenCount = 0;
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