ALTER TABLE UserAccounts DROP FOREIGN KEY UserAccounts_ibfk_1; -- fk Roles
ALTER TABLE Reports DROP FOREIGN KEY Reports_ibfk_1;           -- fk UserAcounts
ALTER TABLE Reports DROP FOREIGN KEY Reports_ibfk_2;           -- fk ReportCategories

DROP TABLE Reports;
DROP TABLE ReportCategories;
DROP TABLE UserAccounts;
DROP TABLE Roles;