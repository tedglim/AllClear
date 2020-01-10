using System.Net.NetworkInformation;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class Variation01Script : MonoBehaviour
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
    private bool isGameOver;
    private bool isWin;
    // private bool noMoves;
    private bool showUndo;
    private bool canUndo;
    private bool wantsUndo;
    private bool movedGemm;
    private bool menuListOn;
    private bool oneClickLock;

//serial values
    [SerializeField]
    private int boardDimX;
    [SerializeField]
    private int boardDimY;
    [SerializeField]
    private int dropOffset;
    [SerializeField]
    private int totalRounds;
    // [SerializeField]
    // private int movesPerRound;
    [SerializeField]
    private int goalNumCyan;
    [SerializeField]
    private int goalNumGreen;
    [SerializeField]
    private int goalNumOrange;
    [SerializeField]
    private int goalNumPink;
    [SerializeField]
    private int goalNumRed;
    [SerializeField]
    private int goalNumViolet;
    [SerializeField]
    private int goalNumYellow;
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
    [SerializeField]
    private float fadeStep;
    
//nonserial values
    private int currNumRounds;
    // private int currNumMoves;
    private int cyansRemaining;
    private int greensRemaining;
    private int orangesRemaining;
    private int pinksRemaining;
    private int redsRemaining;
    private int violetsRemaining;
    private int yellowsRemaining;

//other serialized
    [SerializeField]
    private List<GameObject> GemmOptions;
    // [SerializeField]
    // private GameObject undoButton;
    // [SerializeField]
    // private GameObject roundsButton;

