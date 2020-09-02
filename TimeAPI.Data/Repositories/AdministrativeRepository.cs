using System.Collections.Generic;
using System.Data;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.Data.Repositories
{
    public class AdministrativeRepository : RepositoryBase, IAdministrativeRepository
    {
        public AdministrativeRepository(IDbTransaction transaction) : base(transaction)
        {
        }

        public void Add(Administrative entity)
        {
          entity.id = ExecuteScalar<string>(
                    sql: @"INSERT INTO [dbo].[administrative]
                           (id, dept_id, org_id, administrative_name, summary, created_date, createdby)
                    VALUES (@id, @dept_id, @org_id, @administrative_name,  @summary, @created_date, @createdby);
                    SELECT SCOPE_IDENTITY()",
                    param: entity
                );
        }

        public IEnumerable<Administrative> All()
        {
            return Query<Administrative>(
                sql: "SELECT * FROM [dbo].[administrative] WHERE  is_deleted = 0"
            );
        }

        public Administrative Find(string key)
        {
            return QuerySingleOrDefault<Administrative>(
                sql: "SELECT * FROM [dbo].[administrative] WHERE id = @key and  is_deleted = 0",
                param: new { key }
            );
        }

        public RootObject GetByOrgID(string key)
        {
            RootObject rootObject = new RootObject();
            List<RootDepartmentObject> rootDepartmentObjectsList = new List<RootDepartmentObject>();

            var dep = Query<Department>(
                   sql: @"SELECT DISTINCT(department.id),
                        department.dep_name
                            FROM [dbo].[department] WHERE is_deleted = 0
                            AND org_id =@key",
                    param: new { key }
               );

            foreach (var item in dep)
            {
                RootDepartmentObject rootDepartmentObject = new RootDepartmentObject();
                List<AdministrativeDropDown> administrativesList = new List<AdministrativeDropDown>();
                rootDepartmentObject.id = item.id;
                rootDepartmentObject.dept_name = item.dep_name;
                var result = GetDepartmentObject(item.id) as List<AdministrativeDropDown>;

                if (result.Count > 0)
                    administrativesList.AddRange(result);

                rootDepartmentObject.administratives = administrativesList;
                rootDepartmentObjectsList.Add(rootDepartmentObject);
                rootObject.rootDepartmentObjects = (rootDepartmentObjectsList);
            }

            return rootObject;
        }

        public void Remove(string key)
        {
            Execute(
                sql: @"UPDATE dbo.administrative
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE id = @key",
                param: new { key }
            );
        }

        public void Update(Administrative entity)
        {
            Execute(
                sql: @"UPDATE dbo.administrative
                   SET
                       dept_id = @dept_id,
                       administrative_name = @administrative_name,
                       org_id  = @org_id,
                       summary = @summary,
                       modified_date = @modified_date,
                       modifiedby = @modifiedby
                    WHERE id = @id",
                param: entity
            );
        }

        private IEnumerable<AdministrativeDropDown> GetDepartmentObject(string key)
        {
            return Query<AdministrativeDropDown>(
              sql: @"SELECT
                    ROW_NUMBER() OVER (ORDER BY id) AS rowno,
                    id,
                    administrative_name as text
            FROM[dbo].[administrative]
                WHERE[dbo].[administrative].is_deleted = 0
                AND[dbo].[administrative].dept_id = @key
                ORDER BY[dbo].[administrative].administrative_name ASC",
              param: new { key }
          );
        }

        public dynamic GetAdministrativeTaskByOrgID(string key)
        {
            return Query<dynamic>(
                sql: @"SELECT dbo.department.id as department_id, 
                        dbo.department.dep_name, 
                        dbo.administrative.id,
                        dbo.administrative.administrative_name,
                        dbo.administrative.summary
                    FROM dbo.administrative 
                        INNER JOIN  dbo.department ON dbo.administrative.dept_id =  dbo.department.id
                    WHERE  dbo.administrative.is_deleted = 0 
                        AND  dbo.department.is_deleted = 0 
                        AND  dbo.administrative.org_id =  @key",
                param: new { key }
            );
        }
    }
}