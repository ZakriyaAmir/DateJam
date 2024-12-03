using System;
using System.Collections.Generic;

namespace Watermelon
{
    public static class CustomAttributesDatabase
    {
        private static Dictionary<Type, FieldDrawer> fieldAttributeType;
        private static Dictionary<Type, MethodDrawer> methodAttributeType;
        private static Dictionary<Type, PropertyDrawCondition> conditionsAttributeType;
        private static Dictionary<Type, PropertyDrawer> propertyAttributeType;
        private static Dictionary<Type, PropertyGrouper> groupersAttributeType;
        private static Dictionary<Type, PropertyMeta> metaAttributeType;

        static CustomAttributesDatabase()
        {
            // Drawer attributes
            fieldAttributeType = new Dictionary<Type, FieldDrawer>();
            fieldAttributeType[typeof(ShowNonSerializedFieldAttribute)] = new ShowNonSerializedFieldFieldDrawer();

            // Method attributes
            methodAttributeType = new Dictionary<Type, MethodDrawer>();
            methodAttributeType[typeof(ButtonAttribute)] = new ButtonMethodDrawer();

            // Condition attributes
            conditionsAttributeType = new Dictionary<Type, PropertyDrawCondition>();
            conditionsAttributeType[typeof(HideIfAttribute)] = new HideIfPropertyDrawCondition();
            conditionsAttributeType[typeof(ShowIfAttribute)] = new ShowIfPropertyDrawCondition();

            // Property attributes
            propertyAttributeType = new Dictionary<Type, PropertyDrawer>();
            propertyAttributeType[typeof(MinMaxSliderAttribute)] = new MinMaxSliderPropertyDrawer();
            propertyAttributeType[typeof(ReadOnlyFieldAttribute)] = new ReadOnlyFieldPropertyDrawer();
            propertyAttributeType[typeof(ReorderableListAttribute)] = new ReorderableListPropertyDrawer();
            propertyAttributeType[typeof(ResizableTextAreaAttribute)] = new ResizableTextAreaPropertyDrawer();
            propertyAttributeType[typeof(SliderAttribute)] = new SliderPropertyDrawer();
            propertyAttributeType[typeof(EnumFlagsAttribute)] = new EnumFlagsAttributeDrawer();

            // Group attributes
            groupersAttributeType = new Dictionary<Type, PropertyGrouper>();
            groupersAttributeType[typeof(BoxGroupAttribute)] = new BoxGroupPropertyGrouper();

            // Meta attributes
            metaAttributeType = new Dictionary<Type, PropertyMeta>();
            metaAttributeType[typeof(InfoBoxAttribute)] = new InfoBoxPropertyMeta();
            metaAttributeType[typeof(OnValueChangedAttribute)] = new OnValueChangedPropertyMeta();
        }

        public static FieldDrawer GetFieldAttribute(Type attributeType)
        {
            FieldDrawer drawer;
            if (fieldAttributeType.TryGetValue(attributeType, out drawer))
            {
                return drawer;
            }

            return null;
        }

        public static MethodDrawer GetMethodAttribute(Type attributeType)
        {
            MethodDrawer drawer;
            if (methodAttributeType.TryGetValue(attributeType, out drawer))
            {
                return drawer;
            }

            return null;
        }

        public static PropertyDrawCondition GetDrawConditionAttribute(Type attributeType)
        {
            PropertyDrawCondition drawCondition;
            if (conditionsAttributeType.TryGetValue(attributeType, out drawCondition))
            {
                return drawCondition;
            }

            return null;
        }

        public static PropertyDrawer GetPropertyAttribute(Type attributeType)
        {
            PropertyDrawer drawer;
            if (propertyAttributeType.TryGetValue(attributeType, out drawer))
            {
                return drawer;
            }

            return null;
        }

        public static PropertyGrouper GetGroupAttribute(Type attributeType)
        {
            PropertyGrouper grouper;
            if (groupersAttributeType.TryGetValue(attributeType, out grouper))
            {
                return grouper;
            }

            return null;
        }

        public static PropertyMeta GetMetaAttribute(Type attributeType)
        {
            PropertyMeta meta;
            if (metaAttributeType.TryGetValue(attributeType, out meta))
            {
                return meta;
            }

            return null;
        }

        public static void Clear()
        {
            foreach (PropertyDrawer attribute in propertyAttributeType.Values)
            {
                attribute.ClearCache();
            }
        }
    }
}

