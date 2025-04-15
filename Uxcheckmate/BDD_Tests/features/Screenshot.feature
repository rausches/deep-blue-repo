Feature: Capturing webpage screenshot

Scenario: User initiates screenshot analysis
    Given David has submitted a valid URL for analysis
    And David clicks the "Get Audit" button
    And a logo loader appears for two seconds
    Then David should see a confirmation message and a placeholder displaying the website screenshot being captured
    When the analysis is complete
    Then David should be redirected to the report page without a full page reload
    And David should see the screenshot in the report page
 