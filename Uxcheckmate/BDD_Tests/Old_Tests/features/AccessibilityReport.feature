Feature: Checks for accessibility
    As a user, I want to see a report that is easy to read and follow.

Scenario: User's site has an image with a missing alt tag
Given David has submitted his site for a scan
And one of the images on the site is missing and alt-tag
And his report loads
Then he will see a header for Accessibility Recommendations
Then he will see a subheader for Image Alt Tags 
Then he will see the line of html where the image is
Then he will see a recommendation that says “This image is missing an alt-text tag.”