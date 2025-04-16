using System.Collections.Generic;
using UnityEngine;

namespace Jamcat.Managers.Data
{
    public class BaseMasterData <T> : ScriptableObject where T : BaseData
    {
        public List<T> DataList;
    }

    
}