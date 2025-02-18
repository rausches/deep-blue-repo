Feature: Accessibility Report Generation
  As a Web Developer (Sarah),
  I want to submit my URL and receive an accessibility report,
  So that I know if my site meets accessibility standards.

  Scenario: Site has an image without an alt tag
    Given Sarah submits her URL "missingAlt"
    And the site contains an image without an alt tag
    When the accessibility report loads
    Then she sees an error for an image without an alt tag

  Scenario: Site has no accessibility errors
    Given Sarah submits her URL "perfectSite"
    And the site has no accessibility issues
    When the accessibility report loads
    Then she sees a message that there were no errors found

