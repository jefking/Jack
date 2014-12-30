namespace Jack.Core.IO.Storage
{
    /// <summary>
    /// Location
    /// </summary>
    internal enum Location : byte
    {
        Any = 0
        , Local = 1
        , Remote = 2
    }
    /// <summary>
    /// Drive Type
    /// </summary>
    public enum DriveType : int
    {
        Unknown = 0
        , NoRootDirectory = 1
        , Removable = 2
        , Fixed = 3
        , Remote = 4
        , CDROM = 5
        , RAM = 6
    }
}