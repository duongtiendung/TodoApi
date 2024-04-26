to run docker database
cd TodoApi 
docker-compose up

Set your connection string to your local database by running this command:
 `dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=demo;Uid=root;Pwd=root;"` 
update migration to database run this command:
 `dotnet ef database update`
