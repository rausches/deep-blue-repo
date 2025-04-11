Feature: User enters in url and recieves analysis report

  @analyze
  Scenario: David analyzes his site and views the results

    Given the user navigates to the site
    When the user enters a URL to analyze with "https://momkage-lexy.github.io/"
    And the user starts the analysis

    Then he will see a loading overlay
    And he will be directed to the results view
    And he will see the site URL
    And he will see how many issues his site has
    And he will see a container for design issues with subrows of issues
    And he clicks the broken links section
    Then he will see the broken links row reporting missing or invalid links
    And the broken links listed will be accurate
