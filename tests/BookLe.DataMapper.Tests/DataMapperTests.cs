using System;
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
        public void QueryBuilder_ShouldHydrateCustomerList()
        {
            var customers = new SQLiteQueryBuilder<Customer>()
                .SetSql("select * from Customer")
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
                .SetSql("select * from Customer where CustomerId = 19")
                .MapProperty(c => c.Zip, "PostalCode")
                .GetResult().List;

            Assert.True(customers.First().Zip == "95014");
        }

        [Fact]
        public void QueryBuilder_ShouldPopulateFullName()
        {
            var customers = new SQLiteQueryBuilder<Customer>()
                .SetSql("select * from Customer where CustomerId = 19")
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
                .SetSql("select * from Customer where CustomerId = $CustomerId")
                .AddParameter("$CustomerId", 19)
                .MapProperty(c => c.FullName, row => $"{row.GetString("FirstName")} {row.GetString("LastName")}")
                .GetResult().List;

            Assert.True(customers.First().FullName == "Tim Goyer");
        }


    }
}
