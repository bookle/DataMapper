using BookLe.DataMapper.Tests.Models;
using BookLe.DataMapper.Tests.QueryBuilders;
using System;
using System.Collections.Generic;
using System.Text;

namespace BookLe.DataMapper.Tests.Repositories
{
    public interface ICustomerRepository
    {
        IEnumerable<Customer> GetCustomers();
    }
    public class CustomerRepository : ICustomerRepository
    {
        SQLiteQueryBuilder<Customer> _queryBuilder;
        public CustomerRepository(SQLiteQueryBuilder<Customer> queryBuilder)
        {
            _queryBuilder = queryBuilder;
        }
        public IEnumerable<Customer> GetCustomers()
        {
            var customers = _queryBuilder
                .SetSql("select * from Customer")
                .GetResult().List;
            return customers;
        }
    }
}
