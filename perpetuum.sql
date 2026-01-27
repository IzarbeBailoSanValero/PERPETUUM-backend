-- =======================================================
-- 0. CONFIGURACIÓN INICIAL (REINICIO LIMPIO)
-- =======================================================
DROP DATABASE IF EXISTS PerpetuumDB;
CREATE DATABASE PerpetuumDB CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE PerpetuumDB;

-- =======================================================
-- 1. CREACIÓN DE TABLAS
-- =======================================================

-- 1. FUNERAL_HOME 
CREATE TABLE FuneralHome (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    Name VARCHAR(255) NOT NULL, 
    CIF VARCHAR(255) UNIQUE NOT NULL, 
    ContactEmail VARCHAR(255) UNIQUE NOT NULL, 
    Address VARCHAR(255) NOT NULL, 
    PhoneNumber VARCHAR(255) NOT NULL
);

-- 2. STAFF (Empleados y Admins)
-- Rol: Se determina por la columna 'IsAdmin'
CREATE TABLE Staff (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    FuneralHomeId INT NOT NULL,
    Name VARCHAR(255) NOT NULL,
    DNI VARCHAR(20) UNIQUE NOT NULL, 
    Email VARCHAR(255) UNIQUE NOT NULL, 
    PasswordHash VARCHAR(255) NOT NULL, 
    IsAdmin BOOLEAN DEFAULT FALSE,      -- TRUE = Admin, FALSE = Staff
    FOREIGN KEY (FuneralHomeId) REFERENCES FuneralHome(Id) ON DELETE CASCADE
);

-- 3. USER (Público General)
-- Rol Implícito: "StandardUser"
CREATE TABLE `User` ( 
    Id INT PRIMARY KEY AUTO_INCREMENT,
    Name VARCHAR(255) NOT NULL, 
    BirthDate DATE, 
    Email VARCHAR(255) UNIQUE NOT NULL, 
    PasswordHash VARCHAR(255) NOT NULL, 
    PhoneNumber VARCHAR(255) 
);

-- 4. MEMORIAL_GUARDIAN (Familiares Responsables)
-- Rol Implícito: "Guardian"
CREATE TABLE MemorialGuardian (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    FuneralHomeId INT NOT NULL, 
    StaffId INT NOT NULL, -- Empleado que lo registró
    Name VARCHAR(255) NOT NULL, 
    DNI VARCHAR(255) UNIQUE NOT NULL, 
    Email VARCHAR(255) UNIQUE NOT NULL, 
    PasswordHash VARCHAR(255) NOT NULL, 
    PhoneNumber VARCHAR(255) NOT NULL, 
    FOREIGN KEY (FuneralHomeId) REFERENCES FuneralHome(Id) ON DELETE CASCADE, 
    FOREIGN KEY (StaffId) REFERENCES Staff(Id) ON DELETE CASCADE
);

