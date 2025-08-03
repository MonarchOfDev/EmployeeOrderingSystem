# 🍽️ Employee Cafeteria Credit & Ordering System

A full-stack ASP.NET Core MVC web application developed as part of a technical assessment for a Senior Developer role. The system manages cafeteria credits, bonus deposits, restaurant menus, food orders, and deliveries for employees.

---

## 📌 Project Objective

To encourage healthy eating, employees can deposit money into their cafeteria account and receive bonus credits. The application allows employees to:
* Manage deposits and view bonus-based balances.
* Browse menus from restaurants in Braamfontein.
* Place orders and track their delivery status.

Administrators can:
* Manage employees, restaurants, and menu items.
* Track and update delivery statuses for placed orders.

---

## ⚙️ Technologies Used

* **Framework:** ASP.NET Core MVC (.NET 9)
* **Language:** C#
* **ORM:** Entity Framework Core
* **Database:** SQL Server LocalDB
* **Frontend:** Razor Views, Bootstrap
* **IDE:** Visual Studio / Visual Studio Code

---
## Setup Instructions

1. Clone the repository
git clone https://github.com/MonarchOfDev/EmployeeOrderingSystem.git                                                     

cd CafeteriaCreditSystem

2. Configure the database
Update the ConnectionStrings section in appsettings.json:
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=CafeteriaDb;Trusted_Connection=True;"
}

3. Apply migrations and seed data (if applicable)
update-database

4. Run the application

---------


-----
Notes
The project follows MVC architecture with clean separation of concerns.

Bonus deposit logic is robust and handles month transitions correctly.

Responsive views with Bootstrap for usability.

GitHub repo includes full source, DB migrations, and screenshots