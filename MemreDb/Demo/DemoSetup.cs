using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MemreDb.Memre;
using MemreDb.Memre.Indices;

namespace MemreDb.Demo
{
    static class TableNames
    {
        internal static string companies = "companies";
        internal static string invoices = "invoices";
        internal static string invoiceLineItems = "invoiceLineItems";
        internal static string customers = "customers";
        internal static string employees = "employees";
    }
    internal class DemoSetup
    {
        Database _database;
        uint _widgetsCompanyId;

        public DemoSetup()
        {
            _database = null;
        }

        internal void SetUp()
        {
            _database = new Database("Demo");

            _database.AddTable<Company>(TableNames.companies, "CompanyId", true);
            _database.AddTable<Invoice>(TableNames.invoices, "InvoiceId", true);
            _database.AddTable<InvoiceLineItem>(TableNames.invoiceLineItems, "InvoiceLineItemId", true);
            _database.AddTable<Customer>(TableNames.customers, "CustomerId", true);

            _database.AddConstraint<Company, Customer>(TableNames.companies, TableNames.customers, "CompanyId", false, true);
            _database.AddConstraint<Company, Invoice>(TableNames.companies, TableNames.invoices, "CompanyId", false, true);
            _database.AddConstraint<Invoice, InvoiceLineItem>(TableNames.invoices, TableNames.invoiceLineItems, "InvoiceId", false, true);
            _database.AddConstraint<Customer, Invoice>(TableNames.customers, TableNames.invoices, "CustomerId", false, true);

            _database.AddTable<Address>("addresses", "AddressId", true);
            _database.AddTable<Location>("locations", "LocationId", false);
            _database.AddTable<Employee>("employees", "EmployeeId", true);
            _database.AddConstraint<Company, Employee>(TableNames.companies, TableNames.employees, "CompanyId", false, true);
        }

        internal void CreateData()
        {
            this._widgetsCompanyId = CreateCompany("Widgets Ltd");
            uint employeeId = CreateEmployee("Lacy Lacisness", "l_laciness@example.com", 27, this._widgetsCompanyId);
            employeeId = CreateEmployee("Kelly Kearheart", "k_kearheart@example.com", 30, this._widgetsCompanyId);
            uint otherCompany = CreateCompany("Others Ltd");
            employeeId = CreateEmployee("Olly Otherson", "o_otherson@example.com", 30, otherCompany);

            uint customerId = CreateCustomer(this._widgetsCompanyId, "Dave Davidson", "d_davidson@example.com");

            uint invoiceId = CreateInvoice("December Invoice", this._widgetsCompanyId, customerId);
            uint lineItemId = AddInvoiceItemLineToInvoice(invoiceId, "Green Widgets", 7, (decimal)30.0);
            Console.WriteLine($"Added line item to invoice-id {invoiceId} with id {lineItemId}");
            lineItemId = AddInvoiceItemLineToInvoice(invoiceId, "Blue Widgets", 1, (decimal)90.0);
            Console.WriteLine($"Added line item to invoice-id {invoiceId} with id {lineItemId}");
            lineItemId = AddInvoiceItemLineToInvoice(invoiceId, "Purple Widgets", 5, (decimal)150.0);
            Console.WriteLine($"Added line item to invoice-id {invoiceId} with id {lineItemId}");
            lineItemId = AddInvoiceItemLineToInvoice(invoiceId, "Red Widgets", 7, (decimal)40.0);
            Console.WriteLine($"Added line item to invoice-id {invoiceId} with id {lineItemId}");
            lineItemId = AddInvoiceItemLineToInvoice(invoiceId, "Orange Widgets", 7, (decimal)40.0);
            Console.WriteLine($"Added line item to invoice-id {invoiceId} with id {lineItemId}");
            lineItemId = AddInvoiceItemLineToInvoice(invoiceId, "Pink Widgets", 7, (decimal)40.0);
            Console.WriteLine($"Added line item to invoice-id {invoiceId} with id {lineItemId}");

            customerId = CreateCustomer(this._widgetsCompanyId, "Pete Peterson", "p_peterson@example.com");
            invoiceId = CreateInvoice("December Invoice", this._widgetsCompanyId, customerId);
            AddInvoiceItemLineToInvoice(invoiceId, "Green Widgets", 3, (decimal)30.0);
            AddInvoiceItemLineToInvoice(invoiceId, "Blue Widgets", 2, (decimal)90.0);
            AddInvoiceItemLineToInvoice(invoiceId, "Purple Widgets", 15, (decimal)150.0);
            AddInvoiceItemLineToInvoice(invoiceId, "Red Widgets", 1, (decimal)40.0);
            AddInvoiceItemLineToInvoice(invoiceId, "Orange Widgets", 15, (decimal)3.0);
        }

