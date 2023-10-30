using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DontDestroyController : MonoBehaviour
{
    private DontDestroyController instance;
    [SerializeField] GameObject dontDestroyUIObject;
    private int clickCount = 0;
    private float timer = 0;
    void Awake()
    {
        if (instance != null)
        {
            Destroy(dontDestroyUIObject);
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            DontDestroyOnLoad(dontDestroyUIObject);
        }
    }
    void Update()//¾Û Á¾·á
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            clickCount++;
        }
        if (clickCount > 0)
            timer += Time.deltaTime;

        if (timer > 1f)
        {
            timer = 0f;
            clickCount = 0;
        }
        else if (clickCount > 1)
            Application.Quit();
    }
}
