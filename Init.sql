IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = '--DB_NAME')
BEGIN
    CREATE DATABASE --DB_NAME;
END

GO

IF NOT EXISTS (SELECT * FROM sys.server_principals WHERE name = '--USER_NAME')
BEGIN
    CREATE LOGIN --USER_NAME WITH PASSWORD = '--PASSWORD';
END
GO

USE --DB_NAME;

GO

IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = '--USER_NAME')
BEGIN
    CREATE USER --USER_NAME FOR LOGIN --USER_NAME;
END
GO

ALTER ROLE db_owner ADD MEMBER --USER_NAME;
GO

-- 1. Tablas

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'CatTipoCliente')
BEGIN
	CREATE TABLE CatTipoCliente(
		Id INT IDENTITY(1,1) PRIMARY KEY,
		TipoCliente VARCHAR(50) NOT NULL
	);
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'TblClientes')
BEGIN
	CREATE TABLE TblClientes(
		Id INT IDENTITY(1,1) PRIMARY KEY,
		RazonSocial VARCHAR(100) NOT NULL,
		IdTipoCliente INT NOT NULL,
		FechaCreacion DATE NOT NULL DEFAULT GETDATE(),
		RFC VARCHAR(50) NOT NULL,

		CONSTRAINT FK_Clientes_TipoCliente
			FOREIGN KEY (IdTipoCliente) 
			REFERENCES CatTipoCliente (Id)
	);
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'TblFacturas')
BEGIN
	CREATE TABLE TblFacturas(
		Id INT IDENTITY(1,1) PRIMARY KEY,
		FechaEmisionFactura DATETIME NOT NULL DEFAULT GETDATE(),
		IdCliente INT NOT NULL,
		NumeroFactura INT UNIQUE NOT NULL,
		NumeroTotalArticulos INT NOT NULL DEFAULT 0,
		SubTotalFactura DECIMAL(18,2) NOT NULL DEFAULT 0,
		TotalImpuesto DECIMAL(18,2) NOT NULL DEFAULT 0,
		TotalFactura DECIMAL(18,2) NOT NULL DEFAULT 0,

		CONSTRAINT FK_Facturas_Clientes
			FOREIGN KEY (IdCliente) 
			REFERENCES TblClientes (Id)
	);
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'CatProductos')
BEGIN
	CREATE TABLE CatProductos(
		Id INT IDENTITY(1,1) PRIMARY KEY,
		NombreProducto VARCHAR(50) NOT NULL,
		ImagenProducto VARCHAR(200),
		PrecioUnitario DECIMAL(18, 2) NOT NULL DEFAULT 0 ,
		ext VARCHAR(5)
	);
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'TblDetallesFactura')
BEGIN
	CREATE TABLE TblDetallesFactura(
		Id INT IDENTITY(1,1) PRIMARY KEY,
		IdFactura INT NOT NULL,
		IdProducto INT NOT NULL,
		CantidadDeProducto INT NOT NULL DEFAULT 1,
		PrecioUnitarioProducto DECIMAL(18, 2) NOT NULL,
		SubtotalProducto DECIMAL(18,2) NOT NULL,
		Notas VARCHAR(200),

		CONSTRAINT FK_DetallesFactura_Factura
			FOREIGN KEY (IdFactura) 
			REFERENCES TblFacturas (Id),

		CONSTRAINT FK_DetallesFactura_Producto
			FOREIGN KEY (IdProducto) 
			REFERENCES CatProductos (Id)
	);
END;

GO

-- Tabla y SP adicional para normalizar el manejo de respuestas

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'CatRespuestas')
BEGIN
	CREATE TABLE CatRespuestas(
		Codigo INT PRIMARY KEY,
		Mensaje NVARCHAR(200) NOT NULL
	);

END;

GO

-- 1.5 Clean Tables

-- Tipo de respuestas
DELETE FROM CatRespuestas;

-- Detalles primero
DELETE FROM TblDetallesFactura;
DBCC CHECKIDENT ('TblDetallesFactura', RESEED, 0);

-- Luego facturas
DELETE FROM TblFacturas;
DBCC CHECKIDENT ('TblFacturas', RESEED, 0);

-- Luego clientes
DELETE FROM TblClientes;
DBCC CHECKIDENT ('TblClientes', RESEED, 0);

-- Luego productos
DELETE FROM CatProductos;
DBCC CHECKIDENT ('CatProductos', RESEED, 0);

-- Por último tipos de cliente
DELETE FROM CatTipoCliente;
DBCC CHECKIDENT ('CatTipoCliente', RESEED, 0);

