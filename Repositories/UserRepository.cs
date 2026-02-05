using MySqlConnector;
using PERPETUUM.Models;


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

//---FUNCIONES RELACIONADAS CON AUTENTICACIÓN.

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
        try
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
        catch (MySqlException ex)
        {
            if (ex.Number == 1062) // duplicado 
                _logger.LogWarning("Entrada duplicada en la base de datos.");
            _logger.LogError(ex, "Error de MySQL en createAsync");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error general en createAsync");
            throw;
        }
    }


//- CRUD Y MÉTODO PROPIO
public async Task<User?> GetByIdAsync(int id)
    {
        try{
            using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        string query = "SELECT Id, Name, Email, PasswordHash, PhoneNumber, BirthDate FROM `User` WHERE Id = @Id";

        using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@Id", id);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync()) return MapFromReader(reader);
        return null;
        }catch (MySqlException ex)
        {
            _logger.LogError(ex, $"Error de MYSQL en GetByIdAsync: {ex.Message}");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error general en GetByIdAsync: {ex.Message}");
            throw;
        }
    }

    public async Task<bool> UpdateAsync(User user)
    {
        try
        {
            using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        string query = @"UPDATE `User` 
                         SET Name = @Name, Email = @Email, PhoneNumber = @Phone, BirthDate = @BirthDate 
                         WHERE Id = @Id";

        using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@Id", user.Id);
        command.Parameters.AddWithValue("@Name", user.Name);
        command.Parameters.AddWithValue("@Email", user.Email);
        command.Parameters.AddWithValue("@Phone", (object?)user.PhoneNumber ?? DBNull.Value);
        command.Parameters.AddWithValue("@BirthDate", (object?)user.BirthDate ?? DBNull.Value);

        return await command.ExecuteNonQueryAsync() > 0;
        }catch (MySqlException ex)
        {
            _logger.LogError(ex, $"Error de MYSQL en UpdateAsync: {ex.Message}");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error general en UpdateAsync: {ex.Message}");
            throw;
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
             using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        string query = "DELETE FROM `User` WHERE Id = @Id";

        using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@Id", id);

        return await command.ExecuteNonQueryAsync() > 0;
        }catch (MySqlException ex)
        {
            _logger.LogError(ex, $"Error de MYSQL en DeleteAsync: {ex.Message}");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error general en DeleteAsync: {ex.Message}");
            throw;
        }

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
   
   
