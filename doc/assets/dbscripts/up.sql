DROP SCHEMA IF EXISTS uxcheckmate;
CREATE SCHEMA uxcheckmate;
USE uxcheckmate;

CREATE TABLE Roles (
    RoleID  INT                 NOT NULL AUTO_INCREMENT,
    Name    VARCHAR(50)         NOT NULL UNIQUE,
    PRIMARY KEY (RoleID)
);

CREATE TABLE UserAccounts (
    UserID      INT             NOT NULL AUTO_INCREMENT,
    Email       VARCHAR(255)    NOT NULL UNIQUE,
    Password    VARCHAR(255)    NOT NULL,
    RoleID      INT             NOT NULL DEFAULT 0,
    PRIMARY KEY (UserID),
    FOREIGN KEY (RoleID) REFERENCES Roles(RoleID) ON DELETE RESTRICT
);
