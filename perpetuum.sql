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
('Luz del Camino', 'B44556677', 'central@luzcamino.com', 'Paseo de la Castellana 100, Madrid', '912344556'),
('Tanatorio La Esperanza', 'B99887766', 'contacto@laesperanza.es', 'Calle Mayor 34, Valencia', '963210987'),
('Memorial Valle Verde', 'B11223344', 'info@valleverde.com', 'Carretera Nacional 202 km 15, Zaragoza', '976554433');

-- Staff (Con Admin y Empleados normales)
INSERT INTO Staff (FuneralHomeId, Name, Email, DNI, PasswordHash, IsAdmin) VALUES 
(1, 'Izarbe Bailo', 'admin@perpetuum.com', '00000000A', '$2a$11$jNRYr2iZD2xWNHtuXD6Gj.S4kU6OsQd/B/G49eo49pbuzOORQ1oh6', 1),
(1, 'Roberto García', 'roberto@sanjose.com', '12345678A', '$2a$11$jNRYr2iZD2xWNHtuXD6Gj.S4kU6OsQd/B/G49eo49pbuzOORQ1oh6', 0),
(2, 'Lucía Méndez', 'lucia@eternidad.es', '87654321B', '$2a$11$jNRYr2iZD2xWNHtuXD6Gj.S4kU6OsQd/B/G49eo49pbuzOORQ1oh6', 0),
(3, 'Beatriz Soto', 'beatriz@luzcamino.com', '55443322X', '$2a$11$jNRYr2iZD2xWNHtuXD6Gj.S4kU6OsQd/B/G49eo49pbuzOORQ1oh6', 0),
(4, 'Pablo Navarro', 'pablo@laesperanza.es', '22334455F', '$2a$11$jNRYr2iZD2xWNHtuXD6Gj.S4kU6OsQd/B/G49eo49pbuzOORQ1oh6', 0),
(4, 'Rosa Jiménez', 'rosa@laesperanza.es', '33445566G', '$2a$11$jNRYr2iZD2xWNHtuXD6Gj.S4kU6OsQd/B/G49eo49pbuzOORQ1oh6', 1),
(5, 'Fernando Díaz', 'fernando@valleverde.com', '44556677H', '$2a$11$jNRYr2iZD2xWNHtuXD6Gj.S4kU6OsQd/B/G49eo49pbuzOORQ1oh6', 0),
(1, 'Silvia Mora', 'silvia@sanjose.com', '55667788J', '$2a$11$jNRYr2iZD2xWNHtuXD6Gj.S4kU6OsQd/B/G49eo49pbuzOORQ1oh6', 0),
(2, 'Alberto Vega', 'alberto@eternidad.es', '66778899K', '$2a$11$jNRYr2iZD2xWNHtuXD6Gj.S4kU6OsQd/B/G49eo49pbuzOORQ1oh6', 0);

-- User (Público)
INSERT INTO `User` (Name, BirthDate, Email, PhoneNumber, PasswordHash) VALUES 
('Marta Sánchez', '1992-03-10', 'marta@email.com', '600111222', '$2a$11$jNRYr2iZD2xWNHtuXD6Gj.S4kU6OsQd/B/G49eo49pbuzOORQ1oh6'),
('Javier López', '1985-07-20', 'javier@email.com', '600333444', '$2a$11$jNRYr2iZD2xWNHtuXD6Gj.S4kU6OsQd/B/G49eo49pbuzOORQ1oh6'),
('Elena Prieto', '1978-11-05', 'elena@email.com', '600555666', '$2a$11$jNRYr2iZD2xWNHtuXD6Gj.S4kU6OsQd/B/G49eo49pbuzOORQ1oh6'),
('David Moreno', '1990-01-18', 'david@email.com', '611222333', '$2a$11$jNRYr2iZD2xWNHtuXD6Gj.S4kU6OsQd/B/G49eo49pbuzOORQ1oh6'),
('Isabel Fernández', '1982-09-25', 'isabel@email.com', '622333444', '$2a$11$jNRYr2iZD2xWNHtuXD6Gj.S4kU6OsQd/B/G49eo49pbuzOORQ1oh6');

