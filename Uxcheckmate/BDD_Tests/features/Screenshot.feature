Feature: Webpage screenshot

Scenario: Given the user navigates to the site
    When the user enters a URL to analyze with
    When the user starts the analysis

    Then the user will see a loading overlay
    Then the user should see the result view with the website screenshot
    
