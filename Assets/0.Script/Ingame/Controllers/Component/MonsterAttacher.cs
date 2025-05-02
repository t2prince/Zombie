using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Jamcat.Ingame.Controllers.Component
{
    public class MonsterAttacher : Attacher
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

        public void SpawnMonster()
        {
            if (!InGame.Instance.Runner.IsSharedModeMasterClient) return;
            currentWaveNumber++;

            if (waveStartNumber > currentWaveNumber ||
                waveEndNumber < currentWaveNumber)
                return;

            StartCoroutine(StartWave());
        }
        public void StopWave()
        {
            if (!InGame.Instance.Runner.IsSharedModeMasterClient) return;
            StopAllCoroutines();
        }

        private IEnumerator StartWave()
        {
            if (waveInfos.Count <= 0 || currentWaveNumber >= waveEndNumber) yield break;
            var currentWave = waveInfos[currentWaveNumber];
            yield return Util.Coroutine.WaitForSeconds(currentWave.startDelay);
            
            while (true)
            {
                var monster = InGame.Monster.GetMonster(currentWave.monsterId, currentWave.level);
                monster.SetTarget(InGame.Map.Camp);
                monster.Spawn();
                
                monster.transform.position = transform.position;
                
                yield return Util.Coroutine.WaitForSeconds(currentWave.interval);
            }
            
        }
        
    }
}