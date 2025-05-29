Feature: Role-based dashboard redirection

  Scenario: Standard user visits dashboard
    Given user is logged in as an "User"
    When they visit the dashboard
    Then they should see the "UserDashboard" view
    And they should land on the correct page in browser

  Scenario: Admin visits dashboard
    Given user is logged in as an "Admin"
    When they visit the dashboard
    Then they should see the "AdminDashboard" view
    And they should land on the correct page in browser

