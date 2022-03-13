using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

using Nightmare;
using System;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

[System.Serializable]
public class Player
{
    public int score;
    public int health;
    public int grenades;
    public float[] position = {0, 0, 0};
    public void Clear()
    {
        score = 0;
        health = 0;
        grenades = 0;
        position = new float[]{0, 0, 0};
    }
}

[System.Serializable]
public class EnemyData
{
    public EnemyTypeEnum enemyType;
    public int health;
    public float[] position = {0, 0, 0};
}

[System.Serializable]
public class GameDataList
{
    public string savedScene;
    public int enemiesCount;
    public Player player = new Player();

    public void UpdateGameData(Player p)
    {
        player = p;
    }
}

public class SaveManager : MonoSingletonGeneric<SaveManager>
{
    [SerializeField] private EnemyManager enemyManager = null;
    [SerializeField] private PlayerHealth playerHealth = null;
    public static Action<Player> LoadPlayerData;
    public static Action<string> LoadLevel;
    public static Action Saving;
    public static Action<List<EnemyData>> LoadEnemyData;
    public static Action<EnemyTypeEnum, int, Vector3> SaveEnemyDataAction;
    string path = "Assets/SaveData/savefile.sav";

    public bool IsSaveGameLoading = false;
    GameDataList currentGameData = new GameDataList();

    private void Start()
    {
        // SaveEnemyDataAction += SaveEnemyData;
        enemyManager = FindObjectOfType<EnemyManager>();
    }

    private void Update()
    {
        if(enemyManager == null)
        {
            enemyManager = FindObjectOfType<EnemyManager>();
        }
        if(playerHealth == null)
        {
            playerHealth = FindObjectOfType<PlayerHealth>();
        }
        if(Input.GetKey(KeyCode.CapsLock))
        {
            SaveData();
        }
    }
    public async void SaveData()
    {
        // Saving?.Invoke();
        await Task.Delay(500);
        BinaryFormatter binaryFormatter = new BinaryFormatter();
        FileStream stream = new FileStream(path, FileMode.Create);

        UpdateCurrentGameData();

        
        binaryFormatter.Serialize(stream, currentGameData);

        stream.Close();
        currentGameData.player.Clear();
    }

    private void UpdateCurrentGameData()
    {
        currentGameData.savedScene = SceneManager.GetActiveScene().name;
        Player player = new Player();
        player.score = ScoreManager.score;
        player.health = playerHealth.currentHealth;
        player.grenades = GrenadeManager.grenades;
        for(int i = 0 ; i < player.position.Length ; i++)
        {
            switch(i)
            {
                case 0 :
                {
                    player.position[i] = playerHealth.gameObject.transform.position.x;
                    break;
                }
                case 1 :
                {
                    player.position[i] = playerHealth.gameObject.transform.position.y;
                    break;
                }
                case 2 :
                {
                    player.position[i] = playerHealth.gameObject.transform.position.z;
                    break;
                }
            }
        }
        
        EnemyData enemyData = new EnemyData();

        List<Component> enemyManagersComponents = new List<Component>();

        enemyManager.gameObject.GetComponents(typeof(EnemyManager), enemyManagersComponents);

        int enemiesCount = 0;

        foreach(EnemyManager item in enemyManagersComponents)
        {
            for(int i = 0 ; i < item.enemyList.Count ; i++, enemiesCount++)
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                FileStream stream = new FileStream(path + i, FileMode.Create);
                enemyData.health = item.enemyList[i].currentEnemyHealth;
                enemyData.enemyType = item.currentEnemyType;
                for(int j = 0 ; j < player.position.Length ; j++)
                {
                    switch(j)
                    {
                        case 0 :
                        {
                            enemyData.position[j] = item.enemyList[i].gameObject.transform.position.x;
                            break;
                        }
                        case 1 :
                        {
                            enemyData.position[j] = item.enemyList[i].gameObject.transform.position.y;
                            break;
                        }
                        case 2 :
                        {
                            enemyData.position[j] = item.enemyList[i].gameObject.transform.position.z;
                            break;
                        }
                    }
                }
                binaryFormatter.Serialize(stream, enemyData);
                stream.Close();
            }
            currentGameData.enemiesCount = enemiesCount;
        }
        Debug.Log(enemiesCount);
        currentGameData.UpdateGameData(player);
    }

    // private void SaveEnemyData(EnemyTypeEnum enemyType, int currentHealth, Vector3 currentPosition)
    // {
    //     EnemyData en = new EnemyData();
    //     en.health = currentHealth;
    //     en.position[0] = currentPosition.x;
    //     en.position[1] = currentPosition.y;
    //     en.position[2] = currentPosition.z;
    //     currentGameData.enemyData[enemyType].Add(en);
    // }

    public async void LoadData()
    {
        if(File.Exists(path))
        {
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);
            GameDataList currentGameData = new GameDataList();
            currentGameData = binaryFormatter.Deserialize(stream) as GameDataList;
            
            List<EnemyData> enemyData = new List<EnemyData>();
            BinaryFormatter enemyFormatter = new BinaryFormatter();
            for(int i = 0 ; i < currentGameData.enemiesCount - 1 ; i++)
            {
                FileStream streamEnemy = new FileStream(path + i, FileMode.Open);
                enemyData.Add(enemyFormatter.Deserialize(streamEnemy) as EnemyData);
            }

            IsSaveGameLoading = true;
            SceneManager.LoadSceneAsync("Main");
            await Task.Delay(500);
            LoadLevel?.Invoke(currentGameData.savedScene);
            await Task.Delay(500);
            LoadPlayerData?.Invoke(currentGameData.player);
            LoadEnemyData?.Invoke(enemyData);
            stream.Close();
        }
        else
        {
            Debug.LogError("File Not Found : " + path);
        }
        IsSaveGameLoading = false;
    }

    private void OnDestroy()
    {
        // SaveEnemyDataAction -= SaveEnemyData;
    }
}