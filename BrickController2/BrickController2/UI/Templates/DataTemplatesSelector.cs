using BrickController2.UI.ViewModels;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace BrickController2.UI.Templates;

public class DataTemplatesSelector : DataTemplateSelector
{
    public DataTemplate BoolDataTemplate { get; set; }

    public DataTemplate EnumDataTemplate { get; set; }

    protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
    {
        var itemType = item.GetType();

        if (itemType == typeof(DeviceBoolSettingViewModel))
            return BoolDataTemplate;
        if (itemType == typeof(DeviceEnumSettingViewModel))
            return EnumDataTemplate;

        return default!;
    }
}
