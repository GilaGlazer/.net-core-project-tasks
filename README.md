# ASP.NET MVC Web API Project

This project is an ASP.NET MVC Web API for managing users and items with role-based authorization, JSON file storage, and basic logging.

---

## 🚀 Features

- JWT-based login (`/api/login`)
- Role-based authorization: `User` and `Admin`
- Users can only manage their own items
- Admins can manage all users and items
- Data is stored in JSON files
- Services accessed via interfaces for easy future database integration
- Request logging with timestamp, controller/action names, user, and request duration

---

## 📖 API Endpoints

| URL                | Method | Authorization | Description                 | Request Body     | Response Body        |
|--------------------|--------|---------------|-----------------------------|------------------|----------------------|
| `/api/item`        | GET    | User/Admin    | Get all items of current user | -                | List of items         |
| `/api/item/{id}`   | GET    | User/Admin    | Get a specific item by ID   | -                | Item                  |
| `/api/item`        | POST   | User/Admin    | Add a new item for user     | Item             | Created item + location|
| `/api/item/{id}`   | PUT    | User/Admin    | Update a user's item        | Item             | Updated item          |
| `/api/item/{id}`   | DELETE | User/Admin    | Delete a user's item        | -                | -                     |
| `/api/user`        | GET    | User/Admin    | Get current user's info     | -                | User                   |
| `/api/user`        | GET    | Admin         | Get all users (admin only)  | -                | List of users          |
| `/api/user`        | POST   | Admin         | Add new user (admin only)   | User             | Created user + location|
| `/api/user/{id}`   | DELETE | Admin         | Delete user and their items | -                | -                      |
| `/api/login`       | POST   | No auth needed| Login and get JWT token     | Login credentials| JWT token               |

---

## 🔐 Authorization Rules

- **Admin**:
  - Can add/delete users
  - Can view and edit all users and items
- **Regular User**:
  - Can manage only their own items
  - Can edit only their own user details
  - Cannot elevate own privileges to Admin

---

## 💾 Data Storage

- User and item data is stored in JSON files
- Data access is done through services implementing interfaces (e.g., `IUserService`, `IItemService`)
- Services are registered via Dependency Injection for flexibility

---

## 📝 Logging

- Every API request is logged with:
  - Start date and time
  - Controller and action names
  - Logged-in username (if any)
  - Operation duration in milliseconds

---

## 🖥️ Client-Side Behavior

- Default page shows the logged-in user's items with options to add, update, and delete
- If no token or token expired, user is redirected to the login page
- Admin users see navigation to users list page from the items list

---

## ⚠️ Important Notes

- **No password encryption** — passwords are stored as plain text (for demo/testing only)
- **No Google or external OAuth login**
- Easy to extend with real DB, encrypted passwords, and external login providers

---

## 🏁 Getting Started

1. Configure `appsettings.json` (JSON files path, logging path, etc.)
2. Run the project
3. Use Postman or your frontend to test API endpoints

---

## 📂 Suggested Project Structure

/Controllers
/Models
/Interfaces
/Services
/Middleware
/Data
/Logs

---

## 🤝 Contribution

Feel free to open issues or pull requests for improvements or questions.

---

