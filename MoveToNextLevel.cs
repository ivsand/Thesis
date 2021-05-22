using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MoveToNextLevel : MonoBehaviour
{
    public int nextSceneLoad;
    // Start is called before the first frame update
    void Start()
    {
        PlayerPrefs.SetInt("levelAt", 2);
        nextSceneLoad = SceneManager.GetActiveScene().buildIndex + 1;
    }

    public void NextLevel()
    {
        SceneManager.LoadScene(nextSceneLoad);
        if(nextSceneLoad > PlayerPrefs.GetInt("levelAt"))
        {
            PlayerPrefs.SetInt("levelAt", nextSceneLoad);
            // PlayerPrefs.SetInt("levelAt", 2);
        }
    }
    
}
