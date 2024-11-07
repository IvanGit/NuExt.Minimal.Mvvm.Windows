﻿using Minimal.Mvvm.Windows;
using System.IO;

namespace WpfAppSample.Services
{
    public sealed class EnvironmentService : EnvironmentServiceBase
    {
        public EnvironmentService(string baseDirectory, params string[] args) : base(baseDirectory, Path.Combine(baseDirectory, "AppData"), args)
        {

        }
    }
}
