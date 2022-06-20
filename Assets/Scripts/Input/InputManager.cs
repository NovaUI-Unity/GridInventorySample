using UnityEngine;

namespace NovaSamples.Inventory
{
    /// <summary>
    /// A simple data type holding the pressed state of two input buttons, 
    /// <see cref="PrimaryButtonDown"/> and <see cref="SecondaryButtonDown"/>.
    /// </summary>
    public class InputData
    {
        public bool PrimaryButtonDown = false;
        public bool SecondaryButtonDown = false;

        public bool AnyButtonPressed => PrimaryButtonDown || SecondaryButtonDown;
    }

    /// <summary>
    /// The base class for an InputManager component in the Inventory sample.
    /// </summary>
    public abstract class InputManager : MonoBehaviour
    {
        /// <summary>
        /// Attempt to get the current <see cref="Ray"/> of the pointer represented by <paramref name="controlID"/>.
        /// </summary>
        /// <param name="controlID"></param>
        /// <param name="ray"></param>
        /// <returns></returns>
        public abstract bool TryGetRay(uint controlID, out Ray ray);
    }
}