-- MemorialGuardian (Familiares)
INSERT INTO MemorialGuardian (FuneralHomeId, StaffId, Name, DNI, Email, PhoneNumber, PasswordHash) VALUES 
(1, 2, 'Andrés Pérez ', '11223344C', 'andres@familia.com', '677888999', '$2a$11$jNRYr2iZD2xWNHtuXD6Gj.S4kU6OsQd/B/G49eo49pbuzOORQ1oh6'),
(2, 3, 'Carmen Ruiz ', '55667788D', 'carmen@familia.com', '677000111', '$2a$11$jNRYr2iZD2xWNHtuXD6Gj.S4kU6OsQd/B/G49eo49pbuzOORQ1oh6'),
(3, 4, 'Lucía Torres ', '77665544P', 'lucia@familia.com', '688111222', '$2a$11$jNRYr2iZD2xWNHtuXD6Gj.S4kU6OsQd/B/G49eo49pbuzOORQ1oh6'),
(1, 2, 'María Pérez ', '88776655Q', 'maria.perez@email.com', '699222333', '$2a$11$jNRYr2iZD2xWNHtuXD6Gj.S4kU6OsQd/B/G49eo49pbuzOORQ1oh6'),
(2, 3, 'Antonio Martínez ', '99887766R', 'antonio.martinez@email.com', '688333444', '$2a$11$jNRYr2iZD2xWNHtuXD6Gj.S4kU6OsQd/B/G49eo49pbuzOORQ1oh6'),
(4, 5, 'Patricia Gil ', '55443322S', 'patricia.gil@email.com', '677444555', '$2a$11$jNRYr2iZD2xWNHtuXD6Gj.S4kU6OsQd/B/G49eo49pbuzOORQ1oh6'),
(4, 6, 'Miguel Ángel Costa ', '44332211T', 'miguel.costa@email.com', '666555666', '$2a$11$jNRYr2iZD2xWNHtuXD6Gj.S4kU6OsQd/B/G49eo49pbuzOORQ1oh6'),
(5, 7, 'Ana Belén Ruiz ', '33221100U', 'ana.ruiz@email.com', '655666777', '$2a$11$jNRYr2iZD2xWNHtuXD6Gj.S4kU6OsQd/B/G49eo49pbuzOORQ1oh6');


