using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Rpg.Sys.Secure
{
    [Serializable]
    public class CipherInt
    {
        [SerializeField] private int _priFix;
        [SerializeField] private int _value;

        public int Value
        {
            get => _priFix + _value;
            set
            {
                _priFix = Random.Range(1, 1024);
                _value = value - _priFix;
            }
        }

        public CipherInt()
        {
            _priFix = Random.Range(1, 1024);
        }

        public CipherInt(int val)
        {
            _priFix = Random.Range(1, 1024);
            _value = val - _priFix;
        }

        public static bool operator >(CipherInt x, int y)
        {
            return x.Value > y;
        }

        public static bool operator <(CipherInt x, int y)
        {
            return x.Value < y;
        }
        
        public static bool operator >=(CipherInt x, int y)
        {
            return x.Value >= y;
        }

        public static bool operator <=(CipherInt x, int y)
        {
            return x.Value <= y;
        }
        
        public static bool operator ==(CipherInt x, int y)
        {
            return x.Value == y;
        }

        public static bool operator !=(CipherInt x, int y)
        {
            return !(x == y);
        }

        public static CipherInt operator ++(CipherInt x)
        {
            return new CipherInt(x+1);
        }
        
        public static CipherInt operator --(CipherInt x)
        {
            return new CipherInt(x-1);
        }
        
        public static CipherInt operator +(CipherInt x, CipherInt y)
        {
            return new CipherInt(x.Value + y.Value);
        }

        public static int operator +(CipherInt val, int x)
        {
            return val.Value + x;
        }
        
        public static int operator +(int x, CipherInt val)
        {
            return val.Value + x;
        }
        
        public static int operator -(CipherInt val, int x)
        {
            return val.Value - x;
        }
        
        public static int operator -(int x, CipherInt val)
        {
            return x - val.Value;
        }
    }
}