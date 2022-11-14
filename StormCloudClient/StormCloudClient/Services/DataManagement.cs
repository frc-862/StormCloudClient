using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StormCloudClient.Services
{
    public class DataManagement
    {
        public static void SetValue(string key, string value)
        {
            Preferences.Set(key, value);
        }
        public static void SetValue(string key, int value)
        {
            Preferences.Set(key, value);
        }
        public static void SetValue(string key, bool value)
        {
            Preferences.Set(key, value);
        }

        public static object GetValue(string key)
        {
            if (Preferences.ContainsKey(key))
                return Preferences.Get(key, 0);
            return null;
        }

        public static bool DoesKeyExist(string key)
        {
            return Preferences.ContainsKey(key);
        }
    }

}
