Feature: User Dashboard Feature

# Mock Test Below
Scenario: Reach user Dashboard
    Given user is logged in
    When they click user dash
    Then they should be in user dash page

Scenario: Logout from user dashboard
    Given user is logged in
    When they click logout button
    Then they should be logged out

# Selenium Test Below
@login 
Scenario: Test login with valid credentials
    Given user clicks login link
    When they enter the username and password
    And they click log in button
    Then they should see user dashboard

# Should currently fail Selenium test below until ux-96 implemented
Scenario: View previous report after login
    Given user has logged in to an account with a previously submitted report
    When they go to user dashboard
    Then they should see that report

