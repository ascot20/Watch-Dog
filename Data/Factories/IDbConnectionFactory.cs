using System.Data;

namespace WatchDog.Data.Factories;

public interface IDbConnectionFactory
{
    public IDbConnection CreateConnection();
}