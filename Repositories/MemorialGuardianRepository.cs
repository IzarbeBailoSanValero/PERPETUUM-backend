using MySqlConnector;
using PERPETUUM.Models;
using Microsoft.Extensions.Logging;
using System.Text;

namespace PERPETUUM.Repositories;
public class MemorialGuardianRepository : IMemorialGuardianRepository
{
    private readonly string _connectionString;
    private readonly ILogger<MemorialGuardianRepository> _logger;

    public MemorialGuardianRepository(IConfiguration configuration, ILogger<MemorialGuardianRepository> logger)
    {
        _connectionString = configuration.GetConnectionString("PerpetuumDB") ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger;
    }

    public async Task<MemorialGuardian?> GetByEmailAsync(string email)
    {
        try
        {
            _logger.LogInformation("Buscando guardian por email {Email}", email);

            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            string query = @"SELECT Id, FuneralHomeId, StaffId, Name, DNI, Email, PhoneNumber, PasswordHash 
                             FROM MemorialGuardian WHERE Email = @Email";

            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@Email", email);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                _logger.LogInformation("Guardian encontrado con email {Email}", email);
                return MapFromReader(reader);
            }

            _logger.LogWarning("No se encontró guardian con email {Email}", email);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener guardian por email {Email}", email);
            throw;
        }
    }
}



    public async Task<MemorialGuardian?> GetByIdAsync(int id)
    {
        try
        {
            _logger.LogInformation("Buscando guardian por ID {Id}", id);

            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            string query = @"SELECT Id, FuneralHomeId, StaffId, Name, DNI, Email, PhoneNumber, PasswordHash 
                             FROM MemorialGuardian WHERE Id = @Id";

            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@Id", id);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                _logger.LogInformation("Guardian encontrado con ID {Id}", id);
                return MapFromReader(reader);
            }

            _logger.LogWarning("No se encontró guardian con ID {Id}", id);
            return null;
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
    }

    public async Task<int> AddAsync(MemorialGuardian guardian)
    {
        try
        {
            _logger.LogInformation("Insertando nuevo guardian con email {Email}", guardian.Email);

            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            string query = @"INSERT INTO MemorialGuardian 
                            (FuneralHomeId, StaffId, Name, DNI, Email, PhoneNumber, PasswordHash) 
                             VALUES (@FhId, @StaffId, @Name, @Dni, @Email, @Phone, @Pass);
                             SELECT LAST_INSERT_ID();";

            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@FhId", guardian.FuneralHomeId);
            command.Parameters.AddWithValue("@StaffId", guardian.StaffId);
            command.Parameters.AddWithValue("@Name", guardian.Name);
            command.Parameters.AddWithValue("@Dni", guardian.Dni);
            command.Parameters.AddWithValue("@Email", guardian.Email);
            command.Parameters.AddWithValue("@Phone", guardian.PhoneNumber);
            command.Parameters.AddWithValue("@Pass", guardian.PasswordHash);

            var newId = Convert.ToInt32(await command.ExecuteScalarAsync());

            _logger.LogInformation("Guardian insertado correctamente con ID {Id}", newId);

            return newId;
        }
        catch (MySqlException ex)
        {
            if (ex.Number == 1062) // duplicado 
                _logger.LogWarning("Entrada duplicada en la base de datos.");
            _logger.LogError(ex, "Error de MySQL en AddAsync");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error general en AddAsync");
            throw;
        }
    }

    private MemorialGuardian MapFromReader(MySqlDataReader reader)
    {
        return new MemorialGuardian
        {
            Id = reader.GetInt32("Id"),
            FuneralHomeId = reader.GetInt32("FuneralHomeId"),
            StaffId = reader.GetInt32("StaffId"),
            Name = reader.GetString("Name"),
            Dni = reader.GetString("DNI"),
            Email = reader.GetString("Email"),
            PhoneNumber = reader.GetString("PhoneNumber"),
            PasswordHash = reader.GetString("PasswordHash")
        };
    }
}
