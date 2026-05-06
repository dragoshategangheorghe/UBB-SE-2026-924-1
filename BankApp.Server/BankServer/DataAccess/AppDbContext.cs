using System.Data;
using Microsoft.Data.SqlClient;
namespace BankApp.Server.DataAccess
{
    public class AppDbContext : IDbContext
    {
        private readonly string connectionString;
        private SqlConnection? connection;
        private SqlTransaction? currentTransaction;

        public AppDbContext(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public SqlConnection GetConnection()
        {
            if (connection == null || connection.State == ConnectionState.Closed)
            {
                try
                {
                    connection = new SqlConnection(connectionString);
                    connection.Open();
                }
                catch (SqlException e)
                {
                    throw new Exception($"Failed to connect to the database: {e.Message}", e);
                }
            }
            return connection;
        }

        public SqlTransaction BeginTransaction()
        {
            SqlConnection conn = GetConnection();
            try
            {
                currentTransaction = conn.BeginTransaction();
            }
            catch (SqlException e)
            {
                throw new Exception($"Failed to begin transaction: {e.Message}", e);
            }
            return currentTransaction;
        }

        public void CommitTransaction()
        {
            if (currentTransaction != null)
            {
                currentTransaction.Commit();
                currentTransaction = null;
            }
        }

        public void RollbackTransaction()
        {
            if (currentTransaction != null)
            {
                currentTransaction.Rollback();
                currentTransaction = null;
            }
        }

        public SqlTransaction? GetCurrentTransaction()
        {
            return currentTransaction;
        }

        private void AddParameters(SqlCommand cmd, object[] parameters)
        {
            if (parameters == null)
            {
                return;
            }
            for (int i = 0; i < parameters.Length; i++)
            {
                cmd.Parameters.AddWithValue($"@p{i}", parameters[i] ?? DBNull.Value);
            }
        }

        public IDataReader ExecuteQuery(string sqlStatement, object[] parameters)
        {
            var conn = GetConnection();
            var cmd = new SqlCommand(sqlStatement, conn, currentTransaction);
            AddParameters(cmd, parameters);
            return cmd.ExecuteReader(); // returns rows back
        }

        public int ExecuteNonQuery(string sqlStatement, object[] parameters)
        {
            var conn = GetConnection();
            using var cmd = new SqlCommand(sqlStatement, conn, currentTransaction); // disposes the command when done with it
            AddParameters(cmd, parameters);
            return cmd.ExecuteNonQuery(); // how many rows are affected
        }

        public void Dispose()
        {
            if (currentTransaction != null)
            {
                currentTransaction.Dispose();
            }

            if (connection != null)
            {
                if (connection.State != ConnectionState.Closed)
                {
                    connection.Close();
                }

                connection.Dispose();
                connection = null;
            }
        }

        public object? ExecuteScalar(string sql, object[] parameters)
        {
            var conn = GetConnection();
            using var cmd = new SqlCommand(sql, conn, currentTransaction);
            AddParameters(cmd, parameters);
            return cmd.ExecuteScalar();
        }

        // async methods
        public async Task<SqlConnection> GetConnectionAsync()
        {
            if (connection == null || connection.State == ConnectionState.Closed)
            {
                try
                {
                    connection = new SqlConnection(connectionString);
                    await connection.OpenAsync(); // Deschidere asincronă
                }
                catch (SqlException e)
                {
                    throw new Exception($"Failed to connect to the database: {e.Message}", e);
                }
            }

            return connection;
        }

        public async Task<int> ExecuteNonQueryAsync(string sql, object[] parameters)
        {
            var conn = await GetConnectionAsync();
            using var cmd = new SqlCommand(sql, conn, currentTransaction);
            AddParameters(cmd, parameters);
            return await cmd.ExecuteNonQueryAsync();
        }

        public async Task<object?> ExecuteScalarAsync(string sql, object[] parameters)
        {
            var conn = await GetConnectionAsync();
            using var cmd = new SqlCommand(sql, conn, currentTransaction);
            AddParameters(cmd, parameters);
            return await cmd.ExecuteScalarAsync();
        }

        public async Task<SqlDataReader> ExecuteQueryAsync(string sql, object[] parameters)
        {
            var conn = await GetConnectionAsync();
            var cmd = new SqlCommand(sql, conn, currentTransaction);
            AddParameters(cmd, parameters);
            return await cmd.ExecuteReaderAsync();
        }

        public async Task<SqlTransaction> BeginTransactionAsync()
        {
            SqlConnection conn = await GetConnectionAsync();
            try
            {
                currentTransaction = (SqlTransaction)await conn.BeginTransactionAsync();
            }
            catch (SqlException e)
            {
                throw new Exception($"Failed to begin transaction: {e.Message}", e);
            }
            return currentTransaction;
        }

        public async Task CommitTransactionAsync()
        {
            if (currentTransaction != null)
            {
                await currentTransaction.CommitAsync();
                currentTransaction = null;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (currentTransaction != null)
            {
                await currentTransaction.RollbackAsync();
                currentTransaction = null;
            }
        }
    }
}