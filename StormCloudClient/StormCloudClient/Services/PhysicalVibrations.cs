using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StormCloudClient.Services
{
    public class PhysicalVibrations
    {
        public static void TryVibrate(int ms)
        {
            try
            {
                Vibration.Default.Vibrate(ms / 1000.0);
            }
            catch(Exception e)
            {

            }
            
        }
        public static void TryHaptic(HapticFeedbackType type)
        {
            try
            {
                HapticFeedback.Default.Perform(type);
            }
            catch (Exception e)
            {

            }

        }
    }
}
