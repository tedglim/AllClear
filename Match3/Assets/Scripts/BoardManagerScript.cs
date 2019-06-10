using System.Net.NetworkInformation;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManagerScript : MonoBehaviour
{
    private struct Gem {
        public GameObject gemObj;
        public GameObject gemGridObj;
        public int matchCountHoriz;
        public int matchCountVert;
        public int dropDist;
        //if you been grouped, no queue. If you been queued can still be grouped.
        //grouped
        //Queued
        public bool grouped;
        public bool wasQueued;
        //placement for searching
        //xLoc
        //yLoc
        public int xLoc;
        public int yLoc;
        //number for tracking how many in a row
        public int matchCount;
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
                StartCoroutine(MatchGems());
            }
        }
        if(rainCheck)
        {
            //raincheck version
            Debug.Log("RAINCHECK END");
            touchPos = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
            DropGem();
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
                    grouped = false,
                    wasQueued = false,
                    xLoc = 0,
                    yLoc = 0,
                    matchCount = 0
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
                    currGem.matchCountHoriz = 0;
                    currGem.matchCountVert = 0;
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
            grouped = false,
            wasQueued = false,
            xLoc = 0,
            yLoc = 0,
            matchCount = 0
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
        for (int y = 0; y < boardDimY; y++)
        {
            for (int x = 0; x < boardDimX; x++)
            {
                Debug.Log("Gem Iteration");
                Gem currGem  = gemGridLayout[x,y];
                currGem.xLoc = x;
                currGem.yLoc = y;
                HelperGetStatusWrapper(currGem, x, y);
                List<Gem> gemList = new List<Gem>();
                if(!currGem.grouped)
                {
                    Queue gemQ = new Queue();
                    gemQ.Enqueue(currGem);
                    while (gemQ.Count != 0)
                    {
                        Gem gemLFM = (Gem)gemQ.Dequeue();
                        Debug.Log("DQ: GEMLFM");
                        HelperGetStatusWrapper(gemLFM, gemLFM.xLoc, gemLFM.yLoc);
                        if (gemLFM.gemObj == currGem.gemObj)
                        {
                            gemGridLayout[gemLFM.xLoc, gemLFM.yLoc].grouped = true;
                            gemList.Add(gemGridLayout[gemLFM.xLoc, gemLFM.yLoc]);
                            if (gemLFM.xLoc < boardDimX - 1)
                            {
                                Gem rightGem = gemGridLayout[gemLFM.xLoc + 1, gemLFM.yLoc];
                                //if not grouped AND not Q'd
                                if (!rightGem.grouped && !rightGem.wasQueued)
                                {
                                    //mark as Q'd
                                    rightGem.wasQueued = true;
                                    //update right gem with right coords
                                    rightGem.xLoc = gemLFM.xLoc + 1;
                                    //update right gem with current ycoords
                                    rightGem.yLoc = gemLFM.yLoc;
                                    //add to Q
                                    gemQ.Enqueue(rightGem);
                                }
                            }
                        }
                    }
                } else {
                    Debug.Log("SKIP");
                }
                Debug.Log("# gems of same color: " + gemList.Count);
                // //if HAS NOT been "Grouped: keyword"
                // if(!gemGridLayout[x,y].grouped)
                // {
                //     //make an empty list that will contain (Gem)
                //     List<Gem> gemList = new List<Gem>();
                //     //make an empty Queue(Gem)
                //     Queue gemQ = new Queue();
                //     //put THIS Gem cell in the queue
                //     gemQ.Enqueue(gemGridLayout[x,y]);
                //     //while the queue is NOT empty
                //     while(gemQ.Count != 0)
                //     {
                //         //dequeue from queue and assign to temp
                //         Gem gemLFM = (Gem)gemQ.Dequeue();
                //         Debug.Log("gemLFM x, y: " + gemLFM.xLoc + ", " + gemLFM.yLoc);
                //         Debug.Log("x, y: " + x + ", " + y);
                //         //if cell's GObj is not equal to initial cell
                //         if (gemLFM.gemObj != gemGridLayout[x,y].gemObj)
                //         {
                //             Debug.Log("Default: should always be #1");
                //             // Debug.Log("LFM gemobj: " + gemLFM.gemObj);
                //             // Debug.Log("grid's: " + gemGridLayout[x,y].gemObj);
                //             continue;
                //         //otherwise since the cell is equal to initial cell so
                //         } else 
                //         {
                //             //mark it "Grouped"
                //             gemLFM.grouped = true;
                //             //add this cell to empty list of GObj
                //             gemList.Add(gemLFM);
                //             //Look right of current Gem
                            // if (gemLFM.xLoc < boardDimX - 1)
                            // {
                            //     //if not grouped AND not Q'd
                            //     if (!gemGridLayout[gemLFM.xLoc + 1, gemLFM.yLoc].grouped && !gemGridLayout[gemLFM.xLoc + 1, gemLFM.yLoc].wasQueued)
                            //     {
                            //         Gem rightGem = gemGridLayout[gemLFM.xLoc + 1, gemLFM.yLoc];
                            //         //mark as Q'd
                            //         rightGem.wasQueued = true;
                            //         //update right gem with right coords
                            //         rightGem.xLoc = gemLFM.xLoc + 1;
                            //         //update right gem with current ycoords
                            //         rightGem.yLoc = gemLFM.yLoc;
                            //         //add to Q
                            //         gemQ.Enqueue(rightGem);
                            //         Debug.Log("Right Gem: " + rightGem.xLoc + ", " + rightGem.yLoc);
                            //     }
                            // }
                //             //Look up of current gem
                //             if (gemLFM.yLoc < boardDimY - 1)
                //             {
                //                 //if not grouped AND not Q'd
                //                 if (!gemGridLayout[gemLFM.xLoc, gemLFM.yLoc + 1].grouped && !gemGridLayout[gemLFM.xLoc, gemLFM.yLoc + 1].wasQueued)
                //                 {
                //                     Gem upGem = gemGridLayout[gemLFM.xLoc, gemLFM.yLoc + 1];
                //                     //mark as Q'd
                //                     upGem.wasQueued = true;
                //                     //update up gem with current x
                //                     upGem.xLoc = gemLFM.xLoc;
                //                     //update up gem with up coords
                //                     upGem.yLoc = gemLFM.yLoc + 1;
                //                     //add to Q
                //                     gemQ.Enqueue(upGem);
                //                     Debug.Log("Right Gem: " + upGem.xLoc + ", " + upGem.yLoc);

                //                 }
                //             }
                //             //Look left of current Gem
                //             if (gemLFM.xLoc > 0)
                //             {
                //                 //if not grouped AND not Q'd
                //                 if (!gemGridLayout[gemLFM.xLoc - 1, gemLFM.yLoc].grouped && !gemGridLayout[gemLFM.xLoc - 1, gemLFM.yLoc].wasQueued)
                //                 {
                //                     Gem leftGem = gemGridLayout[gemLFM.xLoc - 1, gemLFM.yLoc];
                //                     //mark as Q'd
                //                     leftGem.wasQueued = true;
                //                     //update left gem with left coords
                //                     leftGem.xLoc = gemLFM.xLoc - 1;
                //                     //update left gem with current ycoords
                //                     leftGem.yLoc = gemLFM.yLoc;
                //                     //add to Q
                //                     gemQ.Enqueue(leftGem);
                //                     Debug.Log("Left Gem: " + leftGem.xLoc + ", " + leftGem.yLoc);

                //                 }
                //             }
                //             //Look down
                //             if (gemLFM.yLoc > 0)
                //             {
                //                 //if not grouped AND not Q'd
                //                 if (!gemGridLayout[gemLFM.xLoc, gemLFM.yLoc - 1].grouped && !gemGridLayout[gemLFM.xLoc, gemLFM.yLoc - 1].wasQueued)
                //                 {
                //                     Gem downGem = gemGridLayout[gemLFM.xLoc, gemLFM.yLoc - 1];
                //                     //mark as Q'd
                //                     downGem.wasQueued = true;
                //                     //update up gem with current x
                //                     downGem.xLoc = gemLFM.xLoc;
                //                     //update up gem with up coords
                //                     downGem.yLoc = gemLFM.yLoc - 1;
                //                     //add to Q
                //                     gemQ.Enqueue(downGem);
                //                     Debug.Log("Down Gem: " + downGem.xLoc + ", " + downGem.yLoc);

                //                 }
                //             }
                //         }
                //     }
                //     for (int i = 0; i < gemList.Count; i++ )
                //     {
                //         //update matchcount with list length
                //         Gem temp = gemList[i];
                //         temp.matchCount = gemList.Count;
                //     }
                // }

            }
        }

        //destroy matches greater than or equal to 3
        // for (int y = 0; y < boardDimY; y++)
        // {
        //     for (int x = 0; x < boardDimX; x++)
        //     {
        //         if(gemGridLayout[x,y].matchCount >= 3)
        //         {
        //             Destroy(gemGridLayout[x,y].gemGridObj);
        //         }
        //     }
        // }

        yield return new WaitForSeconds(.01f);
    }

    private void HelperGetStatus(Gem gem)
    {
        Debug.Log("Color: " + gem.gemObj + " G: " + gem.grouped + " Q: " + gem.wasQueued);
    }

    private void HelperGetStatusWrapper(Gem gem, int x, int y)
    {
        Debug.Log("[x, y]: " + x + "," + y);
        HelperGetStatus(gem);
    }
}
