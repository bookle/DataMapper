using System;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using Xunit;
using BookLe.DataMapper.Extensions;
using BookLe.DataMapper.Tests.QueryBuilders;
using BookLe.DataMapper.Tests.Models;

namespace BookLe.DataMapper.Tests
{
    public class DataMapperTests
    {
        [Fact]
        public void QueryBuilder_ShouldSetSql()
        {
            var sql = "select someThing from someTable";
            var builder = new MockSqlQueryBuilder<Customer>();
            builder.SetSql(sql);
            builder.GetResult();
            var command = builder.InternalProps.Command;
            Assert.True(command.CommandType == CommandType.Text);
            Assert.True(command.CommandText == sql);
        }

        [Fact]
        public void QueryBuilder_ShouldSetStoredProcedure()
        {
            var storedProcName = "someStoredProc";
            var builder = new MockSqlQueryBuilder<Customer>();
            builder.SetStoredProcedure(storedProcName);
            builder.GetResult();
            var command = builder.InternalProps.Command;
            Assert.True(command.CommandType == CommandType.StoredProcedure);
            Assert.True(command.CommandText == storedProcName);
        }


        [Fact]
        public void QueryBuilder_ShouldSetConnectionString()
        {
            var connString = "Some Connection String";
            var builder = new MockSqlQueryBuilder<Customer>();
            builder.SetConnectionString(connString);
            builder.GetResult();
            Assert.True(builder.InternalProps.ConnString == connString);
        }

        [Fact]
        public void QueryBuilder_ShouldSetIgnoreDataColumnNotFound()
        {
            var builder = new MockSqlQueryBuilder<Customer>();
            builder.SetIgnoreDataColumnNotFound(false);
            builder.GetResult();
            Assert.False(builder.InternalProps.IgnoreDataColumnNotFound);
        }

        [Fact]
        public void QueryBuilder_ShouldSetCommandTimeout()
        {
            var builder = new MockSqlQueryBuilder<Customer>();
            builder.SetCommandTimeout(300);
            builder.GetResult();
            Assert.True(builder.InternalProps.Command.CommandTimeout == 300);
        }


        [Fact]
        public void QueryBuilder_ShouldMapProperty()
        {
            var builder = new MockSqlQueryBuilder<Customer>();
            builder.MapProperty(c => c.FirstName, "FName");
            builder.GetResult();
            Assert.True(builder.InternalProps.PropertyMappings.First(pm => pm.PropertyName == "FirstName").ColumnName == "FName");
        }

        [Fact]
        public void QueryBuilder_ShouldHydrateCustomerList()
        {
            var customers = new SQLiteQueryBuilder<Customer>()
                .SetSql("select CustomerId, FirstName, LastName, Company, Address, City, State, Country, PostalCode As Zip, Phone, Fax, Email from Customer")
                .GetResult().List;
           
            Assert.True(customers.Count > 0);
            Assert.True(customers.All(c =>
                c.FirstName.IsNotEmpty() &&
                c.LastName.IsNotEmpty() &&
                c.CustomerId > 0));
        }

        [Fact]
        public void QueryBuilder_ShouldPopulateZipFromPostalCode()
        {
            var customers = new SQLiteQueryBuilder<Customer>()
                .SetSql("select CustomerId, FirstName, LastName, Company, Address, City, State, Country, PostalCode, Phone, Fax, Email from Customer where CustomerId = $CustomerId")
                .AddParameter("$CustomerId", 19)
                .MapProperty(c => c.Zip, "PostalCode")
                .GetResult().List;

            Assert.True(customers.First().Zip == "95014");
        }

        [Fact]
        public void QueryBuilder_ShouldPopulateFullName()
        {
            var customers = new SQLiteQueryBuilder<Customer>()
                .SetSql("select CustomerId, FirstName, LastName, Company, Address, City, State, Country, PostalCode, Phone, Fax, Email from Customer where CustomerId = $CustomerId")
                .AddParameter("$CustomerId", 19)
                .MapProperty(c => c.FullName, row => $"{row.GetString("FirstName")} {row.GetString("LastName")}")
                .GetResult().List;

            Assert.True(customers.First().FullName == "Tim Goyer");
        }

        [Fact]
        public void QueryBuilder_ShouldPopulateInvoicesAndChildCustomer()
        {
            var invoices = new SQLiteQueryBuilder<Invoice>()
                .SetSql(@"
                select inv.*, cust.*
                from Invoice inv
                inner join Customer cust on inv.CustomerId = cust.CustomerId
                where cust.CustomerId = 19")
                .MapProperty(inv => inv.Customer, row => new Customer
                {
                    CustomerId = row.GetInteger("CustomerId").Value,
                    FirstName = row.GetString("FirstName"),
                    LastName = row.GetString("LastName")
                })
                .GetResult().List;

            Assert.True(invoices.First().Customer.FirstName == "Tim");
        }

        [Fact]
        public void QueryBuilder_ShouldPopulateCustomerUsingParameter()
        {
            var customers = new SQLiteQueryBuilder<Customer>()
                .SetSql("select CustomerId, FirstName, LastName, Company, Address, City, State, Country, PostalCode, Phone, Fax, Email from Customer where CustomerId = $CustomerId")
                .AddParameter("$CustomerId", 19)
                .MapProperty(c => c.FullName, row => $"{row.GetString("FirstName")} {row.GetString("LastName")}")
                .GetResult().List;

            Assert.True(customers.First().FullName == "Tim Goyer");
        }

        [Fact]
        public void QueryBuilder_ShouldBeAbleToSupplyConnection()
        {
            using (var conn = new SQLiteConnection(@"Data Source=Data\chinook.db;Version=3;"))
            {
                var customers = new SQLiteQueryBuilder<Customer>()
                    .SetSql("select CustomerId, FirstName, LastName, Company, Address, City, State, Country, PostalCode, Phone, Fax, Email from Customer where CustomerId = $CustomerId")
                    .AddParameter("$CustomerId", 19)
                    .MapProperty(c => c.FullName, row => $"{row.GetString("FirstName")} {row.GetString("LastName")}")
                    .GetResult(conn).List;

                Assert.True(customers.First().FullName == "Tim Goyer");
            }

        }

        [Fact]
        public void QueryBuilder_ShouldBeAbleToSupplyTransaction()
        {
            using (var conn = new SQLiteConnection(@"Data Source=Data\chinook.db;Version=3;"))
            {
                conn.Open();
                using (var tran = conn.BeginTransaction())
                {
                    var customers = new SQLiteQueryBuilder<Customer>()
                        .SetSql("select CustomerId, FirstName, LastName, Company, Address, City, State, Country, PostalCode, Phone, Fax, Email from Customer where CustomerId = $CustomerId")
                        .AddParameter("$CustomerId", 19)
                        .MapProperty(c => c.FullName, row => $"{row.GetString("FirstName")} {row.GetString("LastName")}")
                        .GetResult(tran).List;
                    tran.Commit();
                    Assert.True(customers.First().FullName == "Tim Goyer");
                }

            }

        }


    }
}
