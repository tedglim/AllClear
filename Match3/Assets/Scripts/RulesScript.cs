using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RulesScript : MonoBehaviour
{
    public void StartGame()
    {
        SceneManager.LoadSceneAsync(2);
    }

    public void GetMoreInstructions()
    {
        SceneManager.LoadSceneAsync(1);
    }

    public void GoBack()
    {
        SceneManager.LoadSceneAsync(0);
    }
}
