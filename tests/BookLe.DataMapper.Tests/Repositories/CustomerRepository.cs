using BookLe.DataMapper.Query;
using BookLe.DataMapper.Tests.Models;
using BookLe.DataMapper.Tests.QueryBuilders;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace BookLe.DataMapper.Tests.Repositories
{
    public interface ICustomerRepository
    {
        IEnumerable<Customer> GetCustomers();
    }
    public class CustomerRepository : ICustomerRepository
    {
        SqlQueryBuilder<Customer> _queryBuilder;
        public CustomerRepository(SqlQueryBuilder<Customer> queryBuilder)
        {
            _queryBuilder = queryBuilder;
        }
        public IEnumerable<Customer> GetCustomers()
        {
            var customers = _queryBuilder
                .SetSql("select * from Customer")
                .MapProperty(c => c.Zip, "PostalCode")
                .GetResult().List;
            return customers;
        }
    }
}
