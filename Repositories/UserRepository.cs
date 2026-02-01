using MySqlConnector;
using PERPETUUM.Models;
using Microsoft.Extensions.Logging;
using System.Data; // Necesario para manejar DBNull

namespace PERPETUUM.Repositories;

public class UserRepository : IUserRepository
{
    private readonly string _connectionString;
    private readonly ILogger<UserRepository> _logger;

    public UserRepository(IConfiguration configuration, ILogger<UserRepository> logger)
    {
        _connectionString = configuration.GetConnectionString("PerpetuumDB") 
                            ?? throw new ArgumentNullException("No se encontró la cadena de conexión 'PerpetuumDB'.");
        _logger = logger;
    }


    //para login: 
    // - Devuelve el Modelo User (con el Hash)
    // - No verifica contraseña, solo busca datos --> verificación en el service
    public async Task<User?> GetByEmailAsync(string email)
    {
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        string query = "SELECT * FROM `User` WHERE Email = @Email";

        using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@Email", email);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapFromReader(reader);
        }
        return null;
    }


    // para registro --> Recibe el objeto User ya con la contraseña encriptada (PasswordHash). se encripta en el service.
    public async Task<int> CreateUserAsync(User user)
    {
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        string query = @"INSERT INTO `User` (Name, Email, PasswordHash, PhoneNumber, BirthDate) 
                         VALUES (@Name, @Email, @Pass, @Phone, @Birth);
                         SELECT LAST_INSERT_ID();";

        using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@Name", user.Name);
        command.Parameters.AddWithValue("@Email", user.Email);
        command.Parameters.AddWithValue("@Pass", user.PasswordHash);
        command.Parameters.AddWithValue("@Phone", (object?)user.PhoneNumber ?? DBNull.Value);
        command.Parameters.AddWithValue("@Birth", (object?)user.BirthDate ?? DBNull.Value);

        var idGenerado = await command.ExecuteScalarAsync();
        return Convert.ToInt32(idGenerado); //hay que hacer convert por ue devuelve un tipo de nuemero grande que no se puede castear diredctamente con (int)
    }

   
    private User MapFromReader(MySqlDataReader reader)
    {
        return new User
        {
            Id = reader.GetInt32("Id"),
            Name = reader.GetString("Name"),
            Email = reader.GetString("Email"),
            PasswordHash = reader.GetString("PasswordHash"),
            PhoneNumber = reader.IsDBNull(reader.GetOrdinal("PhoneNumber")) ? null : reader.GetString("PhoneNumber"),
            BirthDate = reader.IsDBNull(reader.GetOrdinal("BirthDate")) ? null : reader.GetDateTime("BirthDate")
        };
    }
}