IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
CREATE TABLE [Candidatos] (
    [Id] uniqueidentifier NOT NULL,
    [NomeCompleto] nvarchar(150) NOT NULL,
    [Email] nvarchar(150) NOT NULL,
    [Telefone] nvarchar(20) NOT NULL,
    [AreaInteresse] nvarchar(100) NOT NULL,
    [ResumoProfissional] NVARCHAR(MAX) NOT NULL,
    [DataCriacao] datetime2 NOT NULL,
    CONSTRAINT [PK_Candidatos] PRIMARY KEY ([Id])
);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261004032203_Inicial', N'9.0.0');

COMMIT;
GO