        internal void RunTests()
        {
            Company company = GetCompany(this._widgetsCompanyId);
            List<Employee> employees = GetEmployees(_widgetsCompanyId);

            //RunQuery1(1);
            RunDeleteQuery1(1,(decimal)40.0);
            RunDeleteQuery2(1);
        }

        void QueryInvoiceForAllLineItems(uint invoiceId)
        {
            IMemreQuery q1 = this._database.CreateQuery(QueryType.Select, TableNames.invoiceLineItems);
            WhereClause w1 = q1.AddWhere();
            SubClause sc1 = w1.GetTopLevelClause();

            sc1.SetSubClauseWithLiteralTo("InvoiceId", SubClauseComparitor.Equals, invoiceId);

            q1.Execute();
            OutputInvoiceLineItemsFromQuery($"Items for invoice {invoiceId}",q1);
        }

        void RunQuery2()
        {
            IMemreQuery q1 = this._database.CreateQuery(QueryType.Select, TableNames.invoiceLineItems);
            WhereClause w1 = q1.AddWhere();
            SubClause sc1 = w1.GetTopLevelClause();

            sc1.SetSubClauseWithLiteralTo("LineItemQuantity", SubClauseComparitor.LessThan, 5);
            q1.Execute();
            OutputInvoiceLineItemsFromQuery("Items with quantity less than 5, for any invoices", q1);
        }

        void OutputCustomersFromQuery(IMemreQuery query)
        {
            List<Customer> customers = query.Select<Customer>("customers");
            foreach (Customer c in customers)
            {
                Console.Write($"Customer {c}");
            }
        }

        void RunQuery3(uint invoiceId)
        {
            Debug.Assert(this._database != null);
            IMemreQuery q1 = this._database.CreateQuery(QueryType.Select, TableNames.invoiceLineItems);
            WhereClause w1 = q1.AddWhere();

            SubClause sc1 = w1.GetTopLevelClause();
            sc1.SetSubClauseToType(SubClauseType.And);
            SubClause sc1_left = sc1.GetLeftSubClause();
            SubClause sc1_right = sc1.GetRightSubClause();

            sc1_left.SetSubClauseWithLiteralTo("LineItemQuantity", SubClauseComparitor.LessThan, (uint)10);
            sc1_right.SetSubClauseWithLiteralTo("InvoiceId", SubClauseComparitor.Equals, invoiceId);
            q1.Execute();
            OutputInvoiceLineItemsFromQuery($"Items for invoice {invoiceId} of quanity less than 10",q1);
        }

        void RunDeleteQuery1(int invoiceId, decimal deleteWithUnitCost)
        {
            IMemreQuery q1 = this._database.CreateQuery(QueryType.Delete, TableNames.invoiceLineItems);
            WhereClause w1 = q1.AddWhere();

            SubClause sc1 = w1.GetTopLevelClause();
            sc1.SetSubClauseToType(SubClauseType.And);
            SubClause sc1_left = sc1.GetLeftSubClause();
            SubClause sc1_right = sc1.GetRightSubClause();

            sc1_left.SetSubClauseWithLiteralTo("LineItemUnitCost", SubClauseComparitor.Equals, deleteWithUnitCost);
            sc1_right.SetSubClauseWithLiteralTo("InvoiceId", SubClauseComparitor.Equals, invoiceId);

            QueryInvoiceForAllLineItems((uint)invoiceId);

            Console.WriteLine($"Deleting line items on invoice {invoiceId} with a unit cost of {deleteWithUnitCost}");
            int? deletedCount = q1.Execute();
            if (deletedCount.HasValue)
            {
                Console.WriteLine($"Deleted {deletedCount} rows");
            }
            else
            {
                Console.WriteLine("Did not delete any rows");
            }
            QueryInvoiceForAllLineItems((uint)invoiceId);
        }

        void RunDeleteQuery2(int invoiceId)
        {
            IMemreQuery q1 = this._database.CreateQuery(QueryType.Delete, TableNames.invoices);
            WhereClause w1 = q1.AddWhere();

            SubClause sc1 = w1.GetTopLevelClause();
            sc1.SetSubClauseWithLiteralTo("InvoiceId", SubClauseComparitor.Equals, invoiceId);

            QueryInvoiceForAllLineItems((uint)invoiceId);

            int? deletedCount = q1.Execute();
            if (deletedCount.HasValue)
            {
                Console.WriteLine($"Deleted {deletedCount} rows");
            }
            else
            {
                Console.WriteLine("Did not delete any rows");
            }
            QueryInvoiceForAllLineItems((uint)invoiceId);

        }

