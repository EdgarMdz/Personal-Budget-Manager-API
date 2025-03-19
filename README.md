# Personal Budget Manager API

Welcome to the **Personal Budget Manager API**, a project designed to showcase my software development skills for job opportunities. This API is built with a focus on clean architecture, maintainability, and scalability. It implements the MVC architecture, Separation of Concerns (SoC), and modern development practices to ensure a robust and flexible solution.

---

## Features

- **MVC Architecture**: Ensures a clear separation of responsibilities between Models, Views, and Controllers.
- **Repository and Unit of Work Patterns**: Simplifies database access and provides flexibility to switch databases if needed.
- **Entity Framework Core**: Uses SQL Server as the database provider with EF Core for ORM functionality.
- **Migrations**: Implements database migrations for version control and schema evolution.
- **Authentication and Authorization**: Implements JWT-based authentication for secure access to API endpoints.
- **Transactional Operations**: Ensures data consistency with transactional support for critical operations.
- **Lazy Loading**: Enabled for efficient data retrieval and navigation of related entities.
- **Error Handling**: Centralized error handling for better debugging and user feedback.
- **Dependency Injection**: Fully utilizes DI for better testability and modularity.
- **RESTful API Design**: Follows REST principles for resource management and HTTP methods.

---

## Design Decisions

This project includes certain patterns and implementations that may not be strictly necessary for a simple budget management system. These were deliberately included to demonstrate proficiency in key software development concepts, such as:

- **Clean Architecture**
- **Repository and Unit of Work Patterns**
- **Dependency Injection**
- **Security Best Practices**

These choices reflect an effort to build a scalable and maintainable system rather than just a basic CRUD application.

---

## Technologies Used

- **.NET 9.0**: The latest version of the .NET platform for building high-performance APIs.
- **Entity Framework Core 9.0.1**: For database access and management.
- **SQL Server**: The relational database used for storing application data.
- **JWT Authentication**: For secure user authentication and authorization.
- **XUnit**: For unit testing the application (under development).
- **Swagger/OpenAPI**: For API documentation and testing.

---

## Project Status

This project is **currently under development**. Some features are still being implemented, including:

- **Unit Tests**: Developed in a separate branch using the Factory Pattern to improve test setup and reusability.
- **Deployment**: The API will be deployed on **Azure Cloud** to provide a demo environment.
- **Additional Features**: More improvements and features will be added in future updates.

---

## Project Structure

The project follows a layered architecture:

- **Controllers**: Handles HTTP requests and responses.
- **Services**: Contains business logic and transactional operations.
- **Repositories**: Provides data access logic using the repository pattern.
- **DataContext**: Defines the database context, entity configurations, and migrations.
- **Models**: Contains DTOs and other data structures used across the application.

---

## Key Design Patterns 

- **Repository Pattern**: Abstracts database access logic, making it easier to switch databases or modify data access strategies.
- **Unit of Work Pattern**: Ensures that multiple database operations are executed within a single transaction.
- **Factory Pattern**: Used in testing (under development) to create reusable and consistent test setups.
- **Dependency Injection**: Promotes loose coupling and testability by injecting dependencies where needed.

---

## How to Run the Project

### 1️. Clone the Repository:
```sh
git clone https://github.com/EdgarMdz/Personal-Budget-Manager-API.git
cd PersonalBudgetManager
```

### 2️. Set Up the Database:
- Ensure you have **SQL Server** installed and running.
- Configure the connection string as an environment variable:
```sh
export PersonalBudgetManager_ConnectionString="Your SQL Server Connection String"
```

### 3️. Run Migrations:
```sh
dotnet ef database update
```

### 4️. Create the PFX Certificate:
```sh
openssl req -x509 -newkey rsa:4096 -keyout key.pem -out cert.pem -days 365
openssl pkcs12 -export -out PersonalBudgetManager.pfx -inkey key.pem -in cert.pem
```
- Set the certificate password as an environment variable:
```sh
export PersonalBudgetManagerCertPass="Your .pfx certificate password"
```
- Move the **PersonalBudgetManager.pfx** file to:
  ```
  ./PersonalBudgetManager.Api/resources/Certificates
  ```

### 5️. Run the Application:
```sh
dotnet run --project PersonalBudgetManager.Api
```

### 6️. Access Swagger UI:
Navigate to **https://localhost:443/swagger** to explore and test the API.

---

## API Endpoints

The API provides endpoints for managing:

- **Users**: Registration, login, and user management.
- **Categories**: CRUD operations for budget categories.
- **Incomes and Expenses**: Track and manage financial transactions.

---

## Testing

Unit tests are currently **under development** in a separate branch. These tests use the **Factory Pattern** to ensure consistency and reusability in test setups. Once completed, they will ensure the reliability of critical components.

To run the tests (when available):
```sh
dotnet test
```

---

## Future Enhancements

- **Code Documentation**: The code will be documented using XML comments to improve readability and maintainability. This is currently a work in progress.
- **Azure Deployment**: The API will be deployed on Azure Cloud for public access.
- **Role-Based Access Control**: More granular permissions for different user roles.
- **Caching**: Improve performance by caching frequently accessed data.
- **Logging and Monitoring**: Enhance logging and monitoring for production readiness.

---

## Why This Project?

This project demonstrates my ability to:

- **Design and implement scalable and maintainable APIs**  
- **Apply modern software development practices and patterns**  
- **Work with relational databases and ORM tools**  
- **Write clean, testable, and reusable code**  

---

## Contact

If you have any questions or would like to discuss this project further, feel free to reach out to me via **[LinkedIn](https://www.linkedin.com/in/your-profile)**.

Thank you for reviewing my project! I look forward to discussing how my skills can contribute to your team.

