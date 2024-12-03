using System.Threading.Tasks;
using UnityEngine;

namespace Watermelon
{
    public class DummyIAPWrapper : BaseIAPWrapper
    {
        public override void BuyProduct(ProductKeyType productKeyType)
        {
            if (!IAPManager.IsInitialised)
            {
                IAPCanvas.ShowMessage("Network error. Please try again later");

                return;
            }

            IAPCanvas.ShowLoadingPanel();
            IAPCanvas.ChangeLoadingMessage("Payment in progress..");

            Tween.NextFrame(() =>
            {
                Debug.Log(string.Format("[IAPManager]: Purchasing - {0} is completed!", productKeyType));

                IAPManager.OnPurchaseCompled(productKeyType);

                IAPCanvas.ChangeLoadingMessage("Payment complete!");
                IAPCanvas.HideLoadingPanel();
            });
        }

        public override ProductData GetProductData(ProductKeyType productKeyType)
        {
            IAPItem iapItem = IAPManager.GetIAPItem(productKeyType);
            if(iapItem != null)
            {
                return new ProductData(iapItem.ProductType);
            }

            return null;
        }

        public override void Initialise(IAPSettings settings)
        {
            Debug.LogWarning("[IAP Manager]: Dummy mode is activated. Configure the module before uploading the game to stores!");

            IAPManager.OnModuleInitialised();
        }

        public override bool IsSubscribed(ProductKeyType productKeyType)
        {
            return false;
        }

        public override void RestorePurchases()
        {
            // DO NOTHING
        }
    }
}