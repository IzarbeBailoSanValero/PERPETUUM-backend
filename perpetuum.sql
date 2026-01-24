CREATE DATABASE IF NOT EXISTS PerpetuumDB CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE PerpetuumDB;

-- 1. FUNERAL_HOME 
CREATE TABLE FuneralHome (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    Name VARCHAR(255) NOT NULL, 
    CIF VARCHAR(255) UNIQUE NOT NULL, 
    ContactEmail VARCHAR(255) UNIQUE NOT NULL, 
    Address VARCHAR(255) NOT NULL, 
    PhoneNumber VARCHAR(255) NOT NULL
);

-- 2. STAFF 
CREATE TABLE Staff (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    FuneralHomeId INT NOT NULL,
    Name VARCHAR(255) NOT NULL,
    Email VARCHAR(255) UNIQUE NOT NULL, 
    DNI VARCHAR(10), 
    FOREIGN KEY (FuneralHomeId) REFERENCES FuneralHome(Id) ON DELETE CASCADE
);

-- 3. USER 
CREATE TABLE `User` ( 
    Id INT PRIMARY KEY AUTO_INCREMENT,
    Name VARCHAR(255) NOT NULL, 
    BirthDate DATE, 
    Email VARCHAR(255) UNIQUE NOT NULL, 
    PhoneNumber VARCHAR(255) 
);

-- 4. MEMORIAL_GUARDIAN
CREATE TABLE MemorialGuardian (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    FuneralHomeId INT NOT NULL, 
    StaffId INT NOT NULL, 
    Name VARCHAR(255) NOT NULL, 
    DNI VARCHAR(255) UNIQUE NOT NULL, 
    Email VARCHAR(255) UNIQUE NOT NULL, 
    PhoneNumber VARCHAR(255) NOT NULL, 
    FOREIGN KEY (FuneralHomeId) REFERENCES FuneralHome(Id) ON DELETE CASCADE, 
    FOREIGN KEY (StaffId) REFERENCES Staff(Id) ON DELETE CASCADE
);

-- 5. DECEASED 
CREATE TABLE Deceased (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    Dni VARCHAR(20) NOT NULL UNIQUE,
    FuneralHomeId INT NOT NULL,
    GuardianId INT NOT NULL, 
    StaffId INT NOT NULL, 
    Name VARCHAR(255) NOT NULL, 
    Epitaph VARCHAR(255) NOT NULL, 
    Biography TEXT NOT NULL, 
    PhotoURL VARCHAR(500) NOT NULL, 
    BirthDate DATE NOT NULL, 
    DeathDate DATE NOT NULL, 
    FOREIGN KEY (FuneralHomeId) REFERENCES FuneralHome(Id) ON DELETE CASCADE,
    FOREIGN KEY (GuardianId) REFERENCES MemorialGuardian(Id) ON DELETE CASCADE,
    FOREIGN KEY (StaffId) REFERENCES Staff(Id) ON DELETE CASCADE
);

-- 6. MEMORY
CREATE TABLE Memory (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    CreatedDate DATETIME DEFAULT CURRENT_TIMESTAMP, 
    Type INT NOT NULL, 
    Status INT NOT NULL DEFAULT 0, 
    TextContent TEXT, 
    MediaURL VARCHAR(500), 
    AuthorRelation VARCHAR(255), 
    DeceasedId INT NOT NULL,
    UserId INT NOT NULL,
    FOREIGN KEY (DeceasedId) REFERENCES Deceased(Id) ON DELETE CASCADE,
    FOREIGN KEY (UserId) REFERENCES `User`(Id) ON DELETE CASCADE
);

-- INSERTS CORREGIDOS
INSERT INTO FuneralHome (Name, CIF, ContactEmail, Address, PhoneNumber) VALUES 
('Tanatorio San José', 'B12345678', 'info@sanjose.com', 'Av. de la Paz 10, Madrid', '910000001'),
('Eternidad Servicios', 'B87654321', 'gestion@eternidad.es', 'Calle del Recuerdo 5, Sevilla', '954000002'),
('Luz del Camino', 'B44556677', 'central@luzcamino.com', 'Paseo de la Castellana 100, Madrid', '912344556'),
('Memoria Eterna Galicia', 'B99887766', 'contacto@memoriagalicia.es', 'Rúa Nova 15, Santiago', '981554433');

