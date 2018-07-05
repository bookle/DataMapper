using BookLe.DataMapper.Query;
using BookLe.DataMapper.Tests.Models;
using BookLe.DataMapper.Tests.QueryBuilders;
using BookLe.DataMapper.Tests.Repositories;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace BookLe.DataMapper.Tests
{
    /// <summary>
    /// Demonstrates how to mock a QueryBuilder
    /// </summary>
    public class CustomerRepositoryTest
    {
        [Fact]
        public void CustomerRepository_GetCustomers_ShouldReturnConcreteList()
        {
            var list = new List<Customer>
            {
                new Customer
                {
                    CustomerId = 1,
                    FirstName = "Bob",
                    LastName = "Jones",
                    EmailAddress = "bobjones@email.com",
                    City = "Atlanta",
                    State = "GA",
                    Zip = "30339",
                    Phone = "8881234567"
                },
                new Customer
                {
                    CustomerId = 1,
                    FirstName = "Dave",
                    LastName = "Smith",
                    EmailAddress = "dave@email.com",
                    City = "Seattle",
                    State = "WA",
                    Zip = "99939",
                    Phone = "8884555444"
                },
                new Customer
                {
                    CustomerId = 1,
                    FirstName = "Erin",
                    LastName = "Mitchell",
                    EmailAddress = "erin@email.com",
                    City = "Macon",
                    State = "GA",
                    Zip = "39999",
                    Phone = "8887774567"
                },

            };
            var dt = new DataTableBuilder<Customer>(list)
                .ColumnFor(c => c.Zip, "PostalCode")
                .Build();
            var command = new MockCommand(dt);
            var sqlBuilder = new SqlQueryBuilder<Customer>(() => command, conn => { });     
            var repository = new CustomerRepository(sqlBuilder);
            var customers = repository.GetCustomers();
            Assert.True(customers.Count() == list.Count);
        }

    }

}
