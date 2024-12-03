using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Watermelon
{
    [CreateAssetMenu(fileName = "Core Organizer", menuName = "Tools/Core Organizer")]
    public class CoreOrganizer : ScriptableObject
    {
        [SerializeField] CoreFolderData[] coreSettings;
    }


    [System.Serializable]
    public class CoreFolderData
    {
        [SerializeField] string name;
        [SerializeField] string directory; // obsolete but we keep it to not break anything
        [SerializeField] string[] directories;
        [SerializeField] bool[] isDirectoryExist;
        [SerializeField] string status;
        [SerializeField] bool include;
    }

}