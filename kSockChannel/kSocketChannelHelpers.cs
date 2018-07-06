using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace kdrcts.kSockChannel
{
    public static class kSocketChannelHelpers
    {
        public static string ObjectSerializer<T>(T @object) where T : class
        {
            if (@object == null)
                throw new ArgumentNullException();
            return JsonConvert.SerializeObject(@object);
        }

        public static byte[] StringToBytes(this string str)
        {
            return System.Text.UTF8Encoding.UTF8.GetBytes(str);
        }

        public static string BytesToString(this byte[] byteArr)
        {
            return System.Text.UTF8Encoding.UTF8.GetString(byteArr);
        }

        public static T ObjectDeserializer<T>(byte[] bytes) where T : class
        {
            if (bytes == null || bytes.Length == 0)
                throw new InvalidOperationException();
            return JsonConvert.DeserializeObject<T>(bytes.BytesToString());
        }
    }
}
