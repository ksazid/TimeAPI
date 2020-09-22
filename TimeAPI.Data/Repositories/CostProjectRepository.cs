using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.Data.Repositories
{
    public class CostProjectRepository : RepositoryBase, ICostProjectRepository
    {
        public CostProjectRepository(IDbTransaction transaction) : base(transaction)
        { }

        public void Add(CostProject entity)
        {
            entity.id = ExecuteScalar<string>(
                    sql: @"INSERT INTO dbo.cost_project
                                  (id, user_id, org_id, package_id, project_type_id, project_name, project_desc, project_prefix, start_date, end_date, completed_date, project_status_id, is_private, is_public, no_of_floors, plot_size, plot_size_unit, buildup_area, buildup_area_unit, discount_amount, profit_margin_amount, created_date, createdby)
                           VALUES (@id, @user_id, @org_id, @package_id, @project_type_id, @project_name, @project_desc, @project_prefix, @start_date, @end_date, @completed_date, @project_status_id, @is_private, @is_public, @no_of_floors, @plot_size, @plot_size_unit, @buildup_area, @buildup_area_unit, @discount_amount, @profit_margin_amount, @created_date, @createdby);
                    SELECT SCOPE_IDENTITY()",
                    param: entity
                );
        }

        public async Task<CostProject> Find(string key)
        {
            return await QuerySingleOrDefaultAsync<CostProject>(
                sql: "SELECT * FROM dbo.cost_project WHERE is_deleted = 0 and id = @key",
                param: new { key }
            );
        }

        public async Task<dynamic> FindByCostProjectID(string key)
        {
            return await QuerySingleOrDefaultAsync<dynamic>(
                sql: @"SELECT 
                           CASE
			                    WHEN dbo.customer.is_company = 1 THEN dbo.customer.company_name
			                    ELSE dbo.customer.first_name + ' ' + dbo.customer.last_name
			                    END AS customer_name, 
                                dbo.customer.id as cst_id,
			                    packages.package_name,
			                    project_type.type_name as project_type_name,
			                    cost_project.* 
                    FROM dbo.cost_project
                        LEFT JOIN customer_x_project on cost_project.id = customer_x_project.project_id
                        LEFT JOIN customer on customer_x_project.cst_id = customer.id
                        LEFT JOIN packages on cost_project.package_id = packages.id
                        LEFT JOIN project_type on cost_project.project_type_id = project_type.id
                    WHERE cost_project.is_deleted = 0 and cost_project.id = @key",
                param: new { key }
            );
        }

        public async Task<CostProject> FindAutoCostProjectPrefixByOrgID(string key, string date)
        {
            return await QuerySingleOrDefaultAsync<CostProject>(
                sql: @"SELECT TOP 1 project_prefix 
                            FROM dbo.cost_project 
                        WHERE   
                            project_prefix like  '%JOB' 
                            AND org_id = @key 
                            AND FORMAT(CAST(created_date AS DATE), 'd', 'EN-US') = FORMAT(CAST(@date AS DATE), 'd', 'EN-US')
                            ORDER BY created_date DESC",
                param: new { key, date }
            );
        }

        public async Task<CostProject> FindCustomCostProjectPrefixByOrgIDAndPrefix(string key, string project_prefix)
        {
            return await QuerySingleOrDefaultAsync<CostProject>(
                sql: @"SELECT TOP 1 project_prefix    
                            FROM dbo.cost_project  WHERE 
                            project_prefix = @project_prefix
                            AND org_id =@key
                            ORDER BY created_date DESC",
                param: new { key, project_prefix }
            );
        }

        public void Remove(string key)
        {
            Execute(
                sql: @"UPDATE dbo.cost_project
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE id = @key",
                param: new { key }
            );
        }

        public void Update(CostProject entity)
        {
            Execute(
                sql: @"UPDATE dbo.cost_project
                   SET
                    user_id = @user_id,
                    package_id = @package_id,
                    project_type_id = @project_type_id,
                    org_id = @org_id,
                    project_name = @project_name,
                    project_desc = @project_desc,
                    project_prefix = @project_prefix,
                    start_date = @start_date,
                    end_date = @end_date,
                    completed_date = @completed_date,
                    project_status_id = @project_status_id,
                    is_private = @is_private,
                    is_public = @is_public,
                    no_of_floors = @no_of_floors, 
                    plot_size = @plot_size, 
                    plot_size_unit = @plot_size_unit, 
                    buildup_area = @buildup_area,
                    buildup_area_unit = @buildup_area_unit,
                    discount_amount = @discount_amount,
                    profit_margin_amount = @profit_margin_amount,
                    modified_date = @modified_date,
                    modifiedby = @modifiedby
                    WHERE id = @id",
                param: entity
            );
        }

        public async Task<IEnumerable<CostProject>> All()
        {
            return await QueryAsync<CostProject>(
                sql: "SELECT * FROM [dbo].[cost_project] where is_deleted = 0"
            );
        }

        public async Task<IEnumerable<dynamic>> FetchAllCostProjectByOrgID(string key)
        {
            //AND entity_contact.is_primary = 1
            return await QueryAsync<dynamic>(
                   sql: @"SELECT
	                            project_type.id as project_type_id,
                                project_type.type_name as project_type_name,
                                entity_contact.name,
		                        entity_contact.phone,
		                        entity_contact.email,
                                cost_project.id as project_id,
                                cost_project.project_name,
	                            CASE
                                WHEN FORMAT(CAST( dbo.cost_project.modified_date AS DATETIME2), N'dd/MM/yyyy') IS NULL
	                            THEN FORMAT(CAST( dbo.cost_project.created_date AS DATETIME2), N'dd/MM/yyyy')
                                ELSE FORMAT(CAST( dbo.cost_project.created_date AS DATETIME2), N'dd/MM/yyyy') 
                                END AS ondate,
	                            dbo.customer.cst_name,
                                cost_project.project_prefix,
                                cost_project.no_of_floors, 
                                cost_project.plot_size, 
                                cost_project.buildup_area, 
                                cost_project.discount_amount, 
                                cost_project.profit_margin_amount, 
                                cost_project.net_total_amount, 
                                e_tl.full_name as project_owner,
                                e_tl.workemail,
                                project_status.project_status_name ,
	                            cost_project.start_date,
                                cost_project.end_date,
                                cost_project.completed_date
                            FROM dbo.cost_project WITH (NOLOCK)
                            LEFT JOIN dbo.employee e_tl ON dbo.cost_project.user_id = e_tl.id
                            LEFT JOIN dbo.project_status ON dbo.cost_project.project_status_id = dbo.project_status.id
                            LEFT JOIN dbo.project_type ON dbo.cost_project.project_type_id = dbo.project_type.id
                            LEFT JOIN dbo.customer_x_project ON dbo.cost_project.id = dbo.customer_x_project.project_id
	                        LEFT JOIN dbo.customer ON dbo.customer_x_project.cst_id = dbo.customer.id
                            LEFT JOIN dbo.entity_contact on dbo.cost_project.id = dbo.entity_contact.entity_id
                            WHERE dbo.cost_project.org_id = @key
	                        AND entity_contact.is_primary = 1
                            AND dbo.cost_project.is_deleted = 0 
	                        AND cost_project.is_quotation != 1
	                        AND entity_contact.is_deleted  = 0
                            ORDER BY FORMAT(CAST(dbo.cost_project.created_date AS DATETIME2), N'MM/dd/yyyy hh:mm tt') DESC",
                      param: new { key }
               );
        }

        public async Task UpdateCostProjectStatusByID(CostProject entity)
        {
           await ExecuteAsync(
               sql: @"UPDATE dbo.cost_project
                   SET
                    project_status_id = @project_status_id,
                    modified_date = @modified_date,
                    modifiedby = @modifiedby
                    WHERE id = @id",
               param: entity
           );
        }

        public async Task UpdateCostProjectDiscountAndProfitMarginByID(CostProject entity)
        {
            await ExecuteAsync(
                sql: @"UPDATE dbo.cost_project
                   SET
                    discount_amount = @discount_amount,
                    profit_margin_amount = @profit_margin_amount,
                    modified_date = @modified_date,
                    modifiedby = @modifiedby
                    WHERE id = @id",
                param: entity
            );
        }

        public async Task UpdateIsQuotationByCostProjectID(CostProject entity)
        {
            await ExecuteAsync(
              sql: @"UPDATE dbo.cost_project
                   SET
                    is_quotation = @is_quotation,
                    modified_date = @modified_date,
                    modifiedby = @modifiedby
                    WHERE id = @id",
              param: entity
          );
        }

        public async Task UpdateCostProjectFinalValueByCostProjectID(CostProject entity)
        {
            await ExecuteAsync(
              sql: @"UPDATE dbo.cost_project
                   SET
                    total_hours = @total_hours,
                    gross_total_amount= @gross_total_amount,
                    profit_margin_amount= @profit_margin_amount,
                    discount_amount= @discount_amount,
                    total_amount= @total_amount,
                    vat_amount= @vat_amount,
                    net_total_amount= @net_total_amount,
                    modifiedby = @modifiedby
                    WHERE id = @id",
              param: entity
          );
        }

        public async Task<string> GetLastAddedCostPrefixByOrgID(string key)
        {
            return await QuerySingleOrDefaultAsync<string>(
                sql: @"SELECT TOP 1 project_prefix from cost_project 
                        WHERE cost_project.org_id = @key 
                        AND cost_project.is_deleted = 0
                        ORDER BY FORMAT(CAST( dbo.cost_project.created_date AS DATETIME2), N'dd/MM/yyyy hh:mm tt') DESC",
                param: new { key }
            );
        }
       
    }
}