Feature: Delete report from dashboard

@DeleteButtonTest
Scenario: Deleting a report from the dashboard
    Given user is logged in
    When they click user dash
    Then they should be in the user dash page
    When the user clicks on one grouped folder of one domain
    Then they will see a delete button
