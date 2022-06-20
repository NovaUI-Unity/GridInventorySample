using Nova;
using UnityEngine;

namespace NovaSamples.Inventory
{
    /// <summary>
    /// A popup UI controller which will display the number of items in the inventory being selected.
    /// </summary>
    public class CountSelector : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("The visual root of the count bar.")]
        private UIBlock2D countBar = null;
        [SerializeField]
        [Tooltip("The visual which will be resized to indicate the percentage of items selected out of the total available.")]
        private UIBlock2D countBarFill = null;
        [SerializeField]
        [Tooltip("The text field to display the number of items currently selected.")]
        private TextBlock countLabel = null;
        [SerializeField]
        [Tooltip("The visual to display an icon of the item type being selected.")]
        private UIBlock2D selectedIcon = null;

        /// <summary>
        /// Is the item selector popup currently active?
        /// </summary>
        public bool IsActive => gameObject.activeSelf;

        /// <summary>
        /// The current number of items selected
        /// </summary>
        public int CurrentCount { get; private set; } = 0;

        private int maxCount = -1;

        /// <summary>
        /// Display this selector at the given <paramref name="worldPosition"/>.
        /// </summary>
        /// <param name="worldPosition">The world position to display this UI.</param>
        /// <param name="maxCount">The max number of items available in the active selection.</param>
        /// <param name="startCount">The starting number of selected items out of the <paramref name="maxCount"/>.</param>
        /// <param name="icon">The icon representing the type of items being selected.</param>
        public void Show(Vector3 worldPosition, InventoryItem item)
        {
            // cache the max value to reuse as this UI gets updated.
            maxCount = item.Count;

            // Adjust the popup location so that the center of the fillbar
            // is at the provided world position
            Vector3 delta = transform.position - countBar.transform.position;
            worldPosition += delta;
            transform.position = worldPosition;

            // Update the icon
            selectedIcon.SetImage(item.Item.Image);

            // Enable this object
            gameObject.SetActive(true);

            // Update the count visuals
            UpdateCount(item.Count / 2);
        }

        /// <summary>
        /// Hide this popup UI.
        /// </summary>
        public void Hide()
        {
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Update the number of items selected based on the provided position.
        /// </summary>
        /// <param name="pointerWorldPosition"></param>
        public void DragToPosition(Vector3 pointerWorldPosition)
        {
            // Get X position in local space of the fillbar.
            float localXPos = countBar.transform.InverseTransformPoint(pointerWorldPosition).x;

            // Get the max width of "fillable" space.
            float width = countBar.CalculatedSize.X.Value;

            // Get the horizontal distance of the pointer location
            // (in local space) from the left edge of the the fillbar.
            float distanceFromLeft = localXPos + 0.5f * width;

            // Convert the distance from the left edge into a ratio, where 
            // "On left edge" == 0 and "On right edge" == 1.
            float percentSelected = Mathf.Clamp01(distanceFromLeft / width);

            // Round up the selection count to the nearest int.
            int newCount = Mathf.RoundToInt(percentSelected * maxCount + 0.5f);

            if (newCount != CurrentCount)
            {
                // Update the visuals if the count changed.
                UpdateCount(newCount);
            }
        }

        /// <summary>
        /// Update the visuals to be in sync with the given <paramref name="newCount"/>.
        /// </summary>
        private void UpdateCount(int newCount)
        {
            // Update the current count
            CurrentCount = newCount;

            // Update the count text displayed
            countLabel.Text = CurrentCount.ToString();

            // Resize the fill indicator to the percentage of
            // items selected out of the max number available.
            float percent = (float)CurrentCount / (float)maxCount;
            countBarFill.Size.X.Percent = percent;
        }
    }
}

