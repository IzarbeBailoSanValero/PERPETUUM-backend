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
(1, 2, 'Andrés Pérez (Hijo)', '11223344C', 'andres@familia.com', '677888999', '$2a$11$jNRYr2iZD2xWNHtuXD6Gj.S4kU6OsQd/B/G49eo49pbuzOORQ1oh6'),
(2, 3, 'Carmen Ruiz (Viuda)', '55667788D', 'carmen@familia.com', '677000111', '$2a$11$jNRYr2iZD2xWNHtuXD6Gj.S4kU6OsQd/B/G49eo49pbuzOORQ1oh6'),
(3, 4, 'Lucía Torres (Hija)', '77665544P', 'lucia@familia.com', '688111222', '$2a$11$jNRYr2iZD2xWNHtuXD6Gj.S4kU6OsQd/B/G49eo49pbuzOORQ1oh6'),
(1, 2, 'María Pérez (Nuera)', '88776655Q', 'maria.perez@email.com', '699222333', '$2a$11$jNRYr2iZD2xWNHtuXD6Gj.S4kU6OsQd/B/G49eo49pbuzOORQ1oh6'),
(2, 3, 'Antonio Martínez (Hijo)', '99887766R', 'antonio.martinez@email.com', '688333444', '$2a$11$jNRYr2iZD2xWNHtuXD6Gj.S4kU6OsQd/B/G49eo49pbuzOORQ1oh6'),
(4, 5, 'Patricia Gil (Hija)', '55443322S', 'patricia.gil@email.com', '677444555', '$2a$11$jNRYr2iZD2xWNHtuXD6Gj.S4kU6OsQd/B/G49eo49pbuzOORQ1oh6'),
(4, 6, 'Miguel Ángel Costa (Hermano)', '44332211T', 'miguel.costa@email.com', '666555666', '$2a$11$jNRYr2iZD2xWNHtuXD6Gj.S4kU6OsQd/B/G49eo49pbuzOORQ1oh6'),
(5, 7, 'Ana Belén Ruiz (Viuda)', '33221100U', 'ana.ruiz@email.com', '655666777', '$2a$11$jNRYr2iZD2xWNHtuXD6Gj.S4kU6OsQd/B/G49eo49pbuzOORQ1oh6');


