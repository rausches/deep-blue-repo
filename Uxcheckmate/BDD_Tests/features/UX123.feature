Feature: Report Generation

Scenario: Accessibility issues are clickable while rest of the report loads
    Given the user generates a report
    When they get to the results page
    And the design issues are still loading 
    And the accessibility issues are done loading
    Then they should be able to view the accessibility issues

Scenario: Sort is not available until report is done loading
    Given the user generates a report
    And they get to the results page
    And the report is still loading
    Then they will not be able to sort

Scenario: Modal pops up once then is view via button
    Given the user generates a report
    And the report is done loading 
    And they click out of the summary modal
    And they sort by severity
    Then they should not see the modal pop up again
    And they click the summary button
    Then the modal should pop up again.