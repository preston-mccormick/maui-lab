# Spec Delta

## ADDED Requirements

### Requirement: A collection action sits under its page
A collection action SHALL appear above the page that was open, one level deeper than that page. Its title SHALL be the action's name. Its detail SHALL be the action's short value, and the detail SHALL be empty when the value is empty. The title SHALL use the amber action color. The action MUST NOT appear as a button, a surface, and a choice.

#### Scenario: Deleted swatch under Linear
- **WHEN** the user deleted a swatch on the Linear page and that visit is shown
- **THEN** above "Navigated to CollectionView Linear Layout page" there is one row titled "Deleted swatch" at one level deeper, its detail is the color name, and the title is amber

#### Scenario: Viewed swipe has no detail
- **WHEN** a "Viewed Swipe" action is shown
- **THEN** the title is "Viewed Swipe" and the detail is empty
