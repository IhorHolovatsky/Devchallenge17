For running app:

1. `docker-compose up -d mssql `

It's needed to build mssql service first, 
because it takes ~15 seconds to start up

2. wait 25 seconds
3. `docker-compose up`
4. wait 10-15 seconds
5. open http://localhost:8000/swagger

The reason why you need to wait 10-15 seconds is that API is making initialization stuff on start, and it requires mssql/redis connection.

For running unit tests (first time it could be slow, dure to package restoring + build):
1. `docker-compose exec tests dotnet test /app --filter "Category=integration"`
2. `docker-compose exec tests dotnet test /app --filter "Category=unit"`

Read documentation.docx file
Also, there are comments in code.


P.S:
redis-commander ui will be located here: 
http://localhost:8081/ 
user: root
pass: qwerty