-- Deceased
INSERT INTO Deceased (Dni, FuneralHomeId, GuardianId, StaffId, Name, Epitaph, Biography, PhotoURL, BirthDate, DeathDate) VALUES 
('11111111A', 1, 1, 2, 'Don Manuel Pérez', 'Un hombre de campo que sembró amor y recogió respeto.', 'Manuel Pérez nació en un pequeño pueblo de Toledo en 1945. Creció entre olivos y viñas, y dedicó toda su vida al campo. Casado con Remedios durante 52 años, tuvo tres hijos y siete nietos. Fue alcalde pedáneo de su aldea durante dos mandatos y siempre defendió las tradiciones y la vida rural. Le gustaba contar historias junto al fuego y su generosidad con los vecinos era proverbial. Falleció en paz rodeado de los suyos.', 'https://images.unsplash.com/photo-1560250097-0b93528c311a?w=400&h=400&fit=crop', '1945-05-15', '2024-01-10'),
('22222222B', 2, 2, 3, 'Doña Sofía Martínez', 'Su luz sigue iluminando nuestro camino.', 'Sofía Martínez nació en Sevilla en 1952. Maestra de profesión, enseñó a leer y escribir a varias generaciones en su pueblo. Amaba la música, el flamenco y los libros. Tras enviudar joven, crió sola a sus dos hijos y nunca dejó de sonreír. Fundó un club de lectura en el barrio y fue voluntaria en Cáritas. Su bondad y su humor quedan en el recuerdo de quienes la conocieron.', 'https://images.unsplash.com/photo-1609220136736-443140cffec6?w=400&h=400&fit=crop', '1952-08-22', '2023-12-15'),
('33333333C', 3, 3, 4, 'Ricardo Torres', 'Visionario que creyó en un mundo mejor.', 'Ricardo Torres (1938-2025) fue ingeniero y emprendedor. Nacido en Barcelona, fundó una empresa de energías renovables en los años ochenta, cuando casi nadie hablaba del cambio climático. Viajero incansable y lector voraz, dejó una biblioteca de más de tres mil volúmenes. Padre de Lucía y abuelo de dos nietos, siempre dijo que su mayor logro fue la familia. Murió sereno, en su casa junto al mar.', 'https://images.unsplash.com/photo-1552374196-c4e7ffc6e126?w=400&h=400&fit=crop', '1938-10-10', '2025-01-05'),
('44444444D', 1, 4, 2, 'Doña Remedios López', 'Madre, abuela y amiga para siempre.', 'Remedios López (1948-2024) nació en Cuenca. Trabajó toda su vida en una fábrica textil y crió a cuatro hijos con esfuerzo y sacrificio. Era la persona a la que todo el mundo acudía en busca de consejo. Sus croquetas y sus tortillas eran legendarias en las fiestas del pueblo. En sus últimos años disfrutó de sus diez nietos y de su huerto. Se fue dormida, como siempre quiso.', 'https://images.unsplash.com/photo-1573496350642-2c8eb13a2c0a?w=400&h=400&fit=crop', '1948-03-12', '2024-06-20'),
('55555555E', 2, 5, 3, 'Don Antonio Martínez Ruiz', 'Caballero de palabra y de corazón.', 'Antonio Martínez Ruiz (1940-2023) fue militar de carrera y después funcionario de prisiones. Nacido en Córdoba, mantuvo toda la vida un carácter recto y un gran sentido del deber. Aficionado a la caza y a la pesca, pasaba los veranos en la sierra con sus nietos. Querido por sus compañeros y por su familia, dejó un legado de honestidad y cariño. Descanse en paz.', 'https://images.unsplash.com/photo-1566616213894-2d4e1baee5d8?w=400&h=400&fit=crop', '1940-11-30', '2023-09-08'),
('66666666F', 3, 3, 4, 'Doña Elena Torres', 'La música de tu risa sigue sonando.', 'Elena Torres (1945-2024) fue pianista y profesora de música en el conservatorio de Madrid. Nacida en Valladolid, dio su primer concierto con catorce años. Casada con Ricardo durante 55 años, formó con él un equipo inseparable. Amaba la ópera, los gatos y el chocolate. Sus alumnos la recuerdan como exigente pero justa. Partió mientras escuchaba su pieza favorita.', 'https://images.unsplash.com/photo-1589391886645-d51941baf7fa?w=400&h=400&fit=crop', '1945-07-04', '2024-02-14'),
('77777777G', 4, 6, 5, 'Don Francisco Gil Serrano', 'Artesano de manos de oro y alma noble.', 'Francisco Gil (1955-2024) nació en Valencia. Ebanista de tercera generación, restauró muebles antiguos y enseñó el oficio a decenas de aprendices. Hombre tranquilo y reflexivo, leía poesía y paseaba por el Turia cada mañana. Padre de Patricia y abuelo de un nieto, dejó un taller lleno de proyectos por terminar. Su familia y sus amigos lo echan mucho de menos.', 'https://images.unsplash.com/photo-1607990281513-2c110a25bd8c?w=400&h=400&fit=crop', '1955-01-20', '2024-08-03'),
('88888888H', 4, 7, 6, 'Doña Concepción Costa', 'Flor del campo que nunca se marchitó.', 'Concepción Costa (1935-2023) nació en un cortijo de Jaén. Casi toda su vida la pasó en el campo, primero ayudando a sus padres y luego al frente de su propia familia. Madre de cinco hijos y abuela de catorce nietos, su casa estaba siempre abierta. Cocinería excepcional y devota de la Virgen de la Cabeza, fue el pilar de su pueblo. Se apagó con la misma serenidad con que vivió.', 'https://images.unsplash.com/photo-1594744803329-e58b31de8bf5?w=400&h=400&fit=crop', '1935-04-18', '2023-11-22'),
('99999999J', 5, 8, 7, 'Don José Ruiz Mendoza', 'Médico de cuerpos y de almas.', 'José Ruiz Mendoza (1950-2024) ejerció como médico de familia en Zaragoza durante más de cuarenta años. Nacido en Teruel, eligió la medicina rural antes de especializarse en atención primaria. Casado con Ana Belén, tuvo dos hijos que siguieron sus pasos. Colega generoso y paciente con los enfermos, muchos vecinos lo consideraban casi de la familia. Falleció tras una breve enfermedad, rodeado de los suyos.', 'https://images.unsplash.com/photo-1594824476967-48c8b964273f?w=400&h=400&fit=crop', '1950-06-09', '2024-04-17'),
('10101010K', 1, 1, 8, 'Doña Pilar Fernández', 'Tejedora de recuerdos y de amor.', 'Pilar Fernández (1942-2023) nació en León. Modista de profesión, vistió a media generación del barrio: comuniones, bodas y eventos. Viuda desde los cincuenta, sacó adelante a sus dos hijas con trabajo y optimismo. Le encantaba el cine clásico y los viajes en autobús por España. Sus vecinos la recuerdan por su amabilidad y su tarta de manzana. Descansa en paz, Pilar.', 'https://images.unsplash.com/photo-1545167622-3a6ac756afa4?w=400&h=400&fit=crop', '1942-12-01', '2023-07-11'),
('12121212L', 2, 2, 9, 'Don Luis Sánchez Mora', 'Marino de tierra y mar.', 'Luis Sánchez Mora (1936-2024) fue oficial de la Marina Mercante. Nacido en Cádiz, recorrió el mundo en barco durante décadas y trajo historias de todos los puertos. Jubilado, se instaló en Sevilla y se dedicó a escribir sus memorias y a cuidar el jardín. Padre de cuatro hijos y abuelo de nueve nietos, era el alma de las reuniones familiares. Se fue en calma, como las aguas que tanto amó.', 'https://images.unsplash.com/photo-1582552938357-32c906288ac8?w=400&h=400&fit=crop', '1936-08-25', '2024-05-30'),
('13131313M', 3, 3, 4, 'Doña Rosa Vega', 'Maestra de vida y de letras.', 'Rosa Vega (1947-2023) nació en Salamanca. Dio clase de Lengua y Literatura en un instituto de Madrid durante treinta y cinco años. Apasionada de Quevedo y de la Generación del 27, contagió a miles de alumnos el amor por la lectura. Viuda, sin hijos, dejó su herencia a una fundación de becas. Sus exalumnos la recuerdan con cariño y gratitud. Que la tierra te sea leve, Rosa.', 'https://images.unsplash.com/photo-1573497019940-1c28c88b4f3e?w=400&h=400&fit=crop', '1947-02-14', '2023-10-19'),
('14141414N', 4, 6, 5, 'Don Vicente Gil', 'Constructor de sueños y de hogares.', 'Vicente Gil (1960-2024) fue arquitecto y aparejador en Valencia. Nacido en Alicante, diseñó viviendas sociales y edificios públicos con criterio y sensibilidad. Aficionado al senderismo y al jazz, era un padre entregado y un amigo leal. Dejó dos hijos y una obra que perdura en la ciudad. Su muerte prematura conmocionó a todos los que lo conocieron. Siempre en nuestro corazón.', 'https://images.unsplash.com/photo-1609137144813-7d9921338f24?w=400&h=400&fit=crop', '1960-09-07', '2024-01-28');

