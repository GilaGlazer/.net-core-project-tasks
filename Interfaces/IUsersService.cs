using webApiProject.Models;
namespace webApiProject.Interfaces;

public interface IUsersService
{
     List<Users> Get();
     Users Get(int id);
     int Insert(Users newItem);
     bool Update(string password, Users newItem);
     bool Delete(string password);
}
