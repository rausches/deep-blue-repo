Feature: Pop-Ups Issue Reporting
As a user, I want to know if I have too many pop ups on my site.

  Scenario: User expands the Pop-Ups section and sees detected issues
    Given the user has generated a report for "https://momkage-lexy.github.io/"
    When the user clicks the Pop Ups section
    Then the Pop Ups section should be visible
    And the pop ups row reports all detected pop ups