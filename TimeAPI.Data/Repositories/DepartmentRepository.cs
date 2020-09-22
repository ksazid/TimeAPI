using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.Data.Repositories
{
    public class DepartmentRepository : RepositoryBase, IDepartmentRepository
    {
        public DepartmentRepository(IDbTransaction transaction) : base(transaction)
        { }

        public void Add(Department entity)
        {
            entity.id = ExecuteScalar<string>(
                    sql: @"INSERT INTO dbo.department
                                  (id, org_id, depart_lead_empid, dep_name, alias, created_date, createdby)
                           VALUES (@id, @org_id, @depart_lead_empid, @dep_name, @alias, @created_date, @createdby);
                    SELECT SCOPE_IDENTITY()",
                    param: entity
                );
        }

        public async Task<Department> Find(string key)
        {
            return await QuerySingleOrDefaultAsync<Department>(
                sql: "SELECT * FROM dbo.department WHERE is_deleted = 0 and id = @key ORDER BY department.dep_name ASC",
                param: new { key }
            );
        }

        public void Remove(string key)
        {
            //DELETE FROM AspNetRoles WHERE Id
            Execute(
                sql: @"UPDATE dbo.department
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE id = @key",
                param: new { key }
            );
        }

        public void Update(Department entity)
        {
            Execute(
                sql: @"UPDATE dbo.department
                   SET
                    id = @id,
                    org_id = @org_id,
                    depart_lead_empid = @depart_lead_empid,
                    dep_name = @dep_name,
                    alias = @alias,
                    modified_date = @modified_date,
                    modifiedby = @modifiedby
                    WHERE id = @id",
                param: entity
            );
        }

        public async Task<IEnumerable<Department>> All()
        {
            return await QueryAsync<Department>(
                sql: "SELECT * FROM [dbo].[department] WITH (NOLOCK) where is_deleted = 0  ORDER BY department.dep_name ASC"
            );
        }

        public async Task<Department> FindByDepartmentName(string dep_name)
        {
            return await QuerySingleOrDefaultAsync<Department>(
                sql: @"SELECT * FROM [dbo].[department] WITH (NOLOCK)
                        WHERE dep_name = @dep_name and is_deleted = 0
                        ORDER BY department.dep_name ASC",
                param: new { dep_name }
            );
        }

        public async Task<Department> FindByDepartmentAlias(string alias)
        {
            return await QuerySingleOrDefaultAsync<Department>(
                sql: @"SELECT * FROM [dbo].[department] WITH (NOLOCK)
                        WHERE alias = @alias and is_deleted = 0
                        ORDER BY department.dep_name ASC",
                param: new { alias }
            );
        }

        public async Task<IEnumerable<Department>> FindDepartmentByOrgID(string org_id)
        {
            return await QueryAsync<Department>(
                sql: @"SELECT * FROM [dbo].[department] WITH (NOLOCK)
                       WHERE org_id = @org_id and is_deleted = 0
                       ORDER BY department.dep_name ASC",
                param: new { org_id }
            );
        }

        public async Task<dynamic> FindAllDepLeadByOrgID(string org_id)
        {
            return await QueryAsync<dynamic>(
                sql: @"SELECT
	                    employee.full_name,
	                    employee.workemail,
	                    designation.designation_name,
	                    department.dep_name
                    FROM department WITH (NOLOCK)
                    LEFT JOIN employee ON department.depart_lead_empid = employee.id
                    LEFT JOIN designation ON department.id = designation.dep_id
                    WHERE department.org_id= @org_id and department.is_deleted = 0
                    ORDER BY employee.full_name ASC",
                param: new { org_id }
            );
        }

        public async Task<dynamic> FindDepLeadByDepID(string DepID)
        {
            return await QuerySingleOrDefaultAsync<dynamic>(
                sql: @"SELECT
                            designation.id as department_id,
                            department.dep_name,
	                        employee.id as employee_id,
	                        employee.full_name,
	                        designation.designation_name,
                            department.dep_name
                        FROM department WITH (NOLOCK)
                        LEFT JOIN employee ON department.depart_lead_empid = employee.id
                        LEFT JOIN designation ON department.id = designation.dep_id
                        WHERE department.id= @DepID and department.is_deleted = 0
                        ORDER BY employee.full_name ASC",
                param: new { DepID }
            );
        }

        public async Task<dynamic> FetchGridDataByDepOrgID(string key)
        {
            return await QueryAsync<dynamic>(
                   sql: @"SELECT
                                ROW_NUMBER() OVER (ORDER BY department.id) AS rowno,
                                department.id as department_id,
                                department.dep_name,
	                            employee.id as employee_id,
                                employee.full_name as lead_name,
                                employee.workemail,
                                department.alias
                            FROM department WITH (NOLOCK)
                            LEFT JOIN employee on department.depart_lead_empid = employee.id
                            WHERE department.org_id =@key AND department.is_deleted = 0
                            ORDER BY department.dep_name ASC",
                      param: new { key }
               );
        }

        public async Task<dynamic> FindAllDepMembersByDepID(string DepID)
        {
            return await QueryAsync<dynamic>(
                sql: @"SELECT
                            department.id as department_id,
                            department.dep_name,
	                        employee.id as employee_id,
	                        employee.full_name,
	                        designation.designation_name,
                            department.dep_name
                        FROM department WITH (NOLOCK)
                        LEFT JOIN employee ON department.depart_lead_empid = employee.id
                        LEFT JOIN designation ON department.id = designation.dep_id
                        WHERE department.id= @DepID and department.is_deleted = 0
                        ORDER BY employee.full_name ASC",
                param: new { DepID }
            );
        }

    }
}