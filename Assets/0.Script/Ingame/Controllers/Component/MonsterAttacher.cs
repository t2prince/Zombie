using System;
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

        private void SpawnMonster()
        {
            currentWaveNumber++;

            if (waveStartNumber > currentWaveNumber ||
                waveEndNumber < currentWaveNumber)
                return;
            
            //TODO: 몬스터 정보 얻어와 스폰하기
        }
        
    }
}