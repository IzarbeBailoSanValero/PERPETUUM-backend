//importaciones
using MySqlConnector;
using PERPETUUM.Models;
using PERPETUUM.DTOs;
using Microsoft.Extensions.Logging;
using System.Text;

namespace PERPETUUM.Repositories;

public class DeceasedRepository : IDeceasedRepository
{
    //variables privadas
    private readonly string _connectionString;
    private readonly ILogger<DeceasedRepository> _logger;



    //constructor e inyección DI
    public DeceasedRepository(IConfiguration configuration, ILogger<DeceasedRepository> logger)
    {
        _connectionString = configuration.GetConnectionString("PerpetuumDB") ?? throw new ArgumentNullException("la cadena 'PerpetuumDB no existe en appsettings'");
        _logger = logger;
    }


    //task
    public async Task<List<Deceased>> GetAllAsync()
    {
        _logger.LogInformation("iniciando GetAllAsync en Deceased");

        var deceasedList = new List<Deceased>();

        try
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                //1. abro conexión
                await connection.OpenAsync();
                //2. construyo la query
                string query = "SELECT Id, FuneralHomeId, GuardianId, StaffId, Name, Epitaph, Biography, PhotoURL, BirthDate, DeathDate FROM Deceased";

                using (var command = new MySqlCommand(query, connection))
                {
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
        _logger.LogInformation("iniciando getByIdAsync en deceased");
        Deceased? deceased = null;

        try
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                string query = @"
                SELECT Id, FuneralHomeId, GuardianId, StaffId, Name, Epitaph, Biography, PhotoURL, BirthDate, DeathDate FROM Deceased WHERE Id = @Id;
                SELECT Id, CreatedDate, Type, Status, TextContent, MediaURL, AuthorRelation, UserId, DeceasedId FROM Memory WHERE DeceasedId = @Id;
                ";

                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            deceased = new Deceased
                            {
                                Id = reader.GetInt32("Id"),
                                FuneralHomeId = reader.GetInt32("FuneralHomeId"),
                                GuardianId = reader.GetInt32("GuardianId"),
                                StaffId = reader.GetInt32("StaffId"),
                                Name = reader.GetString("Name"),
                                Epitaph = reader.GetString("Epitaph"),
                                Biography = reader.GetString("Biography"),
                                PhotoURL = reader.GetString("PhotoURL"),
                                BirthDate = reader.GetDateTime("BirthDate"),
                                DeathDate = reader.GetDateTime("DeathDate"),
                                Memories = new List<Memory>()
                            };
                        }

                        if (await reader.NextResultAsync()) //si hay siguiente conjunto de datos --> las memories del difunto 
                        {
                            //creo el listado de memories que cargará las fotos, condolencias y anécdotas
                            var memories = new List<Memory>();

                            //foto, condolencia y anecdota puede ser nulo, los gestiono para no recibir errores de conversión de nulos de db-api. si hiciesemos un getstring de algo nulo nos daría excepcion. con reader.isdbnull sí tolera.
                            int textContentOrdinal = reader.GetOrdinal("TextContent");
                            int mediaUrlOrdinal = reader.GetOrdinal("MediaURL");
                            int authorRelationOrdinal = reader.GetOrdinal("AuthorRelation");

                            while (await reader.ReadAsync())
                            {
                                memories.Add(new Memory
                                {
                                    Id = reader.GetInt32("Id"),
                                    CreatedDate = reader.GetDateTime("CreatedDate"),
                                    Type = reader.GetInt32("Type"),
                                    Status = reader.GetInt32("Status"),
                                    TextContent = reader.IsDBNull(textContentOrdinal) ? null : reader.GetString(textContentOrdinal),
                                    MediaURL = reader.IsDBNull(mediaUrlOrdinal) ? null : reader.GetString(mediaUrlOrdinal),
                                    AuthorRelation = reader.IsDBNull(authorRelationOrdinal) ? null : reader.GetString(authorRelationOrdinal),
                                    UserId = reader.GetInt32("UserId"),
                                    DeceasedId = reader.GetInt32("DeceasedId")
                                });
                            }

                            deceased.Memories = memories;

                        }

                        if (deceased == null)
                        {
                            _logger.LogWarning($"Deceased con id:{id} NO encontrado");
                            return null;
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
                (FuneralHomeId, GuardianId, StaffId, Name, Epitaph, Biography, PhotoURL, BirthDate, DeathDate) 
                VALUES 
                (@FuneralHomeId, @GuardianId, @StaffId, @Name, @Epitaph, @Biography, @PhotoURL, @BirthDate, @DeathDate);
                SELECT LAST_INSERT_ID();";

                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@FuneralHomeId", deceased.FuneralHomeId);
                    command.Parameters.AddWithValue("@GuardianId", deceased.GuardianId);
                    command.Parameters.AddWithValue("@StaffId", deceased.StaffId);
                    command.Parameters.AddWithValue("@Name", deceased.Name);
                    command.Parameters.AddWithValue("@Epitaph", deceased.Epitaph);
                    command.Parameters.AddWithValue("@Biography", deceased.Biography);
                    command.Parameters.AddWithValue("@PhotoURL", deceased.PhotoURL);
                    command.Parameters.AddWithValue("@BirthDate", deceased.BirthDate);
                    command.Parameters.AddWithValue("@DeathDate", deceased.DeathDate);
                    // Obtengo el ID generado, he buscado cómo.
                    /*
                    Execute Scalar ejecuta consultaSQL + devuelve primera fila + primera colimna --> LAST_INSERT_ID()
                    Devuelve un Object, por si no devuelve nada(null) manejamos la conversión con null ? 0: otro caso
                    no hacemos cast inmediato (int)numero por si ddbb devuelve long, decimal... -->convert.ToInt32 (en lugar de convertir solo int, convierte tipos compatibles)
                    Porque LAST_INSERT_ID() devuelve el valor del AUTO_INCREMENT generado en esa misma conexión.
                    Y como ExecuteScalar() toma la primera columna de la primera fila, te entrega exactamente ese ID.
                    */
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
                    command.Parameters.AddWithValue("@Name", deceased.Name);
                    command.Parameters.AddWithValue("@Epitaph", deceased.Epitaph);
                    command.Parameters.AddWithValue("@Biography", deceased.Biography);
                    command.Parameters.AddWithValue("@PhotoURL", deceased.PhotoURL);
                    command.Parameters.AddWithValue("@BirthDate", deceased.BirthDate);
                    command.Parameters.AddWithValue("@DeathDate", deceased.DeathDate);


                    //nonQueryAsync --> ejecuta + devuelve numero de registros aceptados. Si es mayor que 0, ha funcionado
                    int rowsAffected = await command.ExecuteNonQueryAsync();
                    if (rowsAffected > 0) hasBeenUpdated = true;
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



       public Task<List<Memory>> GetMemoriesByDeceasedIdAsync(int deceased)
    {
        throw new NotImplementedException();
    }

    //  //LO COMENTO PORQUE AÚN NO EXISTE MOMORIES, NO FUNCIONA{
    //     _logger.LogInformation($"Iniciando GetMemoriesByDeceasedIdAsync para Deceased Id {deceasedId}");
        
    //     var memoryList = new List<Memory>();

    //     try
    //     {
    //         using (var connection = new MySqlConnection(_connectionString))
    //         {
    //             await connection.OpenAsync();
                
    //             // Ordenamos por fecha descendente para que salga como un historial
    //             string query = @"
    //                 SELECT Id, CreatedDate, Type, Status, TextContent, MediaURL, AuthorRelation, UserId, DeceasedId 
    //                 FROM Memory 
    //                 WHERE DeceasedId = @DeceasedId 
    //                 ORDER BY CreatedDate DESC;";

    //             using (var command = new MySqlCommand(query, connection))
    //             {
    //                 command.Parameters.AddWithValue("@DeceasedId", deceasedId);

    //                 using (var reader = await command.ExecuteReaderAsync())
    //                 {
    //                     while (await reader.ReadAsync())
    //                     {
    //                         memoryList.Add(new Memory
    //                         {
                                
    //                             Id = reader.GetInt32("Id"),
    //                             CreatedDate = reader.GetDateTime("CreatedDate"),
    //                             Type = reader.GetInt32("Type"),
    //                             Status = reader.GetInt32("Status"),
    //                             UserId = reader.GetInt32("UserId"),
    //                             DeceasedId = reader.GetInt32("DeceasedId"),

    //                             // datos nullables. manejamos con get ordinal y lo pasamos a dbnull. si es nullDB lo pasamos a nullAPI
    //                             TextContent = reader.IsDBNull(reader.GetOrdinal("TextContent")) ? null : reader.GetString("TextContent"),
    //                             MediaURL = reader.IsDBNull(reader.GetOrdinal("MediaURL")) ? null : reader.GetString("MediaURL"),
    //                             AuthorRelation = reader.IsDBNull(reader.GetOrdinal("AuthorRelation")) ? null : reader.GetString("AuthorRelation")
    //                         });
    //                     }
    //                 }
    //             }
    //         }
            
    //         _logger.LogInformation("exito en la recuperación de lista de memories by deceased");
    //         return memoryList;
    //     }
    //     catch (MySqlException ex)
    //     {
    //         _logger.LogError(ex, $"error de MYSQL en petición a BBDD GetAllAsync. Error: {ex.Message}");
    //         throw;
    //     }
    //     catch (Exception ex)
    //     {
    //         _logger.LogError(ex, $"error en petición a BBDD GetAllAsync. Error: {ex.Message}");
    //         throw;
    //     }
    // }
    
    //buscador 1º trimestre
    // SEARCH
    public async Task<List<Deceased>> SearchAsync(DeceasedSearchDTO searchDTO)
    {
        _logger.LogInformation("Inicio de búsqueda en Deceased");

        //defino variable de retorno
        var deceasedList = new List<Deceased>();

        try
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                 //1. variable que creará la query, inicializo
                //stringbuilder sirve para crear strings mediante concatenación
                var sqlBuilder = new System.Text.StringBuilder();
                
                 //2. where 1 = 1 para ir podiendo hacer AND otra condición de carrera
                sqlBuilder.Append("SELECT Id, FuneralHomeId, GuardianId, StaffId, Name, Epitaph, Biography, PhotoURL, BirthDate, DeathDate FROM Deceased WHERE 1=1 ");
                
                //3. FILTROS DE BÚSQUEDA 
                //si hay contenido en los siguientes campos, los concatena
                if (!string.IsNullOrEmpty(searchDTO.Name))
                {
                    sqlBuilder.Append(" AND Name LIKE @Name ");
                }

                //en el dto introducimos solo el año de defunción. el usuario ingresa el año. ese año se compara con el año de la deathDate completa. YEAR(fecha) coge solo el año
                if (searchDTO.DeathYear.HasValue)
                {
                    sqlBuilder.Append(" AND YEAR(DeathDate) = @DeathYear ");
                }

                if (!string.IsNullOrEmpty(searchDTO.SortBy))
                {
                    sqlBuilder.Append($" ORDER BY {searchDTO.SortBy}");
                }

                //4. FINALIZAR CONSULTA SQL
                sqlBuilder.Append(";");


                //5. REALIZAR CONEXIÓN Y EJECUTAR CONSULTA
                using (var command = new MySqlCommand(sqlBuilder.ToString(), connection))
                {
                    //añadimos los pa´rametros si se han utilizado
                    if (!string.IsNullOrEmpty(searchDTO.Name))
                        //le ponemos la interpolación con los %por si está incompleto (LIKE de ddbb)
                        command.Parameters.AddWithValue("@Name", $"%{searchDTO.Name}%");

                    //como int es un tipo nullable, hay que poner le .value, sino da error al intentar acceder al numeroq ue contiene un int
                    if (searchDTO.DeathYear.HasValue)
                        command.Parameters.AddWithValue("@DeathYear", searchDTO.DeathYear.Value);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                           deceasedList.Add(new Deceased { Id = reader.GetInt32("Id"), 
                           FuneralHomeId = reader.GetInt32("FuneralHomeId"), 
                           GuardianId = reader.GetInt32("GuardianId"), 
                           StaffId = reader.GetInt32("StaffId"), 
                           Name = reader.GetString("Name"), 
                           Epitaph = reader.GetString("Epitaph"), 
                           Biography = reader.GetString("Biography"), 
                           PhotoURL = reader.GetString("PhotoURL"), 
                           BirthDate = reader.GetDateTime("BirthDate"), 
                           DeathDate = reader.GetDateTime("DeathDate") });
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
}