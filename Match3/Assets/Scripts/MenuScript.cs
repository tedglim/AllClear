using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuScript : MonoBehaviour
{
    [SerializeField]
    private GameObject menuList;

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

    public void LoadStandardScene()
    {
        SceneManager.LoadSceneAsync(2);
    }

    public void LoadIntenseScene()
    {
        SceneManager.LoadSceneAsync(3);
    }
}
