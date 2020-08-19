using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace TimeAPI.API.Cache
{
    public interface ICacheService
    {
        Task<string> GetCacheValueAsync(string key);
        Task SetCacheValueAsync(string key, string value);

        bool IsCached(string key);

        void Set(string key, object value);

        T Get<T>(string key);

        T GetCollectionItem<T>(string collectionKey, string itemKey);

        IEnumerable<T> GetCollectionItems<T>(string collectionKey, string[] itemKeys);

        IEnumerable<T> GetCollectionItems<T>(string collectionKey, IEnumerable<string> itemKeys);

        IEnumerable<T> GetCollection<T>(string collectionKey);

        void SetCollection<T>(string collectionKey, Expression<Func<T, string>> itemKeyProperty, IEnumerable<T> collection);

        void SetCollection<T>(string collectionKey, string[] itemKeys, IEnumerable<T> collection);

        void SetCollection<T>(string collectionKey, IEnumerable<string> itemKeys, IEnumerable<T> collection);

        void SetCollection(string collectionKey, List<KeyValuePair<string, object>> collection);

        void SetCollectionItem(string collectionKey, string itemKey, object item);
    }
}
