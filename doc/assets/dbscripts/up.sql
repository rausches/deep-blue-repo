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
CREATE TABLE [AccessibilityCategory] (
    [ID] int NOT NULL IDENTITY,
    [Name] varchar(128) NOT NULL,
    [Description] text NULL,
    CONSTRAINT [PK__Accessib__3214EC274D88B479] PRIMARY KEY ([ID])
);

CREATE TABLE [DesignCategory] (
    [ID] int NOT NULL IDENTITY,
    [Name] varchar(128) NOT NULL,
    [Description] text NULL,
    CONSTRAINT [PK__DesignCa__3214EC27E2402227] PRIMARY KEY ([ID])
);

CREATE TABLE [Report] (
    [ID] int NOT NULL IDENTITY,
    [URL] varchar(128) NOT NULL,
    [Date] date NOT NULL,
    CONSTRAINT [PK__Report__3214EC27DC95E762] PRIMARY KEY ([ID])
);

CREATE TABLE [AccessibilityIssue] (
    [ID] int NOT NULL IDENTITY,
    [CategoryID] int NOT NULL,
    [ReportID] int NOT NULL,
    [Message] text NOT NULL,
    [Selector] varchar(128) NOT NULL,
    [Severity] int NOT NULL,
    CONSTRAINT [PK__Accessib__3214EC2759D39646] PRIMARY KEY ([ID]),
    CONSTRAINT [FK_AccessibilityIssue_CategoryID] FOREIGN KEY ([CategoryID]) REFERENCES [AccessibilityCategory] ([ID]) ON DELETE CASCADE,
    CONSTRAINT [FK_AccessibilityIssue_ReportID] FOREIGN KEY ([ReportID]) REFERENCES [Report] ([ID]) ON DELETE CASCADE
);

CREATE TABLE [DesignIssue] (
    [ID] int NOT NULL IDENTITY,
    [CategoryID] int NOT NULL,
    [ReportID] int NOT NULL,
    [Message] text NOT NULL,
    [Severity] int NOT NULL,
    CONSTRAINT [PK__DesignIs__3214EC277C5F35C0] PRIMARY KEY ([ID]),
    CONSTRAINT [FK_DesignIssue_CategoryID] FOREIGN KEY ([CategoryID]) REFERENCES [DesignCategory] ([ID]) ON DELETE CASCADE,
    CONSTRAINT [FK_DesignIssue_ReportID] FOREIGN KEY ([ReportID]) REFERENCES [Report] ([ID]) ON DELETE CASCADE
);

CREATE INDEX [IX_AccessibilityIssue_CategoryID] ON [AccessibilityIssue] ([CategoryID]);

CREATE INDEX [IX_AccessibilityIssue_ReportID] ON [AccessibilityIssue] ([ReportID]);

CREATE INDEX [IX_DesignIssue_CategoryID] ON [DesignIssue] ([CategoryID]);

CREATE INDEX [IX_DesignIssue_ReportID] ON [DesignIssue] ([ReportID]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250220054202_InitialCreate', N'9.0.2');

DECLARE @var sysname;
SELECT @var = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[DesignCategory]') AND [c].[name] = N'ScanMethod');
IF @var IS NOT NULL EXEC(N'ALTER TABLE [DesignCategory] DROP CONSTRAINT [' + @var + '];');
ALTER TABLE [DesignCategory] ALTER COLUMN [ScanMethod] nvarchar(max) NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250222061844_MakeScanMethodNullable', N'9.0.2');

DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[DesignIssue]') AND [c].[name] = N'Message');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [DesignIssue] DROP CONSTRAINT [' + @var1 + '];');
ALTER TABLE [DesignIssue] ALTER COLUMN [Message] varchar(max) NOT NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250223015614_MakeMessageVarchar', N'9.0.2');

ALTER TABLE [AccessibilityIssue] ADD [WCAG] nvarchar(max) NOT NULL DEFAULT N'';

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250223051135_AddWCAGTable', N'9.0.2');

