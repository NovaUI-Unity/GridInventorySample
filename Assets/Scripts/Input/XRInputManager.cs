using Nova;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace NovaSamples.Inventory
{
    public class XRInputManager : InputManager
    {
        /// <summary>
        /// The visuals for a single hand
        /// </summary>
        [Serializable]
        private struct SingleHandVisuals
        {
            /// <summary>
            /// The root of the visuals for the hand.
            /// </summary>
            public GameObject VisualizationRoot;
            /// <summary>
            /// The line renderer used to show where the controller is pointing.
            /// </summary>
            public LineRenderer HandRayRenderer;
            /// <summary>
            /// If the controller ray intersects with Nova content, this will be placed at the
            /// intersection location.
            /// </summary>
            public UIBlock2D RayCollisionVisual;
        }

        private class SingleHand
        {
            private uint id;
            private uint scrollID;
            private SingleHandVisuals visuals;
            private float rayLength;
            private InputData data = new InputData();
            private InputDevice controller;

            public SingleHand(InputDevice controller, uint id, uint scrollID, SingleHandVisuals visuals, float rayLength)
            {
                this.controller = controller;
                this.id = id;
                this.scrollID = scrollID;
                this.visuals = visuals;
                this.rayLength = rayLength;
            }

            public bool TryGetRay(out Ray ray)
            {
                if (!controller.isValid)
                {
                    ray = default;
                    return false;
                }

                if (!controller.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 position))
                {
                    // Couldn't get position of controller
                    ray = default;
                    return false;
                }

                if (!controller.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion rotation))
                {
                    // Couldn't get rotation of controller
                    ray = default;
                    return false;
                }

                // Convert position and rotation to world-space ray
                ray = new Ray(position, rotation * Vector3.forward);
                return true;
            }

            public void Update()
            {
                Vector3 pos = default;
                Quaternion rotation = default;
                bool controllerIsValid =
                    controller.isValid &&
                    controller.TryGetFeatureValue(CommonUsages.devicePosition, out pos) &&
                    controller.TryGetFeatureValue(CommonUsages.deviceRotation, out rotation);

                if (!controllerIsValid)
                {
                    // Controller not active or couldn't get position, disable visuals
                    visuals.VisualizationRoot.SetActive(false);
                    return;
                }


                // Ensure visuals are enabled
                visuals.VisualizationRoot.SetActive(true);

                // Update visuals position
                visuals.VisualizationRoot.transform.SetPositionAndRotation(pos, rotation);

                Ray ray = new Ray(pos, rotation * Vector3.forward);

                controller.TryGetFeatureValue(CommonUsages.primaryButton, out data.PrimaryButtonDown);
                controller.TryGetFeatureValue(CommonUsages.secondaryButton, out data.SecondaryButtonDown);

                Interaction.Point(new Interaction.Update(ray, id, data), data.AnyButtonPressed, accuracy: InputAccuracy.Low);

                controller.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 thumbstick);

                if (thumbstick != Vector2.zero)
                {
                    // Invert thumbstick scroll
                    thumbstick.y = -thumbstick.y;
                    Interaction.Scroll(new Interaction.Update(ray, scrollID), thumbstick);
                }

                // Update Ray
                if (!Interaction.Raycast(ray, out UIBlockHit blockHit))
                {
                    // Intersection failed, so set the ray to full length and disable the collision visual
                    visuals.HandRayRenderer.transform.localScale = new Vector3(1f, 1f, rayLength);
                    visuals.RayCollisionVisual.gameObject.SetActive(false);
                    return;
                }

                float length = Vector3.Distance(ray.origin, blockHit.Position);
                visuals.HandRayRenderer.transform.localScale = new Vector3(1f, 1f, length);
                visuals.RayCollisionVisual.transform.position = blockHit.Position;
                visuals.RayCollisionVisual.transform.rotation = blockHit.UIBlock.transform.rotation;
                visuals.RayCollisionVisual.gameObject.SetActive(true);
            }
        }

        [SerializeField]
        public float rayLength = 10f;
        [SerializeField]
        private SingleHandVisuals leftHandVisuals;
        [SerializeField]
        private SingleHandVisuals rightHandVisuals;

        private const uint RightPointControlID = 1;
        private const uint RightScrollControlID = 2;

        private const uint LeftPointControlID = 3;
        private const uint LeftScrollControlID = 4;

        private SingleHand leftHand = null;
        private SingleHand rightHand = null;

        public override bool TryGetRay(uint controlID, out Ray ray)
        {
            if (controlID == RightPointControlID && rightHand != null)
            {
                return rightHand.TryGetRay(out ray);
            }
            else if (controlID == LeftPointControlID && leftHand != null)
            {
                return leftHand.TryGetRay(out ray);
            }
            else
            {
                ray = default;
                return false;
            }
        }

        private void Start()
        {
            // Start the visuals off disabled
            rightHandVisuals.VisualizationRoot.SetActive(false);
            leftHandVisuals.VisualizationRoot.SetActive(false);

            // Subscribe to device connect/disconect events
            InputDevices.deviceConnected += (_) => UpdateControllers();
            InputDevices.deviceDisconnected += (_) => UpdateControllers();
            UpdateControllers();
        }

        private void Update()
        {
            if (rightHand != null)
            {
                rightHand.Update();
            }
            if (leftHand != null)
            {
                leftHand.Update();
            }
        }

        /// <summary>
        /// Tries to get the controllers
        /// </summary>
        private void UpdateControllers()
        {
            List<InputDevice> controllers = new List<InputDevice>();
            InputDeviceCharacteristics desiredCharacteristics = InputDeviceCharacteristics.HeldInHand | InputDeviceCharacteristics.Controller;

            // Get the right controller
            InputDevices.GetDevicesWithCharacteristics(desiredCharacteristics | InputDeviceCharacteristics.Right, controllers);

            if (controllers.Count > 0)
            {
                rightHand = new SingleHand(controllers[0], RightPointControlID, RightScrollControlID, rightHandVisuals, rayLength);
            }
            else
            {
                rightHand = null;
            }

            // Get the left controller
            InputDevices.GetDevicesWithCharacteristics(desiredCharacteristics | InputDeviceCharacteristics.Left, controllers);
            if (controllers.Count > 0)
            {
                leftHand = new SingleHand(controllers[0], LeftPointControlID, LeftScrollControlID, leftHandVisuals, rayLength);
            }
            else
            {
                leftHand = null;
            }
        }
    }
}

