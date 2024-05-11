// CustomerRepository.cs
using Dapper;
using Microservice;
using Npgsql;

namespace Microservice
{

    public class UserRepository
    {
        private readonly string _connectionString;

        public UserRepository(string connectionString)
        {
            _connectionString = connectionString;
        }
        public User GetCustomerById(int Id)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            var result = connection.QueryFirstOrDefault<User>("SELECT \"ID\", \"Name\" FROM public.\"Customer\" WHERE \"ID\" = @Id;", new { Id });
            return result ?? new User(); // Add null check and return a default Customer object if result is null
        }
    }

    public class User
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }
}