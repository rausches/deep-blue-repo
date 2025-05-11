Feature: User Feedback
# --filter "TestCategory=Feedback"

# Selenium Test Below
@Feedback
Scenario: User submits feedback
    Given the user navigates to the site
    And the user logs in
    When they go to feedback as "User"
    And enter feedback "This is a test message"
    Then they should see a success message


@Feedback
Scenario: Admin views feedback
    Given the user navigates to the site
    And the user logs in as admin
    When they go to feedback as "Admin"
    Then they should see user feedback list


@Feedback
Scenario: User submits empty feedback
    Given the user navigates to the site
    And the user logs in
    When they go to feedback as "User"
    And enter feedback ""
    Then they should see a validation error

@Feedback
Scenario: User submits feedback with invalid characters
    Given the user navigates to the site
    And the user logs in
    When they go to feedback as "User"
    And enter feedback "Δ ∞ γ"
    Then they should see a validation error

