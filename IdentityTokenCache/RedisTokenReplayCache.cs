﻿using System;
using System.Configuration;
using System.IdentityModel.Tokens;
using System.Xml;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace CampusLabs.Identity.Tokens.Cache
{
    public class RedisTokenReplayCache : TokenReplayCache
    {
        public event EventHandler<Exception> Error;

        private static readonly Lazy<ConnectionMultiplexer> LazyConnection = new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(_connectionString));

        public static ConnectionMultiplexer Connection => LazyConnection.Value;

        private static string _connectionString;

        public override void LoadCustomConfiguration(XmlNodeList nodelist)
        {
            if (nodelist != null && nodelist.Count > 0 && nodelist[0] != null && nodelist[0].Name.Equals("redisCache", StringComparison.InvariantCultureIgnoreCase))
            {
                if (nodelist[0].Attributes?["connectionString"] != null)
                    _connectionString = nodelist[0].Attributes["connectionString"].Value;
            }

            if (string.IsNullOrWhiteSpace(_connectionString))
                throw new ConfigurationErrorsException("Missing connectionString attribute on <redisCache>");

        }

        public override void AddOrUpdate(string key, SecurityToken securityToken, DateTime expirationTime)
        {
            try
            {
                var timeSpan = expirationTime - DateTime.Now;

                var cache = Connection.GetDatabase();

                cache.StringSet(key, JsonConvert.SerializeObject(securityToken), timeSpan, When.Always, CommandFlags.FireAndForget);
            }
            catch (Exception e)
            {
                Error?.Invoke(this, e);
            }
        }

        public override bool Contains(string key)
        {
            try
            {
                var cache = Connection.GetDatabase();

                return cache.KeyExists(key);
            }
            catch (Exception e)
            {
                Error?.Invoke(this, e);

                return false;
            }
        }

        public override SecurityToken Get(string key)
        {
            try
            {
                var cache = Connection.GetDatabase();

                return JsonConvert.DeserializeObject<SecurityToken>(cache.StringGet(key));
            }
            catch (Exception e)
            {
                Error?.Invoke(this, e);

                return null;
            }
        }

        public override void Remove(string key)
        {
            try
            {
                var cache = Connection.GetDatabase();

                cache.KeyDelete(key);
            }
            catch (Exception e)
            {
                Error?.Invoke(this, e);
            }
        }
    }
}
