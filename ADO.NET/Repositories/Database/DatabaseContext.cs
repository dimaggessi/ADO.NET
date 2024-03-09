using System.Data;
using System.Data.SqlClient;

namespace ADO.NET.Repositories.DatabaseConnection;


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
public class DatabaseContext
{
    private IDbConnection _connection;

    public DatabaseContext()
    {
        // SqlCredential dentro de SqlConnection seria basicamente usuário e senha
        // SqlConnection varia de acordo com o Banco de Dados que vai ser utilizado
        // por exemplo, poderia ser MySqlConnection
        _connection = new SqlConnection(@"Data Source = DRACULA\MSSQLSERVER01; Initial Catalog = ADO.NET; Integrated Security = True;");
    }
}
