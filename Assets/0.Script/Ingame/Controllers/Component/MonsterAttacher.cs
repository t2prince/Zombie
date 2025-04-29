using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Jamcat.Ingame.Controllers.Component
{
    public class MonsterAttacher : MonoBehaviour
    {
        [Serializable]
        public struct waveInfo
        {
            public int monsterId;
            public int level;
            public float startDelay;
            public float interval;
        }
        
        [SerializeField] private int waveStartNumber;
        [SerializeField] private int waveEndNumber;
        
        private int currentWaveNumber;
        
        public List<waveInfo> waveInfos;

        private void Start()
        {
                
        }

        public void SpawnMonster()
        {
            currentWaveNumber++;

            if (waveStartNumber > currentWaveNumber ||
                waveEndNumber < currentWaveNumber)
                return;

            StartCoroutine(StartWave());
        }
        public void StopWave()
        {
            StopAllCoroutines();
        }

        private IEnumerator StartWave()
        {
            if (waveInfos.Count <= 0 || waveInfos.Count >= waveEndNumber) yield break;
            var currentWave = waveInfos[currentWaveNumber];
            while (true)
            {
                yield return Util.Coroutine.WaitForSeconds(currentWave.startDelay);
                var monster = InGame.Monster.GetMonster(currentWave.monsterId, currentWave.level);
                
                monster.SetTarget(InGame.Map.Camp);
            }
            
        }
        
    }
}