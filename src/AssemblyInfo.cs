﻿using System.Windows;
using System.Windows.Markup;

[assembly: ThemeInfo(
    ResourceDictionaryLocation.None,            //where theme specific resource dictionaries are located
                                                //(used if a resource is not found in the page,
                                                // or application resource dictionaries)
    ResourceDictionaryLocation.SourceAssembly   //where the generic resource dictionary is located
                                                //(used if a resource is not found in the page,
                                                // app, or any theme specific resource dictionaries)
)]

[assembly: XmlnsPrefix("http://schemas.minimalmvvm.com/winfx/xaml/mvvm", "minimal")]
[assembly: XmlnsDefinition("http://schemas.minimalmvvm.com/winfx/xaml/mvvm", "Minimal.Mvvm")]
[assembly: XmlnsDefinition("http://schemas.minimalmvvm.com/winfx/xaml/mvvm", "Minimal.Mvvm.Windows")]
[assembly: XmlnsDefinition("http://schemas.minimalmvvm.com/winfx/xaml/mvvm", "Minimal.Mvvm.Windows.Controls")]