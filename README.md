# DSPAssistant

a really cool bot thats written to run on the dsharpplus server because people are asking the same question just because discord did something stupid idk im bored

## how to run

With docker:
1. Build the Docker image
2. Use the docker-compose file to automatically deploy
  - dont forget to edit the DISCORD_TOKEN env variable or else it will explode (real)

Without docker:
- Set the following environment variables and then just run the project like any other .NET project
  - DISCORD_TOKEN: The token to connect to Discord
  - POSTGRES_USERNAME: Username to connect to the PostgreSQL server
  - POSTGRES_PASSWORD: Password to connect to the PostgreSQL server
  - POSTGRES_HOST: The hostname to the PostgreSQL server
  - POSTGRES_DB: PostgreSQL database name

## does it work?

as long as it doesnt break, yes it does work