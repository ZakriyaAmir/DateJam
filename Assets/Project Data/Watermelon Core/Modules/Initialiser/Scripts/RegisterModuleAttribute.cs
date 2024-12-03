using System;

namespace Watermelon
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class RegisterModuleAttribute : Attribute
    {
        public string Path;
        public bool Core;

        public RegisterModuleAttribute(string path, bool core = false)
        {
            Path = path;
            Core = core;
        }
    }
}
