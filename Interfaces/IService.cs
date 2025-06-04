namespace webApiProject.Interfaces;

public interface IService<T> where T : class
{
     List<T> Get();
    // List<T> GetAllItems();
     T Get(int id);
     int Insert(T newItem);
     bool Update(int id, T newItem);
     bool Delete(int id);
}
