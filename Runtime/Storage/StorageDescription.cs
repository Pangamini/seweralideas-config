#nullable enable
using UnityEngine;

namespace SeweralIdeas.Config
{
    /// <summary>
    /// Describes one way of storing a <see cref="Config"/>, and makes the <see cref="ConfigStorage"/> that does it. An asset
    /// (each kind has its own Create menu entry) that a <see cref="ConfigStoragePlan"/> lists.
    /// </summary>
    public abstract class StorageDescription : ScriptableObject
    {
        /// <summary>Whether this kind of storage works on the platform the game is running on.</summary>
        public abstract bool Available { get; }
        public abstract ConfigStorage GetStorage();
    }
}
