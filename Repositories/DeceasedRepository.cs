
//importaciones
using MySqlConnector;
using CHUCHOS.Models;
using CHUCHOS.DTOs;
using Microsoft.Extensions.Logging;

namespace CHUCHOS.Repositories;

public class ChuchoRepository : IChuchoRepository
{
    //variables privadas
    private readonly string _connectionString;
    private readonly ILogger<ChuchoRepository> _logger;


    //constructor e inyección DI
    public ChuchoRepository(IConfiguration configuration, ILogger<ChuchoRepository> logger)
    {
        _connectionString = configuration.GetConnectionString("ChuchosDB") ?? throw new ArgumentNullException("La cadena 'ChuchosDB' no existe en appsettings");
        _logger = logger;
    }

    //tasks
    public async Task<List<Chucho>> GetAllAsync()                               //en getall no imprimo las adoptionrequest, las imprimire en get by id, aquí será null

    {
        _logger.LogInformation("iniciando GetAllAsync en chucho");

        //defino vairable de retorno
        var chuchos = new List<Chucho>();
        try
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                //1. abro conexión
                await connection.OpenAsync();

                //2. construyo la query
                string query = "SELECT Id, Name, Breed, Background, PhotoUrl, ApproximateBirth, EntryDate, ExitDate, ShelterId, VolunteerId FROM Chucho";

                using (var command = new MySqlCommand(query, connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        //obtengo el índice de la columna null porque no deja acceder por nombre d ecolumna --> me devuelve el numero de la columna. Isdbnull espera un numero 
                        int exitDateOrdinal = reader.GetOrdinal("ExitDate");
                        while (await reader.ReadAsync())
                        {
                            chuchos.Add(new Chucho
                            {
                                Id = reader.GetInt32("Id"),
                                Name = reader.GetString("Name"),
                                Breed = reader.GetString("Breed"),
                                Background = reader.GetString("Background"),
                                PhotoUrl = reader.GetString("PhotoUrl"),
                                ApproximateBirth = reader.GetDateTime("ApproximateBirth"),
                                EntryDate = reader.GetDateTime("EntryDate"),
                                // Usar el índice:
                                ExitDate = reader.IsDBNull(exitDateOrdinal) ? (DateTime?)null : reader.GetDateTime(exitDateOrdinal),
                                ShelterId = reader.GetInt32("ShelterId"),
                                VolunteerId = reader.GetInt32("VolunteerId")
                            });
                        }
                    }
                }


            }
            _logger.LogInformation("petición a BBDD GetAllAsync para chucho exitosa.");
            return chuchos;
        }
        catch (MySqlException ex)
        {
            _logger.LogError(ex, $"error de MYSQL en petición a BBDD GetAllAsync. Error: {ex.Message}");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"error en petición a BBDD GetAllAsync. Error: {ex.Message}");
            throw;
        }

    }



    public async Task<Chucho?> GetByIdAsync(int id)                                    // aquí si imprimo las fichas de oslicitud de adopción
    {
        _logger.LogInformation("iniciando GetByIdAsync en chucho");

        Chucho? chucho = null;

        try
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                //primero saco el chucho y luego la lista de solicitudes para ese chucho, sobre la que iteraré para pintar resultados
                string query = @"
                SELECT Id, Name, Breed, Background, PhotoUrl, ApproximateBirth, EntryDate, ExitDate, ShelterId, VolunteerId FROM Chucho WHERE Id = @Id;
                SELECT Id, RequestDate, RequestMessage, RequestState, ChuchoRequestedId, UserRequesterId FROM AdoptionRequest WHERE Id = @Id;
                ";

                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            //obtengo el índice de la columna null porque no deja acceder por nombre d ecolumna --> me devuelve el numero de la columna. Isdbnull espera un numero 
                            int exitDateOrdinal = reader.GetOrdinal("ExitDate");
                            chucho = (new Chucho
                            {
                                Id = reader.GetInt32("Id"),
                                Name = reader.GetString("Name"),
                                Breed = reader.GetString("Breed"),
                                Background = reader.GetString("Background"),
                                PhotoUrl = reader.GetString("PhotoUrl"),
                                ApproximateBirth = reader.GetDateTime("ApproximateBirth"),
                                EntryDate = reader.GetDateTime("EntryDate"),
                                // Usar el índice:
                                ExitDate = reader.IsDBNull(exitDateOrdinal) ? (DateTime?)null : reader.GetDateTime(exitDateOrdinal),
                                ShelterId = reader.GetInt32("ShelterId"),
                                VolunteerId = reader.GetInt32("VolunteerId"),
                                Requests = new List<AdoptionRequest>()

                            });
                        }
                        if (chucho == null)
                        {
                            _logger.LogWarning($"Petición a BBDD GetByIdAsync fallida. Chucho con id:{id} NO encontrado");
                            return null;
                        }

                        if (await reader.NextResultAsync()) //si hay siguiente conjunto de datos --> las request
                        {
                            var requests = new List<AdoptionRequest>();
                            while (await reader.ReadAsync())
                            {
                                var req = new AdoptionRequest
                                {
                                    Id = reader.GetInt32("Id"),
                                    RequestDate = reader.GetDateTime("RequestDate"),
                                    RequestMessage = reader.GetString("RequestMessage"),
                                    RequestState = reader.GetInt32("RequestState"),
                                    ChuchoRequestedId = reader.GetInt32("ChuchoRequestedId"),
                                    UserRequesterId = reader.GetInt32("UserRequesterId")
                                };
                                requests.Add(req);
                            }

                            chucho.Requests = requests;

                        }

                    }
                }


            }

            return chucho; //chucho encontrado o null

        }
        catch (MySqlException ex)
        {
            _logger.LogError(ex, $"error de MYSQL en petición a BBDD GetByIdAsync. Error: {ex.Message}");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"error en petición a BBDD GetByIdAsync. Error: {ex.Message}");
            throw;
        }
    }


    public async Task<int> AddAsync(Chucho chucho)
    {
        _logger.LogInformation("Iniciando AddAsync en Chucho");

        int newId = 0;

        try
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                string query = @"
               INSERT INTO Chucho (Name, Breed, Background, PhotoUrl, ApproximateBirth, EntryDate, ExitDate, ShelterId, VolunteerId) 
            VALUES (@Name, @Breed, @Background, @PhotoUrl, @ApproximateBirth, @EntryDate, @ExitDate, @ShelterId, @VolunteerId);
            SELECT LAST_INSERT_ID();
            ";

                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Name", chucho.Name);
                    command.Parameters.AddWithValue("@Breed", chucho.Breed);
                    command.Parameters.AddWithValue("@Background", chucho.Background);
                    command.Parameters.AddWithValue("@PhotoUrl", chucho.PhotoUrl);
                    command.Parameters.AddWithValue("@ApproximateBirth", chucho.ApproximateBirth);
                    command.Parameters.AddWithValue("@EntryDate", chucho.EntryDate);
                    //(lo he buscado)
                    //el null de c# no es le mismo que el de la base de datos. Primero compruebo si el objeto que quiero insertar tiene null en la propiedad y sino fijo el valor en el DBnull que acepta la base d edatos
                    //hay que castearlo a object porque : el ternario espera valores del mismo tipo. como tengodatetime vs dbnull, casteo el datetime a object (el tipo más genérico para que lo acepte )
                    command.Parameters.AddWithValue("@ExitDate", chucho.ExitDate != null ? (object)chucho.ExitDate.Value : DBNull.Value);

                    command.Parameters.AddWithValue("@ShelterId", chucho.ShelterId);
                    command.Parameters.AddWithValue("@VolunteerId", chucho.VolunteerId);


                    // Obtengo el ID generado, he buscado cómo.
                    /*
                    Execute Scalar ejecuta consultaSQL + devuelve primera fila + primera colimna --> LAST_INSERT_ID()
                    Devuelve un Object, por si no devuelve nada(null) manejamos la conversión con null ? 0: otro caso
                    no hacemos cast inmediato (int)numero por si ddbb devuelve long, decimal... -->convert.ToInt32 (en lugar de convertir solo int, convierte tipos compatibles)
                    */
                    var result = await command.ExecuteScalarAsync();
                    newId = result is null ? 0 : Convert.ToInt32(result);

                }
            }

            return newId;
        }
        catch (MySqlException ex) //en principio no porque no hay restricciones unique
        {
            _logger.LogError(ex, $"Error de MYSQL en AddAsync. Error: {ex.Message}");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error general en AddAsync. Error: {ex.Message}");
            throw;
        }
    }



    public async Task<bool> UpdateAsync(Chucho chucho)
    {
        bool hasBeenUpdated = false;
        try
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                string query = @"
           UPDATE Chucho
                SET
                    Name = @Name,
                    Breed = @Breed,
                    Background = @Background,
                    PhotoUrl = @PhotoUrl,
                    ApproximateBirth = @ApproximateBirth,
                    EntryDate = @EntryDate,
                    ExitDate = @ExitDate,
                    ShelterId = @ShelterId,
                    VolunteerId = @VolunteerId
                WHERE Id = @Id;
                ";

                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", chucho.Id);
                    command.Parameters.AddWithValue("@Name", chucho.Name);
                    command.Parameters.AddWithValue("@Breed", chucho.Breed);
                    command.Parameters.AddWithValue("@Background", chucho.Background);
                    command.Parameters.AddWithValue("@PhotoUrl", chucho.PhotoUrl);
                    command.Parameters.AddWithValue("@ApproximateBirth", chucho.ApproximateBirth);
                    command.Parameters.AddWithValue("@EntryDate", chucho.EntryDate);
                    command.Parameters.AddWithValue("@ExitDate", chucho.ExitDate.HasValue ? (object)chucho.ExitDate.Value : DBNull.Value);
                    command.Parameters.AddWithValue("@ShelterId", chucho.ShelterId);
                    command.Parameters.AddWithValue("@VolunteerId", chucho.VolunteerId);

                    //nonQueryAsync --> ejecuta + devuelve numero de registros aceptados. Si es mayor que 0, ha funcionado
                    int rowsAffected = await command.ExecuteNonQueryAsync();

                    if (rowsAffected > 0)
                    {
                        hasBeenUpdated = true;
                    }


                }
            }
        }
        catch (MySqlException ex)
        {
            _logger.LogError(ex, $"Error de MYSQL en UpdateAsync. Error: {ex.Message}");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error general en UpdateAsync. Error: {ex.Message}");
            throw;
        }
        return hasBeenUpdated;
    }



    public async Task<bool> DeleteAsync(int id)
    {
        bool hasBeenDeleted = false;

        try
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                string query = @" DELETE FROM Chucho WHERE Id = @Id; ";

                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);



                    //nonQueryAsync --> ejecuta + devuelve numero de registros aceptados. Si es mayor que 0, ha funcionado
                    int rowsAffected = await command.ExecuteNonQueryAsync();

                    if (rowsAffected > 0)
                    {
                        hasBeenDeleted = true;
                    }


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





    //funcionalidad hecha con ayuda
    public async Task<List<Chucho>> SearchAsync(ChuchoSearchDTO searchDTO)
    {
        _logger.LogInformation("inicio de busqueda");

        //inicializo lista a devolver
        var chuchos = new List<Chucho>();

        try
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                //1. variable que creará la query, inicializo
                //stringbuilder sirve para crear strings mediante concatenación
                var sqlBuilder = new System.Text.StringBuilder();

                //where 1 = 1 para ir podiendo hacer AND otra condición de carrera
                sqlBuilder.Append("SELECT Id, Name, Breed, Background, PhotoUrl, ApproximateBirth, EntryDate, ExitDate, ShelterId, VolunteerId FROM Chucho WHERE 1=1 ");

                // FILTROS

                // si es mentira que el string llamado breed está vacío --> añado AND
                if (!string.IsNullOrEmpty(searchDTO.Breed))
                {
                    sqlBuilder.Append(" AND Breed LIKE @Breed ");
                }

                //La función TIMESTAMPDIFF()  sirve para calcular la diferencia entre dos valores de fecha o fecha-hora, devolviendo el resultado en la unidad de tiempo especificada
                //unidad --> years, fecha de inicio, fecha de fin (hoy) --> da la edad <= edad maxima
                if (searchDTO.MaxAge.HasValue)
                {
                    sqlBuilder.Append(" AND TIMESTAMPDIFF(YEAR, ApproximateBirth, CURDATE()) <= @MaxAge ");
                }


                if (!string.IsNullOrEmpty(searchDTO.SortBy))
                {
                    sqlBuilder.Append($" ORDER BY {searchDTO.SortBy}");
                }

                // Cerramos la sentencia SQL
                sqlBuilder.Append(";");

                // query ya creada entera, ahora la ejecutamos

                using (var command = new MySqlCommand(sqlBuilder.ToString(), connection))
                {
                    // Solo añadimos los parámetros de los filtros si se usaron
                    if (!string.IsNullOrEmpty(searchDTO.Breed))
                        command.Parameters.AddWithValue("@Breed", $"%{searchDTO.Breed}%");

                    if (searchDTO.MaxAge.HasValue)
                        command.Parameters.AddWithValue("@MaxAge", searchDTO.MaxAge.Value);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            chuchos.Add(new Chucho
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                Name = reader["Name"].ToString(),
                                Breed = reader["Breed"].ToString(),
                                Background = reader["Background"].ToString(),
                                PhotoUrl = reader["PhotoUrl"].ToString(),
                                ApproximateBirth = Convert.ToDateTime(reader["ApproximateBirth"]),
                                EntryDate = Convert.ToDateTime(reader["EntryDate"]),
                                // Validación básica de nulo para fecha de salida
                                ExitDate = reader["ExitDate"] == DBNull.Value ? null : Convert.ToDateTime(reader["ExitDate"]),
                                ShelterId = Convert.ToInt32(reader["ShelterId"]),
                                VolunteerId = Convert.ToInt32(reader["VolunteerId"])
                            });
                        }
                    }
                }
            }
            return chuchos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en búsqueda");
            throw;
        }
    }













    public async Task<List<Opinion>> GetOpinionsByChuchoIdAsync(int chuchoId)
    {
        _logger.LogInformation($"iniciando GetOpinionsByChuchoIdAsync en chucho con id {chuchoId}");

        List<Opinion> opinionListModels = new List<Opinion>();


        try
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                string query = "SELECT Id, OpinionDate, OpinionMessage, Puntuation, ChuchoId, UserId FROM Opinion WHERE ChuchoId = @chuchoId;";



                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@chuchoId", chuchoId);


                    using (var reader = await command.ExecuteReaderAsync())
                    {

                        while (await reader.ReadAsync())
                        {
                            var newOpinion = new Opinion
                            {
                                Id = reader.GetInt32("Id"),
                                OpinionDate = reader.GetDateTime("OpinionDate"),
                                OpinionMessage = reader.GetString("OpinionMessage"),
                                Puntuation = reader.GetInt32("Puntuation"),
                                ChuchoId = reader.GetInt32("ChuchoId"),
                                UserId = reader.GetInt32("UserId")
                            };

                            opinionListModels.Add(newOpinion);


                        }
                    }
                }


            }


            _logger.LogInformation($"petición a BBDD GetOpinionsByIdAsync exitosa.  se ha encontrado el listado de opiniones del chucho  con id {chuchoId} ");


            return opinionListModels;

        }
        catch (MySqlException ex)
        {
            _logger.LogError(ex, $"error de MYSQL en petición a BBDD GetOpinionsByIdAsync. Error: {ex.Message}");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"error en petición a BBDD GetOpinionsByIdAsync. Error: {ex.Message}");
            throw;
        }


    }





















}