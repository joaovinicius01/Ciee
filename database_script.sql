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
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261004032203_Inicial'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261004032203_Inicial'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261004032203_Inicial', N'9.0.0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261004201111_EmailUnico'
)
BEGIN
    DECLARE @var0 sysname;
    SELECT @var0 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Candidatos]') AND [c].[name] = N'ResumoProfissional');
    IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Candidatos] DROP CONSTRAINT [' + @var0 + '];');
    ALTER TABLE [Candidatos] ALTER COLUMN [ResumoProfissional] NVARCHAR(MAX) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261004201111_EmailUnico'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Candidatos_Email] ON [Candidatos] ([Email]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261004201111_EmailUnico'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261004201111_EmailUnico', N'9.0.0');
END;

COMMIT;
GO

