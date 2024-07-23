using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using Web_App.Http;

namespace Web_App.Session
{
    public static class SessionExtensions
    {
        public static void SetObject(this ISession session, string key, object value)
        {
            session.SetString(key, JsonConvert.SerializeObject(value));
        }

        public static T GetObject<T>(this ISession session, string key)
        {
            string value = session.GetString(key);
            return value == null ? default(T) : JsonConvert.DeserializeObject<T>(value);
        }

        public static void SetBoolean(this ISession session, string key, bool value)
        {
            session.Set(key, BitConverter.GetBytes(value));
        }

        public static bool GetBoolean(this ISession session, string key)
        {
            if (session is MockHttpSession)
            {
                return BitConverter.ToBoolean((byte[])(session as MockHttpSession)[key]);
            }
            byte[] value = session.Get(key);
            if (value != null)
            {
                return BitConverter.ToBoolean(value);
            }
            return default;
        }
    }
}