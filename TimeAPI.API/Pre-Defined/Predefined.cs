using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TimeAPI.API.Pre_Defined
{
    public static class PreDefined
    {
        public static Hashtable GetDepartment()
        {
            Hashtable htDesignation = new Hashtable();
            htDesignation.Add("1", "Accounts");
            htDesignation.Add("2", "Administrative");
            htDesignation.Add("3", "Advertisement & Marketing");
            htDesignation.Add("4", "Construction");
            htDesignation.Add("5", "Customer Service");
            htDesignation.Add("6", "Design");
            htDesignation.Add("7", "Engineering");
            htDesignation.Add("8", "Facilities");
            htDesignation.Add("9", "Finance");
            htDesignation.Add("10", "Human Resources");
            htDesignation.Add("11", "IT & Development");
            htDesignation.Add("12", "Legal");
            htDesignation.Add("13", "Logistics");
            htDesignation.Add("14", "Operation & Production");
            htDesignation.Add("15", "Real Estate");
            htDesignation.Add("16", "Sales");

            return htDesignation;
        }

        public static List<string> GetAccountsDesignation()
        {
            List<string> htDesignation = new List<string>();
            htDesignation.Add("Accountant");
            htDesignation.Add("Accounting Assistant");
            htDesignation.Add("Accounting Clerk");
            htDesignation.Add("Accounting Manager");
            htDesignation.Add("Accounting Supervisor");
            htDesignation.Add("Accounts Administrator");
            htDesignation.Add("Accounts Payable Clerk");
            htDesignation.Add("Accounts Receivable Clerk");
            htDesignation.Add("Accounts Receivable Manager");
            htDesignation.Add("Billing Analyst");
            htDesignation.Add("Billing Clerk");
            htDesignation.Add("Billing Coordinator");
            htDesignation.Add("Billing Specialist");
            htDesignation.Add("Bookkeeper");
            htDesignation.Add("Budget Analyst");
            htDesignation.Add("Budget Manager");
            htDesignation.Add("Certified Public Accountant (CPA)");
            htDesignation.Add("Cost Accountant");
            htDesignation.Add("Cost Analyst");
            htDesignation.Add("Junior Accountant");
            htDesignation.Add("Payroll Accountant");
            htDesignation.Add("Senior Accountant");
            htDesignation.Add("Senior Auditor");
            htDesignation.Add("Staff Accountant");
            htDesignation.Add("Tax Accountant");
            htDesignation.Add("Tax Manager");
            htDesignation.Add("Tax Preparer");

            return htDesignation;
        }

        public static List<string> GetAdministrativeDesignation()
        {
            List<string> htDesignation = new List<string>();
            htDesignation.Add("Administration Manager");
            htDesignation.Add("Administrative Assistant");
            htDesignation.Add("Administrative Coordinator");
            htDesignation.Add("Administrative Officer");
            htDesignation.Add("Administrator");
            htDesignation.Add("Assistant Director");
            htDesignation.Add("Assistant Manager");
            htDesignation.Add("Branch Manager");
            htDesignation.Add("Business Consultant");
            htDesignation.Add("Business Manager");
            htDesignation.Add("CEO");
            htDesignation.Add("Chief Administrative Officer");
            htDesignation.Add("Consultant");
            htDesignation.Add("Contract Administrator");
            htDesignation.Add("COO");
            htDesignation.Add("Data Entry Clerk");
            htDesignation.Add("Data Entry Operator");
            htDesignation.Add("Director of Operations");
            htDesignation.Add("District Manager");
            htDesignation.Add("Document Controller");
            htDesignation.Add("Executive Administrative Assistant");
            htDesignation.Add("Executive Assistant");
            htDesignation.Add("Executive Director");
            htDesignation.Add("Executive Secretary");
            htDesignation.Add("File Clerk");
            htDesignation.Add("Front Office Manager");
            htDesignation.Add("General Manager");
            htDesignation.Add("Head of Operations");
            htDesignation.Add("Mail Clerk");
            htDesignation.Add("Management Trainee");
            htDesignation.Add("Managing Director");
            htDesignation.Add("Office Administrator");
            htDesignation.Add("Office Assistant");
            htDesignation.Add("Office Clerk");
            htDesignation.Add("Office Coordinator");
            htDesignation.Add("Office Manager");
            htDesignation.Add("Operations Manager");
            htDesignation.Add("Operations Supervisor");
            htDesignation.Add("Personal Assistant");
            htDesignation.Add("Program Administrator");
            htDesignation.Add("Program Coordinator");
            htDesignation.Add("Program Director");
            htDesignation.Add("Program Manager");
            htDesignation.Add("Project Administrator");
            htDesignation.Add("Project Coordinator");
            htDesignation.Add("Secretary");
            htDesignation.Add("Senior Administrative Assistant");
            htDesignation.Add("Senior Executive Assistant");
            htDesignation.Add("Senior Vice President");
            htDesignation.Add("Shift Leader");
            htDesignation.Add("Shift supervisor");
            htDesignation.Add("Staff Assistant");
            htDesignation.Add("Strategic Planner");
            htDesignation.Add("Supervisor");
            htDesignation.Add("Team Leader");

            return htDesignation;
        }

        public static List<string> GetAdvertisementDesignation()
        {
            List<string> htDesignation = new List<string>();
            htDesignation.Add("Advertising Account Executive");
            htDesignation.Add("Art Director");
            htDesignation.Add("Assistant Brand Manager");
            htDesignation.Add("Associate Brand Manager");
            htDesignation.Add("Associate Product Manager");
            htDesignation.Add("Brand Ambassador");
            htDesignation.Add("Brand Manager");
            htDesignation.Add("Brand Strategist");
            htDesignation.Add("Chief Marketing Officer");
            htDesignation.Add("Communications Assistant");
            htDesignation.Add("Community Manager");
            htDesignation.Add("Content Creator");
            htDesignation.Add("Content Editor");
            htDesignation.Add("Content Manager");
            htDesignation.Add("Content Marketing Manager");
            htDesignation.Add("Content Strategist");
            htDesignation.Add("Creative Assistant");
            htDesignation.Add("Creative Director");
            htDesignation.Add("CRM Director");
            htDesignation.Add("Digital Account Manager");
            htDesignation.Add("Digital Director");
            htDesignation.Add("Digital Marketing Director");
            htDesignation.Add("Digital Marketing Executive");
            htDesignation.Add("Digital Marketing Manager");
            htDesignation.Add("Digital Marketing Strategist");
            htDesignation.Add("Digital Media Specialist");
            htDesignation.Add("Digital Project Manager");
            htDesignation.Add("Email Marketing Manager");
            htDesignation.Add("Head of Marketing");
            htDesignation.Add("Junior Copywriter");
            htDesignation.Add("Market Research Analyst");
            htDesignation.Add("Marketing Analyst");
            htDesignation.Add("Marketing Assistant");
            htDesignation.Add("Marketing Associate");
            htDesignation.Add("Marketing Communications Specialist");
            htDesignation.Add("Marketing Consultant");
            htDesignation.Add("Marketing Coordinator");
            htDesignation.Add("Marketing Director");
            htDesignation.Add("Marketing Executive");
            htDesignation.Add("Marketing Intern");
            htDesignation.Add("Marketing Manager");
            htDesignation.Add("Marketing Officer");
            htDesignation.Add("Marketing Specialist");
            htDesignation.Add("Marketing Strategist");
            htDesignation.Add("Media Assistant");
            htDesignation.Add("Media Buyer");
            htDesignation.Add("Media Director");
            htDesignation.Add("Media Planner");
            htDesignation.Add("PPC (Pay Per Click) Manager");
            htDesignation.Add("Product Copywriter");
            htDesignation.Add("Product Marketing Manager");
            htDesignation.Add("Promoter");
            htDesignation.Add("Search Engine Marketing (SEM) Specialist");
            htDesignation.Add("Search Engine Optimization (SEO) Specialist");
            htDesignation.Add("Senior Copywriter");
            htDesignation.Add("SEO Analyst");
            htDesignation.Add("SEO Manager");
            htDesignation.Add("Social Media Analyst");
            htDesignation.Add("Social Media Coordinator");
            htDesignation.Add("Social Media Copywriter");
            htDesignation.Add("Social Media Manager");
            htDesignation.Add("Social Media Specialist");
            htDesignation.Add("Social Media Strategist");
            htDesignation.Add("Vice President (VP) of Marketing");

            return htDesignation;
        }

        public static List<string> GetConstructionDesignation()
        {
            List<string> htDesignation = new List<string>();
            htDesignation.Add("Architect");
            htDesignation.Add("Carpenter");
            htDesignation.Add("Construction Estimator");
            htDesignation.Add("Construction Expeditor");
            htDesignation.Add("Construction Foreman");
            htDesignation.Add("Construction Manager");
            htDesignation.Add("Construction Project Manager");
            htDesignation.Add("Construction Superintendent");
            htDesignation.Add("Construction Worker");
            htDesignation.Add("Crane Operator");
            htDesignation.Add("Electrician");
            htDesignation.Add("Electronic Technician");
            htDesignation.Add("Estimator");
            htDesignation.Add("General Laborer");
            htDesignation.Add("Geologist");
            htDesignation.Add("Hydrologist");
            htDesignation.Add("Millwright");
            htDesignation.Add("Painter");
            htDesignation.Add("Pipefitter");
            htDesignation.Add("Plumber");
            htDesignation.Add("Quality Engineer");
            htDesignation.Add("Roofer");
            htDesignation.Add("Test Engineer");
            htDesignation.Add("Welder");

            return htDesignation;
        }

        public static List<string> GetCustomerServiceDesignation()
        {
            List<string> htDesignation = new List<string>();
            htDesignation.Add("Bank Teller");
            htDesignation.Add("Call Center Manager");
            htDesignation.Add("Call Center Representative");
            htDesignation.Add("Call Center Supervisor");
            htDesignation.Add("Customer Service Manager");
            htDesignation.Add("Customer Service Representative");
            htDesignation.Add("Customer Support Specialist");
            htDesignation.Add("Debt Collector");
            htDesignation.Add("Desktop Support Engineer");
            htDesignation.Add("Dispatcher");
            htDesignation.Add("Duty Manager");
            htDesignation.Add("Field Service Representative");
            htDesignation.Add("Field Service Technician");
            htDesignation.Add("Front Desk Representative");
            htDesignation.Add("Help Desk Manager");
            htDesignation.Add("Help Desk Specialist");
            htDesignation.Add("IT Help Desk Technician");
            htDesignation.Add("Receptionist");
            htDesignation.Add("Technical Account Manager");
            htDesignation.Add("Technical Support Engineer");

            return htDesignation;
        }

        public static List<string> GetDesignDesignation()
        {
            List<string> htDesignation = new List<string>();
            htDesignation.Add("Graphic Designer");
            htDesignation.Add("Illustrator");
            htDesignation.Add("Interior Designer");
            htDesignation.Add("Junior Designer");
            htDesignation.Add("Physical Product Designer");
            htDesignation.Add("Product Designer");
            htDesignation.Add("Production Artist");
            htDesignation.Add("Senior Designer");
            htDesignation.Add("UI Designer");
            htDesignation.Add("UI/UX Designer");
            htDesignation.Add("UX Designer");
            htDesignation.Add("Visual Designer");
            htDesignation.Add("Web Designer");

            return htDesignation;
        }

        public static List<string> GetEngineerDesignation()
        {
            List<string> htDesignation = new List<string>();
            htDesignation.Add("Aircraft Mechanic");
            htDesignation.Add("Auto Mechanic");
            htDesignation.Add("Civil Engineer");
            htDesignation.Add("Data Engineer");
            htDesignation.Add("Design Engineer");
            htDesignation.Add("Drafter");
            htDesignation.Add("Electrical Engineer");
            htDesignation.Add("Environmental Engineer");
            htDesignation.Add("Field Engineer");
            htDesignation.Add("Manufacturing Engineer");
            htDesignation.Add("Mechanical Engineer");
            htDesignation.Add("Nuclear Engineer");
            htDesignation.Add("Petroleum Engineer");
            htDesignation.Add("Process Engineer");
            htDesignation.Add("Project Engineer");
            htDesignation.Add("Robotics Engineer");
            htDesignation.Add("Structural Engineer");
            htDesignation.Add("Surveyor");
            htDesignation.Add("Validation Engineer");

            return htDesignation;
        }

        public static List<string> GetFacilitiesDesignation()
        {
            List<string> htDesignation = new List<string>();
            htDesignation.Add("Cleaner");
            htDesignation.Add("Custodian");
            htDesignation.Add("Facilities Coordinator");
            htDesignation.Add("Facilities Manager");
            htDesignation.Add("Forklift Operator");
            htDesignation.Add("Gardener");
            htDesignation.Add("Handyman");
            htDesignation.Add("Janitor");
            htDesignation.Add("Maintenance Manager");
            htDesignation.Add("Maintenance Supervisor");
            htDesignation.Add("Maintenance Technician");
            htDesignation.Add("Maintenance Worker");
            htDesignation.Add("Plant Manager");
            htDesignation.Add("Safety Coordinator");
            htDesignation.Add("Safety Manager");
            htDesignation.Add("Safety Officer");

            return htDesignation;
        }

        public static List<string> GetFinanceDesignation()
        {
            List<string> htDesignation = new List<string>();
            htDesignation.Add("Actuary");
            htDesignation.Add("Assistant Controller");
            htDesignation.Add("CFO");
            htDesignation.Add("Claims Adjuster");
            htDesignation.Add("Controller");
            htDesignation.Add("Director of Finance");
            htDesignation.Add("External Auditor");
            htDesignation.Add("Finance Administrator");
            htDesignation.Add("Finance Assistant");
            htDesignation.Add("Finance Clerk");
            htDesignation.Add("Finance Officer");
            htDesignation.Add("Financial Accountant");
            htDesignation.Add("Financial Adviser");
            htDesignation.Add("Financial Analyst");
            htDesignation.Add("Financial Consultant");
            htDesignation.Add("Financial Controller");
            htDesignation.Add("Financial Manager");
            htDesignation.Add("Financial Planner");
            htDesignation.Add("Financial Specialist");
            htDesignation.Add("Head of Finance");
            htDesignation.Add("Insurance Broker");
            htDesignation.Add("Insurance Underwriter");
            htDesignation.Add("Investment Analyst");
            htDesignation.Add("Investment Banker");
            htDesignation.Add("Nonprofit CFO");
            htDesignation.Add("Personal Banker");
            htDesignation.Add("Retail Banker");
            htDesignation.Add("Senior Financial Analyst");
            htDesignation.Add("Treasurer");

            return htDesignation;
        }

        public static List<string> GetHumanResourcesDesignation()
        {
            List<string> htDesignation = new List<string>();
            htDesignation.Add("Benefits Administrator");
            htDesignation.Add("Bilingual Recruiter");
            htDesignation.Add("Campus Recruiter");
            htDesignation.Add("Chief Human Resources Officer (CHRO)");
            htDesignation.Add("Chief Talent Officer");
            htDesignation.Add("Compensation and Benefits Manager");
            htDesignation.Add("Compensation and Benefits Specialist");
            htDesignation.Add("Compensation Consultant");
            htDesignation.Add("Contract Recruiter");
            htDesignation.Add("Corporate Recruiter");
            htDesignation.Add("Director of Talent");
            htDesignation.Add("Diversity and Inclusion Manager");
            htDesignation.Add("Diversity Recruiter");
            htDesignation.Add("Executive Recruiter");
            htDesignation.Add("Full Cycle Recruiter");
            htDesignation.Add("Head of HR Operations");
            htDesignation.Add("Headhunter");
            htDesignation.Add("Hiring Specialist");
            htDesignation.Add("HR & Admin Officer");
            htDesignation.Add("HR Administrative Assistant");
            htDesignation.Add("HR Administrator");
            htDesignation.Add("HR Advisor");
            htDesignation.Add("HR Analyst");
            htDesignation.Add("HR Assistant");
            htDesignation.Add("HR Business Partner");
            htDesignation.Add("HR Clerk");
            htDesignation.Add("HR Consultant");
            htDesignation.Add("HR Coordinator");
            htDesignation.Add("HR Director");
            htDesignation.Add("HR Executive");
            htDesignation.Add("HR Generalist");
            htDesignation.Add("HR Intern");
            htDesignation.Add("HR Manager");
            htDesignation.Add("HR Officer");
            htDesignation.Add("HR Onboarding Manager");
            htDesignation.Add("HR Onboarding Specialist");
            htDesignation.Add("HR Operations Manager");
            htDesignation.Add("HR Recruiter");
            htDesignation.Add("HR Specialist");
            htDesignation.Add("HRIS Administrator");
            htDesignation.Add("HRIS Manager");
            htDesignation.Add("Internal Recruiter");
            htDesignation.Add("Job Coach");
            htDesignation.Add("Junior Recruiter");
            htDesignation.Add("Payroll Analyst");
            htDesignation.Add("Payroll Clerk");
            htDesignation.Add("Payroll Coordinator");
            htDesignation.Add("Payroll Director");
            htDesignation.Add("Payroll Manager");
            htDesignation.Add("Payroll Officer");
            htDesignation.Add("Payroll Specialist");
            htDesignation.Add("Recruiting Coordinator");
            htDesignation.Add("Recruitment Assistant");
            htDesignation.Add("Recruitment Business Partner");
            htDesignation.Add("Recruitment Consultant");
            htDesignation.Add("Recruitment Manager");
            htDesignation.Add("Recruitment Marketing Manager");
            htDesignation.Add("Recruitment Specialist");
            htDesignation.Add("Regional HR Manager");
            htDesignation.Add("Sales Recruiter");
            htDesignation.Add("Senior HR Manager");
            htDesignation.Add("Sourcing Specialist");
            htDesignation.Add("Staffing Agency Recruiter");
            htDesignation.Add("Staffing Coordinator");
            htDesignation.Add("Staffing Specialist");
            htDesignation.Add("Talent Acquisition Consultant");
            htDesignation.Add("Talent Acquisition Coordinator");
            htDesignation.Add("Talent Acquisition Director");
            htDesignation.Add("Talent Acquisition Manager");
            htDesignation.Add("Talent Acquisition Specialist");
            htDesignation.Add("Talent Scout");
            htDesignation.Add("Talent Sourcer");
            htDesignation.Add("Technical Recruiter");
            htDesignation.Add("Volunteer Coordinator");
            htDesignation.Add("VP of HR");
            htDesignation.Add("VP of Talent Acquisition");
            htDesignation.Add("VP Talent Management");

            return htDesignation;
        }

        public static List<string> GetITDevelopmentDesignation()
        {
            List<string> htDesignation = new List<string>();
            htDesignation.Add(".Net Developer");
            htDesignation.Add("Analytics Manager");
            htDesignation.Add("Android Developer");
            htDesignation.Add("Application Developer");
            htDesignation.Add("Back-End Developer");
            htDesignation.Add("BI (Business Intelligence) Developer");
            htDesignation.Add("BI Consultant");
            htDesignation.Add("Business Analyst");
            htDesignation.Add("Business Operations Manager");
            htDesignation.Add("Chief Information Officer – CIO");
            htDesignation.Add("Computer Security Specialist");
            htDesignation.Add("Computer Technician");
            htDesignation.Add("CTO (Chief Technology Officer)");
            htDesignation.Add("Data Analyst");
            htDesignation.Add("Data Architect");
            htDesignation.Add("Data Manager");
            htDesignation.Add("Data Scientist");
            htDesignation.Add("Database Administrator (DBA)");
            htDesignation.Add("Database Developer");
            htDesignation.Add("DevOps Engineer");
            htDesignation.Add("Director of Engineering");
            htDesignation.Add("Embedded Software Engineer");
            htDesignation.Add("Front-End Developer");
            htDesignation.Add("Full Stack Developer");
            htDesignation.Add("Game Developer");
            htDesignation.Add("Healthcare Data Analyst");
            htDesignation.Add("iOS Developer");
            htDesignation.Add("IT Analyst");
            htDesignation.Add("IT Consultant");
            htDesignation.Add("IT Coordinator");
            htDesignation.Add("IT Director");
            htDesignation.Add("IT Manager");
            htDesignation.Add("IT Operations Manager");
            htDesignation.Add("IT Technician");
            htDesignation.Add("Java Developer");
            htDesignation.Add("Java Software Engineer");
            htDesignation.Add("Lead Data Scientist");
            htDesignation.Add("Machine Learning Engineer");
            htDesignation.Add("Mobile Developer");
            htDesignation.Add("Natural Language Processing Engineer");
            htDesignation.Add("Network Administrator");
            htDesignation.Add("Network Engineer");
            htDesignation.Add("Network Technician");
            htDesignation.Add("PHP Developer");
            htDesignation.Add("Product Manager");
            htDesignation.Add("Product Owner");
            htDesignation.Add("Programmer");
            htDesignation.Add("Project Manager");
            htDesignation.Add("Python Developer");
            htDesignation.Add("QA Engineer");
            htDesignation.Add("QA Tester");
            htDesignation.Add("Ruby on Rails Developer");
            htDesignation.Add("Scrum Master");
            htDesignation.Add("Senior .NET Developer");
            htDesignation.Add("Senior Java Developer");
            htDesignation.Add("Senior Network Engineer");
            htDesignation.Add("Senior Product Manager");
            htDesignation.Add("Senior Python Developer");
            htDesignation.Add("Senior Ruby Developer");
            htDesignation.Add("Senior Software Engineer");
            htDesignation.Add("Senior System Administrator");
            htDesignation.Add("Senior Web Developer");
            htDesignation.Add("Software Architect");
            htDesignation.Add("Software Developer");
            htDesignation.Add("Software Engineer");
            htDesignation.Add("Software Security Engineer");
            htDesignation.Add("System Administrator");
            htDesignation.Add("System Analyst");
            htDesignation.Add("System Security Engineer");
            htDesignation.Add("Systems Engineer");
            htDesignation.Add("Technical Architect");
            htDesignation.Add("Technical Writer");
            htDesignation.Add("Telecommunications Specialist");
            htDesignation.Add("Web Developer");
            htDesignation.Add("Web Programmer");
            htDesignation.Add("Webmaster");

            return htDesignation;
        }

        public static List<string> GetLegalDesignation()
        {
            List<string> htDesignation = new List<string>();
            htDesignation.Add("Project Manager");
            htDesignation.Add("Compliance Manager");
            htDesignation.Add("Compliance Officer");
            htDesignation.Add("Corporate Attorney");
            htDesignation.Add("GDPR Data Protection Officer");
            htDesignation.Add("General Counsel");
            htDesignation.Add("Internal Auditor");
            htDesignation.Add("Legal Assistant");
            htDesignation.Add("Legal Counsel");
            htDesignation.Add("Legal Secretary");
            htDesignation.Add("Litigation Paralegal");
            htDesignation.Add("Risk Manager");

            return htDesignation;
        }

        public static List<string> GetLogisticsDesignation()
        {
            List<string> htDesignation = new List<string>();
            htDesignation.Add("Delivery Driver");
            htDesignation.Add("Driver");
            htDesignation.Add("Inventory Manager");
            htDesignation.Add("Logistics Administrator");
            htDesignation.Add("Logistics Coordinator");
            htDesignation.Add("Logistics Manager");
            htDesignation.Add("Material Handler");
            htDesignation.Add("Materials Manager");
            htDesignation.Add("Order Picker");
            htDesignation.Add("Procurement Manager");
            htDesignation.Add("Purchasing Agent");
            htDesignation.Add("Purchasing Assistant");
            htDesignation.Add("Purchasing Manager");
            htDesignation.Add("Purchasing Officer");
            htDesignation.Add("Receiving Clerk");
            htDesignation.Add("Shipping Manager");
            htDesignation.Add("Sourcing Manager");
            htDesignation.Add("Stock Controller");
            htDesignation.Add("Supply Chain Analyst");
            htDesignation.Add("Supply Chain Manager");
            htDesignation.Add("Transportation Manager");
            htDesignation.Add("Truck Driver");
            htDesignation.Add("Warehouse Associate");
            htDesignation.Add("Warehouse Manager");
            htDesignation.Add("Warehouse Supervisor");
            htDesignation.Add("Warehouse Worker");

            return htDesignation;
        }

        public static List<string> GetOperationProductionDesignation()
        {
            List<string> htDesignation = new List<string>();
            htDesignation.Add("Assembler");
            htDesignation.Add("CNC Operator");
            htDesignation.Add("Machine Operator");
            htDesignation.Add("Machinist");
            htDesignation.Add("Maintenance Mechanic");
            htDesignation.Add("Manager of Quality Assurance");
            htDesignation.Add("Master Grower");
            htDesignation.Add("Mechanic");
            htDesignation.Add("Production Manager");
            htDesignation.Add("Production Planner");
            htDesignation.Add("Production Supervisor");
            htDesignation.Add("Production Worker");
            htDesignation.Add("Quality Inspector");
            htDesignation.Add("Quality Manager");

            return htDesignation;
        }

        public static List<string> GetRealEstateDesignation()
        {
            List<string> htDesignation = new List<string>();
            htDesignation.Add("Leasing Agent");
            htDesignation.Add("Leasing Consultant");
            htDesignation.Add("Property Manager");
            htDesignation.Add("Real Estate Agent");
            htDesignation.Add("Real Estate Appraiser");
            htDesignation.Add("Real Estate Broker");

            return htDesignation;
        }

        public static List<string> GetSalesDesignation()
        {
            List<string> htDesignation = new List<string>();
            htDesignation.Add("Account Coordinator");
            htDesignation.Add("Account Director");
            htDesignation.Add("Account Executive");
            htDesignation.Add("Account Manager");
            htDesignation.Add("Account Officer");
            htDesignation.Add("Account Representative");
            htDesignation.Add("Account Supervisor");
            htDesignation.Add("Assistant Account Executive");
            htDesignation.Add("Business Development Manager");
            htDesignation.Add("Business Development Representative");
            htDesignation.Add("Client Relations Manager");
            htDesignation.Add("Commercial Director");
            htDesignation.Add("Engagement Manager");
            htDesignation.Add("Field Sales Representative");
            htDesignation.Add("Inside Sales Manager");
            htDesignation.Add("Inside Sales Representative");
            htDesignation.Add("Insurance Agent");
            htDesignation.Add("Insurance Sales Representative");
            htDesignation.Add("Junior Account Manager");
            htDesignation.Add("Key Account Manager");
            htDesignation.Add("National Account Manager");
            htDesignation.Add("Regional Sales Manager");
            htDesignation.Add("Relationship Manager");
            htDesignation.Add("Sales Account Executive");
            htDesignation.Add("Sales Account Manager");
            htDesignation.Add("Sales Administrator");
            htDesignation.Add("Sales Assistant");
            htDesignation.Add("Sales Associate");
            htDesignation.Add("Sales Consultant");
            htDesignation.Add("Sales Coordinator");
            htDesignation.Add("Sales Director");
            htDesignation.Add("Sales Engineer");
            htDesignation.Add("Sales Executive");
            htDesignation.Add("Sales Manager");
            htDesignation.Add("Sales Representative");
            htDesignation.Add("Sales Support Specialist");
            htDesignation.Add("Sales Training Specialist");
            htDesignation.Add("Senior Account Executive");
            htDesignation.Add("Senior Account Manager");
            htDesignation.Add("Strategic Account Manager");
            htDesignation.Add("Telemarketer");
            htDesignation.Add("Telesales Representative");
            htDesignation.Add("Territory Manager");
            htDesignation.Add("Territory Sales Representative");
            htDesignation.Add("Visual Merchandiser");

            return htDesignation;
        }
    }
}