DECLARE @var2 sysname;
SELECT @var2 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AccessibilityIssue]') AND [c].[name] = N'Selector');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [AccessibilityIssue] DROP CONSTRAINT [' + @var2 + '];');
ALTER TABLE [AccessibilityIssue] ALTER COLUMN [Selector] varchar(max) NOT NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250223213548_SelectorTypeMax', N'9.0.2');

ALTER TABLE [AccessibilityIssue] ADD [Details] varchar(max) NOT NULL DEFAULT '';

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250303070957_AccessIssuesDetails', N'9.0.2');

CREATE TABLE [FontLegibility] (
    [Id] int NOT NULL IDENTITY,
    [FontName] nvarchar(max) NOT NULL,
    [IsLegible] bit NOT NULL DEFAULT CAST(1 AS bit),
    CONSTRAINT [PK_FontLegibility] PRIMARY KEY ([Id])
);

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'FontName') AND [object_id] = OBJECT_ID(N'[FontLegibility]'))
    SET IDENTITY_INSERT [FontLegibility] ON;
INSERT INTO [FontLegibility] ([Id], [FontName])
VALUES (1, N'Chiller'),
(2, N'Vivaldi'),
(3, N'Old English Text'),
(4, N'Jokerman'),
(5, N'Brush Script'),
(6, N'Bleeding Cowboys'),
(7, N'Curlz MT'),
(8, N'Wingdings'),
(9, N'Zapfino'),
(10, N'TrashHand');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'FontName') AND [object_id] = OBJECT_ID(N'[FontLegibility]'))
    SET IDENTITY_INSERT [FontLegibility] OFF;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250304055656_AddFontLegibilityTable', N'9.0.2');

ALTER TABLE [AccessibilityIssue] ADD [HtmlSnippet] varchar(max) NOT NULL DEFAULT '';

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'FontName') AND [object_id] = OBJECT_ID(N'[FontLegibility]'))
    SET IDENTITY_INSERT [FontLegibility] ON;
INSERT INTO [FontLegibility] ([Id], [FontName])
VALUES (11, N'Comic Sans');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'FontName') AND [object_id] = OBJECT_ID(N'[FontLegibility]'))
    SET IDENTITY_INSERT [FontLegibility] OFF;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250309045613_AddHtmlSnippetToAccessibilityIssue', N'9.0.2');

DECLARE @var3 sysname;
SELECT @var3 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AccessibilityIssue]') AND [c].[name] = N'HtmlSnippet');
IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [AccessibilityIssue] DROP CONSTRAINT [' + @var3 + '];');
ALTER TABLE [AccessibilityIssue] DROP COLUMN [HtmlSnippet];

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250309051431_removehtmlsnippetfromaccessibilitissue', N'9.0.2');

ALTER TABLE [Report] ADD [UserID] nvarchar(max) NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250311061719_AddUserIDToReport', N'9.0.2');

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250401031526_ReSeedAccessibilityCategories', N'9.0.2');

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'ID', N'Description', N'Name') AND [object_id] = OBJECT_ID(N'[AccessibilityCategory]'))
    SET IDENTITY_INSERT [AccessibilityCategory] ON;
INSERT INTO [AccessibilityCategory] ([ID], [Description], [Name])
VALUES (1, NULL, 'Color & Contrast'),
(2, NULL, 'Keyboard & Focus'),
(3, NULL, 'Page Structure & Landmarks'),
(4, NULL, 'Forms & Inputs'),
(5, NULL, 'Link & Buttons'),
(6, NULL, 'Multimedia & Animations'),
(7, NULL, 'Timeouts & Auto-Refresh'),
(8, NULL, 'Motion & Interaction'),
(9, NULL, 'ARIA & Semantic HTML'),
(10, NULL, 'Other');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'ID', N'Description', N'Name') AND [object_id] = OBJECT_ID(N'[AccessibilityCategory]'))
    SET IDENTITY_INSERT [AccessibilityCategory] OFF;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250401032201_SeedAccessibilityCategories', N'9.0.2');

