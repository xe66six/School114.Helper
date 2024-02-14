using System;
using System.Collections.Generic;

namespace SchoolHelper.Bot
{
    public class Helpers
    {
        public static Dictionary<string, string> ParseArguments(string cmd)
        {
            var argArray = cmd.Split("?");
            var ret = new Dictionary<string, string>();

            if (argArray.Length <= 1) return ret;
            
            foreach (var argValue in argArray[1].Split('&'))
            {
                var cmpIndex = argValue.IndexOf('=');
                var arg = argValue[..cmpIndex];
                var value = argValue[(cmpIndex+1)..];
                ret.Add(arg, value);
            }

            return ret;
        }
        
        public static T ConvertDBValue<T>(object obj)
        {
            if (obj == null || obj == DBNull.Value)
            {
                return default(T); // returns the default value for the type
            }
            else
            {
                return (T)obj;
            }
        }
    }
}