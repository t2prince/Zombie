using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Rpg.Sys.Secure
{
    [Serializable]
    public class CipherLong
    {
        [SerializeField] private long _priFix;
        [SerializeField] private long _value;

        public long Value
        {
            get => _priFix + _value;
            set
            {
                _priFix = Random.Range(1, 1024);
                _value = value - _priFix;
            }
        }

        public CipherLong()
        {
            _priFix = Random.Range(1, 1024);
        }

        public CipherLong(long val)
        {
            _priFix = Random.Range(1, 1024);
            _value = val - _priFix;
        }

        public static bool operator >(CipherLong x, long y)
        {
            return x.Value > y;
        }

        public static bool operator <(CipherLong x, long y)
        {
            return x.Value < y;
        }
        
        public static bool operator >=(CipherLong x, long y)
        {
            return x.Value >= y;
        }

        public static bool operator <=(CipherLong x, long y)
        {
            return x.Value <= y;
        }
        
        public static bool operator ==(CipherLong x, long y)
        {
            return x.Value == y;
        }

        public static bool operator !=(CipherLong x, long y)
        {
            return !(x == y);
        }

        public static CipherLong operator ++(CipherLong x)
        {
            return new CipherLong(x+1);
        }
        
        public static CipherLong operator --(CipherLong x)
        {
            return new CipherLong(x-1);
        }
        
        public static CipherLong operator +(CipherLong x, CipherLong y)
        {
            return new CipherLong(x.Value + y.Value);
        }

        public static long operator +(CipherLong val, long x)
        {
            return val.Value + x;
        }
        
        public static long operator +(long x, CipherLong val)
        {
            return val.Value + x;
        }
        
        public static long operator -(CipherLong val, long x)
        {
            return val.Value - x;
        }
        
        public static long operator -(long x, CipherLong val)
        {
            return x - val.Value;
        }
    }
}