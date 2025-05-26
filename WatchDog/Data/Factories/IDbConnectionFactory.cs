using System.Data;

namespace WatchDog.Data.Factories;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}