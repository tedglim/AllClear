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
    // private GameObject prevGem;
    // private GameObject[] prevGemRow;

    public float fallTimeInterval = .01f;
    public float fallPercentIncrease = .05f;

    private Ray touchPos;
    public float underlayAlpha = .25f;
    public float overlayAlpha = .75f;
    private Gem gemClone;
    private Vector2Int prevGemPos;

    public float rotationTimeInterval = .01f;
    public float rotatePercentIncrease = .1f;
    private Vector3 rotationAngle;

    // private bool isTouchingScreen;

    // Start is called before the first frame update
    void Start()
    {
        // isTouchingScreen = false;
        InitBoard();
        MoveGemsDown();
    }

    // Update is called once per frame
    void Update()
    {
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
            ShowGemMovement(touchPos.origin);



        //if finger is off
        } else if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended)
        {
            // touchPos = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
            // Vector2 touchEndPos = touchPos.origin;
            // Debug.Log("End Coordinates: " + touchEndPos);
            // Destroy(gemClone.gemGridObj);
        }
        //
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
                    dropDist = boardYDropOffset
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
                    
                    //reset drop distance
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
        Debug.Log("Start Phase: X: " + touchPosX + ", Y: " + touchPosY);

        //get Gem in grid; change its alpha
        Gem selectedGem = gemGridLayout[touchPosX, touchPosY];
        selectedGem.gemGridObj.GetComponent<SpriteRenderer>().color = ChangeGemAlpha(selectedGem, underlayAlpha);

        //create new Gem at same location
        MakeGemClone(selectedGem, touchPosX, touchPosY);
        prevGemPos = new Vector2Int(touchPosX, touchPosY);
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

    private Color ChangeGemAlpha(Gem gem, float aVal)
    {
        Color gemColor = gem.gemGridObj.GetComponent<SpriteRenderer>().color;
        gemColor.a = aVal;
        return gemColor;
    }

    private void MakeGemClone (Gem origGem, int x, int y)
    {
        gemClone = new Gem {
            gemObj = origGem.gemObj,
            gemGridObj = (GameObject)Instantiate(origGem.gemGridObj, new Vector2(x, y), Quaternion.identity),
            matchCountHoriz = 0,
            matchCountVert = 0,
            dropDist = 0
        };
        gemClone.gemGridObj.GetComponent<SpriteRenderer>().color = ChangeGemAlpha(gemClone, overlayAlpha);
        Debug.Log("Clone Pos: " + x + ", " + y);
    }

    private void ShowGemMovement(Vector2 touchPos)
    {
        //should include how to ignore wildly stupid finger placements
        int touchPosX = GetPosOnAxis(touchPos.x, boardDimX);
        int touchPosY = GetPosOnAxis(touchPos.y, boardDimY);
        Debug.Log("Move Phase: X: " + touchPosX + ", Y: " + touchPosY);

        if (prevGemPos.x != touchPosX || prevGemPos.y != touchPosY)
        {
            Debug.Log("MOVED");
            StartCoroutine(ShowGemMovementEnum(touchPosX, touchPosY));
        } 
        //if currtouchpos is different than first one
                //do gem swap movement
        // StartCoroutine(ShowGemMovementEnum());
    }

//rotator not in ccorrect position from DEBUG. FIX THIS
    IEnumerator ShowGemMovementEnum(int currTouchPosX, int currTouchPosY)
    {
        //inits
        float rotatePercent = 0.0f;
        rotationAngle = new Vector3(0, 0, 180.0f);
        //create GO that will rotate
        GameObject gemRotator = new GameObject();
        // Debug.Log("prevGemPos: " + prevGemPos);
        // Debug.Log("currTouchPosX and Y: " + currTouchPosX + ", " + currTouchPosY);
        gemRotator.transform.position = new Vector2 ((float)prevGemPos.x - (float)(prevGemPos.x - currTouchPosX)/2.0f, (float)prevGemPos.y - (float)(prevGemPos.y - currTouchPosY)/2.0f);
        // gemRotator.transform.position = new Vector2 ((float)(prevGemPos.x - ((prevGemPos.x - currTouchPosX)/2)), (float)(prevGemPos.y - ((prevGemPos.y - currTouchPosY)/2)));
        Debug.Log("Rotator Pos: " + gemRotator.transform.position);

        //Reassign clone and involved gems to Rotator as children
        gemClone.gemGridObj.transform.parent = gemRotator.transform;
        gemGridLayout[prevGemPos.x, prevGemPos.y].gemGridObj.transform.parent = gemRotator.transform;
        gemGridLayout[currTouchPosX, currTouchPosY].gemGridObj.transform.parent = gemRotator.transform;
        Debug.Log("Clone position: " + gemClone.gemGridObj.transform.position);
        Debug.Log("OldGem position: " + gemGridLayout[prevGemPos.x, prevGemPos.y].gemGridObj.transform.position);
        Debug.Log("NewGem position: " + gemGridLayout[currTouchPosX, currTouchPosY].gemGridObj.transform.position);


        //rotate to desired positions
        while(rotatePercent <= 1.0f)
        {
            gemRotator.transform.eulerAngles = Vector3.Lerp(Vector3.zero, rotationAngle, rotatePercentIncrease);
            rotatePercent += rotatePercentIncrease;
            yield return new WaitForSeconds(rotationTimeInterval);
        }

        //finalize rotation and movements
        gemRotator.transform.eulerAngles = rotationAngle;

        //make clone go to new gem pos
        //make old gem to to new gem pos
        //make new gem go to old gem position
        gemClone.gemGridObj.transform.position = new Vector2(currTouchPosX, currTouchPosY);
        Vector2 tempPos = gemGridLayout[prevGemPos.x, prevGemPos.y].gemGridObj.transform.position;
        gemGridLayout[prevGemPos.x, prevGemPos.y].gemGridObj.transform.position = new Vector2(currTouchPosX, currTouchPosY);
        gemGridLayout[currTouchPosX, currTouchPosY].gemGridObj.transform.position = tempPos;

        //swap old gem and new gem in grid
        Gem tempGem = gemGridLayout[prevGemPos.x, prevGemPos.y];
        gemGridLayout[prevGemPos.x, prevGemPos.y] = gemGridLayout[currTouchPosX, currTouchPosY];
        gemGridLayout[currTouchPosX, currTouchPosY] = tempGem;

        //reparent/unparent appropriately
        gemClone.gemGridObj.transform.parent = null;
        gemGridLayout[prevGemPos.x, prevGemPos.y].gemGridObj.transform.parent = transform;
        gemGridLayout[currTouchPosX, currTouchPosY].gemGridObj.transform.parent = transform;

        //Cleanup
        Destroy(gemRotator);
    }

}
