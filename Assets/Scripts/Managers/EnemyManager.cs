using System.Collections.Generic;
using UnityEngine;

namespace Nightmare
{
    public class EnemyManager : PausibleObject
    {
        private PlayerHealth playerHealth;
        public EnemyHealth enemy;
        public EnemyTypeEnum currentEnemyType;
        public float spawnTime = 3f;
        public Transform[] spawnPoints;
        public List<EnemyHealth> enemyList = new List<EnemyHealth>();

        private float timer;
        private int spawned = 0;

        void Start ()
        {
            timer = spawnTime;
            SaveManager.LoadEnemyData += LoadEnemyData;
            EnemyHealth.DeathAction += UpdateList;
        }

        void OnEnable()
        {
            playerHealth = FindObjectOfType<PlayerHealth>();
            StartPausible();
        }

        void OnDestroy()
        {
            StopPausible();
            SaveManager.LoadEnemyData -= LoadEnemyData;
            EnemyHealth.DeathAction -= UpdateList;
        }

        void Update()
        {
            if (isPaused)
                return;

            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                Spawn();
                timer = spawnTime;
            }
        }

        void Spawn ()
        {           
            // If the player has no health left...
            if(playerHealth.currentHealth <= 0f)
            {
                // ... exit the function.
                return;
            }

            // Find a random index between zero and one less than the number of spawn points.
            int spawnPointIndex = Random.Range (0, spawnPoints.Length);

            // Create an instance of the enemy prefab at the randomly selected spawn point's position and rotation.
            
            EnemyHealth temp = Instantiate (enemy, spawnPoints[spawnPointIndex].position, spawnPoints[spawnPointIndex].rotation);
            temp.currentEnemyType = this.currentEnemyType;
            enemyList.Add(temp);
        }

        private void UpdateList(EnemyHealth go)
        {
            for(int i = 0 ; i < enemyList.Count ; i++)
            {
                if(go.GetInstanceID().Equals(enemyList[i].GetInstanceID()))
                {
                    enemyList.RemoveAt(i);
                }
            }
        }

        private void LoadEnemyData(List<EnemyData> enemyDatas)
        {
            for(int i = 0 ; i < enemyDatas.Count ; i++)
            {
                if(enemyDatas[i].enemyType == currentEnemyType)
                {
                    Vector3 pos = new Vector3();
                    pos.x = enemyDatas[i].position[0];
                    pos.y = enemyDatas[i].position[1];
                    pos.z = enemyDatas[i].position[2];
                    EnemyHealth temp = Instantiate (enemy);
                    temp.transform.position = pos;
                    temp.currentEnemyHealth = enemyDatas[i].health;
                    enemyList.Add(temp);
                }
            }
        }
    }
}