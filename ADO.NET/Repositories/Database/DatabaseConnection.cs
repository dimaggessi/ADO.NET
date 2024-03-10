using System.Data;
using System.Data.SqlClient;

namespace ADO.NET.Repositories.Database;


/* [ A conexão é feita através do ADO.NET que possui um conjunto de classes e interfaces
 * que fazem parte do Base Class Library - BCL ]
 * 
 *  [ Para fazer uma conexão com o Banco de Dados é necessário um Provider ]
 * ---*---------------------------------------------------------------------*------------------------------
 *  O provider é um pacote, geralmente criado pela empresa que constrói o Banco de Dados, que vai
 *  implementar as classes do ADO.NET para que seja possível fazer uma conexão, realizar as queries, etc.
 * 
 *  o ADO.NET faz parte do framework .NET, mas o Provider não.
 * ---*---------------------------------------------------------------------*------------------------------
 * 
 *  Sendo assim, é necessário instalar um pacote através do NuGet
 * 
 *  No caso, como o Banco de Dados é SQL Server, se usa o System.Data.SqlClient
 */
public class DatabaseConnection : IDisposable
{
    private readonly IConfiguration _configuration;
    private string _connectionString;
    private SqlConnection _connection;

    public DatabaseConnection(IConfiguration configuration)
    {
        // SqlCredential dentro de SqlConnection seria basicamente usuário e senha
        // SqlConnection varia de acordo com o Banco de Dados que vai ser utilizado
        // por exemplo, poderia ser MySqlConnection

        _configuration = configuration;

        _connectionString = _configuration.GetSection("ConnectionStrings")["ADO.NET"];

        _connection = new SqlConnection(_connectionString);

    }

    // SqlConnection possui uma propriedade ConnectionSate que é um Enum de estados de conexões
    public void Open() 
    {
        if (_connection.State != ConnectionState.Open)
        {
            _connection.Open();
        }
    }

    public void Close()
    {
        if (_connection.State != ConnectionState.Closed)
        { 
            _connection.Close();
        }
    }

    public SqlCommand CreateCommand()
    {
        return _connection.CreateCommand();
    }

    public void Dispose()
    {
        if (_connection is not null)
        {
            _connection.Dispose();
            _connection = null;
        }
    }
}
