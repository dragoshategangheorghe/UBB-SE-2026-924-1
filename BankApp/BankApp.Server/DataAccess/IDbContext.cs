using System.Data;
using Microsoft.Data.SqlClient;
namespace BankApp.Server.DataAccess
{
    public interface IDbContext : IDisposable
    {
        SqlTransaction BeginTransaction();
        void CommitTransaction();
        void RollbackTransaction();
        IDataReader ExecuteQuery(string sql, object[] parameters);
        int ExecuteNonQuery(string sql, object[] parameters);
        object? ExecuteScalar(string sql, object[] parameters);
        Task<SqlConnection> GetConnectionAsync();
        Task<int> ExecuteNonQueryAsync(string sql, object[] parameters);
        Task<object?> ExecuteScalarAsync(string sql, object[] parameters);
        Task<SqlDataReader> ExecuteQueryAsync(string sql, object[] parameters);
        Task<SqlTransaction> BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}