using Microsoft.Data.SqlClient;
using System.Data;

namespace AttendanceMS.Data;

/// <summary>
/// Centralised ADO.NET helper. All DB access in the app goes through this class.
/// Parameterised queries only — no string concatenation → SQL-injection safe.
/// </summary>
public sealed class DbHelper
{
    private readonly string _connectionString;

    public DbHelper(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection string is missing.");
    }

    // ─── Connection factory ────────────────────────────────────────────────
    private SqlConnection CreateConnection() => new(_connectionString);

    // ─── Execute query → DataTable ─────────────────────────────────────────
    public DataTable ExecuteQuery(string sql, params SqlParameter[] parameters)
    {
        using var conn = CreateConnection();
        using var cmd  = new SqlCommand(sql, conn);
        cmd.CommandType = CommandType.Text;
        cmd.Parameters.AddRange(parameters);

        conn.Open();
        using var adapter = new SqlDataAdapter(cmd);
        var dt = new DataTable();
        adapter.Fill(dt);
        return dt;
    }

    // ─── Execute stored procedure → DataTable ──────────────────────────────
    public DataTable ExecuteStoredProcedure(string procName, params SqlParameter[] parameters)
    {
        using var conn = CreateConnection();
        using var cmd  = new SqlCommand(procName, conn);
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.Parameters.AddRange(parameters);

        conn.Open();
        using var adapter = new SqlDataAdapter(cmd);
        var dt = new DataTable();
        adapter.Fill(dt);
        return dt;
    }

    // ─── Execute non-query → rows affected ────────────────────────────────
    public int ExecuteNonQuery(string sql, params SqlParameter[] parameters)
    {
        using var conn = CreateConnection();
        using var cmd  = new SqlCommand(sql, conn);
        cmd.CommandType = CommandType.Text;
        cmd.Parameters.AddRange(parameters);

        conn.Open();
        return cmd.ExecuteNonQuery();
    }

    // ─── Execute scalar (INSERT … OUTPUT INSERTED.Id) ─────────────────────
    public object? ExecuteScalar(string sql, params SqlParameter[] parameters)
    {
        using var conn = CreateConnection();
        using var cmd  = new SqlCommand(sql, conn);
        cmd.CommandType = CommandType.Text;
        cmd.Parameters.AddRange(parameters);

        conn.Open();
        return cmd.ExecuteScalar();
    }

    // ─── Typed scalar shortcut ─────────────────────────────────────────────
    public T? ExecuteScalar<T>(string sql, params SqlParameter[] parameters)
    {
        var result = ExecuteScalar(sql, parameters);
        if (result is null || result == DBNull.Value) return default;
        return (T)Convert.ChangeType(result, typeof(T));
    }

    // ─── SqlParameter factory helpers ─────────────────────────────────────
    public static SqlParameter Param(string name, object? value)
        => new(name, value ?? DBNull.Value);

    public static SqlParameter ParamNullable(string name, object? value)
        => new(name, value ?? (object)DBNull.Value);
}
