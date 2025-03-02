Feature: Checks for broken links
    As a user, I want to know if my site has any broken links

Scenario: User's site has a broken links
    Given David has a site with a broken link
    And he enters his site url into the url submission box
    And the report loads
    Then he should see the URL listed and be told the status code error

Scenario: User's site has no broken links
    Given Sarah has a site with no broken links
    And she enters her site url into the url submission box
    And the report loads
    Then she should not see anything regarding broken links in the report