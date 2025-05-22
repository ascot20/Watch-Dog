using System.Data;
using Npgsql;

namespace WatchDog.Data.Factories;

public class PostDbConnectionFactory:IDbConnectionFactory
{
    private readonly string _connectionString;
    
    public PostDbConnectionFactory(string connectionString)
    {
       this._connectionString = connectionString;
    }

    public IDbConnection CreateConnection()
    {
        var connection = new NpgsqlConnection(this._connectionString);
        connection.Open();
        return connection;
    }
}