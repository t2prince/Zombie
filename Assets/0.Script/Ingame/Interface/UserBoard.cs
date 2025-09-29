using System.Collections.Generic;
using UnityEngine;

namespace Jamcat.Ingame.Interface
{
    public class UserBoard : MonoBehaviour
    {
        private List<string> userNames = new List<string>();

        public void AddUser(string userName)
        {
            if (!userNames.Contains(userName))
            {
                userNames.Add(userName);
            }
        }

        public void RemoveUser(string userName)
        {
            userNames.Remove(userName);
        }

        public List<string> GetUserNames()
        {
            return new List<string>(userNames);
        }
    }
}