-- Deceased (fotos: retratos ancianos/ancianas, sexo acorde; Unsplash CDN directo; Picsum no permite filtrar por edad/sexo)
INSERT INTO Deceased (Dni, FuneralHomeId, GuardianId, StaffId, Name, Epitaph, Biography, PhotoURL, BirthDate, DeathDate) VALUES 
('11111111A', 1, 1, 2, 'Don Manuel Pérez', 'Un hombre de campo que sembró amor y recogió respeto.', 'Manuel Pérez nació en un pequeño pueblo de Toledo en 1945. Creció entre olivos y viñas, y dedicó toda su vida al campo. Casado con Remedios durante 52 años, tuvo tres hijos y siete nietos. Fue alcalde pedáneo de su aldea durante dos mandatos y siempre defendió las tradiciones y la vida rural. Le gustaba contar historias junto al fuego y su generosidad con los vecinos era proverbial. Falleció en paz rodeado de los suyos.', 'https://images.unsplash.com/photo-1560250097-0b93528c311a?w=400&h=400&fit=crop', '1945-05-15', '2024-01-10'),
('22222222B', 2, 2, 3, 'Doña Sofía Martínez', 'Su luz sigue iluminando nuestro camino.', 'Sofía Martínez nació en Sevilla en 1952. Maestra de profesión, enseñó a leer y escribir a varias generaciones en su pueblo. Amaba la música, el flamenco y los libros. Tras enviudar joven, crió sola a sus dos hijos y nunca dejó de sonreír. Fundó un club de lectura en el barrio y fue voluntaria en Cáritas. Su bondad y su humor quedan en el recuerdo de quienes la conocieron.', 'https://images.unsplash.com/photo-1609137144813-7d9921338f24?w=400&h=400&fit=crop', '1952-08-22', '2023-12-15'),
('33333333C', 3, 3, 4, 'Ricardo Torres', 'Visionario que creyó en un mundo mejor.', 'Ricardo Torres (1938-2025) fue ingeniero y emprendedor. Nacido en Barcelona, fundó una empresa de energías renovables en los años ochenta, cuando casi nadie hablaba del cambio climático. Viajero incansable y lector voraz, dejó una biblioteca de más de tres mil volúmenes. Padre de Lucía y abuelo de dos nietos, siempre dijo que su mayor logro fue la familia. Murió sereno, en su casa junto al mar.', 'https://images.unsplash.com/photo-1552374196-c4e7ffc6e126?w=400&h=400&fit=crop', '1938-10-10', '2025-01-05'),
('44444444D', 1, 4, 2, 'Doña Remedios López', 'Madre, abuela y amiga para siempre.', 'Remedios López (1948-2024) nació en Cuenca. Trabajó toda su vida en una fábrica textil y crió a cuatro hijos con esfuerzo y sacrificio. Era la persona a la que todo el mundo acudía en busca de consejo. Sus croquetas y sus tortillas eran legendarias en las fiestas del pueblo. En sus últimos años disfrutó de sus diez nietos y de su huerto. Se fue dormida, como siempre quiso.', 'https://images.unsplash.com/photo-1573497019940-1c28c88b4f3e?w=400&h=400&fit=crop', '1948-03-12', '2024-06-20'),
('55555555E', 2, 5, 3, 'Doña Antonia Martínez Ruiz', 'Caballero de palabra y de corazón.', 'Antonio Martínez Ruiz (1940-2023) fue militar de carrera y después funcionario de prisiones. Nacido en Córdoba, mantuvo toda la vida un carácter recto y un gran sentido del deber. Aficionado a la caza y a la pesca, pasaba los veranos en la sierra con sus nietos. Querido por sus compañeros y por su familia, dejó un legado de honestidad y cariño. Descanse en paz.', 'https://images.unsplash.com/photo-1566616213894-2d4e1baee5d8?w=400&h=400&fit=crop', '1940-11-30', '2023-09-08'),
('77777777G', 4, 6, 5, 'Don Francisco Gil Serrano', 'Artesano de manos de oro y alma noble.', 'Francisco Gil (1955-2024) nació en Valencia. Ebanista de tercera generación, restauró muebles antiguos y enseñó el oficio a decenas de aprendices. Hombre tranquilo y reflexivo, leía poesía y paseaba por el Turia cada mañana. Padre de Patricia y abuelo de un nieto, dejó un taller lleno de proyectos por terminar. Su familia y sus amigos lo echan mucho de menos.', 'https://images.unsplash.com/photo-1607990281513-2c110a25bd8c?w=400&h=400&fit=crop', '1955-01-20', '2024-08-03'),
('88888888H', 4, 7, 6, 'Doña Concepción Costa', 'Flor del campo que nunca se marchitó.', 'Concepción Costa (1935-2023) nació en un cortijo de Jaén. Casi toda su vida la pasó en el campo, primero ayudando a sus padres y luego al frente de su propia familia. Madre de cinco hijos y abuela de catorce nietos, su casa estaba siempre abierta. Cocinería excepcional y devota de la Virgen de la Cabeza, fue el pilar de su pueblo. Se apagó con la misma serenidad con que vivió.', 'https://images.unsplash.com/photo-1594744803329-e58b31de8bf5?w=400&h=400&fit=crop', '1935-04-18', '2023-11-22'),
('99999999J', 5, 8, 7, 'Don José Ruiz Mendoza', 'Médico de cuerpos y de almas.', 'José Ruiz Mendoza (1950-2024) ejerció como médico de familia en Zaragoza durante más de cuarenta años. Nacido en Teruel, eligió la medicina rural antes de especializarse en atención primaria. Casado con Ana Belén, tuvo dos hijos que siguieron sus pasos. Colega generoso y paciente con los enfermos, muchos vecinos lo consideraban casi de la familia. Falleció tras una breve enfermedad, rodeado de los suyos.', 'https://images.unsplash.com/photo-1545167622-3a6ac756afa4?w=400&h=400&fit=crop', '1950-06-09', '2024-04-17'),
('10101010K', 1, 1, 8, 'Doña Pilar Fernández', 'Tejedora de recuerdos y de amor.', 'Pilar Fernández (1942-2023) nació en León. Modista de profesión, vistió a media generación del barrio: comuniones, bodas y eventos. Viuda desde los cincuenta, sacó adelante a sus dos hijas con trabajo y optimismo. Le encantaba el cine clásico y los viajes en autobús por España. Sus vecinos la recuerdan por su amabilidad y su tarta de manzana. Descansa en paz, Pilar.', 'https://images.unsplash.com/photo-1594824476967-48c8b964273f?w=400&h=400&fit=crop', '1942-12-01', '2023-07-11'),
('14141414N', 4, 6, 5, 'Don Vicente Gil', 'Constructor de sueños y de hogares.', 'Vicente Gil (1960-2024) fue arquitecto y aparejador en Valencia. Nacido en Alicante, diseñó viviendas sociales y edificios públicos con criterio y sensibilidad. Aficionado al senderismo y al jazz, era un padre entregado y un amigo leal. Dejó dos hijos y una obra que perdura en la ciudad. Su muerte prematura conmocionó a todos los que lo conocieron. Siempre en nuestro corazón.', 'https://images.unsplash.com/photo-1609220136736-443140cffec6?w=400&h=400&fit=crop', '1960-09-07', '2024-01-28');

