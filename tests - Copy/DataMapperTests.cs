using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BookLe.DataMapper.Extensions;
using BookLe.DataMapper.Tests.QueryBuilders;
using BookLe.DataMapper.Tests.Models;

namespace BookLe.DataMapper.Tests
{
    [TestClass]
    public class DataMapperTests
    {
        [TestMethod]
        public void QueryBuilder_ShouldHydrateCustomerList()
        {
            var customers = new SQLiteQueryBuilder<Customer>()
                .SetSqlText("select * from Customer")
                .GetResult().List;

            Assert.IsTrue(customers.Count > 0);
            Assert.IsTrue(customers.All(c =>
                c.FirstName.IsNotEmpty() &&
                c.LastName.IsNotEmpty() &&
                c.CustomerId > 0));
        }

        [TestMethod]
        public void QueryBuilder_ShouldPopulateZipFromPostalCode()
        {
            var customers = new SQLiteQueryBuilder<Customer>()
                .SetSqlText("select * from Customer where CustomerId = 19")
                .MapProperty(c => c.Zip, "PostalCode")
                .GetResult().List;

            Assert.IsTrue(customers.First().Zip == "95014");
        }

        [TestMethod]
        public void QueryBuilder_ShouldPopulateFullName()
        {
            var customers = new SQLiteQueryBuilder<Customer>()
                .SetSqlText("select * from Customer where CustomerId = 19")
                .MapProperty(c => c.FullName, row => $"{row.GetString("FirstName")} {row.GetString("LastName")}")
                .GetResult().List;

            Assert.IsTrue(customers.First().FullName == "Tim Goyer");
        }

        [TestMethod]
        public void QueryBuilder_ShouldPopulateInvoicesAndChildCustomer()
        {
            var invoices = new SQLiteQueryBuilder<Invoice>()
                .SetSqlText(@"
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

            Assert.IsTrue(invoices.First().Customer.FirstName == "Tim");
        }


    }
}
