USE UxCheckmate;

CREATE TABLE [Report] (
    [ID] INT IDENTITY(1,1),
    [URL] VARCHAR(128) NOT NULL,
    [Date] DATE NOT NULL,
    PRIMARY KEY([ID])
);

CREATE TABLE [AccessibilityCategory] (
    [ID] INT IDENTITY(1,1),
    [Name] VARCHAR(128) NOT NULL,
    [Description] VARCHAR(MAX),
    PRIMARY KEY([ID])
);

CREATE TABLE [AccessibilityIssue] (
    [ID] INT IDENTITY(1,1),
    [CategoryID] INT NOT NULL,
    [ReportID] INT NOT NULL,
    [Message] VARCHAR(MAX) NOT NULL,
    [Details] VARCHAR(MAX) NOT NULL,
    [Selector] VARCHAR(MAX) NOT NULL,
    [WCAG] VARCHAR(MAX) NOT NULL,
    [Severity] INT NOT NULL,
    PRIMARY KEY([ID]),
    CONSTRAINT FK_AccessibilityIssue_CategoryID FOREIGN KEY ([CategoryID]) REFERENCES [AccessibilityCategory]([ID]) ON DELETE CASCADE,
    CONSTRAINT FK_AccessibilityIssue_ReportID FOREIGN KEY ([ReportID]) REFERENCES [Report]([ID]) ON DELETE CASCADE
);

CREATE TABLE [DesignCategory] (
    [ID] INT IDENTITY(1,1),
    [Name] VARCHAR(128) NOT NULL,
    [Description] VARCHAR(MAX),
    [ScanMethod] VARCHAR(128),
    PRIMARY KEY([ID])
);

CREATE TABLE [DesignIssue] (
    [ID] INT IDENTITY(1,1),
    [CategoryID] INT NOT NULL,
    [ReportID] INT NOT NULL,
    [Message] VARCHAR(MAX) NOT NULL,
    [Severity] INT NOT NULL,
    PRIMARY KEY([ID]),
    CONSTRAINT FK_DesignIssue_CategoryID FOREIGN KEY ([CategoryID]) REFERENCES [DesignCategory]([ID]) ON DELETE CASCADE,
    CONSTRAINT FK_DesignIssue_ReportID FOREIGN KEY ([ReportID]) REFERENCES [Report]([ID]) ON DELETE CASCADE
);
