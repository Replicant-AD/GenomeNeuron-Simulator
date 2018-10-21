﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.PostProcessing;
using System;
public class Master : MonoBehaviour
{

    public static Master Instance = null;
    [HideInInspector] public GameObject ManagerGO;
    [HideInInspector] public Manager Manager;
    public Globalizator globalizator;
    public MenuController menuController;

    [Header("Car prefabs")]
    public GameObject blueCarPrefab;
    public GameObject redCarPrefab;
    [Header("If you want to create a demo save file")]
    [Tooltip("If it is checked, the save file won't contains the statistics and the generation counter.")]
    public bool CreateDemoSave = false;

    [Range(0f, 2f)]
    public float popupFadeTime = 0.1f;

    [Range(0f, 2f)]
    public float popupDurationTime = 0.5f;

    [Header("Component references")]
    public GameObject UIStats;
    public GameObject inGameMenu;
    public GameObject mainMenuCanvas;
    public GameObject loadingScreen;
    public GameObject bgLights;
    public GameObject minimapCamera;
    public CameraDrone cameraDrone;
    public GameObject Camera;
    public GameObject popUp;

    [Header("Track prefabs")]
    public GameObject[] TrackPrefabs;
    public GameObject[] WayPointPrefabs;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        ManagerGO = new GameObject("ManagerObject");
        Manager = ManagerGO.AddComponent<Manager>();
        DontDestroyOnLoad(ManagerGO);

        DontDestroyOnLoad(UIStats);
        DontDestroyOnLoad(inGameMenu);
        DontDestroyOnLoad(minimapCamera);
        DontDestroyOnLoad(mainMenuCanvas);
        DontDestroyOnLoad(bgLights);
        mainMenuCanvas.SetActive(true);
        UIStats.SetActive(false);
        inGameMenu.SetActive(false);

    }


    public void JoinGame()
    {
        Manager.JoinGame();
    }


    public void DisconnectGame()
    {
        Manager.DisconnectGame();
    }


    public void SaveGame()
    {
        Manager.SaveGame();
    }

    public void LoadGame()
    {
        Manager.LoadGame();
    }

    public void SaveStats()
    {
        Manager.SaveStats();
    }

    public void StartNewGame()
    {

        Manager.StartGame();
        Camera.transform.position = new Vector3(0.62f, 5.83f, -7.5f);



    }

    /// <summary>
    /// Beállítja a szimuláció sebességet
    /// </summary>
    public void SetTimeScale(float scale = 1.0f)
    {
        Time.timeScale = scale;
    }


    public void ExitGame()
    {
#if UNITY_EDITOR
        Debug.Log("Kilépés...");
        UnityEditor.EditorApplication.isPlaying = false;
#else
         Application.Quit();
#endif
    }



    [ContextMenu("Back to menu")]
    public void BackToMenu()
    {
        Camera.transform.position = new Vector3(0.62f, 305.83f, -7.55f);
        Camera.transform.rotation = new Quaternion();
        Camera.GetComponent<PostProcessingBehaviour>().enabled = false;

        cameraDrone.enabled = false;
        Destroy(GameObject.Find("GeneticAlgorithmDeletable"));
        Destroy(GameObject.Find("TrackDeletable"));
        Destroy(GameObject.Find("WaypointDeletable"));
        Destroy(GameObject.Find("RayHolderDeletable"));
        Destroy(GameObject.Find("CarHolderDeletable"));
        Manager.myUIPrinter.GenerationCount = 0;
        Destroy(Manager.playerCar);
        Destroy(ManagerGO);
        inGameMenu.SetActive(false);
        UIStats.SetActive(false);
        mainMenuCanvas.SetActive(true);

        ManagerGO = new GameObject("ManagerObject");
        Manager = ManagerGO.AddComponent<Manager>();
        DontDestroyOnLoad(ManagerGO);
        menuController.SetSeedText(RandomHelper.Seed.ToString());
    }

    public void SetLanguage(int lang)
    {
        if (lang == 1)
        {
            globalizator.SetLanguage(GameLanguage.ENGLISH);
        }
        else
        {
            globalizator.SetLanguage(GameLanguage.HUNGARIAN);
        }

        menuController.ChangedLanguage();
    }

}
