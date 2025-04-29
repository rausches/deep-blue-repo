Feature: Report Generation
As a user, I want to view the results of the analysis so that I know how to improve it.

  Scenario: User generates a report for a valid URL
    Given the user navigates to the site
    When the user enters "https://example.com" to analyze
    When the user starts the analysis
    Then the system displays a loading overlay with the website screenshot
    Then the report view is displayed
    Then the user clicks the let's begin button
    And the analyzed URL is shown on the page
    And the user sees how many issues were found
    And each issue category header is visible