DELETE FROM [AccessibilityCategory]
WHERE [ID] = 1;
SELECT @@ROWCOUNT;


DELETE FROM [AccessibilityCategory]
WHERE [ID] = 2;
SELECT @@ROWCOUNT;


DELETE FROM [AccessibilityCategory]
WHERE [ID] = 3;
SELECT @@ROWCOUNT;


DELETE FROM [AccessibilityCategory]
WHERE [ID] = 4;
SELECT @@ROWCOUNT;


DELETE FROM [AccessibilityCategory]
WHERE [ID] = 5;
SELECT @@ROWCOUNT;


DELETE FROM [AccessibilityCategory]
WHERE [ID] = 6;
SELECT @@ROWCOUNT;


DELETE FROM [AccessibilityCategory]
WHERE [ID] = 7;
SELECT @@ROWCOUNT;


DELETE FROM [AccessibilityCategory]
WHERE [ID] = 8;
SELECT @@ROWCOUNT;


DELETE FROM [AccessibilityCategory]
WHERE [ID] = 9;
SELECT @@ROWCOUNT;


DELETE FROM [AccessibilityCategory]
WHERE [ID] = 10;
SELECT @@ROWCOUNT;


INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250401032500_RemoveAccessibilityCategories', N'9.0.2');

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'ID', N'Description', N'Name') AND [object_id] = OBJECT_ID(N'[AccessibilityCategory]'))
    SET IDENTITY_INSERT [AccessibilityCategory] ON;
INSERT INTO [AccessibilityCategory] ([ID], [Description], [Name])
VALUES (1, 'Issues related to color contrast and visual accessibility.', 'Color & Contrast'),
(2, 'Problems with keyboard navigation and focus management.', 'Keyboard & Focus'),
(3, 'Issues with headings, ARIA landmarks, and document structure.', 'Page Structure & Landmarks'),
(4, 'Issues with forms, labels, and input fields.', 'Forms & Inputs'),
(5, 'Problems with links, buttons, and interactive elements.', 'Link & Buttons'),
(6, 'Issues related to videos, audio, images, and animations.', 'Multimedia & Animations'),
(7, 'Problems with session timeouts, auto-refreshing pages, and dynamic content updates.', 'Timeouts & Auto-Refresh'),
(8, 'Issues related to animations, scrolling, and movement.', 'Motion & Interaction'),
(9, 'Issues with incorrect or missing ARIA roles and attributes.', 'ARIA & Semantic HTML'),
(10, 'Unknown or experimental WCAG violations', 'Other');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'ID', N'Description', N'Name') AND [object_id] = OBJECT_ID(N'[AccessibilityCategory]'))
    SET IDENTITY_INSERT [AccessibilityCategory] OFF;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250401032852_RerereseedAccessibilityCategories', N'9.0.2');

CREATE TABLE [UserFeedbacks] (
    [Id] int NOT NULL IDENTITY,
    [Message] nvarchar(1000) NOT NULL,
    [UserID] nvarchar(max) NULL,
    CONSTRAINT [PK_UserFeedbacks] PRIMARY KEY ([Id])
);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250506165305_AddUserFeedbackTable', N'9.0.2');

ALTER TABLE [UserFeedbacks] ADD [DateSubmitted] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250510004447_AddDateSubmittedToFeedback', N'9.0.2');

DECLARE @var4 sysname;
SELECT @var4 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Report]') AND [c].[name] = N'Status');
IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [Report] DROP CONSTRAINT [' + @var4 + '];');
ALTER TABLE [Report] ALTER COLUMN [Status] nvarchar(max) NULL;

ALTER TABLE [DesignIssue] ADD [Title] nvarchar(max) NULL;

ALTER TABLE [AccessibilityIssue] ADD [Title] nvarchar(max) NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250512015212_IssueTitle', N'9.0.2');

COMMIT;
GO

