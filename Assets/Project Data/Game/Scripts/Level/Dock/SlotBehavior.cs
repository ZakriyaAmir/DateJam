using UnityEngine;

namespace Watermelon
{
    public class SlotBehavior : MonoBehaviour
    {
        [SerializeField] Vector3 offset;

        [Space]
        [SerializeField] SpriteRenderer backImage;
        [SerializeField] SpriteRenderer coloredImage;

        public Vector3 Position { get; private set; }
        public Vector3 Scale { get; private set; }
        public Vector3 Rotation { get; private set; }
        [SerializeField]
        public SlotCase SlotCase { get; private set; }

        public bool IsOccupied => SlotCase != null;

        private TweenCase colorCase;

        public static SlotBehavior GetTempSlot(SlotBehavior lastSlot, SlotBehavior prevLastSlot)
        {
            var tempSlot = new GameObject("Temp Slot").AddComponent<SlotBehavior>();
            tempSlot.Position = lastSlot.Position + (lastSlot.Position - prevLastSlot.Position);
            tempSlot.Scale = Vector3.one;

            return tempSlot;
        }

        public void Init()
        {
            Position = transform.position + offset;
            Scale = transform.localScale;
            Rotation = transform.eulerAngles;
        }

        public void Assign(SlotCase slotCase, bool instant = false)
        {
            SlotCase = slotCase;

            SlotCase.SubmitMove(Position, Scale, Rotation, instant);
        }

        public void AssingFast(SlotCase slotCase)
        {
            SlotCase = slotCase;

            SlotCase.ShiftMove(Position);
        }

        public void AssingWithoutMove(SlotCase slotCase)
        {
            SlotCase = slotCase;
        }

        public void Move()
        {
            if (SlotCase != null)
            {
                SlotCase.ShiftMove(Position);
            }
        }

        public SlotCase RemoveSlot()
        {
            var slotCase = SlotCase;
            SlotCase = null;
            return slotCase;
        }

        public void ChangeColor(Color color)
        {
            if (coloredImage == null || backImage == null) return;

            colorCase.KillActive();

            coloredImage.color = color;
            coloredImage.transform.localScale = Vector3.zero;
            coloredImage.gameObject.SetActive(true);

            colorCase = coloredImage.DOScale(1, 0.15f).OnComplete(() => {
                backImage.color = color;
                coloredImage.gameObject.SetActive(false);
            });
        }

        public void RestoreColor(Color color)
        {
            if (coloredImage == null || backImage == null) return;

            colorCase.KillActive();

            coloredImage.color = backImage.color;
            coloredImage.transform.localScale = Vector3.one;
            coloredImage.gameObject.SetActive(true);

            backImage.color = color;

            colorCase = coloredImage.DOScale(0, 0.15f).OnComplete(() => {
                coloredImage.gameObject.SetActive(false);
            });
        }

        public void ClearColor()
        {
            colorCase.KillActive();
            if (backImage != null) backImage.color = Color.white;

            if (coloredImage != null) coloredImage.gameObject.SetActive(false);
        }

        public void Clear()
        {
            ClearColor();
            if (SlotCase != null) SlotCase.Clear();
            SlotCase = null;
        }
    }
}