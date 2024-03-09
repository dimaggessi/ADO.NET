using ADO.NET.Models;

namespace ADO.NET.Repositories;

public interface IUsuarioRepository
{
    public List<Usuario> Get();
    public Usuario Get(int id);
    public void Create(Usuario user);
    public void Update(Usuario user);
    public void Delete(int id);
}
