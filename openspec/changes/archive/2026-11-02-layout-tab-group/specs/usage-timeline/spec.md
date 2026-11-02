# Spec Delta

## ADDED Requirements

### Requirement: A layout action sits under its page
A layout action SHALL appear above the page that was open, one level deeper than that page. Its title SHALL be the action's name. Its detail SHALL be the action's short value, and the detail SHALL be empty when the value is empty. The title SHALL use the amber action color. The action MUST NOT appear as a button, a surface, and a choice.

#### Scenario: Added row under Grid Layout
- **WHEN** the user added a row on Grid Layout and that visit is shown
- **THEN** above "Navigated to Grid Layout page" there is one row titled "Added row" at one level deeper, its detail is the row count, and the title is amber

#### Scenario: Viewed info has no detail
- **WHEN** a "Viewed Cross axis" action is shown
- **THEN** the title is "Viewed Cross axis" and the detail is empty
