# RobertsRobotBar

RobertsRobotBar is a WPF application that controls a Universal Robots robotic arm to automatically mix drinks.
The application is designed for use at events and acts as a complete solution for both robot control and management of events, bar setups, and data.

The system makes it possible to plan events, configure bar setups, manage drinks, and control the robot that executes the drink tasks.

---

## Functionality

* Execution of robot scripts through a queue (sequential execution)
* Event-driven control of the robot (no polling)
* Creation and management of events
* Stored bar setups and configurations
* Management of drinks and recipes
* Support for custom (“mix your own”) drinks
* Collection of data and statistics for events
* Integration with Universal Robots via TCP

---

## Technology

The project is developed using:

* C# and .NET (WPF)
* Clean Architecture and MVVM
* Entity Framework Core
* SQL database
* TCP communication with the robot
* NUnit and Moq for testing
* Dependency Injection

---

## Architecture

The project is structured using Clean Architecture with a focus on clear separation of responsibilities and low coupling between layers.

### Presentation

* WPF (MVVM)
* UI and ViewModels
* Data binding and navigation handled through ViewModels

### Application Layer

* Contains business logic and application flow
* Coordinates between UI, domain, and infrastructure
* Uses services and interfaces to ensure loose coupling

### Domain

* Contains core entities such as events, drinks, and bar setups
* Designed with inspiration from Domain-Driven Design (DDD)
* Use of aggregates and relationships to ensure domain consistency
* Focus on clear modeling of business rules

### Infrastructure

* Data access using Entity Framework Core
* Repository implementations
* Robot communication via TCP

---

## Design Principles

The project is developed with a focus on:

* SOLID principles for better structure and maintainability
* Separation of concerns between layers
* Dependency Injection for flexibility and testability

---

## Data and Persistence

* Entity Framework Core is used for database management
* The domain model is designed with clear relationships, making migrations easier to work with
* Aggregates and entity relationships are used to ensure data consistency

---

The purpose of this structure is to make the system easier to maintain, test, and extend over time.
