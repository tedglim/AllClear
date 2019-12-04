using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RulesScript : MonoBehaviour
{
    void Awake()
    {
        // GameEventsScript.clearGems.AddListener(UpdateTextCount);
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // private void UpdateTextCount(GemsDestroyedData data)
    // {

    // }

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
