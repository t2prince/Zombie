using System;
using UnityEngine;

namespace Jamcat.Ingame.Controllers.Component
{
    public class MonsterAttacher
    {
        [Serializable]
        public struct waveInfo
        {
            public int monsterId;
            public int startDelay;
            public int interval;
        }
        
        [SerializeField] private int waveStartNumber;
        [SerializeField] private int waveEndNumber;
        
        private int currentWaveNumber;

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