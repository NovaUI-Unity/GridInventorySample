using Nova;
using System;
using UnityEngine;

namespace NovaSamples.Inventory
{
    /// <summary>
    /// The set of components and basic behavior used to visually represent 
    /// an interactable item in the Inventory system.
    /// </summary>
    [Serializable]
    public class InventoryItemVisuals : ItemVisuals
    {
        [Header("Components")]
        [Tooltip("The root of visual content, which will be enabled/disabled depending on whether or not the inventory slot is empty.")]
        public UIBlock ContentRoot = null;
        [Tooltip("Will display the image of the object in the inventory.")]
        public UIBlock2D Image = null;
        [Tooltip("The text used to indicate the count of the item stored in the given grid slot.")]
        public TextBlock Count = null;
        [Tooltip("A visual used to indicate how \"full\" the given inventory element is. Will be resized as a percentage, calculated via Count / Max.")]
        public UIBlock2D CountFillBar = null;

        [Header("Animations")]
        [SerializeField]
        [Tooltip("The duration of each state transition in seconds.")]
        private float animationDuration = .1f;
        [SerializeField]
        [Tooltip("The animation to run when the inventory item is hovered.")]
        private BodyColorAnimation hoverAnimation;
        [SerializeField]
        [Tooltip("The animation to run when the inventory item is unhovered.")]
        private BodyColorAnimation unhoverAnimation;
        [SerializeField]
        [Tooltip("The animation to run when the inventory item is pressed.")]
        private BodyGradientAnimation pressAnimation;
        [SerializeField]
        [Tooltip("The animation to run when the inventory item is released.")]
        private BodyGradientAnimation releaseAnimation;

        /// <summary>
        /// The AnimationHandle to track active hover/unhover animations
        /// </summary>
        private AnimationHandle hoverAnimationHandle;

        /// <summary>
        /// The AnimationHandle to track active press/release animations
        /// </summary>
        private AnimationHandle pressAnimationHandle;

        /// <summary>
        /// Is this set of visuals being hovered?
        /// </summary>
        private bool isHovered = false;

        /// <summary>
        /// Is this set of visuals being pressed?
        /// </summary>
        private bool isPressed = false;

        /// <summary>
        /// Update the visuals to display the values stored in the provided <paramref name="data"/>.
        /// </summary>
        /// <param name="data"></param>
        public void Bind(InventoryItem data)
        {
            if (data.IsEmpty)
            {
                // Data is empty, hide the content root.
                ContentRoot.gameObject.SetActive(false);
            }
            else
            {
                // Ensure the content root is enabled.
                ContentRoot.gameObject.SetActive(true);

                // Update the corresponding visuals to match the data values.
                Image.SetImage(data.Item.Image);
                Count.Text = data.Count.ToString();
                CountFillBar.Size.X.Percent = Mathf.Clamp01((float)data.Count / InventoryItem.MaxItemsPerSlot);
            }
        }

        /// <summary>
        /// Update the visuals to a "hovered" visual state.
        /// </summary>
        public void Hover()
        {
            if (isHovered)
            {
                // Already hovered, nothing to update.
                return;
            }

            // Cancel any running hover/unhover animations, so we don't have conflicting animations running.
            hoverAnimationHandle.Cancel();

            // Kick off the new hover animation.
            hoverAnimationHandle = hoverAnimation.Run(animationDuration);

            // Update internal hover state.
            isHovered = true;
        }

        /// <summary>
        /// Update the visuals to an "uhovered" visual state.
        /// </summary>
        public void Unhover()
        {
            if (!isHovered)
            {
                // Not hovered, nothing to update.
                return;
            }

            // Cancel any running hover/unhover animations, so we don't have conflicting animations running.
            hoverAnimationHandle.Cancel();

            // Kick off the new unhover animation.
            hoverAnimationHandle = unhoverAnimation.Run(animationDuration);

            // Update internal hover state.
            isHovered = false;
        }


        /// <summary>
        /// Update the visuals to a "pressed" visual state.
        /// </summary>
        public void Press()
        {
            if (isPressed)
            {
                // Already pressed, nothing to update.
                return;
            }

            // Cancel any running press/release animations, so we don't have conflicting animations running.
            pressAnimationHandle.Cancel();

            // Kick off the new press animation.
            pressAnimationHandle = pressAnimation.Run(animationDuration);

            // Update internal pressed state.
            isPressed = true;
        }

        /// <summary>
        /// Update the visuals to a "released" visual state.
        /// </summary>
        public void Release()
        {
            if (!isPressed)
            {
                // Not pressed, nothing to update.
                return;
            }

            // Cancel any running press/release animations, so we don't have conflicting animations running.
            pressAnimationHandle.Cancel();

            // Kick off the new release animation.
            pressAnimationHandle = releaseAnimation.Run(animationDuration);

            // Update internal pressed state.
            isPressed = false;
        }
    }
}

