using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon.IAPStore
{
    [CreateAssetMenu(fileName = "IAP Store Data", menuName = "Content/Data/IAP Store")]
    public class IAPStoreData : ScriptableObject
    {
        [SerializeField] List<IAPStoreOfferData> offers;
    }
}