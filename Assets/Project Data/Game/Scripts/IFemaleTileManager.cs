using System.Collections.Generic;
using UnityEngine;

namespace Watermelon.BusStop
{
    public interface IFemaleTileManager
    {
        abstract void AddUniqueGameObjects(List<GameObject> tempMale);
        abstract void RemoveCharacter(int row, int col);
    }
}