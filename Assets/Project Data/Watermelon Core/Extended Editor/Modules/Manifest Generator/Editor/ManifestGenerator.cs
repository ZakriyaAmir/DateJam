using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;

namespace Watermelon
{
    public static class MatifestGenerator
    {
        private const string TEMPLATE_FILE_NAME = "AndroidManifestTemplate";

        private static readonly string MANIFEST_PATH = Application.dataPath + @"/Plugins/Android/";
        private static readonly string MANIFEST_FILE_NAME = "AndroidManifest.xml";

        private const string PERMISSION_TEMPLATE = "    <uses-permission android:name=\"{0}\" />";
        private static readonly string[] PERMISSIONS = new string[]
        {
            "com.google.android.gms.permission.AD_ID",
            "android.permission.VIBRATE"
        };

        [InitializeOnLoadMethod]
        public static void Initialise()
        {
#if UNITY_ANDROID
            // Check if manifest exists
            if (!File.Exists(MANIFEST_PATH + MANIFEST_FILE_NAME))
            {
                TextAsset templateTextAsset = EditorUtils.GetAsset<TextAsset>(TEMPLATE_FILE_NAME);
    
                if (templateTextAsset != null)
                {
                    StringBuilder sb = new StringBuilder();
                    for(int i = 0; i < PERMISSIONS.Length; i++)
                    {
                        sb.AppendLine(string.Format(PERMISSION_TEMPLATE, PERMISSIONS[i]));
                    }

                    // Insert permissions to file
                    string permissionsText = templateTextAsset.text.Replace("{PERMISSIONS}", sb.ToString());

                    // Create path
                    IOUtils.CreatePath(MANIFEST_PATH);

                    // Create file
                    File.WriteAllText(MANIFEST_PATH + MANIFEST_FILE_NAME, permissionsText);

                    // Update file system
                    AssetDatabase.SaveAssets();
                }
            }
#endif
        }
    }
}