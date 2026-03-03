using MySqlConnector;
using PERPETUUM.DTOs;
using PERPETUUM.Models;
using System.Data;
using Microsoft.Extensions.Logging;

namespace PERPETUUM.Repositories;

public class MemoryRepository : IMemoryRepository
{
    private readonly string _connectionString;
    private readonly ILogger<MemoryRepository> _logger;
    

    public MemoryRepository(IConfiguration configuration, ILogger<MemoryRepository> logger)
    {
       _connectionString = configuration.GetConnectionString("PerpetuumDB") ?? throw new ArgumentNullException("la cadena 'PerpetuumDB' no existe en appsettings");
        _logger = logger;
        
    }

    // 1. Obtener TODAS las memorias --> staff
    public async Task<List<Memory>> GetByDeceasedIdAsync(int deceasedId)
    {
        var list = new List<Memory>();
        try
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();


                string query = @"
                    SELECT Id, CreatedDate, Type, Status, TextContent, MediaURL, AuthorRelation, DeceasedId, UserId FROM Memory WHERE DeceasedId = @Id ORDER BY CreatedDate DESC";

                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", deceasedId);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            list.Add(MapFromReader(reader));
                        }
                    }
                }
            }
            return list;

        }
        catch (MySqlException ex)
        {
            _logger.LogError(ex, $"Error de MYSQL en GetByDeceasedIdAsync. Error: {ex.Message}");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error general en GetByDeceasedIdAsync. Error: {ex.Message}");
            throw;
        }
    }


    // 2.  solo APROBADAS --> para publico
    public async Task<List<Memory>> GetApprovedByDeceasedIdAsync(int deceasedId)
    {
        var list = new List<Memory>();
        try
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                string query = @"
                    SELECT Id, CreatedDate, Type, Status, TextContent, MediaURL, AuthorRelation, DeceasedId, UserId 
                    FROM Memory 
                    WHERE DeceasedId = @Id AND Status = @Status 
                    ORDER BY CreatedDate DESC";

                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", deceasedId);
                    command.Parameters.AddWithValue("@Status", (int)MemoryStatus.Approved); // casteo a int el enum

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            list.Add(MapFromReader(reader));
                        }
                    }
                }
            }
            return list;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo memorias APROBADAS para difunto {Id}", deceasedId);
            throw;
        }
    }

    public async Task<List<MemoryModerationDTO>> GetPendingForModerationAsync(List<int>? deceasedIds = null)
    {
        var memories = new List<MemoryModerationDTO>();

        try
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            string memoryQuery = "SELECT Id, CreatedDate, Type, Status, TextContent, MediaURL, AuthorRelation, DeceasedId, UserId FROM Memory WHERE Status = @PendingStatus";

            if (deceasedIds != null && deceasedIds.Count > 0)
            {
                memoryQuery += " AND DeceasedId IN (";
                for (int i = 0; i < deceasedIds.Count; i++)
                {
                    if (i > 0) memoryQuery += ",";
                    memoryQuery += "@DeceasedId" + i;
                }
                memoryQuery += ")";
            }

            memoryQuery += " ORDER BY CreatedDate DESC";

            using (var memoryCommand = new MySqlCommand(memoryQuery, connection))
            {
                memoryCommand.Parameters.AddWithValue("@PendingStatus", (int)MemoryStatus.Pending);

                if (deceasedIds != null && deceasedIds.Count > 0)
                {
                    for (int i = 0; i < deceasedIds.Count; i++)
                    {
                        memoryCommand.Parameters.AddWithValue("@DeceasedId" + i, deceasedIds[i]);
                    }
                }

                using var memoryReader = await memoryCommand.ExecuteReaderAsync();
                while (await memoryReader.ReadAsync())
                {
                    memories.Add(new MemoryModerationDTO
                    {
                        Id = memoryReader.GetInt32("Id"),
                        CreatedDate = memoryReader.GetDateTime("CreatedDate"),
                        Type = memoryReader.GetInt32("Type"),
                        Status = memoryReader.GetInt32("Status"),
                        TextContent = memoryReader.IsDBNull("TextContent") ? null : memoryReader.GetString("TextContent"),
                        MediaURL = memoryReader.IsDBNull("MediaURL") ? null : memoryReader.GetString("MediaURL"),
                        AuthorRelation = memoryReader.IsDBNull("AuthorRelation") ? null : memoryReader.GetString("AuthorRelation"),
                        DeceasedId = memoryReader.GetInt32("DeceasedId"),
                        UserId = memoryReader.GetInt32("UserId")
                    });
                }
            }

            if (memories.Count == 0) return memories;

            var nameByDeceasedId = new Dictionary<int, string>();
            var ids = new List<int>();
            var seen = new HashSet<int>();

            foreach (var memory in memories)
            {
                if (seen.Add(memory.DeceasedId))
                {
                    ids.Add(memory.DeceasedId);
                }
            }

            string deceasedQuery = "SELECT Id, Name FROM Deceased WHERE Id IN (";
            for (int i = 0; i < ids.Count; i++)
            {
                if (i > 0) deceasedQuery += ",";
                deceasedQuery += "@Id" + i;
            }
            deceasedQuery += ")";

            using (var deceasedCommand = new MySqlCommand(deceasedQuery, connection))
            {
                for (int i = 0; i < ids.Count; i++)
                {
                    deceasedCommand.Parameters.AddWithValue("@Id" + i, ids[i]);
                }

                using var deceasedReader = await deceasedCommand.ExecuteReaderAsync();
                while (await deceasedReader.ReadAsync())
                {
                    nameByDeceasedId[deceasedReader.GetInt32("Id")] = deceasedReader.GetString("Name");
                }
            }

            foreach (var memory in memories)
            {
                if (nameByDeceasedId.TryGetValue(memory.DeceasedId, out var name))
                {
                    memory.DeceasedName = name;
                }
            }

            return memories;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo memorias pendientes de moderación");
            throw;
        }
    }



    public async Task<Memory?> GetByIdAsync(int id)
    {
        try
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();


                string query = @"
                    SELECT Id, CreatedDate, Type, Status, TextContent, MediaURL, AuthorRelation, DeceasedId, UserId 
                    FROM Memory 
                    WHERE Id = @Id";

                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return MapFromReader(reader);
                        }
                    }
                }
            }
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error buscando memoria por ID {Id}", id);
            throw;
        }
    }

    public async Task<int> AddAsync(Memory memory)
    {
        try
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                string query = @"INSERT INTO Memory 
                                (Type, Status, TextContent, MediaURL, AuthorRelation, DeceasedId, UserId, CreatedDate) 
                                VALUES 
                                (@Type, @Status, @Text, @MediaURL, @AuthorRelation, @DeceasedId, @UserId, NOW());
                                SELECT LAST_INSERT_ID();";

                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Type", (int)memory.Type);
                    command.Parameters.AddWithValue("@Status", (int)memory.Status);

                    //si usase ORM sí que podría pasar null directamente. En este caso como uso ADO necesito convertir los null de backend  DBNull de mysql--> compruebo si es null,si no es null añado el valor y sino lo casteo a. dbnull
                    //por que hace falta poner object? 
                    ////el operador ?? necesita que ambos lados sean del mismo tipo o convertibles entre sí
                    /// //string y dbnull no son compatibles --> si le decimos que trate todo como object
                    command.Parameters.AddWithValue("@Text", (object?)memory.TextContent ?? DBNull.Value);
                    command.Parameters.AddWithValue("@MediaURL", (object?)memory.MediaURL ?? DBNull.Value);
                    command.Parameters.AddWithValue("@AuthorRelation", (object?)memory.AuthorRelation ?? DBNull.Value);

                    command.Parameters.AddWithValue("@DeceasedId", memory.DeceasedId);
                    command.Parameters.AddWithValue("@UserId", memory.UserId);

                    return Convert.ToInt32(await command.ExecuteScalarAsync());
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
    }

    public async Task<bool> UpdateStatusAsync(int id, MemoryStatus status)
    {
        try
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string query = "UPDATE Memory SET Status = @Status WHERE Id = @Id";

                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    command.Parameters.AddWithValue("@Status", (int)status);

                    return await command.ExecuteNonQueryAsync() > 0;
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
    }


    //sustituyo el borrado normal por soft delete --> en lugar de eliminar, cambio de estado a "rejected" por si alguien public aalgo ofensivo etc.  // Conceptualmente es un Delete, técnicamente es un Update
    public async Task<bool> DeleteAsync(int id){
         bool hasBeenUpdated = false;
        try
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                string query = "UPDATE Memory SET Status = @Status WHERE Id = @Id";

                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    command.Parameters.AddWithValue("@Status", (int)MemoryStatus.Rejected);
                      int rowsAffected = await command.ExecuteNonQueryAsync();
                    if (rowsAffected > 0) hasBeenUpdated = true;
                }
            }

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
     return hasBeenUpdated;
}
        


    private Memory MapFromReader(MySqlDataReader reader)
    {

        return new Memory
        {
            Id = reader.GetInt32("Id"),
            CreatedDate = reader.GetDateTime("CreatedDate"),
            Type = (MemoryType)reader.GetInt32("Type"),
            Status = (MemoryStatus)reader.GetInt32("Status"),
            TextContent = reader.IsDBNull("TextContent") ? null : reader.GetString("TextContent"),
            MediaURL = reader.IsDBNull("MediaURL") ? null : reader.GetString("MediaURL"),
            AuthorRelation = reader.IsDBNull("AuthorRelation") ? null : reader.GetString("AuthorRelation"),
            DeceasedId = reader.GetInt32("DeceasedId"),
            UserId = reader.GetInt32("UserId")
        };
    }

}
