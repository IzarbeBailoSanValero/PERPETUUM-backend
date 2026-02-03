using MySqlConnector;
using PERPETUUM.Models;
using PERPETUUM.DTOs;
using Microsoft.Extensions.Logging;
using System.Text;

namespace PERPETUUM.Repositories;

public class DeceasedRepository : IDeceasedRepository
{
    private readonly string _connectionString;
    private readonly ILogger<DeceasedRepository> _logger;

    public DeceasedRepository(IConfiguration configuration, ILogger<DeceasedRepository> logger)
    {
        _connectionString = configuration.GetConnectionString("PerpetuumDB") ?? throw new ArgumentNullException("la cadena 'PerpetuumDB no existe en appsettings'");
        _logger = logger;
    }

    public async Task<List<Deceased>> GetAllAsync()
    {
        _logger.LogInformation("iniciando GetAllAsync en Deceased");

        var deceasedList = new List<Deceased>();

        try
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                string query = "SELECT Id, FuneralHomeId, GuardianId, StaffId, Dni, Name, Epitaph, Biography, PhotoURL, BirthDate, DeathDate FROM Deceased";

                using (var command = new MySqlCommand(query, connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            //aquí hago el mapeo dentro, más limpio en un función aparte como en staff, dejo las dos maneras.
                            deceasedList.Add(new Deceased
                            {
                                Id = reader.GetInt32("Id"),
                                FuneralHomeId = reader.GetInt32("FuneralHomeId"),
                                GuardianId = reader.GetInt32("GuardianId"),
                                StaffId = reader.GetInt32("StaffId"),
                                Dni = reader.GetString("Dni"),
                                Name = reader.GetString("Name"),
                                Epitaph = reader.GetString("Epitaph"),
                                Biography = reader.GetString("Biography"),
                                PhotoURL = reader.GetString("PhotoURL"),
                                BirthDate = reader.GetDateTime("BirthDate"),
                                DeathDate = reader.GetDateTime("DeathDate")
                            });
                        }
                    }
                }
            }
            _logger.LogInformation("petición a BBDD GetAllAsync para deceased exitosa.");
            return deceasedList;
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



    public async Task<Deceased?> GetByIdAsync(int id)
    {
        _logger.LogInformation("Iniciando GetByIdAsync en Deceased");
        Deceased? deceased = null;

        try
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                string query = @"
                SELECT Id, FuneralHomeId, GuardianId, StaffId, Dni, Name, Epitaph, Biography, PhotoURL, BirthDate, DeathDate 
                FROM Deceased 
                WHERE Id = @Id;

                SELECT Id, CreatedDate, Type, Status, TextContent, MediaURL, AuthorRelation, UserId, DeceasedId 
                FROM Memory 
                WHERE DeceasedId = @Id;
            ";

                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                      //si no hay registro para leer devuelve nulo --> para lanzar luego 404, sino, se pone a leer
                        if (!await reader.ReadAsync())
                        {
                            _logger.LogWarning($"Deceased con id:{id} NO encontrado");
                            return null; 
                        }

                        deceased = new Deceased
                        {
                            Id = reader.GetInt32("Id"),
                            FuneralHomeId = reader.GetInt32("FuneralHomeId"),
                            GuardianId = reader.GetInt32("GuardianId"),
                            StaffId = reader.GetInt32("StaffId"),
                            Dni = reader.GetString("Dni"),
                            Name = reader.GetString("Name"),
                            Epitaph = reader.GetString("Epitaph"),
                            Biography = reader.GetString("Biography"),
                            PhotoURL = reader.GetString("PhotoURL"),
                            BirthDate = reader.GetDateTime("BirthDate"),
                            DeathDate = reader.GetDateTime("DeathDate"),
                            Memories = new List<Memory>()
                        };

                       //cargamos memorias
                        await reader.NextResultAsync();

                        while (await reader.ReadAsync())
                        {
                            deceased.Memories.Add(new Memory
                            {
                                Id = reader.GetInt32("Id"),
                                CreatedDate = reader.GetDateTime("CreatedDate"),
                                Type = (MemoryType)reader.GetInt32("Type"),
                                Status =(MemoryStatus) reader.GetInt32("Status"),
                                TextContent = reader.IsDBNull(reader.GetOrdinal("TextContent")) ? null : reader.GetString("TextContent"),
                                MediaURL = reader.IsDBNull(reader.GetOrdinal("MediaURL")) ? null : reader.GetString("MediaURL"),
                                AuthorRelation = reader.IsDBNull(reader.GetOrdinal("AuthorRelation")) ? null : reader.GetString("AuthorRelation"),
                                UserId = reader.GetInt32("UserId"),
                                DeceasedId = reader.GetInt32("DeceasedId")
                            });
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

        return deceased;
    }


    public async Task<List<DeceasedSummaryDTO>> GetByGuardianIdAsync(int guardianId)
{
    var list = new List<DeceasedSummaryDTO>();

    try
    {
        using (var connection = new MySqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            
            string query = "SELECT Id, Name, PhotoURL, DeathDate FROM Deceased WHERE GuardianId = @GuardianId";

            using (var command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@GuardianId", guardianId);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        list.Add(new DeceasedSummaryDTO
                        {
                            Id = reader.GetInt32("Id"),
                            Name = reader.GetString("Name"),
                            PhotoURL = reader.IsDBNull(reader.GetOrdinal("PhotoURL")) ? null : reader.GetString("PhotoURL"),
                            DeathDate = reader.GetDateTime("DeathDate")
                        });
                    }
                }
            }
        }
        return list;
    }
    catch (MySqlException ex)
        {
            _logger.LogError(ex, $"Error de MYSQL en GetByGuardianIdAsync: {ex.Message}");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error general en GetByGuardianIdAsync: {ex.Message}");
            throw;
        }

}

    public async Task<int> AddAsync(Deceased deceased)
    {
        _logger.LogInformation("Iniciando AddAsync en Deceased");
        int newId = 0;

        try
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                string query = @"
                INSERT INTO Deceased 
                (FuneralHomeId, GuardianId, StaffId, Dni, Name, Epitaph, Biography, PhotoURL, BirthDate, DeathDate) 
                VALUES 
                (@FuneralHomeId, @GuardianId, @StaffId, @Dni, @Name, @Epitaph, @Biography, @PhotoURL, @BirthDate, @DeathDate);
                SELECT LAST_INSERT_ID();";

                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@FuneralHomeId", deceased.FuneralHomeId);
                    command.Parameters.AddWithValue("@GuardianId", deceased.GuardianId);
                    command.Parameters.AddWithValue("@StaffId", deceased.StaffId);
                    command.Parameters.AddWithValue("@Dni", deceased.Dni);
                    command.Parameters.AddWithValue("@Name", deceased.Name);
                    command.Parameters.AddWithValue("@Epitaph", deceased.Epitaph);
                    command.Parameters.AddWithValue("@Biography", deceased.Biography);
                    command.Parameters.AddWithValue("@PhotoURL", deceased.PhotoURL);
                    command.Parameters.AddWithValue("@BirthDate", deceased.BirthDate);
                    command.Parameters.AddWithValue("@DeathDate", deceased.DeathDate);

                    var result = await command.ExecuteScalarAsync();
                    newId = result is null ? 0 : Convert.ToInt32(result);
                }
            }
        }
        catch (MySqlException ex)
        {
            _logger.LogError(ex, $"Error de MYSQL en AddAsync: {ex.Message}");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error general en AddAsync: {ex.Message}");
            throw;
        }
        return newId;
    }

    public async Task<bool> UpdateAsync(Deceased deceased)
    {
        bool hasBeenUpdated = false;
        try
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                string query = @"
                UPDATE Deceased SET
                    FuneralHomeId = @FuneralHomeId,
                    GuardianId = @GuardianId,
                    StaffId = @StaffId,
                    Dni = @Dni,
                    Name = @Name,
                    Epitaph = @Epitaph,
                    Biography = @Biography,
                    PhotoURL = @PhotoURL,
                    BirthDate = @BirthDate,
                    DeathDate = @DeathDate
                WHERE Id = @Id;";

                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", deceased.Id);
                    command.Parameters.AddWithValue("@FuneralHomeId", deceased.FuneralHomeId);
                    command.Parameters.AddWithValue("@GuardianId", deceased.GuardianId);
                    command.Parameters.AddWithValue("@StaffId", deceased.StaffId);
                    command.Parameters.AddWithValue("@Dni", deceased.Dni);
                    command.Parameters.AddWithValue("@Name", deceased.Name);
                    command.Parameters.AddWithValue("@Epitaph", deceased.Epitaph);
                    command.Parameters.AddWithValue("@Biography", deceased.Biography);
                    command.Parameters.AddWithValue("@PhotoURL", deceased.PhotoURL);
                    command.Parameters.AddWithValue("@BirthDate", deceased.BirthDate);
                    command.Parameters.AddWithValue("@DeathDate", deceased.DeathDate);

                    int rowsAffected = await command.ExecuteNonQueryAsync();
                    if (rowsAffected > 0) hasBeenUpdated = true;
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
        return hasBeenUpdated;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        _logger.LogInformation("Iniciando DeleteAsync en Deceased ");
        bool hasBeenDeleted = false;
        try
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string query = @"DELETE FROM Deceased WHERE Id = @Id;";

                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    int rowsAffected = await command.ExecuteNonQueryAsync();
                    if (rowsAffected > 0) hasBeenDeleted = true;
                }
            }
        }
        catch (MySqlException ex)
        {
            _logger.LogError(ex, $"Error de MYSQL en DeleteAsync. Error: {ex.Message}");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error general en DeleteAsync. Error: {ex.Message}");
            throw;
        }
        return hasBeenDeleted;
    }


    public async Task<List<Deceased>> SearchAsync(DeceasedSearchDTO searchDTO)
    {
        _logger.LogInformation("Inicio de búsqueda en Deceased");

        var deceasedList = new List<Deceased>();

        try
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var sqlBuilder = new System.Text.StringBuilder();

                sqlBuilder.Append("SELECT Id, FuneralHomeId, GuardianId, StaffId, Dni, Name, Epitaph, Biography, PhotoURL, BirthDate, DeathDate FROM Deceased WHERE 1=1 ");

                if (!string.IsNullOrEmpty(searchDTO.Name))
                {
                    sqlBuilder.Append(" AND Name LIKE @Name ");
                }

                if (searchDTO.DeathYear.HasValue)
                {
                    sqlBuilder.Append(" AND YEAR(DeathDate) = @DeathYear ");
                }

                if (!string.IsNullOrEmpty(searchDTO.SortBy))
                {
                    sqlBuilder.Append($" ORDER BY {searchDTO.SortBy}");
                }

                sqlBuilder.Append(";");

                using (var command = new MySqlCommand(sqlBuilder.ToString(), connection))
                {
                    if (!string.IsNullOrEmpty(searchDTO.Name))
                        command.Parameters.AddWithValue("@Name", $"%{searchDTO.Name}%");

                    if (searchDTO.DeathYear.HasValue)
                        command.Parameters.AddWithValue("@DeathYear", searchDTO.DeathYear.Value);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            deceasedList.Add(new Deceased
                            {
                                Id = reader.GetInt32("Id"),
                                FuneralHomeId = reader.GetInt32("FuneralHomeId"),
                                GuardianId = reader.GetInt32("GuardianId"),
                                StaffId = reader.GetInt32("StaffId"),
                                Dni = reader.GetString("Dni"),
                                Name = reader.GetString("Name"),
                                Epitaph = reader.GetString("Epitaph"),
                                Biography = reader.GetString("Biography"),
                                PhotoURL = reader.GetString("PhotoURL"),
                                BirthDate = reader.GetDateTime("BirthDate"),
                                DeathDate = reader.GetDateTime("DeathDate")
                            });
                        }
                    }
                }
            }
            return deceasedList;
        }
        catch (MySqlException ex)
        {
            _logger.LogError(ex, $"Error de MYSQL en SearchAsync. Error: {ex.Message}");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error general en SearchAsync. Error: {ex.Message}");
            throw;
        }
    }

    public async Task<bool> ExistsByDniAsync(string dni, int? excludeId = null)
{
    using (var connection = new MySqlConnection(_connectionString))
    {
        await connection.OpenAsync();
        
        // Construcción dinámica de la query
        var sqlBuilder = new StringBuilder("SELECT COUNT(1) FROM Deceased WHERE Dni = @Dni");

        // Si es Update (tenemos ID), excluimos ese ID de la búsqueda
        if (excludeId.HasValue)
        {
            sqlBuilder.Append(" AND Id != @ExcludeId");
        }

        using (var command = new MySqlCommand(sqlBuilder.ToString(), connection))
        {
            command.Parameters.AddWithValue("@Dni", dni);
            
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