using ADO.NET.Models;
using ADO.NET.Repositories.Database;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Transactions;

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

            // O provider SQL Server utiliza SqlCommand.
            var command = dbConnection.CreateCommand();
            command.CommandText = selectAll;

            // [ DataReader ] ExecuteReader() arquitetura conectada.
            SqlDataReader dataReader = command.ExecuteReader();

            // Read() apenas lê uma linha (é um ponteiro)
            // retorna true se houverem mais linhas a serem lidas.
            while (dataReader.Read())
            {
                Usuario usuario = new Usuario();
                
                // o dataReader vai pegar o valor na coluna de título informado.
                usuario.Id = dataReader.GetInt32("Id");
                usuario.Nome = dataReader.GetString("Nome");
                usuario.Email = dataReader.GetString("Email");
                usuario.Sexo = dataReader.GetString("Sexo");
                usuario.RG = dataReader.GetString("RG");
                usuario.CPF = dataReader.GetString("CPF");
                usuario.NomeMae = dataReader.GetString("NomeMae");
                usuario.SituacaoCadastro = dataReader.GetString("SituacaoCadastro");

                // o GetDateTimeOffset espera receber o número da coluna
                // lembrando que começa a contagem em zero (0).
                usuario.DataCadastro = dataReader.GetDateTimeOffset(8);

                // adiciona o usuário instanciado, na Lista Usuários.
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
        // Concatenação deixa o código vulnerável a SQL Injection: (usar Parameters)
        // LEFT JOIN afirma que sempre vai trazer um usuário, ainda que Contatos seja null.
        string select = $"SELECT *, "
            + "c.Id ContatoId, e.Id EnderecoId, d.Nome NomeDepartamento "
            + "FROM Usuarios AS u "
            + "LEFT JOIN Contatos AS c ON c.UsuarioId = u.Id " 
            + "LEFT JOIN EnderecosEntrega e ON u.Id = e.UsuarioId "
            + "LEFT JOIN UsuariosDepartamentos AS ud ON ud.UsuarioId = u.Id "
            + "LEFT JOIN Departamentos AS d ON d.Id = ud.DepartamentoId "
            + "WHERE u.Id = @Id";

        using var dbConnection = _connection;

        try
        {
            dbConnection.Open();

            SqlCommand command = dbConnection.CreateCommand();
            command.CommandText = select;

            // Utiliza substituição de parâmetros para evitar SQL Injection.
            command.Parameters.AddWithValue("Id", id);

            SqlDataReader dataReader = command.ExecuteReader();

            Usuario usuario = null;
            Contato contato = null;
            Departamento departamento = null;

            while (dataReader.Read())
            {
                if (usuario is null)
                {
                    usuario = new()
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

                    contato = new()
                    {
                        Id = dataReader.GetInt32("ContatoId"),
                        UsuarioId = dataReader.GetInt32("UsuarioId"),
                        Telefone = dataReader.GetString("Telefone"),
                        Celular = dataReader.GetString("Celular")
                    };

                    usuario.Contato = contato;
                }

                EnderecoEntrega endereco = new()
                {
                    Id = dataReader.GetInt32("EnderecoId"),
                    UsuarioId = dataReader.GetInt32("UsuarioId"),
                    NomeEndereco = dataReader.GetString("NomeEndereco"),
                    CEP = dataReader.GetString("CEP"),
                    Estado = dataReader.GetString("Estado"),
                    Cidade = dataReader.GetString("Cidade"),
                    Bairro = dataReader.GetString("Bairro"),
                    Endereco = dataReader.GetString("Endereco"),
                    Numero = dataReader.GetString("Numero"),
                    Complemento = dataReader.GetString("Complemento")
                };          

                usuario.EnderecosEntrega ??= new List<EnderecoEntrega>();

                if (!usuario.EnderecosEntrega.Any(end => end.Id == endereco.Id))
                    usuario.EnderecosEntrega.Add(endereco);

                usuario.Departamentos ??= new List<Departamento>();

                departamento = new()
                {
                    Id = dataReader.GetInt32("DepartamentoId"),
                    Nome = dataReader.GetString("NomeDepartamento")
                };

                if (!usuario.Departamentos.Any(dep => dep.Id == departamento.Id))
                    usuario.Departamentos.Add(departamento);
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

        // caso não encontre o usuário, retorna nulo.
        return null;
    }

    public void Create(Usuario usuario)
    {
        using DatabaseConnection dbConection = _connection;
        dbConection.Open();

        // Inicia uma transaction (ACID).
        SqlTransaction transaction = dbConection.BeginTransaction();

        try
        {
            SqlCommand command = dbConection.CreateCommand();
            command.Transaction = transaction;

            #region Usuario
            // SELECT CAST(scope_identity() AS int)
            // ao final pega o ultimo Identity baseado no escopo de execução do Insert, para retornar ao usuário
            command.CommandText = "INSERT INTO Usuarios(Nome, Email, Sexo, RG, CPF, NomeMae, SituacaoCadastro, DataCadastro) "
            + "VALUES (@Nome, @Email, @Sexo, @RG, @CPF, @NomeMae, @SituacaoCadastro, @DataCadastro);"
            + "SELECT CAST(scope_identity() AS int)";

            command.Parameters.AddWithValue("@Nome", usuario.Nome);
            command.Parameters.AddWithValue("@Email", usuario.Email);
            command.Parameters.AddWithValue("@Sexo", usuario.Sexo);
            command.Parameters.AddWithValue("@RG", usuario.RG);
            command.Parameters.AddWithValue("@CPF", usuario.CPF);
            command.Parameters.AddWithValue("@NomeMae", usuario.NomeMae);
            command.Parameters.AddWithValue("@SituacaoCadastro", usuario.SituacaoCadastro);
            command.Parameters.AddWithValue("@DataCadastro", usuario.DataCadastro);

            
            // command.ExecuteNonQuery(); não retorna valores, apenas a quantidade de linhas afetadas.
            // command.ExecuteScalar(); executa a query e retorna a primeira coluna, da primeira linha, no resultado retornado pela query.
            usuario.Id = (int)command.ExecuteScalar();
            #endregion

            #region Contatos
            command.CommandText = "INSERT INTO Contatos (UsuarioId, Telefone, Celular) "
                + "VALUES (@UsuarioId, @Telefone, @Celular); "
                + "SELECT CAST(scope_identity() AS int)";

            command.Parameters.AddWithValue("@UsuarioId", usuario.Id);
            command.Parameters.AddWithValue("@Telefone", usuario.Contato.Telefone);
            command.Parameters.AddWithValue("@Celular", usuario.Contato.Celular);

            usuario.Contato.UsuarioId = usuario.Id;
            usuario.Contato.Id = (int)command.ExecuteScalar();
            #endregion

            #region Enderecos
            foreach (var endereco in usuario.EnderecosEntrega)
            {
                command = dbConection.CreateCommand();
                command.Transaction = transaction;

                command.CommandText = "INSERT INTO EnderecosEntrega "
                    + "(UsuarioId, NomeEndereco, CEP, Estado, Cidade, Bairro, Endereco, Numero, Complemento) "
                    + "VALUES "
                    + "(@UsuarioId, @NomeEndereco, @CEP, @Estado, @Cidade, @Bairro, @Endereco, @Numero, @Complemento); "
                    + "SELECT CAST(scope_identity() AS int)";

                command.Parameters.AddWithValue("@UsuarioId", usuario.Id);
                command.Parameters.AddWithValue("@NomeEndereco", endereco.NomeEndereco);
                command.Parameters.AddWithValue("@CEP", endereco.CEP);
                command.Parameters.AddWithValue("@Estado", endereco.Estado);
                command.Parameters.AddWithValue("@Cidade", endereco.Cidade);
                command.Parameters.AddWithValue("@Bairro", endereco.Bairro);
                command.Parameters.AddWithValue("@Endereco", endereco.Endereco);
                command.Parameters.AddWithValue("@Numero", endereco.Numero);
                command.Parameters.AddWithValue("@Complemento", endereco.Complemento);

                endereco.Id = (int)command.ExecuteScalar();
                endereco.UsuarioId = usuario.Id;
            }
            #endregion

            #region Departamentos
            foreach (var departamento in usuario.Departamentos) 
            {
                command = dbConection.CreateCommand();
                command.Transaction = transaction;

                command.CommandText = "INSERT INTO UsuariosDepartamentos "
                    + "(UsuarioId, DepartamentoId) "
                    + "VALUES "
                    + "(@UsuarioId, @DepartamentoId);";

                command.Parameters.AddWithValue("@UsuarioId", usuario.Id);
                command.Parameters.AddWithValue("@DepartamentoId", departamento.Id);

                command.ExecuteNonQuery();
            }
            #endregion

            // Termina a transaction com sucesso.
            transaction.Commit();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            try
            {
                // Efetua o Rollback da transaction em caso de erros.
                transaction.Rollback();
            }
            catch (InvalidOperationException e)
            {
                Console.WriteLine($"Error: {e.Message}");
                throw new Exception();
            }
        }
        finally 
        {
            dbConection.Close();
        }
    }

    public void Update(Usuario usuario)
    {
        using DatabaseConnection dbConnection = _connection;
        dbConnection.Open();
        SqlTransaction transaction = dbConnection.BeginTransaction();

        try
        {
            SqlCommand command = dbConnection.CreateCommand();
            command.Transaction = transaction;

            #region Usuario
            command.CommandText = "UPDATE Usuarios SET Nome = @Nome, Email = @Email, Sexo = @Sexo, "
            + "RG = @RG, CPF = @CPF, NomeMae = @NomeMae WHERE Id = @Id";

            command.Parameters.AddWithValue("@Nome", usuario.Nome);
            command.Parameters.AddWithValue("@Email", usuario.Email);
            command.Parameters.AddWithValue("@Sexo", usuario.Sexo);
            command.Parameters.AddWithValue("@RG", usuario.RG);
            command.Parameters.AddWithValue("@CPF", usuario.CPF);
            command.Parameters.AddWithValue("@NomeMae", usuario.NomeMae);
            command.Parameters.AddWithValue("@SituacaoCadastro", usuario.SituacaoCadastro);
            command.Parameters.AddWithValue("@DataCadastro", usuario.DataCadastro);

            command.Parameters.AddWithValue("@Id", usuario.Id);

            command.ExecuteNonQuery();
            #endregion

            #region Contatos
            command = dbConnection.CreateCommand();
            command.Transaction = transaction;

            command.CommandText = "UPDATE Contatos SET "
                + "UsuarioId = @UsuarioId, Telefone = @Telefone, Celular = @Celular "
                + "WHERE Id = @Id";

            command.Parameters.AddWithValue("@UsuarioId", usuario.Id);
            command.Parameters.AddWithValue("@Telefone", usuario.Contato.Telefone);
            command.Parameters.AddWithValue("@Celular", usuario.Contato.Celular);
            command.Parameters.AddWithValue("@Id", usuario.Contato.Id);

            command.ExecuteNonQuery();
            #endregion

            #region Enderecos
            command = dbConnection.CreateCommand();
            command.Transaction = transaction;

            command.CommandText = "DELETE FROM EnderecosEntrega WHERE UsuarioId = @UsuarioId; ";
            command.Parameters.AddWithValue("@UsuarioId", usuario.Id);

            command.ExecuteNonQuery();

            foreach (var endereco in usuario.EnderecosEntrega)
            {
                command = dbConnection.CreateCommand();
                command.Transaction = transaction;

                command.CommandText = "INSERT INTO EnderecosEntrega "
                    + "(UsuarioId, NomeEndereco, CEP, Estado, Cidade, Bairro, Endereco, Numero, Complemento) "
                    + "VALUES "
                    + "(@UsuarioId, @NomeEndereco, @CEP, @Estado, @Cidade, @Bairro, @Endereco, @Numero, @Complemento); "
                    + "SELECT CAST(scope_identity() AS int)";

                command.Parameters.AddWithValue("@UsuarioId", usuario.Id);
                command.Parameters.AddWithValue("@NomeEndereco", endereco.NomeEndereco);
                command.Parameters.AddWithValue("@CEP", endereco.CEP);
                command.Parameters.AddWithValue("@Estado", endereco.Estado);
                command.Parameters.AddWithValue("@Cidade", endereco.Cidade);
                command.Parameters.AddWithValue("@Bairro", endereco.Bairro);
                command.Parameters.AddWithValue("@Endereco", endereco.Endereco);
                command.Parameters.AddWithValue("@Numero", endereco.Numero);
                command.Parameters.AddWithValue("@Complemento", endereco.Complemento);

                endereco.Id = (int)command.ExecuteScalar();
                endereco.UsuarioId = usuario.Id;
            }
            #endregion

            #region Departamentos
            command = dbConnection.CreateCommand();
            command.Transaction = transaction;

            command.CommandText = "DELETE FROM UsuariosDepartamentos WHERE UsuarioId = @UsuarioId; ";
            command.Parameters.AddWithValue("@UsuarioId", usuario.Id);

            command.ExecuteNonQuery();

            foreach (var departamento in usuario.Departamentos)
            {
                command = dbConnection.CreateCommand();
                command.Transaction = transaction;

                command.CommandText = "INSERT INTO UsuariosDepartamentos "
                    + "(UsuarioId, DepartamentoId) "
                    + "VALUES "
                    + "(@UsuarioId, @DepartamentoId);";

                command.Parameters.AddWithValue("@UsuarioId", usuario.Id);
                command.Parameters.AddWithValue("@DepartamentoId", departamento.Id);

                command.ExecuteNonQuery();
            }
            #endregion

            // Termina a transaction com sucesso.
            transaction.Commit();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            try
            {
                // Efetua o Rollback da transaction em caso de erros.
                transaction.Rollback();
            }
            catch (InvalidOperationException e)
            {
                Console.WriteLine($"Error: {e.Message}");
                throw new Exception();
            }
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
