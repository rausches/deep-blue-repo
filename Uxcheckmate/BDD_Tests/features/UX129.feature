Feature: Task List
As a user, I want to view a task list of all issues in the report

Scenario: Logged in user can view a task list of all issues
    Given user is logged in
    And the user views a report
    Then they will see a button for a task list
    And they will click the button
    Then they will see a list of all issues in bullet form