Feature: Validate web-scraping.dev/products URL
  As a user, I want to validate that the Report method correctly processes the URL 'https://web-scraping.dev/products'

  Scenario: User submits the web-scraping.dev/products URL
    Given the user provides the URL "https://web-scraping.dev/products"
    When the Report method processes the URL
    Then the result should be a ViewResult