        void OutputInvoiceLineItemsFromQuery(string message,IMemreQuery query)
        {
            Console.WriteLine();
            Console.WriteLine("Results from query on line items");
            Console.WriteLine(message);
            List<OrderBy> orderBy = new List<OrderBy>();
            orderBy.Add(new OrderBy { MemberToOrder = "LineItemQuantity", Direction = OrderDirection.Descending });
            orderBy.Add(new OrderBy { MemberToOrder = "LineItemUnitCost", Direction = OrderDirection.Descending });
            orderBy.Add(new OrderBy { MemberToOrder = "LineItemDescription", Direction = OrderDirection.Ascending });
            query.OrderBy(orderBy);
            List<InvoiceLineItem> lineItems = query.Select<InvoiceLineItem>(TableNames.invoiceLineItems);
            if (lineItems.Count == 0)
            {
                Console.WriteLine(" No items");
            }
            else
            {
                foreach (InvoiceLineItem l in lineItems)
                {
                    Console.WriteLine($"Line Item {l}");
                }
            }
        }

        internal Company GetCompany(uint companyId)
        {
            Company toReturn = null;

            Debug.Assert(this._database != null);
            IMemreQuery query = this._database.CreateQuery(QueryType.Select, "companies");
            WhereClause where = query.AddWhere();
            SubClause sc1 = where.GetTopLevelClause();

            sc1.SetSubClauseWithLiteralTo("CompanyId", SubClauseComparitor.Equals, companyId);

            query.Execute();
            List<Company> companies = query.Select<Company>("companies");
            if (companies.Count>0)
            {
                toReturn = companies[0];
            }
            return toReturn;
        }

        internal List<Employee> GetEmployees(uint companyId)
        {
            List<Employee> toReturn;

            Debug.Assert(this._database != null);
            IMemreQuery query = this._database.CreateQuery(QueryType.Select, "employees");
            WhereClause where = query.AddWhere();
            SubClause sc1 = where.GetTopLevelClause();

            sc1.SetSubClauseWithLiteralTo("CompanyId", SubClauseComparitor.Equals, companyId);

            query.Execute();
            toReturn = query.Select<Employee>("employees");
            return toReturn;
        }

        uint CreateCompany(string companyName)
        {
            Company company = new Company
            {
                CompanyName = companyName
            };
            return this._database!.AddObjectToTable("companies", company);
        }

        uint CreateInvoice(string invoiceTitle, uint companyId, uint customerId)
        {
            Invoice invoice = new Invoice
            {
                InvoiceTitle = invoiceTitle,
                CompanyId = companyId,
                CustomerId = customerId
            };
            return this._database!.AddObjectToTable("invoices", invoice);
        }

        uint AddInvoiceItemLineToInvoice(uint invoiceId, string lineItemDescription, uint quantity, decimal unitCost)
        {
            InvoiceLineItem lineItem = new InvoiceLineItem
            {
                InvoiceId = invoiceId,
                LineItemDescription = lineItemDescription,
                LineItemUnitCost = unitCost,
                LineItemQuantity = quantity
            };
            return this._database!.AddObjectToTable(TableNames.invoiceLineItems, lineItem);
        }

        uint CreateCustomer(uint companyId, string customerName, string customerEmail)
        {
            Customer customer = new Customer()
            {
                CompanyId = companyId,
                CustomerName = customerName,
                CustomerEmail = customerEmail
            };
            return this._database!.AddObjectToTable("customers", customer);
        }

        uint CreateAddress(string firstLine, string secondLine, string town, string county, string postcode, uint? employeeId)
        {
            Address address = new Address
            {
                FirstLine = firstLine,
                SecondLine = secondLine,
                Town = town,
                County = county,
                PostCode = postcode,
                EmployeeId = employeeId.HasValue ? employeeId.Value : 0
            };
            return this._database!.AddObjectToTable("addresses", address);
        }

        uint CreateEmployee(string firstName, string surname, uint employeeAge, uint? companyId)
        {
            Employee employee = new Employee
            {
                FirstName = firstName,
                Surname = surname,
                Age = employeeAge,
                CompanyId = companyId.HasValue ? companyId.Value : 0
            };
            return this._database!.AddObjectToTable("employees", employee);
        }
    }
}
