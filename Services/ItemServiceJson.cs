using Microsoft.AspNetCore.Mvc;
using webApiProject.Interfaces;
using webApiProject.Models;

namespace webApiProject.Services;

using System.Text.Json;
using System.Text.RegularExpressions;

public class ItemServiceJson<T> : IService<T>
    where T : class, IIdentifiable, new()
{
    private readonly ActiveUserService activeUserService;
    private readonly Func<IService<Users>> usersServiceFactory;

    private List<T> itemList;
    private static string fileName = typeof(T).Name + ".json";
    private string filePath;

    public ItemServiceJson(
        IHostEnvironment env,
        ActiveUserService activeUserService,
        Func<IService<Users>> usersServiceFactory
    )
    {
        try
        {
            this.activeUserService = activeUserService;
            this.usersServiceFactory = usersServiceFactory;
            filePath = Path.Combine(env.ContentRootPath, "Data", fileName);
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"File {filePath} does not exist. Creating a new empty list.");
                itemList = new List<T>();
                saveToFile();
                return;
            }
            using (var jsonFile = File.OpenText(filePath))
            {
                itemList =
                    JsonSerializer.Deserialize<List<T>>(
                        jsonFile.ReadToEnd(),
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    ) ?? new List<T>();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error initializing ItemServiceJson: {ex.Message}");
            itemList = new List<T>();
        }
    }

    private void saveToFile()
    {
        try
        {
            File.WriteAllText(filePath, JsonSerializer.Serialize(itemList));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving to file: {ex.Message}");
            throw;
        }
    }

    public List<T> Get()
    {
        try
        {
            if (activeUserService.Type == "admin")
            {
                return itemList;
            }
            return itemList.Where(i => i.UserId == activeUserService.UserId).ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in Get: {ex.Message}");
            return new List<T>();
        }
    }

    public T Get(int id)
    {
        try
        {
            if (activeUserService.Type == "admin")
            {
                return itemList.FirstOrDefault(i => i.Id == id);
            }
            return itemList.FirstOrDefault(i => i.Id == id && i.UserId == activeUserService.UserId);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in Get by ID: {ex.Message}");
            return null;
        }
    }

    public int Insert(T newItem)
    {
        try
        {
            if (!CheckValueRequest(newItem))
                return -1;
            if (activeUserService.Type == "admin")
            {
                if (string.IsNullOrEmpty(newItem.UserId.ToString()))
                    return -1;
                var usersService = usersServiceFactory();
                var user = usersService.Get(newItem.UserId);
                if (user == null)
                    return -1;
            }
            else
                newItem.UserId = activeUserService.UserId;
            int lastId = itemList.Any() ? itemList.Max(s => s.Id) : 0;
            newItem.Id = lastId + 1;
            itemList.Add(newItem);
            saveToFile();
            return newItem.Id;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in Insert: {ex.Message}");
            return -1;
        }
    }

   public bool Update(int id, T newItem)
    {
        try
        {
            if (!CheckValueRequest(newItem) || newItem.Id != id)
                return false;
            var item = itemList.FirstOrDefault(s => s.Id == id);
            if (item == null)
                return false;

            // אם המשתמש הוא מנהל, אל תאפשר לו לעדכן פריט של משתמש אחר
            // if (activeUserService.Type == "admin" && item.UserId != newItem.UserId)
            // {
            //     Console.WriteLine("Admin cannot update items of other users.");
            //     return false;
            // }
            if (activeUserService.Type != "admin")
                newItem.UserId = activeUserService.UserId;
            else
            {
                var usersService = usersServiceFactory();
                var user = usersService.Get(newItem.UserId);
                if (user == null)
                    return false;
            }
            if (activeUserService.Type != "admin")
                newItem.UserId = activeUserService.UserId;
            else
            {
                var usersService = usersServiceFactory();
                var user = usersService.Get(newItem.UserId);
                if (user == null)
                    return false;
            }

            foreach (var property in typeof(T).GetProperties())
            {
                // if (property.CanWrite)
                // {
                //     var newValue = property.GetValue(newItem);
                //     property.SetValue(item, newValue);
                // }
                if (!property.CanWrite || property.Name == "CreatedAt")
                    continue; // דלג על CreatedAt או על שדות שאי אפשר לכתוב

                var newValue = property.GetValue(newItem);
                property.SetValue(item, newValue);
            }
            saveToFile();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in Update: {ex.Message}");
            return false;
        }
    }

    public bool Delete(int id)
    {
        System.Console.WriteLine("in delete item service begin");
        try
        {
            var item = itemList.FirstOrDefault(s => s.Id == id);
            // בדיקה אם המשתמש הוא בעל הפריט או מנהל
            if (
                item == null
                || (activeUserService.Type != "admin" && item.UserId != activeUserService.UserId)
            )
                return false;

            itemList.Remove(item);
            saveToFile();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in Delete: {ex.Message}");
            return false;
        }
    }

    // private bool CheckValueRequest(T newItem)
    // {
    //     try
    //     {
    //         if (newItem == null)
    //             return false;

    //         var propertiesToCheck = new Dictionary<string, Func<object, bool>>
    //         {
    //             { "Name", value => !string.IsNullOrWhiteSpace(value?.ToString()) },
    //             { "Color", value => !string.IsNullOrWhiteSpace(value?.ToString()) },
    //             { "Size", value => value is int size && size >= 18 && size < 50 },
    //             {
    //                 "Email",
    //                 value =>
    //                     !string.IsNullOrWhiteSpace(value?.ToString())
    //                     && IsValidEmail(value.ToString())
    //             },
    //             { "Password", value => !string.IsNullOrWhiteSpace(value?.ToString()) },
    //         };

    //         foreach (var propertyCheck in propertiesToCheck)
    //         {
    //             var property = typeof(T).GetProperty(propertyCheck.Key);
    //             if (property != null)
    //             {
    //                 var value = property.GetValue(newItem);
    //                 if (!propertyCheck.Value(value))
    //                     return false;
    //             }
    //         }

    //         return true;
    //     }
    //     catch (Exception ex)
    //     {
    //         Console.WriteLine($"Error in CheckValueRequest: {ex.Message}");
    //         return false;
    //     }
    // }
