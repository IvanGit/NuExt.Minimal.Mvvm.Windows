namespace System.Windows
{
    /// <summary>
    /// The BindingProxy class is used to pass DataContext between elements without a direct binding path. 
    /// </summary>
    /// <remarks>
    /// This class is particularly useful in WPF applications where you need to bind data
    /// across different elements that might be separated by templates or other containers
    /// which do not permit direct DataContext inheritance.
    /// 
    /// Example usage:
    /// <code>
    /// <![CDATA[
    /// <Window.Resources>
    ///     <BindingProxy x:Key="proxy" DataContext="{Binding SomeViewModel}" />
    /// </Window.Resources>
    ///
    /// <Grid DataContext="{Binding Source={StaticResource proxy}, Path=DataContext}">
    ///     <!-- Your UI elements here -->
    /// </Grid>
    /// ]]>
    /// </code>
    /// </remarks>
    public class BindingProxy : BindingProxy<object>
    {
    }
}
