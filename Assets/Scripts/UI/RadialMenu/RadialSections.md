# Radial Menu

A Radial Menu for the Text2Scene-Unity-Framework. Can show different menu types based on layer of the
object interacted with.

## Usage

Currently opened by entering Radial mode (aiming on a gameobject with `IsoSpaceEditable` InteractionLayerMask and releasing the `Radial Mode Activate` action, default TriggerPress). Pops up next to the controller.

The menu shown depends on the InteractionLayerMask set in the hit object's `XRGrabInteractable` component. It has to have the `IsoSpaceEditable` layer and any of the following which defines what menu to open. This mapping is defined in `RadialMenuData.RadialLayerMap`.

Aiming on buttons is performed via the `Radial Control` action. `Radial Click` confirms a selection. To close the menu, simply activate the `Radial Mode Cancel` action or activate any Radial Section saying `CANCEL`.

## Creating custom menus

All the different menus are stored in the static class `RadialMenuData`.

The Dictionary `m_radialMenuMap` defines what MenuType corresponds to what List of `RadialSection`s.

The Dictionary `m_radialLayerMap` maps the different interaction layer names to the enum `MenuType`.

### Adding a new Menu Type

Adding a new menu type only requires adding the name to the enum `MenuType` and then adding a corresponding List of `RadialSection`s to `m_radialMenuMap` in the `Init()` function. The options will be shown in clockwise order starting on top. To influence the posistion of the sections, you can add `null` to `menuData` to create a blank field.

```c#
private static void Init()
{
    m_radialMenuMap = new Dictionary<MenuType, List<RadialSection>>();
    // ...
    // Other menus
    // ...

    // Create new menu and fill with data
    List<RadialSection> menuData = new List<RadialSection>()
    {
        new RadialSection("1", "Option 1", null),
        null,
        new RadialSection("2", "Option 2", null),
        new RadialSection("CANCEL", "to MainMenu", null),
        null,
        new RadialSection("3", "Option 3", null)
    };

    // Add the new menu to m_radialMenuMap
    m_radialMenuMap.Add(MenuType.Link, menuData);
}
```

Recursive menus can be defined by adding another `List<RadialSection>` to the `RadialSection` constructor or by adding it later by setting the `childSections` value.

```c#
// Option 1
RadialSection r1 = new RadialSection("1", "", null);
RadialSection r2 = new RadialSection("2", "", new List<RadialSection> { r1 });

// Option 2
RadialSection r3 = new RadialSection("1", "", null);
RadialSection r4 = new RadialSection("2", "", null);

r4.childSections = new List<RadialSection>() { r3 };
```

### Implementing the new Menu Type

To use a newly created menu of type `MenuType.test`, add a new Unity Layer `testLayer` that marks a
gameObject to use this new menu. Now add a mapping to `m_radialLayerMap` in `Init_LayerMap()`:

```c#
private static void Init_LayerMap()
{
    m_radialLayerMap = new Dictionary<string, MenuType>();
    // Add Corresponding Layers and their menu types here.
    // Other MenuTypes ...
    m_radialLayerMap.Add("testLayer", MenuType.test);
}
```

Now to use this new menu on a gameObject, add the layers `IsoSpaceEditable` and `testLayer` to the `XRGrabInteractable` component. Now activating the menu on this object will open the new menu.
