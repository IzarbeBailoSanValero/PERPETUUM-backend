using MySqlConnector;
using PERPETUUM.Models;
using Microsoft.Extensions.Logging;
using System.Text;

namespace PERPETUUM.Repositories;

public class StaffRepository : IStaffRepository
{
    private readonly string _connectionString;
    private readonly ILogger<StaffRepository> _logger;

    public StaffRepository(IConfiguration configuration, ILogger<StaffRepository> logger)
    {
        _connectionString = configuration.GetConnectionString("PerpetuumDB") 
            ?? throw new ArgumentNullException("Connection string not found");
        _logger = logger;
    }

    


    public async Task<List<Staff>> GetAllAsync()
    {
        var list = new List<Staff>();
        try
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string query = "SELECT Id, FuneralHomeId, Name, Email, DNI FROM Staff";

                using (var command = new MySqlCommand(query, connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        list.Add(MapReaderToModel(reader));
                    }
                }
            }
        }
        catch (MySqlException ex)
        {
            _logger.LogError(ex, $"Error de MYSQL en GetAllAsync: {ex.Message}");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error general en GetAllAsync: {ex.Message}");
            throw;
        }
        _logger.LogInformation("petición a BBDD GetAllAsync para staff exitosa.");
        return list;
    }

    

    public async Task<Staff?> GetByIdAsync(int id)
    {
        try
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string query = "SELECT Id, FuneralHomeId, Name, Email, DNI FROM Staff WHERE Id = @Id";

                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return MapReaderToModel(reader);
                        }
                    }
                }
            }
        }
        catch (MySqlException ex)
        {
            _logger.LogError(ex, $"Error de MYSQL en GetByIdAsync: {ex.Message}");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error general en GetByIdAsync: {ex.Message}");
            throw;
        }
        return null;
    }

    
    public async Task<List<Staff>> GetByFuneralHomeIdAsync(int funeralHomeId)
    {
        var list = new List<Staff>();
        try
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string query = "SELECT Id, FuneralHomeId, Name, Email, DNI FROM Staff WHERE FuneralHomeId = @FhId";
                //había pensado que igual habia que comprobar aquí si la funeraria existe pero es mejor en el service porque: si lo metes aquí, harías dos consultas sql siempre. es mejor que lo compruebe el service y solo se hace una si existe, es la capa de acceso a datos rápida. 
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@FhId", funeralHomeId);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            list.Add(MapReaderToModel(reader));
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error en GetByFuneralHomeId {funeralHomeId}");
            throw;
        }
        return list;
    }


    public async Task<int> AddAsync(Staff staff)
{
    using (var connection = new MySqlConnection(_connectionString))
    {
        await connection.OpenAsync();

        string query = @"INSERT INTO Staff (FuneralHomeId, Name, Email, DNI, PasswordHash, IsAdmin) 
                         VALUES (@FuneralHomeId, @Name, @Email, @DNI, @PasswordHash, @IsAdmin);
                         SELECT LAST_INSERT_ID();";

        using (var command = new MySqlCommand(query, connection))
        {
            command.Parameters.AddWithValue("@FuneralHomeId", staff.FuneralHomeId);
            command.Parameters.AddWithValue("@Name", staff.Name);
            command.Parameters.AddWithValue("@Email", staff.Email);
            command.Parameters.AddWithValue("@DNI", staff.DNI);
            command.Parameters.AddWithValue("@PasswordHash", staff.PasswordHash);
            command.Parameters.AddWithValue("@IsAdmin", staff.IsAdmin);

            return Convert.ToInt32(await command.ExecuteScalarAsync());
        }
    }
}
 
   public async Task<bool> UpdateAsync(Staff staff)
    {
        try
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                
                // CAMBIO: Ya no actualizamos el FuneralHomeId
                string query = @"
                    UPDATE Staff SET 
                        Name = @Name, 
                        Email = @Email, 
                        DNI = @DNI
                    WHERE Id = @Id";

                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", staff.Id);
                    // command.Parameters.AddWithValue("@FuneralHomeId", staff.FuneralHomeId); // <-- ELIMINADO
                    command.Parameters.AddWithValue("@Name", staff.Name);
                    command.Parameters.AddWithValue("@Email", staff.Email);
                    command.Parameters.AddWithValue("@DNI", staff.DNI);

                    int affected = await command.ExecuteNonQueryAsync();
                    return affected > 0;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en Update Staff");
            throw;
        }
    }
    
    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string query = "DELETE FROM Staff WHERE Id = @Id";

                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    int affected = await command.ExecuteNonQueryAsync();
                    return affected > 0;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en Delete Staff");
            throw;
        }
    }



// --- VALIDACIONES EXISTS (Explícitas y separadas) ---

    public async Task<bool> ExistsByEmailAsync(string email, int? excludeId = null)
    {
        using (var connection = new MySqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            
           
            var sb = new StringBuilder("SELECT COUNT(1) FROM Staff WHERE Email = @Email");

           
            if (excludeId.HasValue)
            {
                sb.Append(" AND Id != @ExcludeId");
            }

            using (var command = new MySqlCommand(sb.ToString(), connection)) //hay que pasar el stringbuilder a tostring!!!
            {
                
                command.Parameters.AddWithValue("@Email", email);
                
                if (excludeId.HasValue)
                {
                    command.Parameters.AddWithValue("@ExcludeId", excludeId.Value);
                }

                
                int count = Convert.ToInt32(await command.ExecuteScalarAsync());
                return count > 0;
            }
        }
    }

    public async Task<bool> ExistsByDniAsync(string dni, int? excludeId = null)
    {
        using (var connection = new MySqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            
            
            var sb = new StringBuilder("SELECT COUNT(1) FROM Staff WHERE DNI = @DNI");

           
            if (excludeId.HasValue)
            {
                sb.Append(" AND Id != @ExcludeId");
            }

            using (var command = new MySqlCommand(sb.ToString(), connection))
            {
                
                command.Parameters.AddWithValue("@DNI", dni);
                
                if (excludeId.HasValue)
                {
                    command.Parameters.AddWithValue("@ExcludeId", excludeId.Value);
                }

                
                int count = Convert.ToInt32(await command.ExecuteScalarAsync());
                return count > 0;
            }
        }
    }

    public async Task<Staff?> GetByEmailAsync(string email)
{
    using var connection = new MySqlConnection(_connectionString);
    await connection.OpenAsync();
    
    // Necesitamos el PasswordHash e IsAdmin para el Login
    string query = "SELECT * FROM Staff WHERE Email = @Email"; 
    
    using var command = new MySqlCommand(query, connection);
    command.Parameters.AddWithValue("@Email", email);
    
    using var reader = await command.ExecuteReaderAsync();
    if (await reader.ReadAsync())
    {
        return new Staff //con passwordhash y isAdmin
        { 
            Id = reader.GetInt32("Id"),
            Name = reader.GetString("Name"),
            Email = reader.GetString("Email"),
            PasswordHash = reader.GetString("PasswordHash"), 
            IsAdmin = reader.GetBoolean("IsAdmin"),
            FuneralHomeId = reader.GetInt32("FuneralHomeId")
        };
    }
    return null;
}


    private Staff MapReaderToModel(MySqlDataReader reader)
    {
        return new Staff
        {
            Id = reader.GetInt32("Id"),
            FuneralHomeId = reader.GetInt32("FuneralHomeId"),
            Name = reader.GetString("Name"),
            Email = reader.GetString("Email"),
            DNI = reader.GetString("DNI")
        };
    }
}