-- 2. Seeders

INSERT INTO CatRespuestas (Codigo, Mensaje) VALUES
	(0, 'Success'),
	(1, 'Client not found'),
	(2, 'Product not found'),
	(3, 'Validation error'),
	(4, 'Product out stock'),
	(5, 'Invoice not found'),
	(400, 'Bad request'),
	(204, 'Not Content'),
	(500, 'Internal server error');

INSERT INTO CatTipoCliente (TipoCliente) VALUES 
    ('Empresa'),
    ('Distribuidor');


INSERT INTO TblClientes (RazonSocial, IdTipoCliente, FechaCreacion, RFC) VALUES
    ('Geek Store S.A.', 1, GETDATE(), 'GKS123456789'),
    ('Collector´s Hub Ltda.', 2, GETDATE(), 'CHB987654321');


INSERT INTO CatProductos (NombreProducto, PrecioUnitario, ImagenProducto, ext) VALUES
    ('Iron Man', 59.99, NULL, 'jpg'),
    ('Harry Potter', 49.99, NULL, 'png'),
    ('Darth Vader', 69.99, 'https://i.postimg.cc/jd2TQPNs/719.webp', 'jpg');
GO

CREATE OR ALTER PROCEDURE sp_Response
	@Code INT,
	@Data SQL_VARIANT = NULL
AS
BEGIN
	SET NOCOUNT ON;

	IF EXISTS (SELECT 1 FROM sysobjects WHERE name = 'CatRespuestas' AND xtype = 'U')
	BEGIN
		SELECT TOP 1
			cr.Codigo as Code,
			cr.Mensaje as Message,
			@Data AS Data
		FROM CatRespuestas cr
		WHERE cr.Codigo = @Code;
	END
	ELSE
	BEGIN
		SELECT 
			@Code AS Code,
			CASE @Code
				WHEN 0 THEN 'Success'
				WHEN 500 THEN 'Internal server error'
				ELSE 'Unknown  code'
			END AS Message,
			@Data as Data;
	END
END;

GO

-- Tipados para transacciones
DROP PROCEDURE IF EXISTS sp_CreateInvoice;
GO

DROP TYPE IF EXISTS InvoiceDetailType;
GO

CREATE TYPE InvoiceDetailType AS TABLE
(
    IdProducto INT,
    Cantidad INT,
    Notas NVARCHAR(200)
);

GO

-- Creación de SP para la gestión del proyecto
-- SP para la creación de productos
CREATE OR ALTER PROCEDURE sp_CreateProduct
	@NombreProducto VARCHAR(50),
	@ImagenProducto VARBINARY(MAX) = NULL,
	@PrecioUnitario DECIMAL(18,2),
	@Ext VARCHAR(5) = NULL
AS 
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY
		BEGIN TRANSACTION;

		INSERT INTO CatProductos (NombreProducto, ImagenProducto, PrecioUnitario, ext)
		VALUES (@NombreProducto, @ImagenProducto, @PrecioUnitario, @Ext);

		DECLARE @NewId INT = SCOPE_IDENTITY();

		COMMIT TRANSACTION;

		EXEC sp_Response 0, @NewId;
	END TRY
	BEGIN CATCH
		ROLLBACK TRANSACTION;

		EXEC sp_Response 500, NULL;
	END CATCH
END;

GO

-- SP para obtener productos
CREATE OR ALTER PROCEDURE sp_GetProducts
	@Id INT = NULL,
	@Offset INT = 1,
	@Limit INT = 10
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
		IF @Id IS NULL
		BEGIN
			DECLARE @Total INT;
			SELECT @Total = COUNT(*) FROM CatProductos;
			IF (@Total > 0)
			BEGIN
				EXEC sp_Response 0, @Total;

				SELECT 
					p.Id as Id,
					p.NombreProducto as Name,
					p.ext as Ext,
					p.ImagenProducto as Image,
					p.PrecioUnitario as Price
				FROM CatProductos p
				ORDER BY p.Id DESC
				OFFSET (@Offset - 1) * @Limit ROWS
				FETCH NEXT @Limit ROWS ONLY;
			END
			ELSE
			BEGIN
				EXEC sp_Response 204, NULL;
			END
		END
		ELSE
		BEGIN
			IF EXISTS (SELECT 1 FROM CatProductos WHERE Id = @Id)
			BEGIN
				EXEC sp_Response 0, 1;
				SELECT 
					Id as Id,
					NombreProducto as Name,
					ext as Ext,
					ImagenProducto as Image,
					PrecioUnitario as Price
				FROM CatProductos WHERE Id = @Id;
			END
			ELSE
			BEGIN
				EXEC sp_Response 2, NUll;
			END
		END
    END TRY
    BEGIN CATCH
        EXEC sp_Response 500, NULL;
    END CATCH
