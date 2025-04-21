# Trello-Like App

## Overview

This is a ongoing basic Trello-like application developed with .NET 8 and Azure Functions (Solated model). The purpose of this project is to showcase my code and experiment with technologies that I cannot use at work.

### Tech Stack

- .NET 8 - Core framework for the application
- Azure Functions (in-process) - Serverless computing for handling tasks
- OpenTelemetry - Distributed tracing for performance monitoring
- Cosmos DB (NoSQL) - Scalable and flexible database
- Redis (L1 and L2 Cache) - Optimizing performance with caching layers
- FluentResult - Handling and propagating errors elegantly
- FluentValidation - Validating input data
- xUnit - Unit testing framework
- Jaeger - Tracing and debugging tool

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

### Future Improvements

 - Optimized queries and indexing in Cosmos DB
 - Advanced filtering and aggregations
 - More advanced analytics with OpenTelemetry
 - Machine model predictions to fit the best users to their tasks 

This project serves as a sandbox for testing and improving my skills in cloud-based, distributed, and high-performance applications.
