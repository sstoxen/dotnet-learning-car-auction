To run docker for PostgreSQL:

- Navigate into service directory
- docker compose up -d

To run dotnet server for development:

- donet watch

When adding new service

- dotnet new webapi -o src/{new service name} -controllers
- - Creates skaffolding for new service in project.
- - -controllers adds a basic controller for you.
- dotnet sln add src/{new service name}/
- - Adds new service to the solution explorer

When adding db migrations

- dotnet ef migrations add {migration name}

To purge volume:

- docker volume remove {volume name}
- - docker volume remove carsties_pgdata
- - docker volume remove carsties_mongodata
- - - Will only run when it has no open connections.