END;
GO

-- SP para obtener clientes
CREATE OR ALTER PROCEDURE sp_GetClients
	@Id INT = NULL,
	@Offset INT = 1,
	@Limit INT = 10
AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY
		IF @Id IS NULL
		BEGIN
			DECLARE @Total INT;
			SELECT @Total = COUNT(*) FROM TblClientes;

			IF (@Total > 0)
			BEGIN
				EXEC sp_Response 0, @Total;

				SELECT 
					c.Id,
					c.FechaCreacion as CreatedAt,
					c.RazonSocial as BusinessName,
					c.RFC as Reference,
					tc.TipoCliente as ClientType
				FROM TblClientes c
				INNER JOIN CatTipoCliente tc ON c.IdTipoCliente = tc.Id
				ORDER BY c.FechaCreacion DESC
				OFFSET (@Offset - 1) * @Limit ROWS
				FETCH NEXT @Limit ROWS ONLY;
			END
			ELSE
			BEGIN
				EXEC sp_Response 204, NULL;
			END
		END
		ELSE 
		BEGIN
			IF EXISTS (SELECT 1 FROM TblClientes WHERE Id = @Id)
			BEGIN
				EXEC sp_Response 0, 1;
				SELECT 
					c.Id,
					c.FechaCreacion as CreatedAt,
					c.RazonSocial as BusinessName,
					c.RFC as Reference,
					tc.TipoCliente as ClientType
				FROM TblClientes c
				INNER JOIN CatTipoCliente tc ON c.IdTipoCliente = tc.Id
				WHERE c.Id = @Id;
			END
			ELSE
			BEGIN
				EXEC sp_Response 1, NULL;
			END
		END
	END TRY
	BEGIN CATCH
		EXEC sp_Response 500, NULL;
	END CATCH
END;

GO

-- SP para crear Facturas
CREATE OR ALTER PROCEDURE sp_CreateInvoice
	@IdCliente INT,
	@NumeroFactura INT,
	@Detalles InvoiceDetailType READONLY
AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY
		BEGIN TRANSACTION;

		IF NOT EXISTS(SELECT 1 FROM TblClientes WHERE Id = @IdCliente)
		BEGIN
			ROLLBACK TRANSACTION;
			EXEC sp_Response 1, NULL;
			RETURN;
		END

		IF EXISTS (SELECT 1 FROM TblFacturas WHERE NumeroFactura = @NumeroFactura)
		BEGIN
			ROLLBACK TRANSACTION;
			EXEC sp_Response 400, NULL;
			RETURN;
		END

		IF NOT EXISTS (SELECT 1 FROM @Detalles)
		BEGIN
			ROLLBACK TRANSACTION;
			EXEC sp_Response 400, NULL;
			RETURN;
		END

		INSERT INTO TblFacturas(
			IdCliente,
			NumeroFactura,
			NumeroTotalArticulos,
			SubTotalFactura,
			TotalImpuesto,
			TotalFactura
		)
		VALUES (
			@IdCliente,
			@NumeroFactura,
			0, 0, 0, 0
		);

		DECLARE @NewInvoiceId INT = SCOPE_IDENTITY();

		INSERT INTO TblDetallesFactura (
			IdFactura,
			IdProducto,
			CantidadDeProducto,
			PrecioUnitarioProducto,
			SubtotalProducto,
			Notas
		)
		SELECT 
			@NewInvoiceId,
			d.IdProducto,
			d.Cantidad,
			p.PrecioUnitario,
			(d.Cantidad * p.PrecioUnitario),
			d.Notas
		FROM @Detalles d
		INNER JOIN CatProductos p ON d.IdProducto = p.Id;

		UPDATE TblFacturas
		SET NumeroTotalArticulos = (SELECT SUM(CantidadDeProducto) FROM TblDetallesFactura WHERE IdFactura = @NewInvoiceId),
			SubTotalFactura = (SELECT SUM(SubtotalProducto) FROM TblDetallesFactura WHERE IdFactura = @NewInvoiceId),
			TotalImpuesto = (SELECT SUM(SubtotalProducto) * 0.19 FROM TblDetallesFactura WHERE IdFactura = @NewInvoiceId),
			TotalFactura = (SELECT SUM(SubtotalProducto) * 1.19 FROM TblDetallesFactura WHERE IdFactura = @NewInvoiceId)
		WHERE Id = @NewInvoiceId;

		COMMIT TRANSACTION;

		EXEC sp_Response 0, @NewInvoiceId;
	END TRY
	BEGIN CATCH
		ROLLBACK TRANSACTION;
		EXEC sp_Response 500, NULL;
	END CATCH
