using Microsoft.AspNetCore.Connections;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using TimeAPI.API.Serialization;

namespace TimeAPI.API.Cache
{
    public class RedisCacheService : ICacheService
    {
        private readonly IConnectionMultiplexer _connectionMultiplexer;
        private ISerializer<string> _serializer;
        private IDatabase _cache;
        private int _cacheTimeOutMinutes;

        public RedisCacheService(IConnectionMultiplexer connectionMultiplexer, ISerializer<string> serializer)
        {
            _serializer = serializer;
            //var options = new ConfigurationOptions();
            //options.EndPoints.Add(config.Host, config.Port);
            //options.Password = config.Password;
            _cache = connectionMultiplexer.GetDatabase();
            _cacheTimeOutMinutes = 180;
        }

        public async Task<string> GetCacheValueAsync(string key)
        {
            return await _cache.StringGetAsync(key).ConfigureAwait(true);
        }

        public async Task SetCacheValueAsync(string key, string value)
        {
            await _cache.StringSetAsync(key, value).ConfigureAwait(true);
        }

        public T Get<T>(string key)
        {
            return _serializer.Deserialize<T>(_cache.StringGet(key));
        }

        public bool IsCached(string key)
        {
            return _cache.KeyExists(key);
        }

        public void Set(string key, object value)
        {
            _cache.StringSet(key, _serializer.Serialize(value), new TimeSpan(0, _cacheTimeOutMinutes, 0));
        }

        public void SetCollection<T>(string collectionKey, Expression<Func<T, string>> itemKeyProperty, IEnumerable<T> collection)
        {
            int count = collection.Count();
            HashEntry[] hashEntries = new HashEntry[count];
            T item;
            for (int i = 0; i < count; i++)
            {
                item = collection.ElementAt(i);
                hashEntries[i] = new HashEntry(itemKeyProperty.Compile()(item), _serializer.Serialize(item));
            }
            _cache.HashSet(collectionKey, hashEntries);
            _cache.KeyExpire(collectionKey, new TimeSpan(0, _cacheTimeOutMinutes, 0));
        }

        public void SetCollectionItem(string collectionKey, string itemKey, object item)
        {
            _cache.HashSet(collectionKey, itemKey, _serializer.Serialize(item));
            _cache.KeyExpire(collectionKey, new TimeSpan(0, _cacheTimeOutMinutes, 0));
        }

        public T GetCollectionItem<T>(string collectionKey, string itemKey)
        {
            return _serializer.Deserialize<T>(_cache.HashGet(collectionKey, itemKey));
        }

        public IEnumerable<T> GetCollectionItems<T>(string collectionKey, string[] itemKeys)
        {
            int count = itemKeys.Length;
            RedisValue[] redisvalues = new RedisValue[count];
            for (int i = 0; i < count; i++)
            {
                redisvalues[i] = itemKeys[i];
            }
            redisvalues = _cache.HashGet(collectionKey, redisvalues);
            return redisvalues.Select(itm => _serializer.Deserialize<T>(itm));
        }

        public IEnumerable<T> GetCollectionItems<T>(string collectionKey, IEnumerable<string> itemKeys)
        {
            int count = itemKeys.Count();
            RedisValue[] redisvalues = new RedisValue[count];
            int i = 0;
            foreach (var item in itemKeys)
            {
                redisvalues[i] = (RedisValue)item;
                i++;
            }
            redisvalues = _cache.HashGet(collectionKey, redisvalues);
            return redisvalues.Select(itm => _serializer.Deserialize<T>(itm));
        }

        public IEnumerable<T> GetCollection<T>(string collectionKey)
        {
            return _cache.HashGetAll(collectionKey).Select(itm => _serializer.Deserialize<T>(itm.Value));
        }

        public void SetCollection<T>(string collectionKey, string[] itemKeys, IEnumerable<T> collection)
        {
            int count = itemKeys.Length;
            HashEntry[] hashEntries = new HashEntry[count];
            T item;

            for (int i = 0; i < count; i++)
            {
                item = collection.ElementAt(i);
                hashEntries[i] = new HashEntry(itemKeys[i], _serializer.Serialize(item));
            }

            _cache.HashSet(collectionKey, hashEntries);
            _cache.KeyExpire(collectionKey, new TimeSpan(0, _cacheTimeOutMinutes, 0));
        }

        public void SetCollection(string collectionKey, List<KeyValuePair<string, object>> collection)
        {
            int count = collection.Count;
            HashEntry[] hashEntries = new HashEntry[count];
            KeyValuePair<string, object> item;
            for (int i = 0; i < count; i++)
            {
                item = collection[i];
                hashEntries[i] = new HashEntry(item.Key, _serializer.Serialize(item.Value));
            }
            _cache.HashSet(collectionKey, hashEntries);
            _cache.KeyExpire(collectionKey, new TimeSpan(0, _cacheTimeOutMinutes, 0));
        }

        public void SetCollection<T>(string collectionKey, IEnumerable<string> itemKeys, IEnumerable<T> collection)
        {
            int count = itemKeys.Count();
            HashEntry[] hashEntries = new HashEntry[count];
            T item;

            for (int i = 0; i < count; i++)
            {
                item = collection.ElementAt(i);
                hashEntries[i] = new HashEntry(itemKeys.ElementAt(i), _serializer.Serialize(item));
            }

            _cache.HashSet(collectionKey, hashEntries);
            _cache.KeyExpire(collectionKey, new TimeSpan(0, _cacheTimeOutMinutes, 0));
        }
    }
}
