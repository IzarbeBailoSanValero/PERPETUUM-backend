using MySqlConnector;
using PERPETUUM.Models;
using Microsoft.Extensions.Logging;
using System.Text;

namespace PERPETUUM.Repositories;

public class FuneralHomeRepository : IFuneralHomeRepository
{
    private readonly string _connectionString;
    private readonly ILogger<FuneralHomeRepository> _logger;

    public FuneralHomeRepository(IConfiguration configuration, ILogger<FuneralHomeRepository> logger)
    {
        _connectionString = configuration.GetConnectionString("PerpetuumDB") ?? throw new ArgumentNullException("La cadena 'PerpetuumDB' no existe en appsettings");
        _logger = logger;
    }


    public async Task<List<FuneralHome>> GetAllAsync()
    {
        _logger.LogInformation("Iniciando GetAllAsync en FuneralHome");
        var list = new List<FuneralHome>();

        try
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();


                string query = "SELECT Id, Name, CIF, ContactEmail, Address, PhoneNumber FROM FuneralHome";

                using (var command = new MySqlCommand(query, connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        list.Add(new FuneralHome
                        {
                            Id = reader.GetInt32("Id"),
                            Name = reader.GetString("Name"),
                            CIF = reader.GetString("CIF"),
                            ContactEmail = reader.GetString("ContactEmail"),
                            Address = reader.GetString("Address"),
                            PhoneNumber = reader.GetString("PhoneNumber")
                        });
                    }
                }
            }
            _logger.LogInformation("Petición GetAllAsync para FuneralHome exitosa.");
            return list;
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
    }


    public async Task<FuneralHome?> GetByIdAsync(int id)
    {
        _logger.LogInformation($"Iniciando GetByIdAsync en FuneralHome ID: {id}");
        FuneralHome? funeralHome = null;

        try
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string query = "SELECT Id, Name, CIF, ContactEmail, Address, PhoneNumber FROM FuneralHome WHERE Id = @Id";

                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            funeralHome = new FuneralHome
                            {
                                Id = reader.GetInt32("Id"),
                                Name = reader.GetString("Name"),
                                CIF = reader.GetString("CIF"),
                                ContactEmail = reader.GetString("ContactEmail"),
                                Address = reader.GetString("Address"),
                                PhoneNumber = reader.GetString("PhoneNumber")
                            };
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
        return funeralHome;
    }


    public async Task<int> AddAsync(FuneralHome funeralHome)
    {
        _logger.LogInformation("Iniciando AddAsync en FuneralHome");
        int newId = 0;

        try
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                // SOLO INSERT (Sin Select posterior, usamos LastInsertedId)
                string query = @"
                    INSERT INTO FuneralHome (Name, CIF, ContactEmail, Address, PhoneNumber) 
                    VALUES (@Name, @CIF, @ContactEmail, @Address, @PhoneNumber);
                    SELECT LAST_INSERT_ID();";

                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Name", funeralHome.Name);
                    command.Parameters.AddWithValue("@CIF", funeralHome.CIF);
                    command.Parameters.AddWithValue("@ContactEmail", funeralHome.ContactEmail);
                    command.Parameters.AddWithValue("@Address", funeralHome.Address);
                    command.Parameters.AddWithValue("@PhoneNumber", funeralHome.PhoneNumber);


                    var result = await command.ExecuteScalarAsync();
                    newId = result is null ? 0 : Convert.ToInt32(result);
                }
            }
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
        return newId;
    }


    public async Task<bool> UpdateAsync(FuneralHome funeralHome)
    {
        _logger.LogInformation($"Iniciando UpdateAsync en FuneralHome ID: {funeralHome.Id}");
        bool updated = false;

        try
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string query = @"
                    UPDATE FuneralHome SET 
                        Name = @Name, 
                        CIF = @CIF, 
                        ContactEmail = @ContactEmail, 
                        Address = @Address, 
                        PhoneNumber = @PhoneNumber 
                    WHERE Id = @Id;";

                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", funeralHome.Id);
                    command.Parameters.AddWithValue("@Name", funeralHome.Name);
                    command.Parameters.AddWithValue("@CIF", funeralHome.CIF);
                    command.Parameters.AddWithValue("@ContactEmail", funeralHome.ContactEmail);
                    command.Parameters.AddWithValue("@Address", funeralHome.Address);
                    command.Parameters.AddWithValue("@PhoneNumber", funeralHome.PhoneNumber);

                    int rowsAffected = await command.ExecuteNonQueryAsync();
                    if (rowsAffected > 0) updated = true;
                }
            }
        }
        catch (MySqlException ex)
        {
            _logger.LogError(ex, $"Error de MYSQL en UpdateAsync: {ex.Message}");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error general en UpdateAsync: {ex.Message}");
            throw;
        }
        return updated;
    }


    public async Task<bool> DeleteAsync(int id)
    {
        _logger.LogInformation($"Iniciando DeleteAsync en FuneralHome ID: {id}");
        bool deleted = false;

        try
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                string query = "DELETE FROM FuneralHome WHERE Id = @Id;"; //no hacer muchas probatinas, on delete cascade!!!

                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    int rowsAffected = await command.ExecuteNonQueryAsync();
                    if (rowsAffected > 0) deleted = true;
                }
            }
        }
        catch (MySqlException ex)
        {
            _logger.LogError(ex, $"Error de MYSQL en DeleteAsync: {ex.Message}");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error general en DeleteAsync: {ex.Message}");
            throw;
        }
        return deleted;
    }


    //yo habia pensado mla este metodo. mejorado por la IA
    // - excludeId para que no salte con nuestro propio cif al hacer update
    // -  en create dejamos exclude id con null y en create le damos valor

    public async Task<bool> ExistsByCifAsync(string cif, int? excludeId = null)
    {
        using (var connection = new MySqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            var sqlBuilder = new StringBuilder("SELECT COUNT(1) FROM FuneralHome WHERE CIF = @CIF");

            // Si estamos editando, excluimos nuestro propio ID para que no de falso positivo
            if (excludeId.HasValue)
            {
                sqlBuilder.Append(" AND Id != @ExcludeId");
            }
            sqlBuilder.Append(";");
            using (var command = new MySqlCommand(sqlBuilder.ToString(), connection))
            {
                command.Parameters.AddWithValue("@CIF", cif);
                if (excludeId.HasValue)
                {
                    command.Parameters.AddWithValue("@ExcludeId", excludeId.Value);
                }

                int count = Convert.ToInt32(await command.ExecuteScalarAsync());
                return count > 0;
            }
        }
    }
}