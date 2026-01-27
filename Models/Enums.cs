//idea IA: utilizar enum en lugar de magic numbers --> más documentado, mejor
namespace PERPETUUM.Models;

public enum MemoryStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2
}

public enum MemoryType{
Condolence = 1,
Anecdote = 2,
Photo =3
}