Feature: User Dashboard Feature
# --filter "FullyQualifiedName~UserDashboardFeature"

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
    Given the user navigates to the site
    And the user logs in
    Then they should see user dashboard

# Should currently fail Selenium test below until ux-96 implemented
Scenario: View previous report after login
    Given the user navigates to the site
    And the user logs in
    # And they have submitted previous report which default account has
    When they go to user dashboard
    Then they should see that report

