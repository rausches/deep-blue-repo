Feature: Anonymous User Report Limiting
# --filter "TestCategory=AnonLimit"

@AnonLimit
Scenario: Anonymous user hits the report limit
    Given the user navigates to the site
    When the user enters "https://example.com" to analyze
    And the user starts the analysis
    And the report view has loaded
    And the user goes back to the home page
    And the user enters "https://example.com" to analyze
    And the user starts the analysis
    And the report view has loaded
    And the user goes back to the home page
    And the user enters "https://example.com" to analyze
    And the user starts the analysis
    And the report view has loaded
    And the user goes back to the home page
    And the user enters "https://example.com" to analyze
    And the user starts the analysis
    Then the user should see a message saying "Anonymous users may only submit 3 reports per session"

@AnonLimit
Scenario: Logged-in user is never limited
    Given the user navigates to the site
    And their local site url is "http://localhost:5002/badFPattern.html"
    And the user logs in
    When the user enters "https://example.com" to analyze
    And the user starts the analysis
    And the report view has loaded
    And the user goes back to the home page
    And the user enters "https://example.com" to analyze
    And the user starts the analysis
    And the report view has loaded
    And the user goes back to the home page
    And the user enters "https://example.com" to analyze
    And the user starts the analysis
    And the report view has loaded
    And the user goes back to the home page
    When the user enters "http://localhost:5002/badFPattern.html" to analyze
    When the user starts the analysis
    When the report view has loaded
    Then the user clicks the let's begin button
    Then the user should see the Z Pattern issue