using UnityEngine;

namespace Watermelon.IAPStore
{
    public interface IIAPStoreOffer
    {
        public void Init();
        public GameObject GameObject { get; }
        public float Height { get; }
    }
}