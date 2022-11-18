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
            Preferences.Default.Set(key, value);
        }
        public static void SetValue(string key, int value)
        {
            Preferences.Default.Set(key, value);
        }
        public static void SetValue(string key, bool value)
        {
            Preferences.Default.Set(key, value);
        }

        public static object GetValue(string key)
        {
            try
            {
                if (Preferences.Default.ContainsKey(key))
                    return Preferences.Default.Get(key, "");
                
            }catch(Exception ex) { }

            return null;

        }

        public static bool DoesKeyExist(string key)
        {
            return Preferences.Default.ContainsKey(key);
        }
    }

}
