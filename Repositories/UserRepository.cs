using MySqlConnector;
using PERPETUUM.DTOs;
using PERPETUUM.Models;
using PERPETUUM.Repositories;

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
            // Buscamos por Email en la tabla USER
            string query = "SELECT Id, Name, Email, PasswordHash, Role FROM User WHERE Email = @Email";

            using (var command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Email", loginDto.Email);

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read()) // Si encontramos el email...
                    {
                        var storedHash = reader.GetString("PasswordHash");

                        // --- VERIFICACIÓN DE PASSWORD ---
                        // AHORA: Hash falso para probar
                        // FUTURO: bool isValid = BCrypt.Net.BCrypt.Verify(loginDto.Password, storedHash);
                        string inputHash = "HASH_FALSO_" + loginDto.Password; 
                        
                        // IMPORTANTE: Si usas datos reales sin hashear en DB, compara directo:
                        // bool isValid = (loginDto.Password == storedHash); 
                        
                        bool isValid = (storedHash == inputHash); 

                        if (isValid)
                        {
                            // Mapeamos al DTO de salida
                            return new UserDtoOut
                            {
                                UserId = reader.GetInt32("Id"),
                                UserName = reader.GetString("Name"),
                                Email = reader.GetString("Email"),
                                // Si Role es nulo en BD (porque no lo rellenaste), ponemos StandardUser
                                Role = reader.IsDBNull(reader.GetOrdinal("Role")) ? Roles.StandardUser : reader.GetString("Role")
                            };
                        }
                    }
                }
            }
        }
        return null; // Devuelve null si no existe email o password incorrecto
    }

    // MÉTODO REGISTER
    public UserDtoOut AddUserFromCredentials(UserDtoIn userDto)
    {
        using (var connection = new MySqlConnection(_connectionString))
        {
            connection.Open();

            // Simulamos Hash
            string hash = "HASH_FALSO_" + userDto.Password;
            string role = Roles.StandardUser; 

            // Insertamos en User
            string query = @"INSERT INTO User (Name, Email, PasswordHash, Role) 
                             VALUES (@Name, @Email, @Pass, @Role);
                             SELECT LAST_INSERT_ID();";

            using (var command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Name", userDto.UserName);
                command.Parameters.AddWithValue("@Email", userDto.Email);
                command.Parameters.AddWithValue("@Pass", hash);
                command.Parameters.AddWithValue("@Role", role);

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
    
    // ... Resto de métodos (GetById, etc.) ...
}