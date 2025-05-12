Feature: Jira Tasks
As a user, I'd like to be able to export the issue as a task to Jira

Scenario: Logged in user can export to Jira
    Given the user is logged in
    And the user is on the dashboard
    And the user opens a report entry
    Then they will see a button to export to jira
    And they will click the button
    And they will log into Jira
    And they will select the project to add to
    Then they will see a loading spinner 
    Then the Jira modal will close