using System.Collections.Generic;
using System.Data;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Repositories;


namespace TimeAPI.Data.Repositories
{
    public class ReportingRepository : RepositoryBase, IReportingRepository
    {
        public ReportingRepository(IDbTransaction transaction) : base(transaction)
        {

        }

        public void Add(Reporting entity)
        {
            entity.id = ExecuteScalar<string>(
                    sql: @"INSERT INTO dbo.reporting
                                  (id, empid, report_emp_id, created_date, createdby)
                           VALUES (@id, @empid, @report_emp_id, @created_date, @createdby);
                    SELECT SCOPE_IDENTITY()",
                    param: entity
                );
        }

        public Reporting Find(string key)
        {
            return QuerySingleOrDefault<Reporting>(
                sql: "SELECT * FROM dbo.reporting WHERE id = @key and  is_deleted = 0",
                param: new { key }
            );
        }

        public void Remove(string key)
        {
            Execute(
                sql: @"UPDATE dbo.reporting
                   SET
                       modified_date = @modified_date, modifiedby = @modifiedby, is_deleted = 1
                    WHERE id = @key",
                param: new { key }
            );
        }

        public void Update(Reporting entity)
        {
            Execute(
                sql: @"UPDATE dbo.image
                   SET 
                        id = @id, 
                        user_id = @user_id, 
                        img_name = @img_name, 
                        img_url = @img_url, 
                        modified_date = @modified_date, 
                        modifiedby = @modifiedby
                    WHERE id = @id",
                param: entity
            );
        }

        public IEnumerable<Reporting> All()
        {
            return Query<Reporting>(
                sql: "SELECT * FROM [dbo].[reporting] WHERE  is_deleted = 0"
            );
        }

        public Reporting FindReportingHeadByEmpID(string key)
        {
            return QuerySingleOrDefault<Reporting>(
                sql: "SELECT * FROM dbo.reporting WHERE report_emp_id = @key and  is_deleted = 0",
                param: new { key }
            );
        }

        public Reporting FindByReportEmpID(string key)
        {
            return QuerySingleOrDefault<Reporting>(
                sql: "SELECT * FROM dbo.reporting WHERE empid = @key and  is_deleted = 0",
                param: new { key }
            );
        }
    }
}
