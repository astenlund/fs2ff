namespace fs2ff
{
    public class ViewModelLocator
    {
        public static MainViewModel Main => App.GetRequiredService<MainViewModel>();
    }
}
