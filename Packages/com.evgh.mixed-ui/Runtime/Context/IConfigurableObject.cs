namespace Evgh.MixedUI
{
    /// <summary>Sample capability consumed by the initial configuration composite.</summary>
    public interface IConfigurableObject
    {
        string DisplayName { get; }
        bool IsVisible { get; set; }
        float Scale { get; set; }
        bool CanDelete { get; }
        void Delete();
    }
}

