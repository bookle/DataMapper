using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace BookLe.DataMapper.Tests.QueryBuilders
{
    public class MockCommand : IDbCommand
    {
        private SqlConnection conn = null;
        private SqlCommand cmd = null;
        private DataTable dt;

        public MockCommand(DataTable dt)
        {
            this.dt = dt;
            conn = new SqlConnection("");
            cmd = conn.CreateCommand();
        }
        public string CommandText { get => cmd.CommandText; set => cmd.CommandText = value; }
        public int CommandTimeout { get; set; }
        public CommandType CommandType { get; set; }
        public IDbConnection Connection { get; set; }

        public IDataParameterCollection Parameters => cmd.Parameters;

        public IDbTransaction Transaction { get; set; }
        public UpdateRowSource UpdatedRowSource { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public void Cancel()
        {
        }

        public IDbDataParameter CreateParameter()
        {
            return cmd.CreateParameter();
        }

        public void Dispose()
        {
            cmd.Dispose();
            conn.Dispose();
        }

        public int ExecuteNonQuery()
        {
            return 1;
        }

        public IDataReader ExecuteReader()
        {
            return new DataTableReader(dt);
        }

        public IDataReader ExecuteReader(CommandBehavior behavior)
        {
            return ExecuteReader();
        }

        public object ExecuteScalar()
        {
            return 1;
        }

        public void Prepare()
        {
        }
    }
}
