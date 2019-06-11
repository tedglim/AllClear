﻿using System.Net.NetworkInformation;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class BoardManagerScript : MonoBehaviour
{
    private struct Gem {
        public GameObject gemObj;
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
    private Gem[,] backupGemGrid;

    public List<GameObject> gemOptions;
    private GameObject randGem;
    private Gem leftGem;
    private Gem downGem;
    private int initHorizCount;
    private int initVertCount;

    public float fallTimeInterval = .01f;
    public float fallPercentIncrease = .05f;

    private Ray touchPos;
    public float underlayAlpha = .25f;
    public float overlayAlpha = .75f;
    private Gem gemClone;
    private Vector2Int currActiveTouchPos;
    public float moveSpeed = 25.0f;

    public float rotationTimeInterval = .000001f;
    public float rotatePercentIncrease = .25f;
    private Vector3 rotationAngle;
    private bool isRotating = false;
    private bool gridLocked = false;

    private Gem gemLFM;
    private List<Gem> gemListToDestroy = new List<Gem>();
    private bool rainCheck = false;



    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 30;
        InitBoard();
        MoveGemsDown();
    }

    // Update is called once per frame
    void Update()
    {
        if(gridLocked)
        {
            // Debug.Log("Gridlocked in update");
            return;
        }

        //On initial touch of screen
        if(Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            touchPos = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
            Vector2 touchStartPos = touchPos.origin;
            DisplayGemClone(touchStartPos);
            backupGemGrid = gemGridLayout;

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
            } else 
            {
                Debug.Log("NORMAL END");
                touchPos = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
                DropGem();
                gridLocked = true;
                StartCoroutine(MatchGems());
            }
        }
        if(rainCheck)
        {
            //raincheck version
            Debug.Log("RAINCHECK END");
            touchPos = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
            DropGem();
            gridLocked = true;
            StartCoroutine(MatchGems());
            rainCheck = false;  
        }
    }

    private void DropGem()
    {
        gemGridLayout[currActiveTouchPos.x, currActiveTouchPos.y].gemGridObj.GetComponent<SpriteRenderer>().color = ChangeGemAlpha(gemGridLayout[currActiveTouchPos.x, currActiveTouchPos.y], 1.0f);
        if (gemClone.gemGridObj != null)
        {
            Destroy(gemClone.gemGridObj);
            gemClone.destroyed = true;
        }
    }

    private void InitBoard()
    {
        gemGridLayout = new Gem[boardDimX, boardDimY];
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
                        if (randGem.Equals(leftGem.gemObj))
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
                        if (randGem.Equals(downGem.gemObj))
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
                GameObject currentGem = (GameObject)Instantiate(randGem, new Vector2((float)x, (float)y + (float)boardYDropOffset), Quaternion.identity);
                currentGem.transform.parent = transform;
                gemGridLayout[x, y] = new Gem {
                    gemObj = randGem,
                    gemGridObj = currentGem,
                    matchCountHoriz = initHorizCount,
                    matchCountVert = initVertCount,
                    dropDist = boardYDropOffset,
                    groupedHoriz = false,
                    groupedVert = false,
                    xLoc = 0,
                    yLoc = 0,
                    destroyed = false,
                };
            }
        }
    }

    private void MoveGemsDown()
    {
        for(int y = 0; y < boardDimY; y++)
        {
            for(int x = 0; x < boardDimX; x++)
            {
                Gem currGem = gemGridLayout[x, y];
                if(currGem.dropDist > 0)
                {
                    //set start and end pos before coroutine
                    Vector2 startPos = currGem.gemGridObj.transform.position;
                    Vector2 endPos = new Vector2(currGem.gemGridObj.transform.position.x, currGem.gemGridObj.transform.position.y - currGem.dropDist);
                    StartCoroutine(MoveGemsDownEnum(currGem, startPos, endPos));
                    //reset drop distance and matches
                    currGem.matchCountHoriz = 1;
                    currGem.matchCountVert = 1;
                    currGem.dropDist = 0;
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
    }

    private void DisplayGemClone(Vector2 touchPos)
    {
        //should include how to ignore wildly stupid finger placements
        int touchPosX = GetPosOnAxis(touchPos.x, boardDimX);
        int touchPosY = GetPosOnAxis(touchPos.y, boardDimY);
    // Debug.Log("Start Phase: X: " + touchPosX + ", Y: " + touchPosY);
        //get Gem in grid; change its alpha
        Gem selectedGem = gemGridLayout[touchPosX, touchPosY];
        selectedGem.gemGridObj.GetComponent<SpriteRenderer>().color = ChangeGemAlpha(selectedGem, underlayAlpha);
        //create new Gem at same location
        MakeGemClone(selectedGem, touchPosX, touchPosY);
        currActiveTouchPos = new Vector2Int(touchPosX, touchPosY);
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
            gemObj = origGem.gemObj,
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
    // Debug.Log("Clone Pos: " + x + ", " + y);
    }

    //wrapper for gem swap coroutine
    private void ShowGemMovement(Vector2 touchPos)
    {
        //should include how to ignore wildly stupid finger placements
        int touchPosX = GetPosOnAxis(touchPos.x, boardDimX);
        int touchPosY = GetPosOnAxis(touchPos.y, boardDimY);
    // Debug.Log("Move Phase: X: " + touchPosX + ", Y: " + touchPosY);

        if ((currActiveTouchPos.x != touchPosX || currActiveTouchPos.y != touchPosY) && isRotating == false)
        {
            if(touchPosX - currActiveTouchPos.x > 0)
            {
                touchPosX = currActiveTouchPos.x + 1;
            } else if (touchPosX - currActiveTouchPos.x < 0)
            {
                touchPosX = currActiveTouchPos.x - 1;
            } else if (touchPosY - currActiveTouchPos.y > 0)
            {
                touchPosY = currActiveTouchPos.y + 1;
            } else if (touchPosY - currActiveTouchPos.y < 0)
            {
                touchPosY = currActiveTouchPos.y - 1;
            }
            StartCoroutine(ShowGemMovementEnum(touchPosX, touchPosY));
        } 
    }

    IEnumerator ShowGemMovementEnum(int currTouchPosX, int currTouchPosY)
    {
        //inits
        isRotating = true;
        float rotatePercent = 0.0f;
        rotationAngle = new Vector3(0, 0, 180.0f);
        GameObject gemRotator = new GameObject();
        gemRotator.transform.position = new Vector2 ((float)currActiveTouchPos.x - (float)(currActiveTouchPos.x - currTouchPosX)/2.0f, (float)currActiveTouchPos.y - (float)(currActiveTouchPos.y - currTouchPosY)/2.0f);

        // update involved gems's parent to gem Rotator
        gemGridLayout[currActiveTouchPos.x, currActiveTouchPos.y].gemGridObj.transform.parent = gemGridLayout[currTouchPosX, currTouchPosY].gemGridObj.transform.parent = null;
        gemGridLayout[currActiveTouchPos.x, currActiveTouchPos.y].gemGridObj.transform.parent = gemGridLayout[currTouchPosX, currTouchPosY].gemGridObj.transform.parent = gemRotator.transform;

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
        gemGridLayout[currActiveTouchPos.x, currActiveTouchPos.y].gemGridObj.transform.parent = gemGridLayout[currTouchPosX, currTouchPosY].gemGridObj.transform.parent = null;
        gemGridLayout[currActiveTouchPos.x, currActiveTouchPos.y].gemGridObj.transform.parent = gemGridLayout[currTouchPosX, currTouchPosY].gemGridObj.transform.parent = transform;

        //swap old gem and new gem in grid
        Gem tempGem = gemGridLayout[currActiveTouchPos.x, currActiveTouchPos.y];
        gemGridLayout[currActiveTouchPos.x, currActiveTouchPos.y] = gemGridLayout[currTouchPosX, currTouchPosY];
        gemGridLayout[currTouchPosX, currTouchPosY] = tempGem;

        //update touch position
        currActiveTouchPos.x = currTouchPosX;
        currActiveTouchPos.y = currTouchPosY;

        //Cleanup
        Destroy(gemRotator);
        isRotating = false;
    }

    //actually match gems together and pop them
    IEnumerator MatchGems()
    {
        gemListToDestroy = new List<Gem>();
        //Horizontal check first
        for (int y = 0; y < boardDimY; y++)
        {
            for (int x = 0; x < boardDimX; x++)
            {
                Debug.Log("START OF 2X FOR LOOP");
                gemGridLayout[x, y].xLoc = x;
                gemGridLayout[x, y].yLoc = y;
                Gem currGem = gemGridLayout[x,y];
                Debug.Log("START GEM STATS");
                HelperGetStatus(currGem, x, y);
                if(!currGem.groupedHoriz)
                {
                    List<Gem> gemList = new List<Gem>();
                    Queue gemQ = new Queue();
                    gemQ.Enqueue(currGem);
                    while (gemQ.Count != 0)
                    {
                        Gem gemLFM = (Gem)gemQ.Dequeue();
                        Debug.Log("DQ GEM STATS");
                        HelperGetStatus(gemLFM, gemLFM.xLoc, gemLFM.yLoc);
                        if (gemLFM.gemObj == currGem.gemObj)
                        {
                            // gemGridLayout[gemLFM.xLoc, gemLFM.yLoc].grouped = true;
                            gemList.Add(gemGridLayout[gemLFM.xLoc, gemLFM.yLoc]);
                            //Look right of current gem
                            if (gemLFM.xLoc < boardDimX - 1)
                            {
                                //update right gem with right coords
                                gemGridLayout[gemLFM.xLoc + 1, gemLFM.yLoc].xLoc = gemLFM.xLoc + 1;
                                //update right gem with current ycoords
                                gemGridLayout[gemLFM.xLoc + 1, gemLFM.yLoc].yLoc = gemLFM.yLoc;
                                gemQ.Enqueue(gemGridLayout[gemLFM.xLoc + 1, gemLFM.yLoc]);
                            }
                        }
                    }
                    for (int i = 0; i < gemList.Count; i++ )
                    {
                        //need 3 to be matched
                        if (gemList.Count >= 3) {
                            gemGridLayout[gemList[i].xLoc, gemList[i].yLoc].groupedHoriz = true;
                            gemListToDestroy.Add(gemList[i]);
                            Debug.Log("Matched Gem Coordinates: " + "x: " + gemList[i].xLoc + ", y: " + gemList[i].yLoc);
                        }
                    }
                } else {
                    Debug.Log("SKIP");
                }
            }
        }

        //Vertical check next
        for (int y = 0; y < boardDimY; y++)
        {
            for (int x = 0; x < boardDimX; x++)
            {
                Debug.Log("START OF 2X FOR LOOP");
                gemGridLayout[x, y].xLoc = x;
                gemGridLayout[x, y].yLoc = y;
                Gem currGem = gemGridLayout[x,y];
                Debug.Log("START GEM STATS");
                HelperGetStatus(currGem, x, y);
                if(!currGem.groupedVert)
                {
                    List<Gem> gemList = new List<Gem>();
                    Queue gemQ = new Queue();
                    gemQ.Enqueue(currGem);
                    while (gemQ.Count != 0)
                    {
                        Gem gemLFM = (Gem)gemQ.Dequeue();
                        Debug.Log("DQ GEM STATS");
                        HelperGetStatus(gemLFM, gemLFM.xLoc, gemLFM.yLoc);
                        if (gemLFM.gemObj == currGem.gemObj)
                        {
                            // gemGridLayout[gemLFM.xLoc, gemLFM.yLoc].grouped = true;
                            gemList.Add(gemGridLayout[gemLFM.xLoc, gemLFM.yLoc]);
                            //Look right of current gem
                            if (gemLFM.yLoc < boardDimY - 1)
                            {
                                //update right gem with right coords
                                gemGridLayout[gemLFM.xLoc, gemLFM.yLoc + 1].xLoc = gemLFM.xLoc;
                                //update right gem with current ycoords
                                gemGridLayout[gemLFM.xLoc, gemLFM.yLoc + 1].yLoc = gemLFM.yLoc + 1;
                                gemQ.Enqueue(gemGridLayout[gemLFM.xLoc, gemLFM.yLoc + 1]);
                            }
                        }
                    }
                    for (int i = 0; i < gemList.Count; i++ )
                    {
                        //need 3 to be matched
                        if (gemList.Count >= 3) {
                            gemGridLayout[gemList[i].xLoc, gemList[i].yLoc].groupedVert = true;
                            gemListToDestroy.Add(gemList[i]);
                            Debug.Log("Matched Gem Coordinates: " + "x: " + gemList[i].xLoc + ", y: " + gemList[i].yLoc);
                        }
                    }
                } else {
                    Debug.Log("SKIP");
                }
            }
        }

        //Destroy matched gems and check how many were destroyed
        int count = 0;
        for (int i = 0; i < gemListToDestroy.Count; i++)
        {
            Debug.Log(gemListToDestroy[i].xLoc + ", " + gemListToDestroy[i].yLoc);
            Destroy(gemListToDestroy[i].gemGridObj);
            gemGridLayout[gemListToDestroy[i].xLoc, gemListToDestroy[i].yLoc].destroyed = true;
            count++;
        }
        Debug.Log("Destroy list size: " + count);
        if (count == 0)
        {
            yield return new WaitForSeconds(.01f);
            gridLocked = false;
            Debug.Log("NOT GRIDLOCKED");

        } else 
        {
            Debug.Log("GRIDLOCKED");
            //MAKE FUNCTION TO CHECK BOARD STATUS AT THIS POINT. THINGS ARE BEING MARKED AS DESTROYED.

            // MoveLeftoverGemsDown();
            // MoveNewGemsDown();
        }
    }

    private void HelperGetStatus(Gem gem, int x, int y)
    {
        Debug.Log("Color: " + gem.gemObj + "; Coordinates[x, y]: " + x + "," + y);
    }

    private void MoveLeftoverGemsDown()
    {
        int count = 0;
        for (int y = 1; y < boardDimY; y++)
        {
            for (int x = 0; x < boardDimX; x++)
            {
                //if not a gem that was blown up
                if (!gemGridLayout[x, y].destroyed)
                {
                    count++;
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
        Debug.Log(count + " should be move down");
    }

    // private void MoveNewGemsDown()
    // {
    //     for (int i = 0; i < gemListToDestroy.Count; i++)
    //     {
    //         MakeNewGem(gemListToDestroy[i].xLoc, gemListToDestroy[i].yLoc);
    //     }
    //     gemListToDestroy.Clear();
    //     MoveGemsDown();

    // }

    // private void MakeNewGem(int x, int y)
    // {
    //     GameObject newGem = gemOptions[UnityEngine.Random.Range(0, gemOptions.Count)];
    //     GameObject currentGem = (GameObject)Instantiate(newGem, new Vector2((float)x, (float)y + (float)boardYDropOffset), Quaternion.identity);
    //     currentGem.transform.parent = transform;
    //     gemGridLayout[x, y] = new Gem {
    //         gemObj = newGem,
    //         gemGridObj = currentGem,
    //         matchCountHoriz = initHorizCount,
    //         matchCountVert = initVertCount,
    //         dropDist = boardYDropOffset,
    //         groupedHoriz = false,
    //         groupedVert = false,
    //         xLoc = 0,
    //         yLoc = 0,
    //         destroyed = false,
    //     };
    // }
}
