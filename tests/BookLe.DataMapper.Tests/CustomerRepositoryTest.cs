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
            var repository = new CustomerRepository(new MockSQLiteQueryBuilder<Customer>());
            var customers = repository.GetCustomers();
            Assert.True(customers != null);
        }

    }

    class MockSQLiteQueryBuilder<T> : SQLiteQueryBuilder<T> where T : class, new()
    {
        /// <summary>
        /// Overrides GetResult so no real database statements are executed.
        /// </summary>
        /// <returns></returns>
        public override QueryResult<T> GetResult()
        {
            return new QueryResult<T>
            {
                List = new System.Collections.Generic.List<T>()
            };
        }
    }
}
