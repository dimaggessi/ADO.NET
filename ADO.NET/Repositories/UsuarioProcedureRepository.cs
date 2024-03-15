using ADO.NET.Models;
using ADO.NET.Repositories.Database;
using System.Data;
using System.Data.SqlClient;

namespace ADO.NET.Repositories;

public class UsuarioProcedureRepository : IUsuarioProcedureRepository
{
    private readonly DatabaseConnection _connection;
    public UsuarioProcedureRepository(DatabaseConnection connection)
    {
        _connection = connection;
    }

    public void Create(Usuario usuario)
    {
        using DatabaseConnection dbConection = _connection;

        try
        {
            dbConection.Open();
            SqlCommand command = dbConection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "CadastrarUsuario";

            command.Parameters.AddWithValue("@Nome", usuario.Nome);
            command.Parameters.AddWithValue("@Email", usuario.Email);
            command.Parameters.AddWithValue("@Sexo", usuario.Sexo);
            command.Parameters.AddWithValue("@RG", usuario.RG);
            command.Parameters.AddWithValue("@CPF", usuario.CPF);
            command.Parameters.AddWithValue("@NomeMae", usuario.NomeMae);
            command.Parameters.AddWithValue("@SituacaoCadastro", usuario.SituacaoCadastro);
            command.Parameters.AddWithValue("@DataCadastro", usuario.DataCadastro);

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

    public void Delete(int id)
    {
        using var dbConnection = _connection;

        try
        {
            SqlCommand command = dbConnection.CreateCommand();
            command.CommandType= CommandType.StoredProcedure;
            command.CommandText = "DeletarUsuario";
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

    public List<Usuario> Get()
    {
        using var dbConnection = _connection;
        List<Usuario> usuarios = new List<Usuario>();

        try
        {
            dbConnection.Open();
            SqlCommand command = dbConnection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;

            command.CommandText = "SelecionarUsuarios";

            SqlDataReader dataReader = command.ExecuteReader();

            while (dataReader.Read())
            {
                Usuario usuario = new Usuario();

                usuario.Id = dataReader.GetInt32("Id");
                usuario.Nome = dataReader.GetString("Nome");
                usuario.Email = dataReader.GetString("Email");
                usuario.Sexo = dataReader.GetString("Sexo");
                usuario.RG = dataReader.GetString("RG");
                usuario.CPF = dataReader.GetString("CPF");
                usuario.NomeMae = dataReader.GetString("NomeMae");
                usuario.SituacaoCadastro = dataReader.GetString("SituacaoCadastro");
                usuario.DataCadastro = dataReader.GetDateTimeOffset(8);

                usuarios.Add(usuario);
            }
        }
        catch (Exception ex)
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
        using var dbConnection = _connection;
        
        try
        {
            dbConnection.Open();

            SqlCommand command = dbConnection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "SelecionarUsuario";
            command.Parameters.AddWithValue("@Id", id);

            SqlDataReader dataReader = command.ExecuteReader();

            while (dataReader.Read())
            {
                Usuario usuario = new()
                {
                    Id = dataReader.GetInt32("Id"),
                    Nome = dataReader.GetString("Nome"),
                    Email = dataReader.GetString("Email"),
                    Sexo = dataReader.GetString("Sexo"),
                    RG = dataReader.GetString("RG"),
                    CPF = dataReader.GetString("CPF"),
                    NomeMae = dataReader.GetString("NomeMae"),
                    SituacaoCadastro = dataReader.GetString("SituacaoCadastro"),
                    DataCadastro = dataReader.GetDateTimeOffset(8)
                };
                return usuario;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        finally
        {
            dbConnection.Close();
        }

        return null;
    }

    public void Update(Usuario usuario)
    {
        using DatabaseConnection dbConection = _connection;

        try
        {
            dbConection.Open();
            SqlCommand command = dbConection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "AtualizarUsuario";

            command.Parameters.AddWithValue("@Id", usuario.Id);
            command.Parameters.AddWithValue("@Nome", usuario.Nome);
            command.Parameters.AddWithValue("@Email", usuario.Email);
            command.Parameters.AddWithValue("@Sexo", usuario.Sexo);
            command.Parameters.AddWithValue("@RG", usuario.RG);
            command.Parameters.AddWithValue("@CPF", usuario.CPF);
            command.Parameters.AddWithValue("@NomeMae", usuario.NomeMae);
            command.Parameters.AddWithValue("@SituacaoCadastro", usuario.SituacaoCadastro);
            command.Parameters.AddWithValue("@DataCadastro", usuario.DataCadastro);

            command.ExecuteNonQuery();
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
}
