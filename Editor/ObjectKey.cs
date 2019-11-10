using System;
using UnityEngine;

namespace Editor {
    /**
     * Key class for identifying duplicate assets while importing.
     */
    internal struct ObjectKey : IEquatable<ObjectKey> {

        private readonly string _asset;
        private readonly Vector3 _position;

        public ObjectKey(string asset, Vector3 position) {
            _asset = asset;
            _position = position;
        }

        public bool Equals(ObjectKey other) {
            return string.Equals(_asset, other._asset) && _position.Equals(other._position);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            return obj is ObjectKey && Equals((ObjectKey) obj);
        }

        public override int GetHashCode() {
            unchecked {
                return ((_asset != null ? _asset.GetHashCode() : 0) * 397) ^ _position.GetHashCode();
            }
        }

    }
}