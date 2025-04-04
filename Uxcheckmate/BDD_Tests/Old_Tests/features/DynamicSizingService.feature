Feature: Dynamic Sizing in HTML


Scenario: HTML contains responsive elements
Given I am David
And my site has no dynamic sizing elements
When I enter the url of my site
And the report loads
Then the report should let me know I need to add dynamic sizing and why

 
Scenario: HTML does not contain responsive elements
Given I am Priya
And my site has proper dynamic sizing elements
When I enter the url of my site
And the report loads
Then the report should not tell me that it may look bad on a phone