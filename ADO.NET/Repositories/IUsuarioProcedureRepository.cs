using ADO.NET.Models;

namespace ADO.NET.Repositories;

public interface IUsuarioProcedureRepository
{
    public List<Usuario> Get();
    public Usuario Get(int id);
    public void Create(Usuario usuario);
    public void Update(Usuario usuario);
    public void Delete(int id);
}
