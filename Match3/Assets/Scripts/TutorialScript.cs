using System.Net.NetworkInformation;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TutorialScript : MonoBehaviour
{
//booleans
    private bool isFirstDrop;
    private bool areGemmsFalling;
    private bool isGemmSelected;
    private bool isGemmCloneAlive;
    private bool canGemmFollowMe;
    private bool isRotating;
    private bool wantGemmDrop;
    private bool isMatching;
    private bool didDestroy;
    private bool canContinueMatching;

//serial values
    [SerializeField]
    private int boardDimX;
    [SerializeField]
    private int boardDimY;
    [SerializeField]
    private int dropOffset;
    [SerializeField]
    private float fallTimeInterval;
    [SerializeField]
    private float fallPercentIncrease;
    [SerializeField]
    private float underlayAlpha;
    [SerializeField]
    private float overlayAlpha;
    [SerializeField]
    private float moveSpeed;
    [SerializeField]
    private float rotationTimeInterval;
    [SerializeField]
    private float rotatePercentIncrease;
    
//nonserial values
    private float redsDestroyed;
    private float greensDestroyed;
    private float cyansDestroyed;

//other
    [SerializeField]
    private List<GameObject> GemmOptions;
    private Gemm[,] GemmGridLayout;
    private struct Gemm
    {
        public GameObject gemmGObj;
        public bool destroyed;
        public int dropDist;
        public int gridXLoc;
        public int gridYLoc;
    }
    private struct GemmLoc
    {
        public int gridXLoc;
        public int gridYLoc;
    }
    private Ray touchPos;
    private Vector2Int prevActiveTouchPos;
    private Vector3 rotationAngle;
    private Gemm gemmClone;
    private Dictionary<GemmLoc, Gemm> CurrGemmMatchDict;
    private Dictionary<GemmLoc, Gemm> GemmDictToDestroy;

    // declare inits and get script references
    void Awake()
    {
        GameEventsScript.gameIsOver.AddListener(GameOver);
        CurrGemmMatchDict = new Dictionary<GemmLoc, Gemm>();
        GemmDictToDestroy = new Dictionary<GemmLoc, Gemm>();
    }

    private void GameOver()
    {
        SceneManager.LoadSceneAsync(3);
    }

    void Start()
    {
        isMatching = false;
        areGemmsFalling = false;
        isFirstDrop = true;
        StartCoroutine(SetupTutorialBoard());
    }

    void Update()
    {
        if (isFirstDrop)
        {
            return;
        }
        if (isMatching)
        {
            return;
        }
        if (areGemmsFalling)
        {
            return;
        }
        if (Input.GetMouseButtonDown(0))
        {
            touchPos = Camera.main.ScreenPointToRay(Input.mousePosition);
            
            if (Mathf.RoundToInt(touchPos.origin.x) < boardDimX && Mathf.RoundToInt(touchPos.origin.x) > -1 && Mathf.RoundToInt(touchPos.origin.y) < boardDimY && Mathf.RoundToInt(touchPos.origin.y) > -1)
            {
                isGemmSelected = true;
                DisplayGemmClone(touchPos.origin);
            }
        // }
        } else if (Input.GetMouseButton(0))
        {
            touchPos = Camera.main.ScreenPointToRay(Input.mousePosition);

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
        if (areGemmsFalling)
        {
            return;
        }
        if (isMatching)
        {
            return;
        }
        if(isGemmSelected && canGemmFollowMe)
        {
            if(isGemmCloneAlive)
            {
                gemmClone.gemmGObj.transform.Translate((touchPos.origin - gemmClone.gemmGObj.transform.position) * Time.fixedDeltaTime * moveSpeed);
                ShowGemmMovement(touchPos.origin);
            }
        }
        if (wantGemmDrop)
        {
            DropGemm();
            StartCoroutine(MatchGemms());
        }
    }

    //Wrapper for creating initial board
    IEnumerator SetupTutorialBoard()
    {
        yield return new WaitForSecondsRealtime(1.0f);
        MakeGemmsInGrid();
        MoveGemmsDown();
        isFirstDrop = false;
    }

    //Create Gemms in Grid
    private void MakeGemmsInGrid()
    {
        GemmGridLayout = new Gemm[boardDimX, boardDimY];
        for (int y = 0; y < boardDimY; y++)
        {
            for (int x = 0; x < boardDimX; x++)
            {
                int z = x + 2;
                GameObject gemm;

                //if rows 3/6, shift gem to the left by 1 color
                if (y % GemmOptions.Count == GemmOptions.Count - 1)
                {
                    gemm = GemmOptions[z%GemmOptions.Count];
                } else 
                {
                    gemm = GemmOptions[x%GemmOptions.Count];
                }
                MakeGemm(gemm, x, y);
            }
        }
    }

    //Instantiate Gemm
    private void MakeGemm(GameObject gemmGObj, int x, int y)
    {
        GameObject currentGemm = (GameObject)Instantiate(gemmGObj, new Vector2((float)x, (float)y + (float)dropOffset), Quaternion.identity);
        currentGemm.transform.parent = transform;
        GemmGridLayout[x, y] = new Gemm {
            gemmGObj = currentGemm,
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
                if(GemmGridLayout[x, y].dropDist > 0)
                {
                    Vector2 startPos = GemmGridLayout[x, y].gemmGObj.transform.position;
                    Vector2 endPos = new Vector2(GemmGridLayout[x, y].gemmGObj.transform.position.x, GemmGridLayout[x, y].gemmGObj.transform.position.y - GemmGridLayout[x, y].dropDist);
                    StartCoroutine(MoveGemsDownEnum(GemmGridLayout[x, y], startPos, endPos));
                    GemmGridLayout[x, y].dropDist = 0;
                }
            }
        }
    }

    //animate move Gemms down
    IEnumerator MoveGemsDownEnum(Gemm gemm, Vector2 start, Vector2 end)
    {
        areGemmsFalling = true;
        float fallPercent = 0.0f;
        while(fallPercent <= 1.0f)
        {
            gemm.gemmGObj.transform.position = Vector2.Lerp(start, end, fallPercent);
            fallPercent += fallPercentIncrease;
            yield return new WaitForSeconds(fallTimeInterval);
        }
        gemm.gemmGObj.transform.position = end;
        areGemmsFalling = false;
    }

    //Wrapper for showing gemm clone
    private void DisplayGemmClone(Vector2 touchPos)
    {
        int gridXPos = GetPosOnGrid(touchPos.x, boardDimX);
        int gridYPos = GetPosOnGrid(touchPos.y, boardDimY);
        prevActiveTouchPos = new Vector2Int(gridXPos, gridYPos);

        //get Gem in grid
        Gemm selectedGem = GemmGridLayout[gridXPos, gridYPos];
        //create Gem Clone at same location
        MakeGemmClone(selectedGem, gridXPos, gridYPos);
        //change its alpha
        selectedGem.gemmGObj.GetComponent<SpriteRenderer>().color = ChangeGemmAlpha(selectedGem, underlayAlpha);
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
    private void MakeGemmClone (Gemm origGemm, int x, int y)
    {
        gemmClone = new Gemm {
            gemmGObj = (GameObject)Instantiate(origGemm.gemmGObj, new Vector2(x, y), Quaternion.identity),
            dropDist = 0,
            gridXLoc = x,
            gridYLoc = y,
        };
        gemmClone.gemmGObj.GetComponent<SpriteRenderer>().color = ChangeGemmAlpha(gemmClone, overlayAlpha);
        isGemmCloneAlive = true;
    }

    //adjusts gemm transparency
    private Color ChangeGemmAlpha(Gemm gemm, float aVal)
    {
        Color gemmColor = gemm.gemmGObj.GetComponent<SpriteRenderer>().color;
        gemmColor.a = aVal;
        return gemmColor;
    }

    //wrapper for gem swap coroutine
    private void ShowGemmMovement(Vector2 touchPos)
    {
        int gridXPos = GetPosOnGrid(touchPos.x, boardDimX);
        int gridYPos = GetPosOnGrid(touchPos.y, boardDimY);

        //Updates gem movement when finger moves to new cell and as fast as the rotation happens.
        if ((prevActiveTouchPos.x != gridXPos || prevActiveTouchPos.y != gridYPos) && !isRotating)
        {
            //diagonals
            if(gridXPos - prevActiveTouchPos.x > 0 && gridYPos - prevActiveTouchPos.y > 0)
            {
                gridXPos = prevActiveTouchPos.x + 1;
                gridYPos = prevActiveTouchPos.y + 1;
                StartCoroutine(ShowGemmMovementEnum(gridXPos, gridYPos));
            } else if (gridXPos - prevActiveTouchPos.x > 0 && gridYPos - prevActiveTouchPos.y < 0)
            {
                gridXPos = prevActiveTouchPos.x + 1;
                gridYPos = prevActiveTouchPos.y - 1;
                StartCoroutine(ShowGemmMovementEnum(gridXPos, gridYPos));
            } else if (gridXPos - prevActiveTouchPos.x < 0 && gridYPos - prevActiveTouchPos.y < 0)
            {
                gridXPos = prevActiveTouchPos.x - 1;
                gridYPos = prevActiveTouchPos.y - 1;
                StartCoroutine(ShowGemmMovementEnum(gridXPos, gridYPos));
            } else if (gridXPos - prevActiveTouchPos.x < 0 && gridYPos - prevActiveTouchPos.y > 0)
            {
                gridXPos = prevActiveTouchPos.x - 1;
                gridYPos = prevActiveTouchPos.y + 1;
                StartCoroutine(ShowGemmMovementEnum(gridXPos, gridYPos));
            //cardinal directions
            } else if(gridXPos - prevActiveTouchPos.x > 0)
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

    //shows rotation speed of gemm movement
    IEnumerator ShowGemmMovementEnum(int currTouchPosX, int currTouchPosY)
    {
        //inits
        isRotating = true;
        float rotatePercent = 0.0f;
        rotationAngle = new Vector3(0, 0, 180.0f);
        GameObject gemRotator = new GameObject();
        gemRotator.transform.position = new Vector2 ((float)prevActiveTouchPos.x - (float)(prevActiveTouchPos.x - currTouchPosX)/2.0f, (float)prevActiveTouchPos.y - (float)(prevActiveTouchPos.y - currTouchPosY)/2.0f);

        // update involved gems's parent to gem Rotator
        GemmGridLayout[prevActiveTouchPos.x, prevActiveTouchPos.y].gemmGObj.transform.Rotate(0.0f, 0.0f, 180.0f, Space.Self);        
        GemmGridLayout[currTouchPosX, currTouchPosY].gemmGObj.transform.Rotate(0.0f, 0.0f, 180.0f, Space.Self);
        GemmGridLayout[prevActiveTouchPos.x, prevActiveTouchPos.y].gemmGObj.transform.parent = GemmGridLayout[currTouchPosX, currTouchPosY].gemmGObj.transform.parent = null;
        GemmGridLayout[prevActiveTouchPos.x, prevActiveTouchPos.y].gemmGObj.transform.parent = GemmGridLayout[currTouchPosX, currTouchPosY].gemmGObj.transform.parent = gemRotator.transform;

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
        GemmGridLayout[prevActiveTouchPos.x, prevActiveTouchPos.y].gemmGObj.transform.parent = GemmGridLayout[currTouchPosX, currTouchPosY].gemmGObj.transform.parent = null;
        GemmGridLayout[prevActiveTouchPos.x, prevActiveTouchPos.y].gemmGObj.transform.parent = GemmGridLayout[currTouchPosX, currTouchPosY].gemmGObj.transform.parent = transform;

        //swap old gem and new gem in grid
        Gemm tempGem = GemmGridLayout[prevActiveTouchPos.x, prevActiveTouchPos.y];
        GemmGridLayout[prevActiveTouchPos.x, prevActiveTouchPos.y] = GemmGridLayout[currTouchPosX, currTouchPosY];
        GemmGridLayout[currTouchPosX, currTouchPosY] = tempGem;

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
        GemmGridLayout[prevActiveTouchPos.x, prevActiveTouchPos.y].gemmGObj.GetComponent<SpriteRenderer>().color = ChangeGemmAlpha(GemmGridLayout[prevActiveTouchPos.x, prevActiveTouchPos.y], 1.0f);
        if (gemmClone.gemmGObj != null)
        {
            Destroy(gemmClone.gemmGObj);
        }
        wantGemmDrop = false;
        isGemmCloneAlive = false;
    }

    //Matches gemms; looks for 3 gemms or more in a row
    IEnumerator MatchGemms()
    {
        Debug.Log("Start Match");
        ResetBoardForMatching();
        isMatching = true;
        for (int y = 0; y < boardDimY; y++)
        {
            for (int x = 0; x < boardDimX; x++)
            {
                Debug.Log("CURRENT POSITION: " + x + ", " + y);
                if(!GemmGridLayout[x,y].destroyed)
                {
                    if (x >= boardDimX - 2)
                    {
                        //only do vert checks
                        CheckForMatches(x, y, false);
                    } else if (y >= boardDimY - 2)
                    {
                        //only do horiz checks
                        CheckForMatches(x, y, true);
                    } else 
                    {
                        //do both horiz and vert checks
                        CheckForMatches(x, y, false);
                        CheckForMatches(x, y, true);
                    }
                    CountAndDestroyGems();
                    if(didDestroy)
                    {
                        //if u destroyed shit, you should wait
                        yield return new WaitForSeconds(1.0f);
                        didDestroy = false;
                    }
                }
            }
        }
        //looped through everything
        SetupRemainingGemmsForDrop();
        RemakeDestroyedGemms();
        MoveGemmsDown();
//BUG AREA
        yield return new WaitUntil(() => !areGemmsFalling);
        Debug.Log("done moving");
        checkGridState();
//check list of boolean states?
        Debug.Log(canContinueMatching);
        Debug.Log(isMatching);

        if(canContinueMatching)
        {
            canContinueMatching = false;
            StartCoroutine(RepeatMatchGemms());
        } else
        {
            isMatching = false;
        }
    }

    private void ResetBoardForMatching()
    {
        for (int y =0; y < boardDimY; y++)
        {
            for (int x =0; x < boardDimX; x++)
            {
                GemmGridLayout[x,y].destroyed = false;
                GemmGridLayout[x,y].dropDist = 0;
                GemmGridLayout[x,y].gridXLoc = x;
                GemmGridLayout[x,y].gridYLoc = y;               
            }
        }
    }

    //chooses 1 gemm, gathers all matches linked to that gemm.
    private void CheckForMatches(int x, int y, bool isHoriz)
    {
        CurrGemmMatchDict.Clear();
        Queue gemmQ = new Queue();
        gemmQ.Enqueue(GemmGridLayout[x,y]);
        int qCount = 0;
        //Q each gem in grid in succession, if DQ'd gem is same as Orig, add gemm to Q
        while (gemmQ.Count != 0)
        {
            qCount++;
            Debug.Log("qCount iteration: " + qCount);
            Gemm gemmLFM = (Gemm)gemmQ.Dequeue();
            //if dQ'd gemm same as OG gemm
            if (gemmLFM.gemmGObj.tag == GemmGridLayout[x,y].gemmGObj.tag)
            {
                Debug.Log("gemms are same: ");
                //make key
                GemmLoc gemmLoc = new GemmLoc {
                    gridXLoc = gemmLFM.gridXLoc,
                    gridYLoc = gemmLFM.gridYLoc,
                };
                //if dictionary already has key,  
                if(CurrGemmMatchDict.ContainsKey(gemmLoc))
                {
                    //skip
                } else 
                {
                    //add to list of gemms that are of the same color
                    CurrGemmMatchDict.Add(gemmLoc, GemmGridLayout[gemmLFM.gridXLoc, gemmLFM.gridYLoc]);
                    //can check right
                    if(isHoriz)
                    {
                        if (gemmLFM.gridXLoc < boardDimX - 1)
                        {
                            if (!GemmGridLayout[gemmLFM.gridXLoc + 1, gemmLFM.gridYLoc].destroyed)
                            {
                                Debug.Log("Looked Right");
                                gemmQ.Enqueue(GemmGridLayout[gemmLFM.gridXLoc + 1, gemmLFM.gridYLoc]);
                            }
                        }
                    //can check up
                    } else
                    {
                        if (gemmLFM.gridYLoc < boardDimY - 1)
                        {
                            if (!GemmGridLayout[gemmLFM.gridXLoc, gemmLFM.gridYLoc + 1].destroyed)
                            {
                                Debug.Log("Looked Up");
                                gemmQ.Enqueue(GemmGridLayout[gemmLFM.gridXLoc, gemmLFM.gridYLoc + 1]);
                            }
                        }
                    } 
                }
            }
        }
        SetGemmsToDestroy();
    }

    //mark gemms as need to be destroyed
    private void SetGemmsToDestroy()
    {
        //Mark gems for Destruction if 3+ matched
        if(CurrGemmMatchDict.Count >= 3)
        {
            foreach(var gemm in CurrGemmMatchDict)
            {
                if(!GemmDictToDestroy.ContainsKey(gemm.Key))
                {
                    GemmDictToDestroy.Add(gemm.Key, gemm.Value);
                }
            }
        }
        Debug.Log("# of considered candidates to destroy: " + CurrGemmMatchDict.Count);
    }

    private void CountAndDestroyGems()
    {
        CountDestroyedGems();
        DestroyGems();
    }
    private void CountDestroyedGems()
    {
        foreach (var gemm in GemmDictToDestroy)
        {
            if(gemm.Value.gemmGObj.tag == "Red")
            {
                redsDestroyed++;
            } else if (gemm.Value.gemmGObj.tag == "Green")
            {
                greensDestroyed++;
            } else if (gemm.Value.gemmGObj.tag == "Cyan")
            {
                cyansDestroyed++;
            }
        }
    }

    private void DestroyGems()
    {
        Debug.Log("# Gems to destroy: " + GemmDictToDestroy.Count);
        foreach (var gemm in GemmDictToDestroy)
        {
            int a = gemm.Value.gridXLoc;
            int b = gemm.Value.gridYLoc;
            if (!GemmGridLayout[a, b].destroyed)
            {
                GemmGridLayout[a, b].destroyed = true;
                Destroy(gemm.Value.gemmGObj);
                didDestroy = true;
                canContinueMatching = true;
            }
        }
        Debug.Log("redsdestroyed: " + redsDestroyed);
        Debug.Log("greensdestroyed: " + greensDestroyed);
        Debug.Log("cyansdestroyed: " + cyansDestroyed);
        GemmDictToDestroy.Clear();
    }

    //if gemms were deleted, drop remaining gems to fill in gaps
    private void SetupRemainingGemmsForDrop()
    {
        for (int y = 1; y < boardDimY; y++)
        {
            for (int x = 0; x < boardDimX; x++)
            {
                if (!GemmGridLayout[x, y].destroyed)
                {
                    int dropDistance = 0;
                    for (int i = 1; i <= y; i++)
                    {
                        if (GemmGridLayout[x, y - i].destroyed)
                        {
                            dropDistance++;
                        }
                    }
                    if (dropDistance > 0)
                    {
                        Gemm temp = GemmGridLayout[x, y - dropDistance];
                        GemmGridLayout[x, y - dropDistance] = GemmGridLayout[x, y];
                        GemmGridLayout[x, y - dropDistance].dropDist = dropDistance;
                        GemmGridLayout[x, y] = temp;
                    }
                }
            }
        }
    }

    //Remake/Replace destroyed gemms
    private void RemakeDestroyedGemms()
    {
        for (int y = 0; y < boardDimY; y++)
        {
            for (int x = 0; x < boardDimX; x++)
            {
                if (GemmGridLayout[x,y].destroyed)
                {
                    GameObject newGemm = GemmOptions[UnityEngine.Random.Range(0, GemmOptions.Count)];
                    MakeGemm(newGemm, x, y);
                }
            }
        }
    }

    IEnumerator RepeatMatchGemms()
    {
        yield return new WaitUntil(() => !areGemmsFalling);
        checkGridState();
        StartCoroutine(MatchGemms());
    }

    //Debug function for diff attributes
    private void checkGridState()
    {
        for (int y = 0; y < boardDimY; y++)
        {
            for (int x = 0; x < boardDimX; x++)
            {
                Debug.Log("Coordinates: [" + x + ", " + y + "]" + "\n" + 
                "[DropDistance, gridX, gridY]: [" + GemmGridLayout[x, y].dropDist + ", " + GemmGridLayout[x,y].gridXLoc + ", " + GemmGridLayout[x, y].gridYLoc + "]");
                Debug.Log("Color: " + GemmGridLayout[x, y].gemmGObj.tag + "\n" +
                "World Space: " + GemmGridLayout[x, y].gemmGObj.transform.position);
            }
        }
    }
}