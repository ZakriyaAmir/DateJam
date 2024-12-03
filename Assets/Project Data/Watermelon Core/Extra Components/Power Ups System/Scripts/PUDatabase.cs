using UnityEngine;

namespace Watermelon
{
    [CreateAssetMenu(fileName = "Power Ups Database", menuName = "Content/Power Ups/Database")]
    public class PUDatabase : ScriptableObject
    {
        [SerializeField] PUSettings[] powerUps;
        public PUSettings[] PowerUps => powerUps;
    }
}
