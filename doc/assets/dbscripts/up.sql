DROP SCHEMA IF EXISTS uxcheckmate;
CREATE SCHEMA uxcheckmate;
USE uxcheckmate;

CREATE TABLE Roles (
    RoleID  INT                     NOT NULL AUTO_INCREMENT,
    Name    VARCHAR(50)             NOT NULL UNIQUE,
    PRIMARY KEY (RoleID)
);

CREATE TABLE UserAccounts (
    UserID      INT                 NOT NULL AUTO_INCREMENT,
    Email       VARCHAR(255)        NOT NULL UNIQUE,
    Password    VARCHAR(255)        NOT NULL,
    RoleID      INT                 NOT NULL DEFAULT 0,
    PRIMARY KEY (UserID),
    FOREIGN KEY (RoleID) REFERENCES Roles(RoleID) ON DELETE RESTRICT
);

CREATE TABLE ReportCategories (
    CategoryID      INT             NOT NULL AUTO_INCREMENT,
    Name            VARCHAR(255)    NOT NULL UNIQUE,
    Description     TEXT            NOT NULL,
    OpenAIPrompt    TEXT            NOT NULL,
    PRIMARY KEY (CategoryID)
);

CREATE TABLE Reports (
    ReportID        INT             NOT NULL AUTO_INCREMENT,
    UserID          INT             NOT NULL,
    CategoryID      INT             NOT NULL,
    Date            DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Recommendations TEXT            NOT NULL,
    PRIMARY KEY (ReportID),
    FOREIGN KEY (UserID) REFERENCES UserAccounts(UserID) ON DELETE CASCADE,
    FOREIGN KEY (CategoryID) REFERENCES ReportCategories(CategoryID) ON DELETE RESTRICT
);