INSERT INTO Staff (FuneralHomeId, Name, Email, DNI) VALUES 
(1, 'Roberto García', 'roberto.staff@sanjose.com', '12345678A'),
(2, 'Lucía Méndez', 'lucia.mendez@eternidad.es', '87654321B'),
(1, 'Alberto Ruiz', 'alberto@sanjose.com', '12345678Z'),
(3, 'Beatriz Soto', 'b.soto@luzcamino.com', '55443322X'),
(4, 'Xoán García', 'xoan@memoriagalicia.es', '99001122M');

INSERT INTO `User` (Name, BirthDate, Email, PhoneNumber) VALUES 
('Marta Sánchez', '1992-03-10', 'marta.sanchez@email.com', '600111222'),
('Javier López', '1985-07-20', 'javier.lopez@email.com', '600333444'),
('Elena Prieto', '1978-11-05', 'elena.prieto@email.com', '600555666'),
('Carlos Herrera', '1970-01-01', 'carlos.herrera@radio.fm', '600000001'),
('Sofía Lorente', '1995-12-12', 'sofia.lorente@diseno.com', '600000002'),
('Manuel Vizcaíno', '1965-05-05', 'mvizcaino@empresa.es', '600000003'),
('Isabel Pantoja', '1980-08-08', 'isabel@musica.com', '600000004'),
('Fernando Alonso', '1981-07-29', 'fernando@racing.com', '600000005');

INSERT INTO MemorialGuardian (FuneralHomeId, StaffId, Name, DNI, Email, PhoneNumber) VALUES 
(1, 1, 'Andrés Pérez (Hijo)', '11223344C', 'andres.perez@familia.com', '677888999'),
(2, 2, 'Carmen Ruiz (Viuda)', '55667788D', 'carmen.ruiz@familia.com', '677000111'),
(3, 4, 'Lucía Torres (Hija)', '77665544P', 'lucia.torres@gmail.com', '688111222'),
(4, 5, 'Brais Méndez (Hermano)', '33445566L', 'brais.m@outlook.es', '622333444'),
(1, 3, 'Elena García (Sobrina)', '22334455K', 'elena.g@familia.es', '633444555');

INSERT INTO Deceased (Dni, FuneralHomeId, GuardianId, StaffId, Name, Epitaph, Biography, PhotoURL, BirthDate, DeathDate) VALUES 
('11111111A', 1, 1, 1, 'Don Manuel Pérez', 'Un hombre de campo', 'Bio...', 'https://...', '1945-05-15', '2024-01-10'),
('22222222B', 2, 2, 2, 'Doña Sofía Martínez', 'Su luz...', 'Bio...', 'https://...', '1952-08-22', '2023-12-15'),
('33333333C', 3, 3, 4, 'Ricardo Torres', 'Visionario...', 'Bio...', 'https://...', '1938-10-10', '2025-01-05'),
('44444444D', 4, 4, 5, 'Uxía Castro', 'A voz...', 'Bio...', 'https://...', '1942-02-28', '2025-01-12'),
('55555555E', 1, 5, 3, 'Julian García', 'Abuelo...', 'Bio...', 'https://...', '1930-11-11', '2025-01-18');


INSERT INTO Memory (Type, Status, TextContent, MediaURL, AuthorRelation, DeceasedId, UserId) VALUES 
(1, 1, 'Siento mucho vuestra pérdida...', NULL, 'Amigo', 3, 1),
(2, 1, 'Recuerdo su consejo...', NULL, 'Compañero', 3, 2),
(3, 1, 'Foto del equipo...', 'https://...', 'Compañero', 3, 3),
(1, 0, 'Mis condolencias.', NULL, 'Familiar', 3, 4),
(1, 1, 'Grazas por tanto...', NULL, 'Seguidor', 4, 5),
(2, 1, 'O día que recitou...', NULL, 'Amigo', 4, 1),
(3, 1, 'Primeira edición...', 'https://...', 'Alumno', 4, 2),
(2, 0, 'Me contó una historia...', NULL, 'Vecino', 4, 3),
(1, 1, 'Buen viaje...', NULL, 'Amigo', 5, 4),
(1, 1, 'Todo nuestro apoyo...', NULL, 'Vecina', 5, 2),
(3, 1, 'Julian paseando...', 'https://...', 'Familiar', 5, 1),
(2, 1, 'Nunca olvidaré...', NULL, 'Amigo', 5, 3),
(3, 0, 'Foto Navidad.', 'https://...', 'Sobrino', 5, 5),
(1, 0, 'Descanse en paz.', NULL, 'Familiar', 5, 1),
(2, 1, 'Me enseñó a jugar...', NULL, 'Amigo', 5, 2);