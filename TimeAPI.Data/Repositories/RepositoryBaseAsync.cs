using Dapper;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace TimeAPI.Data.Repositories
{
    public abstract class RepositoryBaseAsync
    {
        private IDbTransaction _transaction;
        private IDbConnection Connection { get { return _transaction.Connection; } }

        public RepositoryBaseAsync(IDbTransaction transaction)
        {
            _transaction = transaction;
        }
        //async methods
        protected async Task<T> ExecuteScalarAsync<T>(string sql, object param)
        {
            return await Connection.ExecuteScalarAsync<T>(sql, param, _transaction);
        }

        protected async Task<T> QuerySingleOrDefaultAsync<T>(string sql, object param)
        {
            return await Connection.QuerySingleOrDefaultAsync<T>(sql, param, _transaction);
        }

        protected async Task<IEnumerable<T>> QueryAsync<T>(string sql, object param = null)
        {
            return await Connection.QueryAsync<T>(sql, param, _transaction);
        }

        protected async Task ExecuteAsync(string sql, object param)
        {
            await Connection.ExecuteAsync(sql, param, _transaction);
        }
    }
}