using System;
using System.Collections.Generic;
using System.Text;
namespace Jjamcat
{
    public static class StringHelper
    {
        private static readonly List<string> _numbers;
        private static readonly List<string> _zeroFormattedNumString;

        private const int STATIC_INT_MAX = 100;

        static StringHelper()
        {
            _numbers = new List<string>
            {
                "0",
                "1",
                "2",
                "3",
                "4",
                "5",
                "6",
                "7",
                "8",
                "9",
                "10",
                "11",
                "12",
                "13",
                "14",
                "15",
                "16",
                "17",
                "18",
                "19",
                "20",
                "21",
                "22",
                "23",
                "24",
                "25",
                "26",
                "27",
                "28",
                "29",
                "30",
                "31",
                "32",
                "33",
                "34",
                "35",
                "36",
                "37",
                "38",
                "39",
                "40",
                "41",
                "42",
                "43",
                "44",
                "45",
                "46",
                "47",
                "48",
                "49",
                "50",                
            };
            
            _zeroFormattedNumString = new List<string>()
            {
                "00",
                "01",
                "02",
                "03",
                "04",
                "05",
                "06",                
                "07",
                "08",
                "09",
            };

        }
        
        static readonly StringBuilder StringBuilder = new StringBuilder();

        public static string ToStaticString(this long num)
        {
            if (num > int.MaxValue || num < int.MinValue)
                return num.ToString();
            
            return ((int)num).ToStaticString();            
        }

        public static string ToStaticString(this int num)
        {
            if(num < 0)
                return num.ToString();
            
            if (num > STATIC_INT_MAX)
                return num.ToString();
            
            if (_numbers.Count > num)
            {
                if (_numbers[num] != string.Empty && _numbers[num] != null)
                    return _numbers[num];

                _numbers[num] = num.ToString();
                return _numbers[num];
            }

            var newList = new List<string>(new string[num - _numbers.Count + 1]);
            
            _numbers.AddRange(newList);
            _numbers[num] = num.ToString();

            return _numbers[num];
        }

        public static string Format(string format, params object[] args)
        {
            StringBuilder.Length = 0;
			
            StringBuilder.AppendFormat(format, args);
            return StringBuilder.ToString();
        }

        public static string Combine(params object[] args)
        {
            StringBuilder.Length = 0;
            foreach (var str in args)
            {
                StringBuilder.Append(str);
            }            
            return StringBuilder.ToString();            
        }

        
        public static string ToNumberFormat(this int num)
        {
            //return num >= 10 ? num.ToStaticString() : _zeroFormattedNumString[num]; 
            return num.ToStaticString();
        }

        public static bool ContainsIgnoreCase(this string str, string key)
        {
            return str.ToLower().Contains(key.ToLower());
        }

        public static string ToNumberString(this long number)
        {             
            return ((double)number).ToNumberString();
        }
        
        public static string ToNumberString(this int number)
        {
            return ((double)number).ToNumberString();  
        }
        
        public static string ToNumberString(this double num)
        {
            return num switch
            {
                // Quintillion
                >= 1e18 => (num / 1e18).ToString("0.##") + "Qi",
                // Quadrillion
                >= 1e15 => (num / 1e15).ToString("0.##") + "Qa",
                // Trillion
                >= 1e12 => (num / 1e12).ToString("0.##") + "T",
                // Billion
                >= 1e9 => (num / 1e9).ToString("0.##") + "B",
                // Million
                >= 1e6 => (num / 1e6).ToString("0.##") + "M",
                // Thousand
                >= 1e3 => (num / 1e3).ToString("0.##") + "K",
                _ => num.ToString("#,0")
            };
        }
    }

}