private bool CheckValueRequest(T newItem)
{
    try
    {
        if (newItem == null)
            return false;

        var propertiesToCheck = new Dictionary<string, Func<object, bool>>
        {
            // Title לא ריק
            { "Title", value => !string.IsNullOrWhiteSpace(value?.ToString()) },

            // Priority בין 1 ל-3
            { "Priority", value => value is int p && p >= 1 && p <= 3 },

            // Status תקף לערך של TaskStatus enum
            { "Status", value => Enum.IsDefined(typeof(TaskStatus), value) },

            // UserId גדול מ-0
            { "UserId", value => value is int uid && uid > 0 },

            // CreatedAt תאריך תקין, לא בעתיד
            { "CreatedAt", value => value is DateTime dt && dt <= DateTime.Now }
        };

        foreach (var propertyCheck in propertiesToCheck)
        {
            var property = typeof(T).GetProperty(propertyCheck.Key);
            if (property != null)
            {
                var value = property.GetValue(newItem);
                if (!propertyCheck.Value(value))
                {
                    Console.WriteLine($"Invalid value for property '{propertyCheck.Key}': {value}");
                    return false;
                }
            }
        }

        return true;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error in CheckValueRequest: {ex.Message}");
        return false;
    }
}

    private bool IsValidEmail(string email)
    {
        try
        {
            if (string.IsNullOrEmpty(email))
                return false;

            string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, pattern);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in IsValidEmail: {ex.Message}");
            return false;
        }
    }

    public List<T> GetAllItems()
    {
        try
        {
            return itemList;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in Get: {ex.Message}");
            return new List<T>();
        }
    }
}

public static partial class ServiceUlilities
{
    public static IServiceCollection AddItemService(this IServiceCollection services)
    {
        services.AddScoped<IService<Users>, UsersServiceJson>();
        return services;
    }
}
