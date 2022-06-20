# Nova Grid Inventory Sample

A grid-style inventory management UI which includes a scrollable, dynamically populated inventory with support for moving/rearranging items in the inventory.

https://user-images.githubusercontent.com/8591310/176091472-4e5a9387-fd63-4a12-8669-02ee8a8cc489.mp4

# Tutorial

For an end-to-end walk-through of building out this sample, see the following video:

[![InventoryTutorial](https://img.youtube.com/vi/dpXxMlPaRNg/0.jpg)](https://www.youtube.com/watch?v=dpXxMlPaRNg)


## Setup

This sample does not include Nova, which must be imported before the sample can be used. After cloning the repo:

1. Open the project in Unity. The project will have errors due to the missing Nova assets, so when prompted by Unity either open the project in Safe Mode or select `Ignore`.
1. Import the Nova asset into the project via the Package Manager.
    - When selecting the files to import, be sure to deselect the Nova settings (`Nova/Resources/NovaSettings.asset`) as they are already included and configured for the sample.

## Script Highlights

- [`InventoryPanel`](Assets/Scripts/InventoryPanel.cs): The component responsible for binding data sources to the character/armory grids and handling user input events. This is the root level script which handles most of the complexity and is a good starting point for investigating the sample's functionality further.
- [`InventoryItemVisuals`](Assets/Scripts/InventoryItemVisuals.cs): The [`ItemVisuals`](https://novaui.io/manual/ItemView.html#itemvisuals) used to visually represent an interactable item in the grid.
- [`CountSelector`](Assets/Scripts/CountSelector.cs): The component which performs the functionality of the right-click and drag count selection.

## Scenes

- `Scenes/Inventory`: PC/Mouse
- `Scenes/InventoryXR`: XR (controllers)
    - Tested on Oculus Quest 2

## Attributions

- Icons: https://www.kenney.nl/assets/voxel-pack
- Background Content: https://www.kenney.nl/assets/retro-medieval-kit
- Font: https://fonts.google.com/specimen/Rajdhani