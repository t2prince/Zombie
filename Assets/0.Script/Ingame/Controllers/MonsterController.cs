using System;
using System.Collections.Generic;
using Jamcat.Ingame.Character;
using Jamcat.Managers.Monster;
using UnityEngine;

namespace Jamcat.Ingame
{
    public class MonsterController : MonoBehaviour
    {
        private List<GameObjectPool<Monster>> _monsterPoolList = new List<GameObjectPool<Monster>>();

        private void Start()
        {
            var data = MonsterManager.GetMonsterData();
            foreach (var monsterData in data)
            {
                var pool = new GameObjectPool<Monster>(8, () =>
                {
                    var monster = Instantiate(monsterData.monster);
                    monster.transform.SetParent(transform);
                    monster.transform.localPosition = Vector3.zero;
                    monster.transform.localRotation = Quaternion.identity;
                    monster.transform.localScale = Vector3.one;
                    monster.gameObject.SetActive(false);

                    return monster;
                });
                
                _monsterPoolList.Add(pool);
            }
        }
    }
}