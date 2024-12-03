using System;

namespace Watermelon
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class ButtonAttribute : DrawerAttribute
    {
        private string text;
        public string Text => text;

        private object[] methodParams;
        public object[] Params => methodParams;

        private string visabilityConditionName;
        public string VisabilityConditionName => visabilityConditionName;

        private ButtonVisability visabilityOption;
        public ButtonVisability VisabilityOption => visabilityOption;
        
        public ButtonAttribute()
        {
            text = null;
            methodParams = null;

            visabilityConditionName = null;
        }

        public ButtonAttribute(string text = null, params object[] methodParams)
        {
            this.text = text;
            this.methodParams = methodParams;

            visabilityConditionName = null;
        }

        public ButtonAttribute(string text = null, string visabilityMethodName = "", ButtonVisability visabilityOption = ButtonVisability.ShowIf, params object[] methodParams)
        {
            this.text = text;
            this.visabilityConditionName = visabilityMethodName;
            this.visabilityOption = visabilityOption;
            this.methodParams = methodParams;
        }
    }
}
