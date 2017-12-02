# Budget Planner #

### What is this repository for? ###

This application is a simple budget planner written in ASP.NET Core. It contains a custom ASP.NET identity implementation using dapper to allow uses to register and log in to the site. There is a main budget page where categories can be managed and money can be added to the budget and assigned to each category. There is also a transactions page where transactions can be added and the budget will be updated accordingly. 

### How do I get set up? ###

To set up this project, first you must run the database script in the Database folder to create the database.

### How do I run the tests? ###

The solution contains two NUnit test projects. BudgetPlanner.IntegrationTests contains integration tests to test the data access layer with an in memory database. BudgetPlanner.Tests contains unit tests. 

No setup is required to run the integration tests as an in memory SQLite database is used to run the tests against.
