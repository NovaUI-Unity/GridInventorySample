using Nova;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NovaSamples.Inventory
{
    /// <summary>
    /// The component responsible for binding data sources to the character/armory <see cref="GridView"/>'s
    /// and handling user input events.
    /// </summary>
    public class InventoryPanel : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("The scriptable object asset containing a list of inventory items. This database will be used to generate a random set of elements which will populate the grid.")]
        private ItemDatabase inventoryDataBase = null;
        [SerializeField]
        [Tooltip("The input manager sending input events to this inventory panel.")]
        private InputManager inputManager = null;

        [Header("Character Grid")]
        [SerializeField]
        [Tooltip("The non-scrollable GridView to display the user's inventory in the game.")]
        private GridView characterGridView = null;
        [SerializeField]
        [Tooltip("The ClipMask to fade when the user is interacting with the Character Grid View.")]
        private ClipMask characterGridMask = null;
        [SerializeField]
        [Tooltip("The number of items the character inventory can store. Because the Character Grid View isn't scrollable, this should not exceed the total number of items which can fit in the Character Grid View.")]
        private int characterSlotCount = 30;

        [Header("Armory Grid")]
        [SerializeField]
        [Tooltip("The scrollable GridView to display the armory's inventory in the game.")]
        private GridView armoryGridView = null;
        [SerializeField]
        [Tooltip("The ClipMask to fade when the user is interacting with the Armory Grid View.")]
        private ClipMask armoryGridMask = null;
        [SerializeField]
        [Tooltip("The number of items the armory inventory can store. Because the Armory Grid View is scrollable, this value can exceed the number of items which can fit in the Armory Grid View.")]
        private int armorySlotCount = 200;

        [Header("Grid Row Style")]
        [SerializeField]
        [Tooltip("A length to configure the height of the Armory/Character grid rows.")]
        private Length gridRowHeight;
        [SerializeField]
        [Tooltip("A set of lengths to configure the padding to apply inward per grid row.")]
        private LengthRect gridRowPadding;
        [SerializeField]
        [Tooltip("The gradient used to style the background of each grid row.")]
        private RadialGradient gridRowBackground;
        [SerializeField]
        [Tooltip("The gradient color used to style the background of each grid row in the Character Grid View.")]
        private Color characterGridRowColor = Color.white;
        [SerializeField]
        [Tooltip("The gradient color used to style the background of each grid row in the Armory Grid View.")]
        private Color armoryGridRowColor = Color.black;

        [Header("Selected Item")]
        [SerializeField]
        [Tooltip("The visual root of the Selected Item View. Will be enabled/disabled as an object is selected and moved around the grid.")]
        private Transform selectedItemRoot = null;
        [SerializeField]
        [Tooltip("The ItemView whose visuals will display the type/count of the selected item.")]
        private ItemView selectedItemView = null;

        [Header("Item Selector")]
        [SerializeField]
        [Tooltip("The popup visual which will appear when the user performs a right-click + drag on a non-empty grid cell. Allows the user to adjust the number of items being selected.")]
        private CountSelector countSelector = null;

        [Header("Close Button")]
        [SerializeField]
        [Tooltip("The dummy close button.")]
        private UIBlock2D closeButton = null;
        [SerializeField]
        [Tooltip("The duration of the hover/unhover animation for the close button.")]
        private float closeButtonAnimationDuration = .15f;
        [SerializeField]
        [Tooltip("The hover animation for the close button.")]
        private BodyGradientAnimation closeButtonHoverAnimation;
        [SerializeField]
        [Tooltip("The unhover animation for the close button.")]
        private BodyGradientAnimation closeButtonUnhoverAnimation;

        /// <summary>
        /// A tint color to apply to an entire grid while a popup is enabled and rendering in front of it.
        /// </summary>
        private static readonly Color DisabledGridColor = new Color(0.8f, 0.8f, 0.8f, 0.5f);

        /// <summary>
        /// The default "untinted" color of a grid. Set when the popup closes.
        /// </summary>
        private static readonly Color EnabledGridColor = Color.white;

        /// <summary>
        /// The list of items stored in the character's inventory.
        /// </summary>
        private List<InventoryItem> characterInventory = null;
        /// <summary>
        /// The list of items stored in the armory's inventory.
        /// </summary>
        private List<InventoryItem> armoryInventory = null;

        /// <summary>
        /// The coroutine which will run while a selection is actively moving items
        /// around the character grid, armory grid, or between the character/armory grids.
        /// </summary>
        private Coroutine updateSelectedCoroutine = null;

        /// <summary>
        /// The visuals to display the active selection as it's moved around the UI.
        /// </summary>
        private InventoryItemVisuals selectedItemVisuals = null;
        /// <summary>
        /// The underlying data storing the information displayed by <see cref="selectedItemVisuals"/>.
        /// </summary>
        private InventoryItem selectedItem = null;
        /// <summary>
        /// The control ID of the input control responsible for triggering a "selection move".
        /// While the <see cref="updateSelectedCoroutine"/> is running, we'll use this ID to query the
        /// <see cref="inputManager"/> for the new pointer location. This will allow us to move the selected
        /// content around the grid based on where the user is pointing.
        /// </summary>
        private uint activeControlID;
        /// <summary>
        /// The animation handle for close button hover/unhover
        /// </summary>
        private AnimationHandle closeButtonAnimationHandle = default;

        private void EnsureDataSourcesInitialized()
        {
            if (characterInventory != null && armoryInventory != null)
            {
                // Already initialized
                return;
            }

            // Initialize the characterInventory and armoryInventory with their
            // own randomly generated inventory data sets.
            characterInventory = inventoryDataBase.GetRandomItems(characterSlotCount);
            armoryInventory = inventoryDataBase.GetRandomItems(armorySlotCount);
            selectedItemVisuals = selectedItemView.Visuals as InventoryItemVisuals;
        }

        private void Start()
        {
            EnsureDataSourcesInitialized();

            // Initialized both grid views
            InitializeGrid(characterGridView, ref characterInventory);
            InitializeGrid(armoryGridView, ref armoryInventory);

            // Subscribe to close button gestures
            closeButton.AddGestureHandler<Gesture.OnHover>(HandleCloseButtonHover);
            closeButton.AddGestureHandler<Gesture.OnUnhover>(HandleCloseButtonUnhover);
            closeButton.AddGestureHandler<Gesture.OnClick>(HandleCloseButtonClick);
        }

        private void InitializeGrid(GridView grid, ref List<InventoryItem> dataSource)
        {
            // Subscribe to gesture and data-bind events. Data binding events must be
            // subscribed to before calling grid.SetDataSource().
            SubscribeToEvents(grid);

            // SetDataSource only needs to be called once, and calling it multiple times
            // will incur unnecessary work
            if (grid.DataSourceItemCount == 0)
            {
                // Assign the data source to the grid.
                grid.SetDataSource(dataSource);
            }
        }

        #region Events
        /// <summary>
        /// Subscribe to bind/gesture events on the given <paramref name="grid"/>.
        /// </summary>
        private void SubscribeToEvents(GridView grid)
        {
            grid.SetSliceProvider(ProvideGridSlice);
            grid.AddDataBinder<InventoryItem, InventoryItemVisuals>(HandleBind);
            grid.AddGestureHandler<Gesture.OnClick, InventoryItemVisuals>(HandleClick);
            grid.AddGestureHandler<Gesture.OnHover, InventoryItemVisuals>(HandleHover);
            grid.AddGestureHandler<Gesture.OnUnhover, InventoryItemVisuals>(HandleUnhover);
            grid.AddGestureHandler<Gesture.OnPress, InventoryItemVisuals>(HandlePress);
            grid.AddGestureHandler<Gesture.OnRelease, InventoryItemVisuals>(HandleRelease);
            grid.AddGestureHandler<Gesture.OnDrag, InventoryItemVisuals>(HandleDrag);
        }

        /// <summary>
        /// GridView data-bind handler. Invoked when more items are paged into one of the two grids we're tracking.
        /// </summary>
        private void HandleBind(Data.OnBind<InventoryItem> evt, InventoryItemVisuals target, int index) => target.Bind(evt.UserData);

        /// <summary>
        /// Apply a hover state to the item being hovered.
        /// </summary>
        private void HandleHover(Gesture.OnHover evt, InventoryItemVisuals target, int index) => target.Hover();

        /// <summary>
        /// Remove the hover state from the item previously hovered.
        /// </summary>
        private void HandleUnhover(Gesture.OnUnhover evt, InventoryItemVisuals target, int index) => target.Unhover();

        /// <summary>
        /// If the <see cref="countSelector"/> popup is enabled, adjust the number of items selected as the user drags the cursor.
        /// </summary>
        private void HandleDrag(Gesture.OnDrag evt, InventoryItemVisuals target, int index)
        {
            if (!countSelector.IsActive)
            {
                // Popup not enabled, nothing to update.
                return;
            }

            // Update the selection count based on the current pointer position
            countSelector.DragToPosition(evt.PointerPositions.Current);
        }

        /// <summary>
        /// Handle press by updating visual states and possibly enabling the <see cref="countSelector"/> popup, 
        /// depending on which button was used to trigger the "press" gesture.
        /// </summary>
        private void HandlePress(Gesture.OnPress evt, InventoryItemVisuals target, int index)
        {
            // Update the "pressed" item to a press visual state
            target.Press();

            if (selectedItem != null || !IsSecondaryButton(evt.Interaction))
            {
                // Item already selected or not a right click
                return;
            }

            // Activate the count selector
            GridView gridView = GetContainingGrid(target, out List<InventoryItem> items);
            InventoryItem item = items[index];

            if (item.IsEmpty || item.Count == 1 || !TryGetInputLocation(evt.Interaction.Ray, out Vector3 worldPos))
            {
                // Don't bring up the selector if there is only 1 item
                return;
            }

            // Set the selection's grid's tint color to the faded state
            GetGridMask(gridView).Tint = DisabledGridColor;

            // Enable the selector popup at the given world position and initialize the selection count at half of what's available.
            countSelector.Show(worldPos, item);
        }

        /// <summary>
        /// Handle release by updating visual states and possibly moving a selected number of items to a new location in the inventory system.
        /// </summary>
        private void HandleRelease(Gesture.OnRelease evt, InventoryItemVisuals target, int index)
        {
            target.Release();

            if (!countSelector.IsActive)
            {
                // Count selector not active or not a right click
                return;
            }

            // The count has been selected
            GridView gridView = GetContainingGrid(target, out List<InventoryItem> items);
            InventoryItem item = items[index];

            if (countSelector.CurrentCount == item.Count)
            {
                // Selected the max available number of items

                // Clear since the selected number is being moved
                // to a new location in one of the two grids.
                items[index] = InventoryItem.Empty;

                // Update the grid cell, since we just changed the underlying data.
                gridView.Rebind(index);

                // Begin the selection move state
                SelectItem(item, evt.Interaction.ControlID);
            }
            else if (countSelector.CurrentCount > 0)
            {
                // Only selected some of the items available but not all of them
                InventoryItem newItem = new InventoryItem()
                {
                    Item = item.Item,
                    Count = countSelector.CurrentCount,
                };

                // Subtract the number of selected items from the
                // the total available at the selected location.
                item.Count -= countSelector.CurrentCount;

                // Update the grid cell, since we just changed the underlying data.
                gridView.Rebind(index);

                // Begin the selection move state
                SelectItem(newItem, evt.Interaction.ControlID);
            }

            // Reset the grid's tint color to the default state
            GetGridMask(gridView).Tint = EnabledGridColor;

            // Disable the item selector popup
            countSelector.Hide();
        }


        /// <summary>
        /// On click, either place the active selection or begin a new selection.
        /// </summary>
        private void HandleClick(Gesture.OnClick evt, InventoryItemVisuals target, int index)
        {
            if (IsSecondaryButton(evt.Interaction) || (selectedItem != null && evt.Interaction.ControlID != activeControlID))
            {
                // If right click or there is already a selected item and clicked with a different input device
                return;
            }

            GridView gridView = GetContainingGrid(target, out List<InventoryItem> items);
            InventoryItem clickedItem = items[index];

            if (selectedItem != null)
            {
                // We already have a selected item, so try to place the item in the clicked destination
                if (clickedItem.IsEmpty)
                {
                    // Destination is empty, so just replace the empty item
                    items[index] = selectedItem;

                    // Update the grid cell, since we just changed the underlying data.
                    gridView.Rebind(index);

                    // We can stop moving the selected visuals, since the user placed them in the clicked location.
                    DeselectItem();
                }
                else if (clickedItem.Item == selectedItem.Item && clickedItem.Count < InventoryItem.MaxItemsPerSlot)
                {
                    // Hit an item of the same type, so place as many of the current item into
                    // the slot as possible
                    int countToMove = Mathf.Min(InventoryItem.MaxItemsPerSlot - clickedItem.Count, selectedItem.Count);
                    clickedItem.Count += countToMove;

                    // Update the grid cell, since we just changed the underlying data.
                    gridView.Rebind(index);

                    // Adjust the number of items available in the active selection, since we just placed some of them.
                    selectedItem.Count -= countToMove;

                    if (selectedItem.Count == 0)
                    {
                        // The currently selected item is now empty, so we can stop the active selection.
                        DeselectItem();
                    }
                    else
                    {
                        // Not empty, so rebind since we just changed the underlying data.
                        selectedItemVisuals.Bind(selectedItem);
                    }
                }
                else
                {
                    // Hit a non-empty item, swap them
                    items[index] = selectedItem;

                    // Update the grid cell, since we just changed the underlying data.
                    gridView.Rebind(index);

                    // Update the selected item and corresponding visuals to the newly clicked item.
                    selectedItem = clickedItem;
                    selectedItemVisuals.Bind(clickedItem);
                }
            }
            else
            {
                if (clickedItem.IsEmpty)
                {
                    // Don't do anything for empty slots
                    return;
                }

                // Replace item in gridview with empty slot
                items[index] = InventoryItem.Empty;

                // Update the grid cell, since we just changed the underlying data.
                gridView.Rebind(index);

                // Begin the selection move state
                SelectItem(clickedItem, evt.Interaction.ControlID);
            }
        }

        /// <summary>
        /// Since this isn't a complete sample, we just log.
        /// </summary>
        private void HandleCloseButtonClick(Gesture.OnClick evt)
        {
            Debug.Log("Close!");
        }

        /// <summary>
        /// Animates the close button on unhover
        /// </summary>
        private void HandleCloseButtonUnhover(Gesture.OnUnhover evt)
        {
            // Cancel the current animation if it exists and start a new one
            closeButtonAnimationHandle.Cancel();
            closeButtonAnimationHandle = closeButtonUnhoverAnimation.Run(closeButtonAnimationDuration);
        }

        /// <summary>
        /// Animates the close button on hover
        /// </summary>
        private void HandleCloseButtonHover(Gesture.OnHover evt)
        {
            // Cancel the current animation if it exists and start a new one
            closeButtonAnimationHandle.Cancel();
            closeButtonAnimationHandle = closeButtonHoverAnimation.Run(closeButtonAnimationDuration);
        }
        #endregion

        /// <summary>
        /// Configure the visuals for a given grid row.
        /// </summary>
        private void ProvideGridSlice(int sliceIndex, GridView gridView, ref GridSlice2D gridSlice)
        {
            // Configure space between grid row elements to fill available space
            gridSlice.AutoLayout.AutoSpace = true;

            // AutoSize overrides Size, so make sure it's not set.
            gridSlice.Layout.AutoSize.Y = AutoSize.None;

            // Assign row height.
            gridSlice.Layout.Size.Y = gridRowHeight;

            // Adjust row padding.
            gridSlice.Layout.Padding.XY = gridRowPadding;

            // Apply gradient, but adjust the color depending on which GridView this is for.
            gridSlice.Gradient = gridRowBackground;
            gridSlice.Gradient.Color = gridView == characterGridView ? characterGridRowColor : armoryGridRowColor;
        }

        /// <summary>
        /// Given a ray, try to determine where the ray intersects the UI plane.
        /// </summary>
        private bool TryGetInputLocation(Ray ray, out Vector3 worldPos)
        {
            // Create a new UI plane with the given normal and position.
            Plane gridPlane = new Plane(transform.forward, transform.position);

            // Raycast against the UI plane to get the point of intersection.
            if (gridPlane.Raycast(ray, out float distance) && distance > 0f)
            {
                worldPos = ray.GetPoint(distance);
                return true;
            }
            else
            {
                worldPos = default;
                return false;
            }
        }

        /// <summary>
        /// Gets the containing grid (and associated data) of the provided <paramref name="visuals"/>.
        /// </summary>
        private GridView GetContainingGrid(InventoryItemVisuals visuals, out List<InventoryItem> items)
        {
            if (characterGridView.TryGetSourceIndex(visuals.View, out _))
            {
                // The item view is tracked by the characterGridView.
                items = characterInventory;
                return characterGridView;
            }
            else if (armoryGridView.TryGetSourceIndex(visuals.View, out _))
            {
                // The item view is tracked by the armoryGridView.
                items = armoryInventory;
                return armoryGridView;
            }
            else
            {
                // Neither the characterGridView nor armoryGridView is tracking the given visuals.View. 
                // We won't hit this in this particlar sample, but it's good practice just in case something
                // has gone wrong.
                throw new System.Exception("Failed to get grid for visuals.");
            }
        }

        /// <summary>
        /// Get the ClipMask for the given <paramref name="gridView"/>.
        /// </summary>
        private ClipMask GetGridMask(GridView gridView)
        {
            return gridView == characterGridView ? characterGridMask : armoryGridMask;
        }

        /// <summary>
        /// Enable the active selection visuals and begin moving the active selection 
        /// to a new location in the inventory.
        /// </summary>
        private void SelectItem(InventoryItem item, uint controlID)
        {
            activeControlID = controlID;
            selectedItem = item;
            selectedItemRoot.gameObject.SetActive(true);
            selectedItemVisuals.Bind(selectedItem);
            updateSelectedCoroutine = StartCoroutine(UpdateSelectedItemPosition());
        }


        /// <summary>
        /// While running, move the selection visuals to the new pointer location.
        /// </summary>
        private IEnumerator UpdateSelectedItemPosition()
        {
            while (true)
            {
                // The raycast might fail if not looking at the UI
                if (inputManager.TryGetRay(activeControlID, out Ray ray) &&
                    TryGetInputLocation(ray, out Vector3 worldPos))
                {
                    // Move the selected item to the ray position.
                    selectedItemRoot.position = worldPos;
                }

                yield return null;
            }
        }

        /// <summary>
        /// Stop moving the active selection and hide the active selection visuals.
        /// </summary>
        private void DeselectItem()
        {
            StopCoroutine(updateSelectedCoroutine);
            updateSelectedCoroutine = null;
            selectedItemRoot.gameObject.SetActive(false);
            selectedItem = null;
        }

        /// <summary>
        /// Was the <paramref name="interaction"/> sent by a context/right-click button?
        /// </summary>
        /// <param name="interaction"></param>
        /// <returns></returns>
        private static bool IsSecondaryButton(Interaction.Update interaction)
        {
            if (interaction.UserData is InputData inputData)
            {
                return inputData.SecondaryButtonDown;
            }

            return false;
        }
    }
}
