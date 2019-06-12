using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManagerScript : MonoBehaviour
{
    public bool isGameOver = false;
    public GameObject restartMenu;

    // Start is called before the first frame update
    void Start()
    {
        // restartMenu = GameObject.Find("Canvas/RestartMenu");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Restart()
    {
        SceneManager.LoadSceneAsync(0);
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void GameOver()
    {
        isGameOver = true;
        restartMenu.SetActive(true);


    }
}