END;
GO

CREATE OR ALTER PROCEDURE sp_GetInvoices
    @Id INT = NULL,
	@Offset INT = 1,
	@Limit INT = 10
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        IF @Id IS NULL
        BEGIN
			DECLARE @Total INT;
			SELECT @Total = COUNT(*) FROM TblFacturas;
			
			IF (@Total > 0)
			BEGIN
				EXEC sp_Response 0, @Total;

				SELECT 
					f.Id,
					f.FechaEmisionFactura AS InvoiceDate,
					f.NumeroFactura AS InvoiceNumber,
					c.RazonSocial AS ClientName,
					tc.TipoCliente AS ClientType,
					f.NumeroTotalArticulos AS TotalItems,
					f.SubTotalFactura AS SubTotalAmount,
					f.TotalImpuesto AS TaxAmount,
					f.TotalFactura AS TotalAmount
				FROM TblFacturas f
				INNER JOIN TblClientes c ON f.IdCliente = c.Id
				INNER JOIN CatTipoCliente tc ON f.IdCliente = tc.Id
				 ORDER BY f.FechaEmisionFactura DESC
				OFFSET (@Offset - 1) * @Limit ROWS
				FETCH NEXT @Limit ROWS ONLY;

				SELECT 
					d.Id,
					d.IdFactura AS InvoiceId,
					d.IdProducto AS ProductId,
					p.NombreProducto AS ProductName,
					d.CantidadDeProducto AS Quantity,
					d.PrecioUnitarioProducto AS UnitPrice,
					d.SubtotalProducto AS SubTotalPrice,
					d.Notas AS Notes
				FROM TblDetallesFactura d
				INNER JOIN CatProductos p ON d.IdProducto = p.Id
				WHERE d.IdFactura IN (
					SELECT f.Id
					FROM TblFacturas f
					ORDER BY f.FechaEmisionFactura DESC
					OFFSET (@Offset - 1) * @Limit ROWS
					FETCH NEXT @Limit ROWS ONLY
				);
            END
			ELSE
			BEGIN
				EXEC sp_Response 204, NULL;
			END
        END
        ELSE
        BEGIN
		    IF EXISTS (SELECT 1 FROM TblFacturas WHERE Id = @Id)
			BEGIN

				EXEC sp_Response 0, @Id;

				SELECT 
					f.Id,
					f.FechaEmisionFactura AS InvoiceDate,
					f.NumeroFactura AS InvoiceNumber,
					c.RazonSocial AS ClientName,
					tc.TipoCliente AS ClientType,
					f.NumeroTotalArticulos AS TotalItems,
					f.SubTotalFactura AS SubTotalAmount,
					f.TotalImpuesto AS TaxAmount,
					f.TotalFactura AS TotalAmount
				FROM TblFacturas f
				INNER JOIN TblClientes c ON f.IdCliente = c.Id
				INNER JOIN CatTipoCliente tc ON f.IdCliente = tc.Id
				WHERE f.Id = @Id;

				SELECT 
					d.Id,
					d.IdFactura AS InvoiceId,
					d.IdProducto AS ProductId,
					p.NombreProducto AS ProductName,
					d.CantidadDeProducto AS Quantity,
					d.PrecioUnitarioProducto AS UnitPrice,
					d.SubtotalProducto AS SubTotalPrice,
					d.Notas AS Notes
				FROM TblDetallesFactura d
				INNER JOIN CatProductos p ON d.IdProducto = p.Id
				WHERE d.IdFactura = @Id;
			END
			ELSE
			BEGIN
				EXEC sp_Response 5, NULL;
			END
        END
    END TRY
    BEGIN CATCH
        EXEC sp_Response 500, NULL;
    END CATCH
END;

GO