//other nonserialized
    private Gemm[,] GemmGridLayout;
    private Gemm[,] GemmGridLayoutCopy;
    private struct Gemm
    {
        public GameObject gemmGObj;
        public string tagId;
        public bool floodVisited;
        public bool floodMatched;
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
    private Gemm leftGemm;
    private Gemm downGemm;
    private Ray touchPos;
    private Vector2Int prevActiveTouchPos;
    private Vector3 rotationAngle;
    private Gemm gemmClone;
    private Dictionary<GemmLoc, Gemm> FloodMatchDict;
    private Dictionary<GemmLoc, Gemm> GemmDictToDestroy;
    private List<GemmLoc> Check3List;
    // private Image undoButtonImg;
    // private Color undoButtonOrigColor;


    // declare inits
    void Awake()
    {
        GemmGridLayout = new Gemm[boardDimX, boardDimY];
        GemmGridLayoutCopy = new Gemm[boardDimX, boardDimY];
        FloodMatchDict = new Dictionary<GemmLoc, Gemm>();
        GemmDictToDestroy = new Dictionary<GemmLoc, Gemm>();
        Check3List = new List<GemmLoc>();
        // undoButtonImg = undoButton.GetComponent<Image>();
        // undoButtonOrigColor = undoButtonImg.color;

        isMatching = false;
        areGemmsFalling = false;
        isFirstDrop = true;
        canUndo = false;
        showUndo = false;
        wantsUndo = false;
        menuListOn = false;
        oneClickLock = false;

        cyansRemaining = goalNumCyan;
        greensRemaining = goalNumGreen;
        orangesRemaining = goalNumOrange;
        pinksRemaining = goalNumPink;
        redsRemaining = goalNumRed;
        violetsRemaining = goalNumViolet;
        yellowsRemaining = goalNumYellow;

        // currNumMoves = movesPerRound;
        currNumRounds = totalRounds;

        // SwapUndoStates();
    }

    // //controls display of "UNDO" vs "ROUND"
    // private void SwapUndoStates()
    // {
    //     showUndo = !showUndo;
    //     undoButton.GetComponent<Image>().color = undoButtonOrigColor;
    //     undoButton.SetActive(!showUndo);
    //     roundsButton.SetActive(showUndo);
    // }

    void Start()
    {
        StartCoroutine(GameEventFuncs());
        StartCoroutine(SetupInitialBoard());
    }

    IEnumerator GameEventFuncs()
    {
        yield return new WaitForSeconds(0);
        GameEventsScript.menuListOnOff.AddListener(IsMenuListOn);
        // GameEventsScript.undoOnOff.AddListener(DoUndo);
        // GameEventsScript.countRound.Invoke(new GameEventsScript.CountRoundsData(currNumRounds, totalRounds));
        // GameEventsScript.countRoundV1.Invoke(new GameEventsScript.CountRoundsV1Data(currNumRounds));
        GameEventsScript.clearGems.Invoke(new GameEventsScript.DestroyedGemsData(cyansRemaining, greensRemaining, orangesRemaining, pinksRemaining, redsRemaining, violetsRemaining, yellowsRemaining));
        GameEventsScript.countMove.Invoke(new GameEventsScript.CountMoveData(currNumRounds, totalRounds));
    }

    //Controls boolean for undo state from ResetAlphaScript
    // private void DoUndo()
    // {
    //     wantsUndo = !wantsUndo;
    // }

    //tracks menu list state
    private void IsMenuListOn()
    {
        menuListOn = !menuListOn;
        oneClickLock = true;
    }

    void Update()
    {
        if (isFirstDrop || isMatching || areGemmsFalling || isGameOver || menuListOn)
        {
            return;
        }

        //prevents board from being touched during open menu
        if(!menuListOn && oneClickLock)
        {
            oneClickLock = false;
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            //copy BoardState for Revert
            GemmGridLayoutCopy = GemmGridLayout.Clone() as Gemm[,];

            //track cursor position/state for if gemmselected, clone, and undo button
            touchPos = Camera.main.ScreenPointToRay(Input.mousePosition);            
            if (Mathf.RoundToInt(touchPos.origin.x) < boardDimX && Mathf.RoundToInt(touchPos.origin.x) > -1 && Mathf.RoundToInt(touchPos.origin.y) < boardDimY && Mathf.RoundToInt(touchPos.origin.y) > -1)
            {
                // if(!canUndo)
                // {
                //     SwapUndoStates();
                //     canUndo = true;
                // }
                isGemmSelected = true;
                DisplayGemmClone(touchPos.origin);
            }
        } else if (Input.GetMouseButton(0))
        {
            //track cursor position/state for if gemm can follow cursor
            touchPos = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Mathf.RoundToInt(touchPos.origin.x) < boardDimX && Mathf.RoundToInt(touchPos.origin.x) > -1 && Mathf.RoundToInt(touchPos.origin.y) < boardDimY && Mathf.RoundToInt(touchPos.origin.y) > -1)
            {
                canGemmFollowMe = true;
            }
        } else if (Input.GetMouseButtonUp(0))
        {
            //check for UNDO
            if(wantsUndo)
            {
                DropGemm();
                GemmGridLayout = GemmGridLayoutCopy;
                ClearGridLayout();
                RemakeGemmsForUndo();
                // currNumMoves = movesPerRound;
                // GameEventsScript.countMove.Invoke(new GameEventsScript.CountMoveData(currNumMoves, movesPerRound));
            } else
            {
                //proceed to matching state
                wantGemmDrop = true;
            }

            //Swap out UNDO button
            // if(canUndo)
            // {
            //     SwapUndoStates();
            //     canUndo = false;
            // }

            //Reset States
            isGemmSelected = canGemmFollowMe = wantsUndo = false;
        }
    }

    void FixedUpdate()
    {
        if (isFirstDrop || areGemmsFalling || isMatching || isGameOver || menuListOn)
        {
            return;
        }

        //prevents board from being touched during open menu
        if(!menuListOn && oneClickLock)
        {
            oneClickLock = false;
            return;
        }

        //if GemmSelected and Gemmcan follow cursor
        if(isGemmSelected && canGemmFollowMe)
        {
            //if GemmClone alive and there are remaining moves, show all the gemm movements on screen
            if(isGemmCloneAlive)
            {
                gemmClone.gemmGObj.transform.Translate((touchPos.origin - gemmClone.gemmGObj.transform.position) * Time.fixedDeltaTime * moveSpeed);
                ShowGemmMovement(touchPos.origin);
            }
            
            // if(isGemmCloneAlive && !noMoves)
            // {
            //     gemmClone.gemmGObj.transform.Translate((touchPos.origin - gemmClone.gemmGObj.transform.position) * Time.fixedDeltaTime * moveSpeed);
            //     ShowGemmMovement(touchPos.origin);

            //     //once you run out of moves, hide clone, noMoves = true
            //     if (currNumMoves <= 0)
            //     {
            //         gemmClone.gemmGObj.SetActive(false);
            //         noMoves = true;
            //     }
            
            // //if there are moves remaining, set noMoves = false
            // } else if (currNumMoves > 0)
            // {
            //     noMoves = false;
            // }
        }

        //when player releases gemm and gemm has moved, start matching
        if (wantGemmDrop)
        {
            DropGemm();
            if (movedGemm)
            {
                StartCoroutine(MatchGemms());
            }
        }
    }

    //Wrapper for creating initial board
    IEnumerator SetupInitialBoard()
    {
        yield return new WaitForSecondsRealtime(1.0f);
        MakeGemmsInGrid();
        MoveGemmsDown();
        isFirstDrop = false;
    }

    //Create Gemms in Standard Grid
    private void MakeGemmsInGrid()
    {
        GameObject randGemm;
        for (int y = 0; y < boardDimY; y++)
        {
            for (int x = 0; x < boardDimX; x++)
            {
                //List gemm options
                List<GameObject> availableGems = new List<GameObject>();    
                availableGems.AddRange(GemmOptions);
                
                //detect if 2 in a row left and down when in row/column 3+
                //assign a random gemm that makes it so grid does not contain 3 in a rows
                while(true)
                {
                    randGemm = availableGems[UnityEngine.Random.Range(0, availableGems.Count)];
                    if (x > 1)
                    {
                        leftGemm = GemmGridLayout[x - 1, y];
                        if (randGemm.tag == leftGemm.tagId)
                        {
                            leftGemm = GemmGridLayout[x - 2, y];
                            if (randGemm.tag == leftGemm.tagId)
                            {
                                availableGems.Remove(randGemm);
                                continue;
                            }
                        }
                    }
                    if (y > 1)
                    {
                        downGemm = GemmGridLayout[x, y - 1];
                        if (randGemm.tag == downGemm.tagId)
                        {
                            downGemm = GemmGridLayout[x, y - 2];
                            if (randGemm.tag == downGemm.tagId)
                            {
                                availableGems.Remove(randGemm);
                                continue;
                            }
                        }
                    }
                    break;
                }
                MakeGemm(randGemm, x, y);
            }
        }
    }

    //Instantiate Gemm in Setup Grid
    private void MakeGemm(GameObject gemmGObj, int x, int y)
    {
        GameObject currentGemm = (GameObject)Instantiate(gemmGObj, new Vector2((float)x, (float)y + (float)dropOffset), Quaternion.identity);
        currentGemm.transform.parent = transform;
        GemmGridLayout[x, y] = new Gemm {
            gemmGObj = currentGemm,
            tagId = currentGemm.tag,
            floodVisited = false,
            floodMatched = false,
            destroyed = false,
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
        //make gemm clone at cursor position
        int gridXPos = GetPosOnGrid(touchPos.x, boardDimX);
        int gridYPos = GetPosOnGrid(touchPos.y, boardDimY);
        prevActiveTouchPos = new Vector2Int(gridXPos, gridYPos);
        Gemm selectedGem = GemmGridLayout[gridXPos, gridYPos];
        MakeGemmClone(selectedGem, gridXPos, gridYPos);
        selectedGem.gemmGObj.GetComponent<SpriteRenderer>().color = ChangeGemmAlpha(selectedGem, underlayAlpha);
    }

    //Get grid position of cursor x/y independently
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
        };
        gemmClone.gemmGObj.GetComponent<SpriteRenderer>().color = ChangeGemmAlpha(gemmClone, overlayAlpha);
        isGemmCloneAlive = true;
    }

    //adjusts gemm transparency (clone and gid)
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

        //Updates gem movement when finger moves to new cell
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
        movedGemm = true;
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

        //Count Move
        // if(currNumMoves > 0)
        // {
        //     currNumMoves--;
        // }
        // GameEventsScript.countMove.Invoke(new GameEventsScript.CountMoveData(currNumMoves, movesPerRound));
        
    }

    //Setup for Gemm Matching
    private void DropGemm()
    {
        GemmGridLayout[prevActiveTouchPos.x, prevActiveTouchPos.y].gemmGObj.GetComponent<SpriteRenderer>().color = ChangeGemmAlpha(GemmGridLayout[prevActiveTouchPos.x, prevActiveTouchPos.y], 1.0f);
        if (gemmClone.gemmGObj != null)
        {
            Destroy(gemmClone.gemmGObj);
        }
        wantGemmDrop = isGemmCloneAlive = false;
    }

    //Matches gemms; looks for 3 gemms or more in a row
    IEnumerator MatchGemms()
    {
        ResetBoardForMatching();
        isMatching = true;
        for (int y = 0; y < boardDimY; y++)
        {
            for (int x = 0; x < boardDimX; x++)
            {
                if(!GemmGridLayout[x,y].destroyed)
                {
                    ResetBoardForFloodMarking();
                    FloodMark(x, y, GemmGridLayout[x, y].gemmGObj.tag);
                    Check3PlusInDirectionWrapper(1, 0);
                    Check3PlusInDirectionWrapper(0, 1);
                    CountAndDestroyGemms();
                    if(didDestroy)
                    {
                        GameEventsScript.clearGems.Invoke(new GameEventsScript.DestroyedGemsData(cyansRemaining, greensRemaining, orangesRemaining, pinksRemaining, redsRemaining, violetsRemaining, yellowsRemaining));
                        yield return new WaitForSeconds(0.25f);
                        didDestroy = false;
                    }
                }
            }
        }
        
        //Cleanup board after matching
        SetupRemainingGemmsForDrop();
        RemakeDestroyedGemms();
        MoveGemmsDown();
        yield return new WaitUntil(() => !areGemmsFalling);

        //if gemms were destroyed, we can repeat matching process 
        if(canContinueMatching)
        {
            canContinueMatching = false;
            StartCoroutine(RepeatMatchGemms());
        } else
        {
            //Cleanup board, reset moves, count round, check win/gameover conditions
            isMatching = false;
            movedGemm = false;
            // currNumMoves = movesPerRound;
            // GameEventsScript.countMove.Invoke(new GameEventsScript.CountMoveData(currNumMoves, movesPerRound));
            currNumRounds--;
            // GameEventsScript.countRoundV1.Invoke(new GameEventsScript.CountRoundsV1Data(currNumRounds));
            GameEventsScript.countMove.Invoke(new GameEventsScript.CountMoveData(currNumRounds, totalRounds));

            // GameEventsScript.countRound.Invoke(new GameEventsScript.CountRoundsData(currNumRounds, totalRounds));
            if (redsRemaining <= 0 && greensRemaining <= 0 && cyansRemaining <= 0 && orangesRemaining <= 0 && pinksRemaining <= 0 && violetsRemaining <= 0 && yellowsRemaining <= 0)
            {
                isGameOver = true;
                isWin = true;
            } else if (currNumRounds < 1)
            {
                isGameOver = true;
                isWin = false;
            }
        }
        yield return new WaitForSeconds(1.0f);

        //check game over
        if (isGameOver)
        {
            GameEventsScript.gameIsOver.Invoke(new GameEventsScript.GameOverData(isWin));
        }
    }

    //cleanup all gemm states for matching
    private void ResetBoardForMatching()
    {
        for (int y =0; y < boardDimY; y++)
        {
            for (int x =0; x < boardDimX; x++)
            {
                GemmGridLayout[x,y].tagId = GemmGridLayout[x,y].gemmGObj.tag;
                GemmGridLayout[x,y].floodVisited = false;
                GemmGridLayout[x,y].floodMatched = false;
                GemmGridLayout[x,y].destroyed = false;
                GemmGridLayout[x,y].dropDist = 0;
                GemmGridLayout[x,y].gridXLoc = x;
                GemmGridLayout[x,y].gridYLoc = y;               
            }
        }
    }

    //cleanup gemm state for flood marking
    private void ResetBoardForFloodMarking()
    {
        for (int y =0; y < boardDimY; y++)
        {
            for (int x =0; x < boardDimX; x++)
            {
                GemmGridLayout[x,y].floodVisited = false;
                GemmGridLayout[x,y].floodMatched = false;             
            }
        }
    }

    //Mark blob of same colored gemms; track with Floodmatchdict
    private void FloodMark(int x, int y, string origTag)
    {
        //create ToDo List
        Stack gemmStack = new Stack();
        gemmStack.Push(GemmGridLayout[x,y]);
        //while ToDo List not empty
        while (gemmStack.Count > 0)
        {
            Gemm gemmLFM = (Gemm) gemmStack.Pop();
            GemmGridLayout[gemmLFM.gridXLoc, gemmLFM.gridYLoc].floodVisited = true;
            if (gemmLFM.gemmGObj.tag == origTag)
            {
                GemmGridLayout[gemmLFM.gridXLoc, gemmLFM.gridYLoc].floodMatched = true;
                //create key to add to running list
                GemmLoc key = new GemmLoc {
                    gridXLoc = gemmLFM.gridXLoc,
                    gridYLoc = gemmLFM.gridYLoc,
                };
                if(!FloodMatchDict.ContainsKey(key))
                {
                    FloodMatchDict.Add(key, GemmGridLayout[gemmLFM.gridXLoc, gemmLFM.gridYLoc]);
                }
                //add appropriate cells around current to the ToDoList
                if (gemmLFM.gridXLoc > 0)
                {
                    if (!GemmGridLayout[gemmLFM.gridXLoc - 1, gemmLFM.gridYLoc].floodVisited && !GemmGridLayout[gemmLFM.gridXLoc - 1, gemmLFM.gridYLoc].destroyed)
                    {
                        gemmStack.Push(GemmGridLayout[gemmLFM.gridXLoc - 1, gemmLFM.gridYLoc]);
                    }
                }
                if (gemmLFM.gridXLoc < boardDimX - 1)
                {
                    if (!GemmGridLayout[gemmLFM.gridXLoc + 1, gemmLFM.gridYLoc].floodVisited && !GemmGridLayout[gemmLFM.gridXLoc + 1, gemmLFM.gridYLoc].destroyed)
                    {
                        gemmStack.Push(GemmGridLayout[gemmLFM.gridXLoc + 1, gemmLFM.gridYLoc]);
                    }
                }
                if (gemmLFM.gridYLoc > 0)
                {
                    if (!GemmGridLayout[gemmLFM.gridXLoc, gemmLFM.gridYLoc - 1].floodVisited && !GemmGridLayout[gemmLFM.gridXLoc, gemmLFM.gridYLoc - 1].destroyed)
                    {
                        gemmStack.Push(GemmGridLayout[gemmLFM.gridXLoc, gemmLFM.gridYLoc - 1]);
                    }
                }
                if (gemmLFM.gridYLoc < boardDimY - 1)
                {
                    if (!GemmGridLayout[gemmLFM.gridXLoc, gemmLFM.gridYLoc + 1].floodVisited && !GemmGridLayout[gemmLFM.gridXLoc, gemmLFM.gridYLoc + 1].destroyed)
                    {
                        gemmStack.Push(GemmGridLayout[gemmLFM.gridXLoc, gemmLFM.gridYLoc + 1]);
                    }
                }
            }
        }
    }

    //wrapper for check 3 plus gemms in a row method. uses horiz and vert int to indicate whether searching horizontal or vertical matches
    //if the gemm's been visited, skip over that iteration. it's been accounted for.
    private void Check3PlusInDirectionWrapper(int horiz, int vert)
    {
        foreach(var gemm in FloodMatchDict)
        {
            Check3PlusInDirection(gemm.Key, horiz, vert);
        }
    }

    //Q each candidate for horiz/vert 3 in a row 1 at a time.
    //if match, add to temp; else see if temp has >= 3
    //if so, record for destroying and clear temp, otherwise clear temp.
    private void Check3PlusInDirection(GemmLoc gemmKey, int horiz, int vert)
    {
        Queue Check3Q = new Queue();
        Check3Q.Enqueue(gemmKey);
        while(Check3Q.Count > 0)
        {
            GemmLoc key = (GemmLoc) Check3Q.Dequeue();
            if (FloodMatchDict.ContainsKey(key))
            {
                Check3List.Add(key);
                GemmLoc newKey = new GemmLoc {
                    gridXLoc = key.gridXLoc + horiz,
                    gridYLoc = key.gridYLoc + vert,
                };
                Check3Q.Enqueue(newKey);
            } else
            {
                if(Check3List.Count >= 3)
                {
                    for(int i = 0; i < Check3List.Count; i++)
                    {
                        if(!GemmDictToDestroy.ContainsKey(Check3List[i]))
                        {
                            GemmDictToDestroy.Add(Check3List[i], GemmGridLayout[Check3List[i].gridXLoc, Check3List[i].gridYLoc]);
                        }
                    }
                }
                Check3List.Clear();
                break;
            }
        }
    }

    //count and destroy Gemms
    private void CountAndDestroyGemms()
    {
        CountDestroyedGemms();
        DestroyGemms();
    }

    private void CountDestroyedGemms()
    {
        foreach (var gemm in GemmDictToDestroy)
        {
            if (gemm.Value.gemmGObj.tag == "Cyan")
            {
                if(cyansRemaining > 0)
                {
                    cyansRemaining--;
                }
            } else if (gemm.Value.gemmGObj.tag == "Green")
            {
                if(greensRemaining > 0)
                {
                    greensRemaining--;
                }
            } else if (gemm.Value.gemmGObj.tag == "Orange")
            {
                if(orangesRemaining > 0)
                {
                    orangesRemaining--;
                }
            } else if (gemm.Value.gemmGObj.tag == "Pink")
            {
                if(pinksRemaining > 0)
                {
                    pinksRemaining--;
                }
            } else if(gemm.Value.gemmGObj.tag == "Red")
            {
                if(redsRemaining > 0)
                {
                    redsRemaining--;
                }
            } else if (gemm.Value.gemmGObj.tag == "Violet")
            {
                if(violetsRemaining > 0)
                {
                    violetsRemaining--;
                }
            } else if (gemm.Value.gemmGObj.tag == "Yellow")
            {
                if(yellowsRemaining > 0)
                {
                    yellowsRemaining--;
                }
            }
        }
    }

    //Destroy gemms, fade them out, reset floodmatch and gemmtodestroy dictionaries
    private void DestroyGemms()
    {
        foreach (var gemm in GemmDictToDestroy)
        {
            int a = gemm.Value.gridXLoc;
            int b = gemm.Value.gridYLoc;
            if (!GemmGridLayout[a, b].destroyed)
            {
                GemmGridLayout[a, b].destroyed = true;
                StartCoroutine(fadeGemm(gemm.Value.gemmGObj, 1.0f));
                didDestroy = true;
                canContinueMatching = true;
            }
        }
        FloodMatchDict.Clear();
        GemmDictToDestroy.Clear();
    }

    //Fades gemms to be destroyed
    IEnumerator fadeGemm(GameObject gemm, float fadeDuration)
    {
        SpriteRenderer sr = gemm.GetComponent<SpriteRenderer>();
        for (float t = 0f; t < fadeDuration; t += Time.deltaTime)
        {
            Color c = sr.color;
            Color fine = sr.color;
            c.a = c.a - fadeStep;
            sr.color = c;
           
            yield return null;
 
            if (sr.color.a <= 0f)
            {
                Destroy(gemm);
                break;
            }
 
        }
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

    //Repeats Gemm Matching process
    IEnumerator RepeatMatchGemms()
    {
        yield return new WaitUntil(() => !areGemmsFalling);
        StartCoroutine(MatchGemms());
    }

    //Prep for Undo procedure
    private void ClearGridLayout()
    {
        for(int y = 0; y < boardDimY; y++)
        {
            for(int x = 0; x < boardDimX; x++)
            {
                if (GemmGridLayout[x,y].gemmGObj != null)
                {
                    Destroy(GemmGridLayout[x,y].gemmGObj);
                }
            }
        }
    }

    //Remake Gemms for UNDO process; revert to previous board state
    private void RemakeGemmsForUndo()
    {
        for(int y = 0; y < boardDimY; y++)
        {
            for(int x = 0; x < boardDimX; x++)
            {
                for(int z = 0; z < GemmOptions.Count; z++)
                {
                    if (GemmGridLayout[x,y].tagId == GemmOptions[z].tag)
                    {
                        GameObject gObj = (GameObject) Instantiate(GemmOptions[z], new Vector2((float)x, (float)y), Quaternion.identity);
                        gObj.transform.parent = transform;
                        GemmGridLayout[x,y].gemmGObj = gObj;
                        break;
                    }
                }
            }
        } 
    }

    //Debug function for main grid
    private void checkGridState()
    {
        for (int y = 0; y < boardDimY; y++)
        {
            for (int x = 0; x < boardDimX; x++)
            {
                Debug.Log("Coordinates: [" + x + ", " + y + "]" + "\n" + 
                "[DropDistance, gridX, gridY]: [" + GemmGridLayout[x, y].dropDist + ", " + GemmGridLayout[x,y].gridXLoc + ", " + GemmGridLayout[x, y].gridYLoc + "]");
                Debug.Log("Color: " + GemmGridLayout[x, y].gemmGObj.tag + ": " + GemmGridLayout[x,y].tagId + "\n" +
                "World Space: " + GemmGridLayout[x, y].gemmGObj.transform.position);
                Debug.Log("Matched: " + GemmGridLayout[x, y].floodMatched + "\n" +
                "Visited: " + GemmGridLayout[x,y].floodVisited);
            }
        }
    }

    //Debug function for grid copy
    private void checkGridStateCopy()
    {
        for (int y = 0; y < boardDimY; y++)
        {
            for (int x = 0; x < boardDimX; x++)
            {
                Debug.Log("Coordinates: [" + x + ", " + y + "]" + "\n" + 
                "[DropDistance, gridX, gridY]: [" + GemmGridLayoutCopy[x, y].dropDist + ", " + GemmGridLayoutCopy[x,y].gridXLoc + ", " + GemmGridLayoutCopy[x, y].gridYLoc + "]");
                Debug.Log("Color: " + GemmGridLayoutCopy[x, y].gemmGObj.tag + ": " + GemmGridLayoutCopy[x,y].tagId + "\n" +
                "World Space: " + GemmGridLayoutCopy[x, y].gemmGObj.transform.position);
                Debug.Log("Matched: " + GemmGridLayoutCopy[x, y].floodMatched + "\n" +
                "Visited: " + GemmGridLayoutCopy[x,y].floodVisited);
            }
        }
    }

}
