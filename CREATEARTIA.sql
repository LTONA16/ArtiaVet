USE ArtiaVet;
GO
CREATE TABLE TipoUsuario (
    id INT PRIMARY KEY IDENTITY(1,1),
    tipo VARCHAR(50) NOT NULL CHECK (tipo IN ('Admin', 'Veterinario', 'Recepcionista'))
);
GO
CREATE TABLE Usuarios (
    id INT PRIMARY KEY IDENTITY(1,1),
    tipoUsuario INT NOT NULL,
    username VARCHAR(100) NOT NULL,
    password VARBINARY(256) NOT NULL,
    nombre VARCHAR(100) NOT NULL,
    imagen VARBINARY(MAX),
    CONSTRAINT FK_Usuarios_TipoUsuario FOREIGN KEY (tipoUsuario)
        REFERENCES TipoUsuario(id)
);
GO
CREATE TABLE Dueños (
    id INT PRIMARY KEY IDENTITY(1,1),
    nombre VARCHAR(200) NOT NULL,
    email VARCHAR(200) NOT NULL,
    numeroTelefono VARCHAR(10)
);
GO
CREATE TABLE TiposAnimales (
    id INT PRIMARY KEY IDENTITY(1,1),
    tipo VARCHAR(50) NOT NULL
);
GO
CREATE TABLE Alergias (
    id INT PRIMARY KEY IDENTITY(1,1),
    nombre VARCHAR(100) NOT NULL
);
GO
CREATE TABLE Mascotas (
    id INT PRIMARY KEY IDENTITY(1,1),
    tipoAnimal INT NOT NULL,
    dueñoID INT NOT NULL,
    raza VARCHAR(50),
    edad INT,
    nombre VARCHAR(50) NOT NULL,
    notas VARCHAR(500),
    CONSTRAINT FK_Mascotas_TipoAnimal FOREIGN KEY (tipoAnimal)
        REFERENCES TiposAnimales(id),
    CONSTRAINT FK_Mascotas_Dueño FOREIGN KEY (dueñoID)
        REFERENCES Dueños(id)
);
GO
CREATE TABLE AlergiasMascotas (
    id INT PRIMARY KEY IDENTITY(1,1),
    alergiaID INT NOT NULL,
    mascotaID INT NOT NULL,
    CONSTRAINT FK_AlergiasMascotas_Alergia FOREIGN KEY (alergiaID)
        REFERENCES Alergias(id),
    CONSTRAINT FK_AlergiasMascotas_Mascota FOREIGN KEY (mascotaID)
        REFERENCES Mascotas(id)
);
GO
CREATE TABLE TiposCitas (
    id INTEGER PRIMARY KEY IDENTITY(1,1),
    nombre VARCHAR(50) NOT NULL,
    importe DECIMAL(10,2) NOT NULL
);
GO
CREATE TABLE Insumos (
    id INTEGER PRIMARY KEY IDENTITY(1,1),
    nombre VARCHAR(100) NOT NULL,
    precioUnitario DECIMAL(8,2) NOT NULL
);
GO
CREATE TABLE Inventario (
    id INTEGER PRIMARY KEY IDENTITY(1,1),
    insumoID INT UNIQUE NOT NULL, -- UNIQUE para asegurar la relación 1:1 con Insumos
    cantidad DECIMAL(8,2) NOT NULL CHECK (cantidad >= 0),
    CONSTRAINT FK_Inventario_Insumo FOREIGN KEY (insumoID)
        REFERENCES Insumos(id)
);
GO
CREATE TABLE Citas (
    id INTEGER PRIMARY KEY IDENTITY(1,1),
    veterinarioID INT NOT NULL,
    mascotaID INT NOT NULL,
    tipoCitaID INT NOT NULL,
    fechaCita DATE NOT NULL,
    importeAdicional DECIMAL(10,2) DEFAULT 0.00,
    observaciones VARCHAR(500),
    
    -- Relación con la tabla Usuarios (el veterinario)
    CONSTRAINT FK_Citas_Veterinario FOREIGN KEY (veterinarioID)
        REFERENCES Usuarios(id),
    
    -- Relación con la tabla Mascotas (ya existente)
    CONSTRAINT FK_Citas_Mascota FOREIGN KEY (mascotaID)
        REFERENCES Mascotas(id),
        
    -- Relación con la tabla TiposCitas
    CONSTRAINT FK_Citas_TipoCita FOREIGN KEY (tipoCitaID)
        REFERENCES TiposCitas(id)
);
GO
CREATE TABLE InsumosDeCitas (
    id INTEGER PRIMARY KEY IDENTITY(1,1),
    citaID INT NOT NULL,
    insumoID INT NOT NULL,
    cantidadUsada DECIMAL(8,2) NOT NULL,
    
    -- Restricción para asegurar que no se repita un insumo en la misma cita
    CONSTRAINT UQ_InsumoPorCita UNIQUE (citaID, insumoID),
    
    -- Relación con la tabla Citas
    CONSTRAINT FK_InsumosDeCitas_Cita FOREIGN KEY (citaID)
        REFERENCES Citas(id),
        
    -- Relación con la tabla Insumos
    CONSTRAINT FK_InsumosDeCitas_Insumo FOREIGN KEY (insumoID)
        REFERENCES Insumos(id)
);
GO
CREATE TABLE RecordatoriosDueños (
    id INT PRIMARY KEY IDENTITY(1,1),
    citaID INT NOT NULL,
    fechaRecordatorio DATE NOT NULL,
    asunto VARCHAR(50) NOT NULL,
    mensaje VARCHAR(500),
    
    -- Relación con la tabla Citas
    CONSTRAINT FK_RecordatoriosDueños_Cita FOREIGN KEY (citaID)
        REFERENCES Citas(id)
);
GO
CREATE TABLE Facturas (
    id INT PRIMARY KEY IDENTITY(1,1),
    citaID INT UNIQUE NOT NULL, -- UNIQUE para asegurar que solo haya una factura por cita (Relación 1:1)
    monto DECIMAL(12,2) NOT NULL,
    iva DECIMAL(12,2) NOT NULL,
    total DECIMAL(12,2) NOT NULL,
    
    -- Relación con la tabla Citas
    CONSTRAINT FK_Facturas_Cita FOREIGN KEY (citaID)
        REFERENCES Citas(id)
);
GO