-- Memory (Type 1=Condolencia, 2=Anécdota, 3=Foto. Status 0=Pendiente, 1=Aprobado. Fotos con URL Cloudinary formato 1:1)
INSERT INTO Memory (Type, Status, TextContent, MediaURL, AuthorRelation, DeceasedId, UserId) VALUES 
-- Difunto 1 - Manuel Pérez
(1, 1, 'Siento mucho vuestra pérdida. Don Manuel fue un vecino ejemplar y un amigo de verdad. Siempre tenía una palabra amable.', NULL, 'Amigo', 1, 1),
(1, 1, 'Siempre le recordaremos con cariño en las fiestas del pueblo. Las historias que contaba eran únicas.', NULL, 'Vecino', 1, 2),
(2, 1, 'Recuerdo la última vendimia que hicimos juntos. Manuel nos contó cómo era el campo cuando era niño. Ese día nos regaló un queso de la abuela y nos dijo: "Esto es lo que de verdad importa".', NULL, 'Vecino', 1, 3),
(3, 1, 'Foto del homenaje en la plaza del pueblo.', 'https://res.cloudinary.com/demo/image/upload/c_fill,g_center,ar_1:1,w_600/sample.jpg', 'Vecino', 1, 4),
-- Difunto 2 - Sofía Martínez
(1, 1, 'Mis condolencias a la familia. Doña Sofía fue mi maestra y me enseñó a amar los libros.', NULL, 'Exalumna', 2, 3),
(2, 1, 'En el club de lectura siempre elegía los libros más bonitos. Recuerdo cuando nos leyó en voz alta el primer capítulo de Cien años de soledad. Se le saltaban las lágrimas y a nosotros también.', NULL, 'Compañera', 2, 5),
(3, 1, 'Sofía en la biblioteca del barrio, su segundo hogar.', 'https://res.cloudinary.com/demo/image/upload/c_fill,g_center,ar_1:1,w_600/woman.jpg', 'Amiga', 2, 1),
-- Difunto 3 - Ricardo Torres
(1, 1, 'Un visionario y un caballero. Tuvo una vida plena y nos dejó un ejemplo a seguir.', NULL, 'Amigo', 3, 2),
(2, 1, 'La última vez que nos vimos me enseñó su huerto solar en la azotea. "Esto lo instalamos en el 89", dijo. Fue de los primeros. Siempre adelantado a su tiempo.', NULL, 'Colega', 3, 4),
(3, 1, 'Ricardo en su estudio, rodeado de libros.', 'https://res.cloudinary.com/demo/image/upload/c_fill,g_center,ar_1:1,w_600/sample.jpg', 'Conocido', 3, 5),
-- Difunto 4 - Remedios López
(1, 1, 'Remedios era como una segunda madre para muchos. Sus croquetas y su cariño nos acompañarán siempre.', NULL, 'Vecina', 4, 1),
(2, 1, 'Cada Nochevieja nos invitaba a su casa y sacaba la tortilla de patatas más grande del mundo. Decía que la receta era de su madre y que no la escribiría nunca. Qué suerte haberla probado.', NULL, 'Amiga', 4, 2),
(3, 1, 'Remedios en la fiesta del barrio.', 'https://res.cloudinary.com/demo/image/upload/c_fill,g_center,ar_1:1,w_600/woman.jpg', 'Vecina', 4, 4),
-- Difunto 5 - Antonio Martínez Ruiz
(1, 1, 'Don Antonio fue un hombre íntegro y un buen amigo. Le recordaremos con respeto y cariño.', NULL, 'Compañero', 5, 3),
(2, 1, 'Cuando íbamos a pescar al pantano siempre llevaba el almuerzo repartido para todos. Y si alguien no había pescado nada, él le regalaba uno de los suyos. Así era.', NULL, 'Amigo', 5, 5),
(3, 1, 'Antonio en la sierra con los nietos.', 'https://res.cloudinary.com/demo/image/upload/c_fill,g_center,ar_1:1,w_600/sample.jpg', 'Vecino', 5, 4),
-- Difunto 6 - Elena Torres
(1, 1, 'Elena era una artista y una persona maravillosa. La música pierde a una de las grandes.', NULL, 'Colega', 6, 1),
(2, 1, 'Me dio clase de piano durante años. El día de mi examen de acceso al conservatorio me dijo: "Toca como si solo existieras tú y la música". Lo llevo grabado.', NULL, 'Exalumna', 6, 5),
(3, 1, 'Elena al piano en un concierto benéfico.', 'https://res.cloudinary.com/demo/image/upload/c_fill,g_center,ar_1:1,w_600/woman.jpg', 'Amiga', 6, 3),
-- Difunto 7 - Francisco Gil
(1, 1, 'Paco era un maestro del oficio y una bellísima persona. El taller no será lo mismo sin él.', NULL, 'Aprendiz', 7, 2),
(2, 1, 'Me enseñó a encolar una silla sin que se notara. Decía que el buen trabajo es el que no se ve. Yo sigo usando sus trucos cada día.', NULL, 'Aprendiz', 7, 4),
(3, 1, 'Francisco en el taller con el último mueble que restauró.', 'https://res.cloudinary.com/demo/image/upload/c_fill,g_center,ar_1:1,w_600/sample.jpg', 'Vecino', 7, 5),
-- Difunto 8 - Concepción Costa
(1, 1, 'Doña Concha era la matriarca del pueblo. Su casa y su corazón estaban siempre abiertos.', NULL, 'Vecina', 8, 1),
(2, 1, 'El día de la romería siempre montaba la mesa bajo el árbol. Había para cincuenta y nunca faltaba sitio. "El que no cabe es que no quiere", decía.', NULL, 'Amiga', 8, 4),
(3, 1, 'Concha en la cocina del cortijo.', 'https://res.cloudinary.com/demo/image/upload/c_fill,g_center,ar_1:1,w_600/woman.jpg', 'Vecina', 8, 3),
-- Difunto 9 - José Ruiz Mendoza
(1, 1, 'El doctor Ruiz fue mi médico durante años. Un profesional excepcional y una gran persona.', NULL, 'Paciente', 9, 2),
(2, 1, 'Una vez fui a urgencias por la noche y me atendió él. Me reconoció y me preguntó por mi madre por su nombre. Eso no se olvida.', NULL, 'Paciente', 9, 1),
(3, 1, 'El doctor Ruiz en la consulta, como lo recordamos.', 'https://res.cloudinary.com/demo/image/upload/c_fill,g_center,ar_1:1,w_600/sample.jpg', 'Vecino', 9, 5),
-- Difunto 10 - Pilar Fernández
(1, 1, 'Pilar me hizo el vestido de mi boda. Era una artista y una mujer encantadora.', NULL, 'Clienta', 10, 2),
(2, 1, 'Cuando me medía para el traje siempre me contaba historias del barrio. Me hizo el vestido de comunión a mi hija con el mismo patrón que el mío. Tres generaciones con sus manos.', NULL, 'Clienta', 10, 4),
(3, 1, 'Pilar en su taller de costura.', 'https://res.cloudinary.com/demo/image/upload/c_fill,g_center,ar_1:1,w_600/woman.jpg', 'Vecina', 10, 1),
-- Difunto 11 - Luis Sánchez Mora
(1, 1, 'Luis tenía historias de medio mundo. Un hombre de mar y de palabra. Descansa en paz.', NULL, 'Amigo', 11, 3),
(2, 1, 'En las cenas familiares sacaba el álbum de fotos de los puertos. Bombay, Hamburgo, Buenos Aires... Nos hacía viajar sin salir del salón.', NULL, 'Vecino', 11, 1),
(3, 1, 'Luis en el jardín con sus rosales.', 'https://res.cloudinary.com/demo/image/upload/c_fill,g_center,ar_1:1,w_600/sample.jpg', 'Conocido', 11, 5),
-- Difunto 12 - Rosa Vega
(1, 1, 'Doña Rosa fue mi profesora de Literatura. Me abrió los ojos a la poesía. Gracias por todo.', NULL, 'Exalumna', 12, 4),
(2, 1, 'Nos hizo memorizar el "Érase un hombre a una nariz pegado" y lo recitamos en clase. Se reía tanto que tuvimos que parar. La poesía para ella era alegría.', NULL, 'Exalumno', 12, 3),
(3, 1, 'Rosa en la sala de profesores, con su taza de té.', 'https://res.cloudinary.com/demo/image/upload/c_fill,g_center,ar_1:1,w_600/woman.jpg', 'Colega', 12, 2),
-- Difunto 13 - Vicente Gil
(1, 1, 'Vicente era un profesional brillante y un amigo leal. La ciudad pierde a uno de los mejores.', NULL, 'Colega', 13, 1),
(2, 1, 'Fuimos juntos a ver el solar del nuevo centro de salud. Me explicó cada detalle de luz y ventilación. Lo que hacía siempre tenía un porqué.', NULL, 'Amigo', 13, 4),
(3, 1, 'Vicente en la obra del último proyecto.', 'https://res.cloudinary.com/demo/image/upload/c_fill,g_center,ar_1:1,w_600/sample.jpg', 'Conocido', 13, 5);