-- Memory (Type 1=Condolencia, 2=Anécdota, 3=Foto. Status 0=Pendiente, 1=Aprobado. Fotos con URL Cloudinary formato 1:1)
INSERT INTO Memory (Type, Status, TextContent, MediaURL, AuthorRelation, DeceasedId, UserId) VALUES 
-- Difunto 1 - Manuel Pérez
(1, 1, 'Siento mucho vuestra pérdida. Don Manuel fue un vecino ejemplar y un amigo de verdad. Siempre tenía una palabra amable.', NULL, 'Amigo', 1, 1),
(1, 1, 'Siempre le recordaremos con cariño en las fiestas del pueblo. Las historias que contaba eran únicas.', NULL, 'Vecino', 1, 2),
(2, 1, 'Recuerdo la última vendimia que hicimos juntos. Manuel nos contó cómo era el campo cuando era niño. Ese día nos regaló un queso de la abuela y nos dijo: "Esto es lo que de verdad importa".', NULL, 'Vecino', 1, 3),
(3, 1, 'Foto del homenaje en la plaza del pueblo.', 'https://picsum.photos/seed/mem600/600/600', 'Vecino', 1, 4),
-- Difunto 2 - Sofía Martínez
(1, 1, 'Mis condolencias a la familia. Doña Sofía fue mi maestra y me enseñó a amar los libros.', NULL, 'Exalumna', 2, 3),
(2, 1, 'En el club de lectura siempre elegía los libros más bonitos. Recuerdo cuando nos leyó en voz alta el primer capítulo de Cien años de soledad. Se le saltaban las lágrimas y a nosotros también.', NULL, 'Compañera', 2, 5),
(3, 1, 'Sofía en la biblioteca del barrio, su segundo hogar.', 'https://picsum.photos/seed/mem601/600/600', 'Amiga', 2, 1),
-- Difunto 3 - Ricardo Torres
(1, 1, 'Un visionario y un caballero. Tuvo una vida plena y nos dejó un ejemplo a seguir.', NULL, 'Amigo', 3, 2),
(2, 1, 'La última vez que nos vimos me enseñó su huerto solar en la azotea. "Esto lo instalamos en el 89", dijo. Fue de los primeros. Siempre adelantado a su tiempo.', NULL, 'Colega', 3, 4),
(3, 1, 'Ricardo en su estudio, rodeado de libros.', 'https://picsum.photos/seed/mem600/600/600', 'Conocido', 3, 5),
-- Difunto 4 - Remedios López
(1, 1, 'Remedios era como una segunda madre para muchos. Sus croquetas y su cariño nos acompañarán siempre.', NULL, 'Vecina', 4, 1),
(2, 1, 'Cada Nochevieja nos invitaba a su casa y sacaba la tortilla de patatas más grande del mundo. Decía que la receta era de su madre y que no la escribiría nunca. Qué suerte haberla probado.', NULL, 'Amiga', 4, 2),
(3, 1, 'Remedios en la fiesta del barrio.', 'https://picsum.photos/seed/mem601/600/600', 'Vecina', 4, 4),
-- Difunto 5 - Antonio Martínez Ruiz
(1, 1, 'Doña Antonia fue un hombre íntegro y un buen amigo. Le recordaremos con respeto y cariño.', NULL, 'Compañero', 5, 3),
(2, 1, 'Cuando íbamos a pescar al pantano siempre llevaba el almuerzo repartido para todos. Y si alguien no había pescado nada, ella le regalaba uno de los suyos. Así era.', NULL, 'Amigo', 5, 5),
(3, 1, 'Antonia en la sierra con los nietos.', 'https://picsum.photos/seed/mem600/600/600', 'Vecino', 5, 4),
-- Difunto 6 - Francisco Gil (antes 7)
(1, 1, 'Paco era un maestro del oficio y una bellísima persona. El taller no será lo mismo sin él.', NULL, 'Aprendiz', 6, 2),
(2, 1, 'Me enseñó a encolar una silla sin que se notara. Decía que el buen trabajo es el que no se ve. Yo sigo usando sus trucos cada día.', NULL, 'Aprendiz', 6, 4),
(3, 1, 'Francisco en el taller con el último mueble que restauró.', 'https://picsum.photos/seed/mem600/600/600', 'Vecino', 6, 5),
-- Difunto 7 - Concepción Costa (antes 8)
(1, 1, 'Doña Concha era la matriarca del pueblo. Su casa y su corazón estaban siempre abiertos.', NULL, 'Vecina', 7, 1),
(2, 1, 'El día de la romería siempre montaba la mesa bajo el árbol. Había para cincuenta y nunca faltaba sitio. "El que no cabe es que no quiere", decía.', NULL, 'Amiga', 7, 4),
(3, 1, 'Concha en la cocina del cortijo.', 'https://picsum.photos/seed/mem601/600/600', 'Vecina', 7, 3),
-- Difunto 8 - José Ruiz Mendoza (antes 9)
(1, 1, 'El doctor Ruiz fue mi médico durante años. Un profesional excepcional y una gran persona.', NULL, 'Paciente', 8, 2),
(2, 1, 'Una vez fui a urgencias por la noche y me atendió él. Me reconoció y me preguntó por mi madre por su nombre. Eso no se olvida.', NULL, 'Paciente', 8, 1),
(3, 1, 'El doctor Ruiz en la consulta, como lo recordamos.', 'https://picsum.photos/seed/mem600/600/600', 'Vecino', 8, 5),
-- Difunto 9 - Pilar Fernández (antes 10)
(1, 1, 'Pilar me hizo el vestido de mi boda. Era una artista y una mujer encantadora.', NULL, 'Clienta', 9, 2),
(2, 1, 'Cuando me medía para el traje siempre me contaba historias del barrio. Me hizo el vestido de comunión a mi hija con el mismo patrón que el mío. Tres generaciones con sus manos.', NULL, 'Clienta', 9, 4),
(3, 1, 'Pilar en su taller de costura.', 'https://picsum.photos/seed/mem601/600/600', 'Vecina', 9, 1),
-- Difunto 10 - Vicente Gil (antes 13)
(1, 1, 'Vicente era un profesional brillante y un amigo leal. La ciudad pierde a uno de los mejores.', NULL, 'Colega', 10, 1),
(2, 1, 'Fuimos juntos a ver el solar del nuevo centro de salud. Me explicó cada detalle de luz y ventilación. Lo que hacía siempre tenía un porqué.', NULL, 'Amigo', 10, 4),
(3, 1, 'Vicente en la obra del último proyecto.', 'https://picsum.photos/seed/mem600/600/600', 'Conocido', 10, 5);