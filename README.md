# Trello-Like App

## Overview

This is a **ongoing** basic Trello-like application developed with .NET 8 and Azure Functions (Isolated model). The purpose of this project is to showcase my code and experiment with technologies that I cannot use at work.

### Tech Stack

- .NET 8 - Core framework for the application
- Azure Functions (Isolated model) - Serverless computing for handling tasks
- .NET API - A second approach for the same purpose of Azure Function one
- Kafka - To send a message when a task is assigned to an user.
- OpenTelemetry - Distributed tracing for performance monitoring
- Cosmos DB (NoSQL) - Scalable and flexible database
- Redis (L1 and L2 Cache) - Optimizing performance with caching layers
- FluentResult - Handling and propagating errors elegantly
- FluentValidation - Validating input data
- xUnit - Unit testing framework
- MailKit - To send emails
- Jaeger - Tracing and debugging tool
- Docker - Docket Compose is used to run Jaeger, Redis, Kafka and Zookeeper.
- API Versioning - To handle different versions of the API
- Rate Limiting - To limit the API calls
- Authentication and authorization - Google JWT is being used to handle these features
- Mediatr - To handle task notifications

### Features

- Create, update, delete, and list tasks (CRUD operations)
- Caching strategies with Redis for improved performance
- Distributed tracing with OpenTelemetry and Jaeger

### Setup and Installation

Clone the repository:
 - git clone <repository-url>
 - cd <repository-folder>

Install dependencies:
 - dotnet restore

Configure environment variables for Cosmos DB and Redis.
Run the application:
 - func start

### Testing

Run unit tests with:
 - dotnet test

### Endpoints

#### Tasks
 - GET	/Tasks	Get all tasks
 - POST	/Tasks	Create a new task
 - PUT	/Tasks	Update a task
 - DELETE	/Tasks	Delete all tasks
 - GET	/Tasks/{id}	Get a task by ID
 - DELETE	/Tasks/{id}	Delete a task by ID
 - GET	/Tasks/assignedTo/{email}	Get tasks assigned to a specific user
 - PATCH	/Tasks/{id}/assign/{email}	Assign a task to a user

#### Users
 - GET	/Users	Get all users
 - DELETE	/Users	Delete all users
 - GET	/Users/{email}	Get a user by email

### Future Improvements

 - Optimized queries and indexing in Cosmos DB
 - Advanced filtering and aggregations
 - Finalizing basic features
 - Add more unit tests
 - More advanced analytics with OpenTelemetry
 - Machine model predictions to fit the best users to their tasks 

This project serves as a sandbox for testing and improving my skills in cloud-based, distributed, and high-performance applications.
