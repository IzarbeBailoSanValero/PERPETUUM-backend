using MySqlConnector;
using PERPETUUM.DTOs;
using PERPETUUM.Models;
using System.Data;
using Microsoft.Extensions.Logging;
using BCrypt.Net;


using MySqlConnector;
using PERPETUUM.Models;



namespace PERPETUUM.Repositories;

public class UserRepository : IUserRepository
{
    private readonly string _connectionString;

    public UserRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("PerpetuumDB");
    }

   // MÉTODO LOGIN
    public UserDtoOut? GetUserFromCredentials(LoginDtoIn loginDto)
    {
        using (var connection = new MySqlConnection(_connectionString))
        {
            connection.Open();
            string query = "SELECT Id, Name, Email, PasswordHash FROM User WHERE Email = @Email";

            using (var command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Email", loginDto.Email);

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read()) 
                    {
                        var storedHash = reader.GetString("PasswordHash");

                        // Verificación con BCrypt --> Verify toma (passwordTextoPlano, hashGuardadoEnBD) --> Devuelve true si coinciden, false si no.
                        bool isValid = BCrypt.Net.BCrypt.Verify(loginDto.Password, storedHash);

                        if (isValid)
                        {
                            return new UserDtoOut
                            {
                                UserId = reader.GetInt32("Id"),
                                UserName = reader.GetString("Name"),
                                Email = reader.GetString("Email"),
                                Role = Roles.StandardUser // Rol por defecto
                            };
                        }
                    }
                }
            }
        }
        return null; //  si no existe email o password incorrecto
    }

   
    public UserDtoOut AddUserFromCredentials(UserDtoIn userDto)
    {
        using (var connection = new MySqlConnection(_connectionString))
        {
            connection.Open();

            //  Usamos BCrypt para hashear la contraseña
            string hash = BCrypt.Net.BCrypt.HashPassword(userDto.Password);
            
            string role = Roles.StandardUser; 

            string query = @"INSERT INTO User (Name, Email, PasswordHash) 
                             VALUES (@Name, @Email, @Pass);
                             SELECT LAST_INSERT_ID();";

            using (var command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Name", userDto.UserName);
                command.Parameters.AddWithValue("@Email", userDto.Email);
                command.Parameters.AddWithValue("@Pass", hash); 

                int newId = Convert.ToInt32(command.ExecuteScalar());

                return new UserDtoOut
                {
                    UserId = newId,
                    UserName = userDto.UserName,
                    Email = userDto.Email,
                    Role = role
                };
            }
        }
    }
    


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
        return new User
        {
            Id = reader.GetInt32("Id"),
            Name = reader.GetString("Name"),
            Email = reader.GetString("Email"),
            PasswordHash = reader.GetString("PasswordHash") 
            
        };
    }
    return null;
}

//TODO: QUITAR FUNCIÓN DE AÑADIR VIEJA
public async Task<int> CreateUserAsync(User user)
{
    using var connection = new MySqlConnection(_connectionString);
    await connection.OpenAsync();
    
    // Aquí YA NO hasheamos, asumimos que viene hasheada del servicio
    string query = @"INSERT INTO `User` (Name, Email, PasswordHash, CreatedDate) 
                     VALUES (@Name, @Email, @Pass, NOW());
                     SELECT LAST_INSERT_ID();";

    using var command = new MySqlCommand(query, connection);
    command.Parameters.AddWithValue("@Name", user.Name);
    command.Parameters.AddWithValue("@Email", user.Email);
    command.Parameters.AddWithValue("@Pass", user.PasswordHash);

    return Convert.ToInt32(await command.ExecuteScalarAsync());
}
    // ... Resto de métodos (GetById, etc.) ...
    //TODO::::::::
}