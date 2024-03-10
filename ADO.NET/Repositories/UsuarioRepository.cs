using ADO.NET.Models;
using ADO.NET.Repositories.Database;
using System.Data;
using System.Data.SqlClient;

namespace ADO.NET.Repositories;

public class UsuarioRepository : IUsuarioRepository
{
    /*
     *       [ Classes Principais do ADO.NET ]
     * 
     * --------------------------------------------
     * : Estabelecem conexão com o Banco de dados :
     * --------------------------------------------
     * 
     *  IDbConnection, DbConnection, SqlConnection 
     *  
     * --------------------------------------------
     * : Enviar comandos para que sejam executados :
     * --------------------------------------------
     * 
     *  Command => INSERT, UPDATE, DELETE
     *  
     * --------------------------------------------
     * : Efetuar Consultas :
     * --------------------------------------------
     * 
     *  DataReader => utiliza arquitetura conectada
     *  
     *  DataAdapter => utiliza arquitetura desconectada 
     *      (guarda as informações na memória e desconecta)
     * --------------------------------------------
     */
    private readonly DatabaseConnection _connection;
    public UsuarioRepository(DatabaseConnection connection)
    {
        _connection = connection;
    }
    public List<Usuario> Get()
    {
        using var dbConnection = _connection;
        List<Usuario> usuarios = new List<Usuario>();

        string selectAll = "SELECT * FROM Usuarios";

        try
        {
            dbConnection.Open();

            // O provider SQL Server utiliza SqlCommand
            var command = dbConnection.CreateCommand();
            command.CommandText = selectAll;

            // [ DataReader ] ExecuteReader() arquitetura conectada
            SqlDataReader dataReader = command.ExecuteReader();

            // Read() apenas lê uma linha (é um ponteiro)
            // retorna true se houverem mais linhas a serem lidas
            while (dataReader.Read())
            {
                Usuario usuario = new Usuario();
                
                // o dataReader vai pegar o valor na coluna de título informado
                usuario.Id = dataReader.GetInt32("Id");
                usuario.Nome = dataReader.GetString("Nome");
                usuario.Email = dataReader.GetString("Email");
                usuario.Sexo = dataReader.GetString("Sexo");
                usuario.RG = dataReader.GetString("RG");
                usuario.CPF = dataReader.GetString("CPF");
                usuario.NomeMae = dataReader.GetString("NomeMae");
                usuario.SituacaoCadastro = dataReader.GetString("SituacaoCadastro");

                // o GetDateTimeOffset espera receber o número da coluna
                // lembrando que começa a contagem em zero (0)
                usuario.DataCadastro = dataReader.GetDateTimeOffset(8);

                // adiciona o usuário instanciado, na Lista Usuários
                usuarios.Add(usuario);
            };
        }
        catch(Exception ex) 
        {
            Console.WriteLine(ex.Message);
        }
        finally 
        { 
            dbConnection.Close();
        }

        return usuarios;
    }

    public Usuario Get(int id)
    {
        // Concatenação deixa o código vulnerável a SQL Injection:
        string select = $"SELECT * FROM Usuarios WHERE Id = @Id";


        using var dbConnection = _connection;

        Usuario usuario = new();

        try
        {
            dbConnection.Open();

            SqlCommand command = dbConnection.CreateCommand();
            command.CommandText = select;

            // Utiliza substituição de parâmetros para evitar SQL Injection
            command.Parameters.AddWithValue("Id", id);

            SqlDataReader dataReader = command.ExecuteReader();

            while (dataReader.Read())
            {
                usuario.Id = dataReader.GetInt32("Id");
                usuario.Nome = dataReader.GetString("Nome");
                usuario.Email = dataReader.GetString("Email");
                usuario.Sexo = dataReader.GetString("Sexo");
                usuario.RG = dataReader.GetString("RG");
                usuario.CPF = dataReader.GetString("CPF");
                usuario.NomeMae = dataReader.GetString("NomeMae");
                usuario.SituacaoCadastro = dataReader.GetString("SituacaoCadastro");
                usuario.DataCadastro = dataReader.GetDateTimeOffset(8);
            }
            return usuario;
        }
        catch(Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        finally
        {
            dbConnection.Close();
        }

        // caso não encontre o usuário, retorna nulo
        return null;
    }

    public void Create(Usuario usuario)
    {
        using var dbConection = _connection;
        string insert = "INSERT INTO Usuarios(Nome, Email, Sexo, RG, CPF, NomeMae, SituacaoCadastro, DataCadastro) " 
            + "VALUES (@Nome, @Email, @Sexo, @RG, @CPF, @NomeMae, @SituacaoCadastro, @DataCadastro);"
            + "SELECT CAST(scope_identity() AS int)";
        // ao final pega o ultimo Identity baseado no escopo de execução do Insert, para retornar ao usuário

        try
        {
            SqlCommand command = dbConection.CreateCommand();
            command.CommandText = insert;

            command.Parameters.AddWithValue("@Nome", usuario.Nome);
            command.Parameters.AddWithValue("@Email", usuario.Email);
            command.Parameters.AddWithValue("@Sexo", usuario.Sexo);
            command.Parameters.AddWithValue("@RG", usuario.RG);
            command.Parameters.AddWithValue("@CPF", usuario.CPF);
            command.Parameters.AddWithValue("@NomeMae", usuario.NomeMae);
            command.Parameters.AddWithValue("@SituacaoCadastro", usuario.SituacaoCadastro);
            command.Parameters.AddWithValue("@DataCadastro", usuario.DataCadastro);

            dbConection.Open();

            // ExecuteNonQuery() não retorna valores, apenas a quantidade de linhas afetadas
            // command.ExecuteNonQuery();

            // o Scalar executa a query e retorna a primeira coluna, da primeira linha, no resultado retornado pela query
            usuario.Id = (int)command.ExecuteScalar();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        finally 
        {
            dbConection.Close();
        }
    }

    public void Update(Usuario usuario)
    {
        using var dbConnection = _connection;
        string update = "UPDATE Usuarios SET Nome = @Nome, Email = @Email, Sexo = @Sexo, "
            + "RG = @RG, CPF = @CPF, NomeMae = @NomeMae WHERE Id = @Id";

        try
        {

            SqlCommand command = dbConnection.CreateCommand();
            command.CommandText = update;

            command.Parameters.AddWithValue("@Nome", usuario.Nome);
            command.Parameters.AddWithValue("@Email", usuario.Email);
            command.Parameters.AddWithValue("@Sexo", usuario.Sexo);
            command.Parameters.AddWithValue("@RG", usuario.RG);
            command.Parameters.AddWithValue("@CPF", usuario.CPF);
            command.Parameters.AddWithValue("@NomeMae", usuario.NomeMae);
            command.Parameters.AddWithValue("@SituacaoCadastro", usuario.SituacaoCadastro);
            command.Parameters.AddWithValue("@DataCadastro", usuario.DataCadastro);

            command.Parameters.AddWithValue("@Id", usuario.Id);

            dbConnection.Open();

            command.ExecuteNonQuery();

        }
        catch (Exception ex) 
        {
            Console.WriteLine(ex.Message);
        }
        finally
        {
            dbConnection.Close();
        }
    }

    public void Delete(int id)
    {
        using var dbConnection = _connection;
        string delete = "DELETE FROM Usuarios WHERE Id = @Id;";

        try
        {
            SqlCommand command = dbConnection.CreateCommand();
            command.CommandText = delete;
            command.Parameters.AddWithValue("@Id", id);

            dbConnection.Open();
            command.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        finally
        {
            dbConnection.Close();
        }
    }
}
