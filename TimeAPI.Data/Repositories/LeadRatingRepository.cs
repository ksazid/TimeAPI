using System.Collections.Generic;
using System.Data;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.Data.Repositories
{
    public class LeadRatingRepository : RepositoryBase, ILeadRatingRepository
    {
        public LeadRatingRepository(IDbTransaction transaction)
           : base(transaction)
        { }
        public void Add(LeadRating entity)
        {
            entity.id = ExecuteScalar<string>(
                    sql: @"INSERT INTO dbo.lead_rating
                            (id, org_id, rating_name, rating_desc, created_date, createdby)
                    VALUES (@id, @org_id, @rating_name, @rating_desc, @created_date, @createdby);
                    SELECT SCOPE_IDENTITY()",
                    param: entity
                );
        }

        public LeadRating Find(string key)
        {
            return QuerySingleOrDefault<LeadRating>(
                sql: "SELECT * FROM dbo.lead_rating WHERE id = @key and is_deleted = 0",
                param: new { key }
            );
        }

        public IEnumerable<LeadRating> All()
        {
            return Query<LeadRating>(
                sql: "SELECT * FROM dbo.lead_rating where is_deleted = 0"
            );
        }

        public IEnumerable<LeadRating> LeadRatingByOrgID(string key)
        {
            return Query<LeadRating>(
                sql: "SELECT * FROM dbo.lead_rating where is_deleted = 0 and org_id = @key",
                param: new { key }
            );
        }

        public void Remove(string key)
        {
            Execute(
                sql: @"UPDATE dbo.lead_rating
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE id = @key",
                param: new { key }
            );
        }

        public void Update(LeadRating entity)
        {
            Execute(
                sql: @"UPDATE dbo.lead_rating
                           SET 
                            org_id = @org_id, 
                            rating_name = @rating_name, 
                            rating_desc = @rating_desc,
                            modified_date = @modified_date,
                            modifiedby = @modifiedby
                         WHERE id = @id",
                param: entity
            );
        }

    }
}