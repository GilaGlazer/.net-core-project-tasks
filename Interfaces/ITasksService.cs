using webApiProject.Models;
namespace webApiProject.Interfaces;

public interface IService
{

     List<Tasks> Get();
     Tasks Get(int id);
     int Insert(Tasks newItem);
     bool Update(int id, Tasks newItem);
     bool Delete(int id);
}
