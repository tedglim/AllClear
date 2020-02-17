using System.Net.NetworkInformation;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class TutorialRAScript : MonoBehaviour
{

//booleans
    private bool isFirstDrop;
    private bool areGemmsFalling;
    private bool isGemmSelected;
    private bool isGemmCloneAlive;
    private bool canGemmFollowMe;
    private bool isRotating;
    private bool w8ForRotation;
    private bool wantGemmDrop;
    private bool isMatching;
    private bool didDestroy;
    private bool canContinueMatching;
    private bool isGameOver;
    private bool isWin;
    private bool movedGemm;
    private bool didAllClearAtLeastOnce;
    private bool didAllClear;
    private bool part1AllClear;
    private bool allClearFXOn;
    private bool bonusFXOn;
    private bool gameOverTriggered;
    private bool textLock;
    private bool tutorialTransition01Lock;
    private bool tutorialTransition02Lock;
    private bool tutorialTransition04Lock;
    private bool tutorialTransition05Lock;
    private bool tutorialTransition06Lock;

//serial values
    [SerializeField]
    private int boardDimX;
    [SerializeField]
    private int boardDimY;
    [SerializeField]
    private int dropOffset;
    [SerializeField]
    private int totalMoves;
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
    private int allClearBonusAmount;
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
    private int currNumMoves;
    private int cyansRemaining;
    private int greensRemaining;
    private int orangesRemaining;
    private int pinksRemaining;
    private int redsRemaining;
    private int violetsRemaining;
    private int yellowsRemaining;
    private int gemmsDestroyedin1Board;
    private float gameOverTimer;

//other serialized
    [SerializeField]
    private List<GameObject> GemmOptions;
    [SerializeField]
    public string difficulty;
    [SerializeField]
    private GameObject tutorialButton01;
    private Text tutorialButton01Text;
    [SerializeField]
    private GameObject tutorialButton02;
    private Text tutorialButton02Text;
    [SerializeField]
    private GameObject tutorialButton03;
    private Text tutorialButton03Text;
    [SerializeField]
    private GameObject tutorialButton04;
    private Text tutorialButton04Text;
    [SerializeField]
    private GameObject tutorialButton05;
    private Text tutorialButton05Text;
    [SerializeField]
    private GameObject tutorialButton06;
    private Text tutorialButton06Text;
    [SerializeField]
    private GameObject tutorialEndContainer;
    [SerializeField]
    private GameObject tutorialWin;
    [SerializeField]
    private GameObject tutorialLose;

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

    // declare inits
    void Awake()
    {
        GemmGridLayout = new Gemm[boardDimX, boardDimY];
        GemmGridLayoutCopy = new Gemm[boardDimX, boardDimY];
        FloodMatchDict = new Dictionary<GemmLoc, Gemm>();
        GemmDictToDestroy = new Dictionary<GemmLoc, Gemm>();
        Check3List = new List<GemmLoc>();
        tutorialButton01Text = tutorialButton01.transform.Find("Text").GetComponent<Text>();
        tutorialButton02Text = tutorialButton02.transform.Find("Text").GetComponent<Text>();
        tutorialButton02.SetActive(false);
        tutorialButton03Text = tutorialButton03.transform.Find("Text").GetComponent<Text>();
        tutorialButton03.SetActive(false);
        tutorialButton04Text = tutorialButton04.transform.Find("Text").GetComponent<Text>();
        tutorialButton04.SetActive(false);
        tutorialButton05Text = tutorialButton05.transform.Find("Text").GetComponent<Text>();
        tutorialButton05.SetActive(false);
        tutorialButton06Text = tutorialButton06.transform.Find("Text").GetComponent<Text>();
        tutorialButton06.SetActive(false);

        isMatching = false;
        areGemmsFalling = false;
        isFirstDrop = true;
        w8ForRotation = false;
        wantGemmDrop = false;
        didAllClear = false;
        didAllClearAtLeastOnce = false;
        part1AllClear = false;
        gameOverTriggered = false;
        textLock = true;

        cyansRemaining = goalNumCyan;
        greensRemaining = goalNumGreen;
        orangesRemaining = goalNumOrange;
        pinksRemaining = goalNumPink;
        redsRemaining = goalNumRed;
        violetsRemaining = goalNumViolet;
        yellowsRemaining = goalNumYellow;
        gemmsDestroyedin1Board = 0;
        gameOverTimer = 0f;

        currNumMoves = totalMoves;
    }

    void Start()
    {
        StartCoroutine(GameEventFuncs());
        StartCoroutine(SetupTutorialBoard());
    }

    IEnumerator GameEventFuncs()
    {
        yield return new WaitForSeconds(0);
        GameEventsScript.endAllClearFX.AddListener(endAllClearFX);
        GameEventsScript.endBonusFX.AddListener(endBonusFX);
        GameEventsScript.clearGems.Invoke(new GameEventsScript.DestroyedGemsData(cyansRemaining, greensRemaining, orangesRemaining, pinksRemaining, redsRemaining, violetsRemaining, yellowsRemaining, bonusFXOn, allClearBonusAmount));
        GameEventsScript.countRound.Invoke(new GameEventsScript.CountRoundData(currNumMoves, totalMoves));
    }

    void Update()
    {
        if (isFirstDrop || isMatching || areGemmsFalling || isGameOver || w8ForRotation || textLock)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            //track cursor position/state for if gemmselected and spawn clone
            touchPos = Camera.main.ScreenPointToRay(Input.mousePosition);
            
            //tutorial: check touchpos loc is correct, otherwise reset indicators
            if(tutorialTransition01Lock)
            {
                int gridXPos = GetPosOnGrid(touchPos.origin.x, boardDimX);
                int gridYPos = GetPosOnGrid(touchPos.origin.y, boardDimY);
                if (gridXPos != 0 || gridYPos != 4)
                {
                    return;
                } else 
                {
                    GameEventsScript.tutorialEvent01dot5.Invoke();
                }
            } else if (tutorialTransition04Lock)
            {
                int gridXPos = GetPosOnGrid(touchPos.origin.x, boardDimX);
                int gridYPos = GetPosOnGrid(touchPos.origin.y, boardDimY);
                if (gridXPos != 0 || gridYPos != 2)
                {
                    return;
                } else 
                {
                    GameEventsScript.tutorialEvent04dot5.Invoke();
                }
            }

            if (Mathf.RoundToInt(touchPos.origin.x) < boardDimX && Mathf.RoundToInt(touchPos.origin.x) > -1 && Mathf.RoundToInt(touchPos.origin.y) < boardDimY && Mathf.RoundToInt(touchPos.origin.y) > -1)
            {
                if(tutorialTransition06Lock)
                {
                    GameEventsScript.tutorialEvent06.Invoke();
                }
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
            //send out signal to drop the gemm
            StartCoroutine(delayDrop());
        }
    }

    void FixedUpdate()
    {
        if (isFirstDrop || isRotating || areGemmsFalling || isMatching || isGameOver || textLock)
        {
            return;
        }

        //if GemmSelected and Gemmcan follow cursor
        if(isGemmSelected && canGemmFollowMe)
        {
            //if GemmClone alive, show gemm movements on screen
            if(isGemmCloneAlive)
            {
                //During tutorial, make sure gemm moves on specified row only
                if(tutorialTransition01Lock)
                {
                    Vector3 pos = new Vector3(touchPos.origin.x, 4f, touchPos.origin.z);
                    gemmClone.gemmGObj.transform.Translate((pos - gemmClone.gemmGObj.transform.position) * Time.fixedDeltaTime * moveSpeed);
                    ShowGemmMovement(pos);
                    return;
                } else if(tutorialTransition04Lock)
                {
                    Vector3 pos = new Vector3(touchPos.origin.x, 2f, touchPos.origin.z);
                    gemmClone.gemmGObj.transform.Translate((pos - gemmClone.gemmGObj.transform.position) * Time.fixedDeltaTime * moveSpeed);
                    ShowGemmMovement(pos);
                    return;
                }
                gemmClone.gemmGObj.transform.Translate((touchPos.origin - gemmClone.gemmGObj.transform.position) * Time.fixedDeltaTime * moveSpeed);
                ShowGemmMovement(touchPos.origin);
            }
        }

        //when player releases gemm and gemm has moved, start matching
        if (wantGemmDrop)
        {
            w8ForRotation = false;
            DropGemm();
            if (movedGemm)
            {
                //during tutorial, if gemm is not dropped in correct location, reset indicators and board
                //otherwise move on to next tutorial part
                if (tutorialTransition01Lock)
                {
                    if(prevActiveTouchPos.x != 5 || prevActiveTouchPos.y != 4)
                    {
                        GemmGridLayout = GemmGridLayoutCopy;
                        ClearGridLayout();
                        RemakeGemmsForUndo();
                        return;
                    } else 
                    {
                        //fix opacity, turn on/off appropriate images
                        for(int y = 2; y < boardDimY - 1; y++)
                        {
                            for(int x = 0; x < boardDimX; x++)
                            {
                                GemmGridLayout[x, y].gemmGObj.GetComponent<SpriteRenderer>().color = ChangeGemmAlpha(GemmGridLayout[x, y], 1f);
                            }
                        }
                        tutorialButton01.SetActive(false);
                        tutorialButton02.SetActive(true);
                        GameEventsScript.tutorialEvent02.Invoke();
                        tutorialTransition01Lock = false;
                        tutorialTransition02Lock = true;
                        textLock = true;
                        return;
                    }
                } else if (tutorialTransition04Lock)
                {
                    if(prevActiveTouchPos.x != 5 || prevActiveTouchPos.y != 2)
                    {
                        GemmGridLayout = GemmGridLayoutCopy;
                        ClearGridLayout();
                        RemakeGemmsForUndo();
                        return;
                    } else 
                    {
                        for(int y = 0; y < boardDimY; y++)
                        {
                            for(int x = 0; x < boardDimX; x++)
                            {
                                GemmGridLayout[x, y].gemmGObj.GetComponent<SpriteRenderer>().color = ChangeGemmAlpha(GemmGridLayout[x, y], 1f);
                            }
                        }
                        tutorialButton04.SetActive(false);
                        GameEventsScript.tutorialEvent05.Invoke();
                        tutorialTransition04Lock = false;
                        tutorialTransition05Lock = true;
                        textLock = true;
                    }
                }
                StartCoroutine(MatchGemms());
            }
        }
    }

    //Wrapper for creating initial board
    IEnumerator SetupTutorialBoard()
    {
        yield return new WaitForSecondsRealtime(0);
        MakeGemmsInTutorialGrid();
        MoveGemmsDown();
        GemmGridLayoutCopy = GemmGridLayout.Clone() as Gemm[,];
        isFirstDrop = false;
    }

    //Create Gemms in Grid
    private void MakeGemmsInTutorialGrid()
    {
        GameObject gemm;
        for (int y = 0; y < boardDimY; y++)
        {
            for (int x = 0; x < boardDimX; x++)
            {
                //List gemm options
                List<GameObject> availableGems = new List<GameObject>();    
                availableGems.AddRange(GemmOptions);
                
                //detect if 2 in a row left and down when in row/column 3+
                //assign a random gemm that makes it so grid does not contain 3 in a rows
                if (y == 4)
                {
                    gemm = GemmOptions[(x)%GemmOptions.Count];
                } else if (y == 2 || y == 3)
                {
                    gemm = GemmOptions[(x+1)%GemmOptions.Count];
                } else 
                {
                    gemm = GemmOptions[(x+2)%GemmOptions.Count];
                }
                MakeGemm(gemm, x, y);
            }
        }
    }

    //Instantiate Gemm in Setup Grid
    private void MakeGemm(GameObject gemmGObj, int x, int y)
    {
        int drop;
        if (isFirstDrop)
        {
            drop = 0;
        } else 
        {
            drop = dropOffset;
        }
        GameObject currentGemm = (GameObject)Instantiate(gemmGObj, new Vector2((float)x, (float)y + (float)drop), Quaternion.identity);
        currentGemm.transform.parent = transform;
        GemmGridLayout[x, y] = new Gemm {
            gemmGObj = currentGemm,
            tagId = currentGemm.tag,
            floodVisited = false,
            floodMatched = false,
            destroyed = false,
            dropDist = drop,
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

    //delay want gemmdrop request
    IEnumerator delayDrop()
    {
        w8ForRotation = true;
        yield return new WaitUntil(() => !isRotating);
        wantGemmDrop = true;

        //Reset States
        isGemmSelected = canGemmFollowMe = false;
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

    //adjusts gemm transparency (clone and grid)
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
    }

    //Setup for Gemm Matching
    private void DropGemm()
    {
        // fix opacity of selected gemm
        if(movedGemm || (!tutorialTransition01Lock && !tutorialTransition04Lock))
        {
            GemmGridLayout[prevActiveTouchPos.x, prevActiveTouchPos.y].gemmGObj.GetComponent<SpriteRenderer>().color = ChangeGemmAlpha(GemmGridLayout[prevActiveTouchPos.x, prevActiveTouchPos.y], 1.0f);
        } else if (tutorialTransition01Lock)
        {
            GemmGridLayout[0, 4].gemmGObj.GetComponent<SpriteRenderer>().color = ChangeGemmAlpha(GemmGridLayout[0, 4], 1.0f);
        } else if (tutorialTransition04Lock)
        {
            GemmGridLayout[0, 2].gemmGObj.GetComponent<SpriteRenderer>().color = ChangeGemmAlpha(GemmGridLayout[0, 2], 1.0f);
        }

        //gemmclone cleanup
        if (gemmClone.gemmGObj != null)
        {
            Destroy(gemmClone.gemmGObj);
            if(tutorialTransition01Lock)
            {
                GameEventsScript.tutorialEvent01dot5.Invoke();
            }
            if(tutorialTransition04Lock)
            {
                GameEventsScript.tutorialEvent04dot5.Invoke();
            }
            if(tutorialTransition06Lock)
            {
                GameEventsScript.tutorialEvent06.Invoke();
            }
        }
        wantGemmDrop = isGemmCloneAlive = false;
    }

    //Matches gemms; looks for 3 gemms or more in a row
    IEnumerator MatchGemms()
    {
        ResetBoardForMatching();
        isMatching = true;
        //match and clear gemms
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
                        GameEventsScript.clearGems.Invoke(new GameEventsScript.DestroyedGemsData(cyansRemaining, greensRemaining, orangesRemaining, pinksRemaining, redsRemaining, violetsRemaining, yellowsRemaining, bonusFXOn, allClearBonusAmount));
                        yield return new WaitForSeconds(0.25f);
                        didDestroy = false;
                    }
                }
            }
        }
        checkForAllClear();
        if(didAllClear && part1AllClear)
        {
            part1AllClear = false;
            yield return new WaitUntil(() => !allClearFXOn);
        }
        
        //Replenish board after matching
        SetupRemainingGemmsForDrop();
        RemakeDestroyedGemms();
        MoveGemmsDown();
        yield return new WaitUntil(() => !areGemmsFalling);

        CheckGameOver();
        if(isGameOver)
        {
            fallPercentIncrease *= 2;
        }
        //if gemms were destroyed, we can repeat matching process 
        if(canContinueMatching)
        {
            canContinueMatching = false;
            StartCoroutine(RepeatMatchGemms());
        } else
        {
            //if all clear happened, apply bonus
            if(didAllClear)
            {
                cyansRemaining -= allClearBonusAmount;
                greensRemaining -= allClearBonusAmount;
                orangesRemaining -= allClearBonusAmount;
                pinksRemaining -= allClearBonusAmount;
                redsRemaining -= allClearBonusAmount;
                yellowsRemaining -= allClearBonusAmount;
                violetsRemaining -= allClearBonusAmount;
                GameEventsScript.clearGems.Invoke(new GameEventsScript.DestroyedGemsData(cyansRemaining, greensRemaining, orangesRemaining, pinksRemaining, redsRemaining, violetsRemaining, yellowsRemaining, bonusFXOn, allClearBonusAmount));
                currNumMoves--;
                GameEventsScript.countRound.Invoke(new GameEventsScript.CountRoundData(currNumMoves, totalMoves));
                CheckGameOver();
                TriggerGameOver();
                //if game isn't over, apply bonus FX
                if(!isGameOver)
                {
                    bonusFXOn = true;
                    fallPercentIncrease /= 2;
                    GameEventsScript.startBonusFX.Invoke(new GameEventsScript.DestroyedGemsData(cyansRemaining, greensRemaining, orangesRemaining, pinksRemaining, redsRemaining, violetsRemaining, yellowsRemaining, bonusFXOn, allClearBonusAmount));
                    yield return new WaitUntil(() => !bonusFXOn);
                    //wait till bonus FX done before moving on
                    if(tutorialTransition05Lock)
                    {
                        tutorialButton04.SetActive(false);
                        tutorialButton05.SetActive(true);
                        GameEventsScript.tutorialEvent05dot5.Invoke();
                    }
                }
                didAllClear = false;
            } else
            {
                //no all clear, just count the move and check gameover conditions
                if(didBoardChange())
                {
                    currNumMoves--;
                    GameEventsScript.countRound.Invoke(new GameEventsScript.CountRoundData(currNumMoves, totalMoves));
                    CheckGameOver();
                    TriggerGameOver();
                }
            }

            //reset bools
            isMatching = false;
            movedGemm = false;
        }
        //wait till matching is done before moving on
        if (tutorialTransition02Lock)
        {
            for(int y = 0; y < 2; y++)
            {
                for(int x = 0; x < boardDimX; x++)
                {
                    GemmGridLayout[x, y].gemmGObj.GetComponent<SpriteRenderer>().color = ChangeGemmAlpha(GemmGridLayout[x, y], 1f);
                }
            }
            tutorialButton02.SetActive(false);
            tutorialButton03.SetActive(true);
            GameEventsScript.tutorialEvent03.Invoke();
            tutorialTransition02Lock = false;
            textLock = true;
        }
    }

    //check gameover condition
    private void CheckGameOver()
    {
        if (redsRemaining <= 0 && greensRemaining <= 0 && cyansRemaining <= 0 && orangesRemaining <= 0 && pinksRemaining <= 0 && violetsRemaining <= 0 && yellowsRemaining <= 0)
        {
            isGameOver = true;
            isWin = true;
        } else if (currNumMoves < 1)
        {
            isGameOver = true;
            isWin = false;
        }
    }

    //cleanup tutorial imgs; display appropriate panels
    private void TriggerGameOver()
    {
        if (isGameOver && !gameOverTriggered)
        {
            tutorialButton06Text.text = "";
            GameEventsScript.tutorialEvent06.Invoke();
            gameOverTriggered = true;
            tutorialEndContainer.SetActive(true);
            if(isWin)
            {
                tutorialWin.SetActive(true);
            } else
            {
                tutorialLose.SetActive(true);
            }
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

    //count destroyed gemms
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
        gemmsDestroyedin1Board += GemmDictToDestroy.Count;
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

    //check for allclear
    private void checkForAllClear()
    {
        if (gemmsDestroyedin1Board == (boardDimX * boardDimY))
        {
            if(!didAllClearAtLeastOnce)
            {
                didAllClearAtLeastOnce = true;
            }
            didAllClear = true;
            part1AllClear = true;
            allClearFXOn = true;
            GameEventsScript.startAllClearFX.Invoke();
            fallPercentIncrease *= 2;
        }
        gemmsDestroyedin1Board = 0;
    }

    private void endAllClearFX()
    {
        allClearFXOn = false;
    }

    private void endBonusFX()
    {
        bonusFXOn = false;
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
                    //randomize recreated gemms for last part of tutorial
                    if(tutorialTransition05Lock)
                    {
                        GameObject randGemm;
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
                    //setup tutorial for all-clear opportunity
                    } else if(tutorialTransition02Lock)
                    {
                        GameObject tutGemm;
                        if (y == 3 || y == 4)
                        {
                            tutGemm = GemmOptions[(x+2)%GemmOptions.Count];
                            MakeGemm(tutGemm, x, y);
                        } else if (y == 2)
                        {
                            tutGemm = GemmOptions[(x+1)%GemmOptions.Count];
                            MakeGemm(tutGemm, x, y);
                        }
                    //do the normal drops
                    } else
                    {
                        GameObject newGemm = GemmOptions[UnityEngine.Random.Range(0, GemmOptions.Count)];
                        MakeGemm(newGemm, x, y);
                    }
                }
            }
        }
    }

    //check to see if Gemm Board state changed
    private bool didBoardChange()
    {
        for (int y = 0; y < boardDimY; y++)
        {
            for (int x = 0; x < boardDimX; x++)
            {
                if (GemmGridLayout[x,y].tagId != GemmGridLayoutCopy[x,y].tagId)
                {
                    GemmGridLayoutCopy = GemmGridLayout.Clone() as Gemm[,];
                    return true;
                }
            }
        }        
        return false;
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

        //during tutorial, remake gemms with correct opacity
        for(int y = 0; y < boardDimY; y++)
        {
            for(int x = 0; x < boardDimX; x++)
            {
                if ((tutorialTransition01Lock && y < boardDimY-1) || (tutorialTransition04Lock && (y < 2 || y > 2)))
                {
                    GemmGridLayout[x, y].gemmGObj.GetComponent<SpriteRenderer>().color = ChangeGemmAlpha(GemmGridLayout[x, y], .5f);
                }                
            }
        }

    }

    //first button click: unlock board, change text, turn on move indicators
    public void tutorialTransition01()
    {
        textLock = false;
        tutorialButton01Text.text = "Let's make a move. Drag the Orb across the board.";
        tutorialTransition01Lock = true;
        GameEventsScript.tutorialEvent01.Invoke();
        changeAlphasT01();
    }

    //make specific gemms transparent
    private void changeAlphasT01()
    {
        for(int y = 0; y < boardDimY - 1; y++)
        {
            for(int x = 0; x < boardDimX; x++)
            {
                GemmGridLayout[x, y].gemmGObj.GetComponent<SpriteRenderer>().color = ChangeGemmAlpha(GemmGridLayout[x, y], .5f);
            }
        }
    }

    public void tutorialTransition02()
    {
        textLock = false;
        GameEventsScript.tutorialEvent02dot5.Invoke();
    }

    public void tutorialTransition03()
    {
        tutorialButton03.SetActive(false);
        tutorialButton04.SetActive(true);
        GameEventsScript.tutorialEvent03dot5.Invoke();
    }

    public void tutorialTransition04()
    {
        textLock = false;
        tutorialButton04Text.text = "Let's drag the Orb across the board again!";
        tutorialTransition04Lock = true;
        GameEventsScript.tutorialEvent04.Invoke();
        changeAlphasT04();
    }

    //make specific gemms transparent
    private void changeAlphasT04()
    {
        for(int y = 0; y < 2; y++)
        {
            for(int x = 0; x < boardDimX; x++)
            {
                GemmGridLayout[x, y].gemmGObj.GetComponent<SpriteRenderer>().color = ChangeGemmAlpha(GemmGridLayout[x, y], .5f);
            }
        }
        for(int y = 3; y < boardDimY; y++)
        {
            for(int x = 0; x < boardDimX; x++)
            {
                GemmGridLayout[x, y].gemmGObj.GetComponent<SpriteRenderer>().color = ChangeGemmAlpha(GemmGridLayout[x, y], .5f);
            }
        }
    }

    public void tutorialTransition05()
    {
        tutorialButton05.SetActive(false);
        tutorialButton06.SetActive(true);
    }

    public void tutorialTransition06()
    {
        GemmGridLayoutCopy = GemmGridLayout.Clone() as Gemm[,];
        tutorialTransition05Lock = false;
        textLock = false;
        tutorialButton06Text.text = "Now complete the Tutorial.";
        GameEventsScript.tutorialEvent05dot51.Invoke();
        tutorialTransition06Lock = true;
    }

    public void MainMenu()
    {
        SceneManager.LoadSceneAsync(0);
    }

    public void RedoTutorial()
    {
        SceneManager.LoadSceneAsync(1);
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
}
