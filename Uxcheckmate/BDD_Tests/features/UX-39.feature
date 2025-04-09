Feature: User enters in url and recieves analysis report

  Scenario: David analyzes his site and views the results

    Given David is on the homepage
    And he enters "https://momkage-lexy.github.io/" into the submission box
    And he clicks the analyze button

    Then he will see a loading overlay
    And he will be directed to the results view
    And he will see the site URL
    And he will see how many issues his site has
    And he will see a container for design issues with subrows of issues
    And he clicks the broken links section
    Then he will see the broken links row reporting missing or invalid links
    And the broken links listed will be accurate
