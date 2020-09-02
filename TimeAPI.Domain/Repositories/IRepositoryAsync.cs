using System.Collections.Generic;
using System.Threading.Tasks;

namespace TimeAPI.Domain.Repositories
{
    public interface IRepositoryAsync<TEntity, TKey> where TEntity : class
    {
        Task<IEnumerable<TEntity>> All();

        Task<TEntity> Find(TKey key);

        void Add(TEntity entity);

        void Update(TEntity entity);

        void Remove(TKey key);
 
    }
}
