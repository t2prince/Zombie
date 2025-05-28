using System;
using System.Collections.Generic;
using Fusion;
using Jamcat.Ingame.Character;
using Jamcat.Managers.Monster;
using UnityEngine;

namespace Jamcat.Ingame
{
    public class MonsterController : MonoBehaviour
    {
        private NetworkRunner _runner => InGame.Instance.Runner;
        private List<GameObjectPool<Monster>> _monsterPoolList = new List<GameObjectPool<Monster>>();

        private void Start()
        {
            var data = MonsterManager.GetMonsterData();
            foreach (var monsterData in data)
            {
                 var pool = new GameObjectPool<Monster>(0, () =>
                 { 
                     var monster = _runner.Spawn(monsterData.monster).GetComponent<Monster>(); 
                     monster.transform.SetParent(transform); 
                     monster.transform.localScale = Vector3.one; 
                     monster.id = monsterData.id;
                
                    return monster;
                });
                
                _monsterPoolList.Add(pool);
            }
        }

        public Monster GetMonster(int id, int level)
        {
            var pool = _monsterPoolList[id];
            var monster = pool.Pop();
            monster.Level = level;
            monster.Spawn();
            monster.OnKilled += CollectMonster;

            return monster;
        }

        public void CollectMonster(BaseCharacter character)
        {
            var monster = character.GetComponent<Monster>();
            if (monster == null) return;
            
            monster.gameObject.SetActive(false);
        }
    }
}