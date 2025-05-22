Feature: Report loading and reveal

Scenario: Accessibility issues are clickable while rest of the report loads
    Given the user has generated a report for "https://momkage-lexy.github.io/"
    And the design issues are still loading 
    Then they should be able to view the accessibility issues

Scenario: Sort is not available until report is done loading
    Given the user has generated a report for "https://momkage-lexy.github.io/"
    And the design issues are still loading
    Then they will not be able to sort

Scenario: Modal pops up once then is view via button
    Given the user has generated a report for "https://momkage-lexy.github.io/"
    Then the user clicks the let's begin button
    And they sort by severity
    Then they should not see the modal pop up again
    And the user clicks the summary button
    Then the user will see a modal containing the summary