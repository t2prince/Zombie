using System;

namespace Jamcat.Ingame
{
    public partial class InGame
    {
        public enum EventType
        {
            Killed,
        }
        public static void Notify(EventType eventType, params object[] args)
        {
            switch (eventType)
            {
                case EventType.Killed:
                    OnKilled(args[0].ToString(), args[1].ToString());
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
            }
        }

        private static void OnKilled(string name, string type)
        {
            
        }
    }
}