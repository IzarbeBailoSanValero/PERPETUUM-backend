





// public async Task<int> CreateGuardianAsync(GuardianCreateDTO dto)
// {
//     // ... validaciones de DNI ...

//     // HASHEAR ANTES DE GUARDAR
//     string passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

//     var guardian = new MemorialGuardian
//     {
//         FuneralHomeId = dto.FuneralHomeId,
//         StaffId = dto.StaffId,
//         Name = dto.Name,
//         Dni = dto.Dni,
//         Email = dto.Email,
//         PhoneNumber = dto.PhoneNumber,
//         PasswordHash = passwordHash // Guardamos el hash
//     };

//     return await _repository.AddAsync(guardian);
// }