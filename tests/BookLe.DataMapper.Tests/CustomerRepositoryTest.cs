using BookLe.DataMapper.Query;
using BookLe.DataMapper.Tests.Models;
using BookLe.DataMapper.Tests.QueryBuilders;
using BookLe.DataMapper.Tests.Repositories;
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
            var repository = new CustomerRepository(new MockSqlQueryBuilder<Customer>());
            var customers = repository.GetCustomers();
            Assert.True(customers != null);
        }

    }

}