CREATE OR ALTER PROCEDURE sp_GetInvoicesByNumber
    @Number INT = NULL,
	@Offset INT = 1,
	@Limit INT = 10
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        IF @Number IS NULL
        BEGIN
			EXEC sp_Response 400, NULL;
		END
		ELSE 
		BEGIN
			DECLARE @Total INT;
			SELECT @Total = COUNT(*) FROM TblFacturas WHERE NumeroFactura = @Number;
			
			IF (@Total > 0)
			BEGIN
				EXEC sp_Response 0, @Total;

				SELECT 
					f.Id,
					f.FechaEmisionFactura AS InvoiceDate,
					f.NumeroFactura AS InvoiceNumber,
					c.RazonSocial AS ClientName,
					tc.TipoCliente AS ClientType,
					f.NumeroTotalArticulos AS TotalItems,
					f.SubTotalFactura AS SubTotalAmount,
					f.TotalImpuesto AS TaxAmount,
					f.TotalFactura AS TotalAmount
				FROM TblFacturas f
				INNER JOIN TblClientes c ON f.IdCliente = c.Id
				INNER JOIN CatTipoCliente tc ON f.IdCliente = tc.Id
				WHERE f.NumeroFactura = @Number
				ORDER BY f.FechaEmisionFactura DESC
				OFFSET (@Offset - 1) * @Limit ROWS
				FETCH NEXT @Limit ROWS ONLY;

				SELECT 
					d.Id,
					d.IdFactura AS InvoiceId,
					d.IdProducto AS ProductId,
					p.NombreProducto AS ProductName,
					d.CantidadDeProducto AS Quantity,
					d.PrecioUnitarioProducto AS UnitPrice,
					d.SubtotalProducto AS SubTotalPrice,
					d.Notas AS Notes
				FROM TblDetallesFactura d
				INNER JOIN CatProductos p ON d.IdProducto = p.Id
				INNER JOIN TblFacturas fe ON d.IdFactura = fe.Id
				WHERE fe.NumeroFactura = @Number
				AND d.IdFactura IN (
					SELECT f.Id
					FROM TblFacturas f
					ORDER BY f.FechaEmisionFactura DESC
					OFFSET (@Offset - 1) * @Limit ROWS
					FETCH NEXT @Limit ROWS ONLY
				);
            END
			ELSE
			BEGIN
				EXEC sp_Response 5, NULL;
			END
        END
    END TRY
    BEGIN CATCH
        EXEC sp_Response 500, NULL;
    END CATCH
END;

GO

CREATE OR ALTER PROCEDURE sp_GetInvoicesByClient
    @ClientId INT = NULL,
	@Offset INT = 1,
	@Limit INT = 10
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        IF @ClientId IS NULL
        BEGIN
			EXEC sp_Response 400, NULL;
		END
		ELSE 
		BEGIN
			DECLARE @Total INT;
			SELECT @Total = COUNT(*) FROM TblFacturas WHERE IdCliente = @ClientId;
			
			IF (@Total > 0)
			BEGIN
				EXEC sp_Response 0, @Total;

				SELECT 
					f.Id,
					f.FechaEmisionFactura AS InvoiceDate,
					f.NumeroFactura AS InvoiceNumber,
					c.RazonSocial AS ClientName,
					tc.TipoCliente AS ClientType,
					f.NumeroTotalArticulos AS TotalItems,
					f.SubTotalFactura AS SubTotalAmount,
					f.TotalImpuesto AS TaxAmount,
					f.TotalFactura AS TotalAmount
				FROM TblFacturas f
				INNER JOIN TblClientes c ON f.IdCliente = c.Id
				INNER JOIN CatTipoCliente tc ON f.IdCliente = tc.Id
				WHERE f.IdCliente = @ClientId
				ORDER BY f.FechaEmisionFactura DESC
				OFFSET (@Offset - 1) * @Limit ROWS
				FETCH NEXT @Limit ROWS ONLY;

				SELECT 
					d.Id,
					d.IdFactura AS InvoiceId,
					d.IdProducto AS ProductId,
					p.NombreProducto AS ProductName,
					d.CantidadDeProducto AS Quantity,
					d.PrecioUnitarioProducto AS UnitPrice,
					d.SubtotalProducto AS SubTotalPrice,
					d.Notas AS Notes
				FROM TblDetallesFactura d
				INNER JOIN CatProductos p ON d.IdProducto = p.Id
				INNER JOIN TblFacturas fe ON d.IdFactura = fe.Id
				WHERE fe.IdCliente = @ClientId
				AND d.IdFactura IN (
					SELECT f.Id
					FROM TblFacturas f
					ORDER BY f.FechaEmisionFactura DESC
					OFFSET (@Offset - 1) * @Limit ROWS
					FETCH NEXT @Limit ROWS ONLY
				);
            END
			ELSE
			BEGIN
				EXEC sp_Response 1, NULL;
			END
        END
    END TRY
    BEGIN CATCH
        EXEC sp_Response 500, NULL;
    END CATCH
END;