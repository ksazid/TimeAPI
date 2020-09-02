using Dapper;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace TimeAPI.Data.Repositories
{
    public abstract class RepositoryBase
    {
        private IDbTransaction _transaction;
        private IDbConnection Connection { get { return _transaction.Connection; } }

        public RepositoryBase(IDbTransaction transaction)
        {
            _transaction = transaction;
        }

        protected T ExecuteScalar<T>(string sql, object param)
        {
            return Connection.ExecuteScalar<T>(sql, param, _transaction);
        }

        protected T QuerySingleOrDefault<T>(string sql, object param)
        {
            return Connection.QuerySingleOrDefault<T>(sql, param, _transaction);
        }

        protected IEnumerable<T> Query<T>(string sql, object param = null)
        {
            return Connection.Query<T>(sql, param, _transaction);
        }

        protected void Execute(string sql, object param)
        {
            Connection.Execute(sql, param, _transaction);
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

        protected async 
        Task
ExecuteAsync(string sql, object param)
        {
            await Connection.ExecuteAsync(sql, param, _transaction);
        }
    }
}