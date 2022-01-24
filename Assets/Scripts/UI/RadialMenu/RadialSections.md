# Radial Menu

A Radial Menu for the Text2Scene-Unity-Framework. Has a small default menu that demonstrates most use cases.

## Usage

Currently opened by entering edit mode (aiming on a gameobject with `edit` LayerMask and releasing the `Edit Mode Activate` action, default Southern area of main 2D axis). Pops up next to the right controller.

Aiming on buttons is performed via the `Radial Control` action. `Radial Click` confirms a selection. To close the menu, simply activate the `Edit Mode Cancel` action.

## Creating custom menus

Recursively define the menu as follows:

```c#
// Small sample menu
// Main menu page
RadialSection r1 = new RadialSection("1", "", null);
RadialSection r2 = new RadialSection("2", "", null);

// Add subpage to open when clicking on 1
RadialSection r11 = new RadialSection("1.1", "", null);
r1.childSections = new List<RadialSection>() { r11 };

// Add 1 and 2 to main menu
radialSections = new List<RadialSection>() { r1, r2 };
```

## Default Input Actions

...
