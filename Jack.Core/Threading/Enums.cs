namespace Jack.Core.Threading
{
    #region Loading State
    /// <summary>
    /// Loading State
    /// </summary>
    public enum LoadingState : byte
    {
        NotLoaded = 0
        , Loading = 1
        , Loaded = 2
        , Failed = 3
    }
    #endregion
}