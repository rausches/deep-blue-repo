Feature: Broken Links Analysis
As a user, I want to know if my site has any broken links

  Scenario: User expands the Broken Links section and sees detected issues
    Given the user has generated a report for "https://momkage-lexy.github.io/"
    When the user clicks the broken links section
    Then the broken links section should be visible
    And the broken links row reports missing or invalid links