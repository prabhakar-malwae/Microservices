// CustomerRepository.cs
using Dapper;
using Microservice;
using Npgsql;

namespace Microservice
{

    public class CustomerRepository
    {
        private readonly string _connectionString;

        public CustomerRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IEnumerable<Customer> GetAllCustomers()
        {
            using var connection = new NpgsqlConnection(_connectionString);
            return connection.Query<Customer>("SELECT \"ID\", \"Name\" FROM public.\"Customer\";").ToList();
        }
        public bool InsertCustomer(Customer customer)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            var sql = "INSERT INTO public.\"Customer\" (\"Name\") VALUES (@Name);";
            int rowsEffected = connection.Execute(sql, new { Name = customer.Name });
            return rowsEffected > 0;
        }
        public Customer GetCustomerById(int Id)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            var result = connection.QueryFirstOrDefault<Customer>("SELECT \"ID\", \"Name\" FROM public.\"Customer\" WHERE \"ID\" = @Id;", new { Id });
            return result ?? new Customer(); // Add null check and return a default Customer object if result is null
        }
    }
}