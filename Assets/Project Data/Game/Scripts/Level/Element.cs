using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon.BusStop
{
    [System.Serializable]
    public class Element
    {
        [SerializeField] Transform transform;
        public Transform Transform => transform;

        [SerializeField] Vector2 size;
        public Vector2 Size => size;

        public bool IsPositionInRange(Vector3 position)
        {
            Bounds bounds = new Bounds(transform.position, size);

            return bounds.Contains(position);
        }
    }
}
