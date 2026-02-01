public async Task<MemorialGuardian?> GetByEmailAsync(string email)
{
    using var connection = new MySqlConnection(_connectionString);
    await connection.OpenAsync();
    string query = "SELECT * FROM MemorialGuardian WHERE Email = @Email";
    
    using var command = new MySqlCommand(query, connection);
    command.Parameters.AddWithValue("@Email", email);
    
    using var reader = await command.ExecuteReaderAsync();
    if (await reader.ReadAsync())
    {
        return new MemorialGuardian
        {
            Id = reader.GetInt32("Id"),
            Name = reader.GetString("Name"),
            Email = reader.GetString("Email"),
            PasswordHash = reader.GetString("PasswordHash"), 
            FuneralHomeId = reader.GetInt32("FuneralHomeId")
        };
    }
    return null;
}