using System.Data;

namespace GVC.MobileAPI.Data;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}