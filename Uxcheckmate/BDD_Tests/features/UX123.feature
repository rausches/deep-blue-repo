Feature: Report Generation

Scenario: Accessibility issues are clickable while rest of the report loads
    Given the user generates a report
    When they get to the results page
    And the design issues are still loading 
    Then they should be able to view the accessibility issues

Scenario: Sort is not available until report is done loading
    Given the user generates a report
    And they get to the results page
    And the design issues are still loading
    Then they will not be able to sort
    And the report is done loading 
    And the user clicks the let's begin button
    Then they will be able to sort

Scenario: Modal pops up once then is view via button
    Given the user generates a report
    And the report is done loading 
    And they click out of the summary modal
    And they sort by severity
    Then they should not see the modal pop up again
    And the user clicks the let's begin button
    Then the user will see a modal containing the summary