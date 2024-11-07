# NuExt.Minimal.Mvvm.Windows

`NuExt.Minimal.Mvvm.Windows` is an extension for the lightweight MVVM framework [NuExt.Minimal.Mvvm](https://github.com/IvanGit/NuExt.Minimal.Mvvm). This package is specifically designed to enhance development for WPF applications by providing additional components and utilities that simplify development, reduce routine work, and add functionality to your MVVM applications. A key focus of this package is to offer robust support for asynchronous operations, making it easier to manage complex scenarios involving asynchronous tasks and commands.

### Commonly Used Types

- **`Minimal.Mvvm.ModelBase`**: Base class for creating bindable models.
- **`Minimal.Mvvm.Windows.ControlViewModel`**: Base class for control-specific ViewModels and designed for asynchronous disposal.
- **`Minimal.Mvvm.Windows.DocumentContentViewModelBase`**: Base class for ViewModels that represent document content.
- **`Minimal.Mvvm.Windows.WindowViewModel`**: Base class for window-specific ViewModels.
- **`Minimal.Mvvm.Windows.IAsyncDialogService`**: Displays dialog windows asynchronously.
- **`Minimal.Mvvm.Windows.IAsyncDocument`**: Asynchronous document created with `IAsyncDocumentManagerService`.
- **`Minimal.Mvvm.Windows.IAsyncDocumentManagerService`**: Manages asynchronous documents.
- **`Minimal.Mvvm.Windows.InputDialogService`**: Shows modal dialogs asynchronously.
- **`Minimal.Mvvm.Windows.OpenWindowsService`**: Manages open window ViewModels within the application.
- **`Minimal.Mvvm.Windows.SettingsService`**: Facilitates saving and loading settings.
- **`Minimal.Mvvm.Windows.TabbedDocumentService`**: Manages tabbed documents within a UI.
- **`Minimal.Mvvm.Windows.ViewLocator`**: Locates and initializes views based on view models.
- **`Minimal.Mvvm.Windows.WindowPlacementService`**: Saves and restores window placement between runs.

### Installation

You can install `NuExt.Minimal.Mvvm.Windows` via [NuGet](https://www.nuget.org/):

```sh
dotnet add package NuExt.Minimal.Mvvm.Windows
```

Or through the Visual Studio package manager:

1. Go to `Tools -> NuGet Package Manager -> Manage NuGet Packages for Solution...`.
2. Search for `NuExt.Minimal.Mvvm.Windows`.
3. Click "Install".

### Usage Examples

For comprehensive examples of how to use the package, refer to the [samples](samples) directory in this repository and the [NuExt.Minimal.Mvvm.MahApps.Metro](https://github.com/IvanGit/NuExt.Minimal.Mvvm.MahApps.Metro) repository. These samples illustrate best practices for using these extensions.

### Contributing

Contributions are welcome! Feel free to submit issues, fork the repository, and send pull requests. Your feedback and suggestions for improvement are highly appreciated.

### Acknowledgements

Special thanks to the creators and maintainers of the [DevExpress MVVM Framework](https://github.com/DevExpress/DevExpress.Mvvm.Free). The author has been inspired by its advanced features and design philosophy for many years. However, as technology evolves, the DevExpress MVVM Framework has started to resemble more of a legacy framework, falling behind modern asynchronous best practices and contemporary development paradigms. The need for better support for asynchronous programming, greater simplicity, and improved performance led to the creation of these projects.

### License

Licensed under the MIT License. See the LICENSE file for details.