-- 5. DECEASED (Difuntos)
CREATE TABLE Deceased (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    Dni VARCHAR(20) NOT NULL UNIQUE,
    FuneralHomeId INT NOT NULL,
    GuardianId INT NOT NULL, -
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

-- 6. MEMORY (Condolencias y Recuerdos)
CREATE TABLE Memory (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    CreatedDate DATETIME DEFAULT CURRENT_TIMESTAMP, 
    Type INT NOT NULL, -- 1=Texto, 2=Imagen, etc.
    Status INT NOT NULL DEFAULT 0, -- 0=Pendiente, 1=Aprobado, 2= denegado
    TextContent TEXT, 
    MediaURL VARCHAR(500), 
    AuthorRelation VARCHAR(255),
    DeceasedId INT NOT NULL,
    UserId INT NOT NULL, 
    FOREIGN KEY (DeceasedId) REFERENCES Deceased(Id) ON DELETE CASCADE,
    FOREIGN KEY (UserId) REFERENCES `User`(Id) ON DELETE CASCADE
);

-- =======================================================
-- 2. INSERCIÓN DE DATOS DE PRUEBA (SEEDING)
-- NOTA: Todos los PasswordHash corresponden a la contraseña: "123456"
-- =======================================================

-- Funeral Homes
INSERT INTO FuneralHome (Name, CIF, ContactEmail, Address, PhoneNumber) VALUES 
('Tanatorio San José', 'B12345678', 'info@sanjose.com', 'Av. de la Paz 10, Madrid', '910000001'),
('Eternidad Servicios', 'B87654321', 'gestion@eternidad.es', 'Calle del Recuerdo 5, Sevilla', '954000002'),
('Luz del Camino', 'B44556677', 'central@luzcamino.com', 'Paseo de la Castellana 100, Madrid', '912344556');

-- Staff (Con Admin y Empleados normales)
INSERT INTO Staff (FuneralHomeId, Name, Email, DNI, PasswordHash, IsAdmin) VALUES 
(1, 'Izarbe Bailo', 'admin@perpetuum.com', '00000000A', '$2a$11$Z5.1.2.3.4.5.6.7.8.9.0.A.B.C.D.E', 1), -- ESTE ERES TÚ (ADMIN)
(1, 'Roberto García', 'roberto@sanjose.com', '12345678A', '$2a$11$Z5.1.2.3.4.5.6.7.8.9.0.A.B.C.D.E', 0), -- Staff normal
(2, 'Lucía Méndez', 'lucia@eternidad.es', '87654321B', '$2a$11$Z5.1.2.3.4.5.6.7.8.9.0.A.B.C.D.E', 0),
(3, 'Beatriz Soto', 'beatriz@luzcamino.com', '55443322X', '$2a$11$Z5.1.2.3.4.5.6.7.8.9.0.A.B.C.D.E', 0);

-- User (Público)
INSERT INTO `User` (Name, BirthDate, Email, PhoneNumber, PasswordHash) VALUES 
('Marta Sánchez', '1992-03-10', 'marta@email.com', '600111222', '$2a$11$Z5.1.2.3.4.5.6.7.8.9.0.A.B.C.D.E'),
('Javier López', '1985-07-20', 'javier@email.com', '600333444', '$2a$11$Z5.1.2.3.4.5.6.7.8.9.0.A.B.C.D.E'),
('Elena Prieto', '1978-11-05', 'elena@email.com', '600555666', '$2a$11$Z5.1.2.3.4.5.6.7.8.9.0.A.B.C.D.E');

-- MemorialGuardian (Familiares)
INSERT INTO MemorialGuardian (FuneralHomeId, StaffId, Name, DNI, Email, PhoneNumber, PasswordHash) VALUES 
(1, 2, 'Andrés Pérez (Hijo)', '11223344C', 'andres@familia.com', '677888999', '$2a$11$Z5.1.2.3.4.5.6.7.8.9.0.A.B.C.D.E'),
(2, 3, 'Carmen Ruiz (Viuda)', '55667788D', 'carmen@familia.com', '677000111', '$2a$11$Z5.1.2.3.4.5.6.7.8.9.0.A.B.C.D.E'),
(3, 4, 'Lucía Torres (Hija)', '77665544P', 'lucia@familia.com', '688111222', '$2a$11$Z5.1.2.3.4.5.6.7.8.9.0.A.B.C.D.E');

-- Deceased
INSERT INTO Deceased (Dni, FuneralHomeId, GuardianId, StaffId, Name, Epitaph, Biography, PhotoURL, BirthDate, DeathDate) VALUES 
('11111111A', 1, 1, 2, 'Don Manuel Pérez', 'Un hombre de campo', 'Bio de Manuel...', 'https://via.placeholder.com/150', '1945-05-15', '2024-01-10'),
('22222222B', 2, 2, 3, 'Doña Sofía Martínez', 'Su luz ilumina', 'Bio de Sofía...', 'https://via.placeholder.com/150', '1952-08-22', '2023-12-15'),
('33333333C', 3, 3, 4, 'Ricardo Torres', 'Visionario', 'Bio de Ricardo...', 'https://via.placeholder.com/150', '1938-10-10', '2025-01-05');

-- Memory
INSERT INTO Memory (Type, Status, TextContent, MediaURL, AuthorRelation, DeceasedId, UserId) VALUES 
(1, 1, 'Siento mucho vuestra pérdida...', NULL, 'Amigo', 1, 1),
(1, 1, 'Siempre le recordaremos con cariño.', NULL, 'Vecino', 1, 2),
(1, 0, 'Mis condolencias a la familia.', NULL, 'Conocido', 2, 3);