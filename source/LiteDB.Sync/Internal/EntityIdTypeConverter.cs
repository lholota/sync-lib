using System;
using System.ComponentModel;
using System.Globalization;

namespace LiteDB.Sync.Internal
{
    //public class EntityIdTypeConverter : TypeConverter
    //{
    //    public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
    //    {
    //        return sourceType == typeof(EntityId);
    //    }

    //    public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
    //    {
    //        var entityId = value as EntityId;

    //        if (entityId == null)
    //        {
    //            throw new ArgumentException("The value must be an EntityId.");
    //        }

    //        return entityId.ToString();
    //    }

    //    public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
    //    {
    //        return destinationType == typeof(string);
    //    }

    //    public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
    //    {
    //        var str = value as string;

    //        if (str == null)
    //        {
    //            throw new ArgumentException("The value must be a string.");
    //        }

    //        return EntityId.FromString(str);
    //    }
    //}
}