using System.Net.NetworkInformation;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManagerScript : MonoBehaviour
{
    private struct Gem {
        public GameObject gemObj;
        public int matchCount;
        public int dropDist;
    }

    public float camOffsetX = 2.5f;
    public float camOffsetY = 2.0f;
    public int boardDimX = 6;
    public int boardDimY = 5;
    public int boardYDropOffset = 5;
    private Gem[,] gemGridLayout;

    public List<GameObject> gemOptions;
    private GameObject prevGem;
    private GameObject[] prevGemRow;

    public float fallDelay = .01f;
    public float fallPercentInterval = .05f;

    // Start is called before the first frame update
    void Start()
    {
        InitBoard();
        MoveGemsDown();
    }

    // Update is called once per frame
    void Update()
    {
        //
    }

    private void InitBoard()
    {
        gemGridLayout = new Gem[boardDimX, boardDimY];
        prevGemRow = new GameObject[boardDimX];
        prevGem = null;
        for (int y = 0; y < boardDimY; y++)
        {
            for (int x = 0; x < boardDimX; x++)
            {
                //List possible gem options
                List<GameObject> availableGems = new List<GameObject>();
                availableGems.AddRange(gemOptions);

                //bottom row don't consider
                if(y > 0) {
                    //remove gem object from previous row 1 position below
                    availableGems.Remove(prevGemRow[x]);
                }

                //far left, don't consider
                if (x > 0)
                {
                    //remove gem object from prev position: 1 pos left;
                    availableGems.Remove(prevGem);
                }

                //assign Gem to all locations that need to know it, and instantiate
                prevGem = availableGems[UnityEngine.Random.Range(0, availableGems.Count)];
                prevGemRow[x] = prevGem;
                GameObject currentGem = (GameObject)Instantiate(prevGem, new Vector2((float)x - camOffsetX, (float)y - camOffsetY + (float)boardYDropOffset), Quaternion.identity);
                currentGem.transform.parent = transform;
                gemGridLayout[x,y] = new Gem {
                    gemObj = currentGem,
                    matchCount = 0,
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
                    Vector2 startPos = currGem.gemObj.transform.position;
                    Vector2 endPos = new Vector2(currGem.gemObj.transform.position.x, currGem.gemObj.transform.position.y - currGem.dropDist);
                    StartCoroutine(MoveGemsDown(currGem, startPos, endPos));
                    
                    //reset drop distance
                    currGem.dropDist = 0;
                }
            }
        }
    }

    //moves Gems down at specified lerp rate
    IEnumerator MoveGemsDown(Gem gem, Vector2 start, Vector2 end)
    {
        float fallPercent = 0.0f;
        while(fallPercent <= 1.0f)
        {
            gem.gemObj.transform.position = Vector2.Lerp(start, end, fallPercent);
            fallPercent += fallPercentInterval;
            yield return new WaitForSeconds(fallDelay);
        }
        gem.gemObj.transform.position = end;
    }
}
