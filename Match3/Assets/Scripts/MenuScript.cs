using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuScript : MonoBehaviour
{
    //Serialized values
    [SerializeField]
    private GameObject menuList;
    [SerializeField]
    private string difficulty;

    //non-serialized values
    private bool inside;
    
    void Start()
    {
        inside = false;
        if(menuList != null)
        {
            menuList.SetActive(false);
        }
    }

    void Update()
    {
        if(inside)
        {
            return;
        } else 
        {
            if(menuList != null)
            {
                if(Input.GetMouseButtonDown(0) && menuList.activeInHierarchy)
                {
                    menuList.SetActive(false);
                    GameEventsScript.menuListOnOff.Invoke();
                }
            }
        }
    }

    public void DetectInMenu()
    {
        inside = true;
    }

    public void DetectOutMenu()
    {
        inside = false;
    }

    public void DisplayMenuList()
    {
        if(menuList != null)
        {
            if(!menuList.activeInHierarchy)
            {
                menuList.SetActive(true);
                GameEventsScript.menuListOnOff.Invoke();
            } else 
            {
                menuList.SetActive(false);
                GameEventsScript.menuListOnOff.Invoke();
            }
        }
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadSceneAsync(0);
    }

    public void LoadTutorialScene()
    {
        SceneManager.LoadSceneAsync(1);
    }

    public void LoadEasy()
    {
        if(menuList != null)
        {
            if(menuList.activeInHierarchy)
            {
                GameEventsScript.gameIsOver.Invoke(new GameEventsScript.GameOverData(difficulty, true, false, false, 0, 0));
            }
        }
        SceneManager.LoadSceneAsync(2);
    }

    public void LoadMedium()
    {
        if(menuList != null)
        {
            if(menuList.activeInHierarchy)
            {
                GameEventsScript.gameIsOver.Invoke(new GameEventsScript.GameOverData(difficulty, true, false, false, 0, 0));
            }
        }
        SceneManager.LoadSceneAsync(3);
    }

    public void LoadHard()
    {
        if(menuList != null)
        {
            if(menuList.activeInHierarchy)
            {
                GameEventsScript.gameIsOver.Invoke(new GameEventsScript.GameOverData(difficulty, true, false, false, 0, 0));
            }
        }
        SceneManager.LoadSceneAsync(4);
    }
}
