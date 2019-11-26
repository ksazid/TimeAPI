using System;
using System.Collections.Generic;
using System.Text;
using TimeAPI.Domain.Entities;

namespace TimeAPI.Domain.Repositories
{
    public interface IReportingRepository : IRepository<Reporting, string>
    {
        Reporting FindReportingHeadByEmpID(string empid);
        Reporting FindByReportEmpID(string report_emp_id);
    }
}
