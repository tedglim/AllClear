using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManagerScript : MonoBehaviour
{
    public int boardX = 5;
    public int boardY = 5;

    private GameObject prevGem;
    private GameObject[,] boardLayout;
    public List<GameObject> gemPool;

    // Start is called before the first frame update
    void Start()
    {
        InitBoard();
        
    }

    private void InitBoard()
    {
        boardLayout = new GameObject[boardX, boardY];
        GameObject prevGem = null;
        GameObject[] prevRowGems = new GameObject[boardX];
        for (int y = 0; y < boardY; y++)
        {
            for (int x = 0; x < boardX; x++)
            {
                List<GameObject> availableGems = new List<GameObject>();
                availableGems.AddRange(gemPool);
                availableGems.Remove(prevGem);
                availableGems.Remove(prevRowGems[x]);
                prevGem = availableGems[UnityEngine.Random.Range(0, availableGems.Count)];
                prevRowGems[x] = prevGem;
                boardLayout[x, y] = (GameObject)Instantiate(prevGem, new Vector2(x - 2, y - 2), Quaternion.identity);
                boardLayout[x,y].transform.parent = transform;
                // boardLayout.Ad
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
