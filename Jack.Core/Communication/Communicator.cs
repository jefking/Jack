using System;

namespace Jack.Core.Communication
{
    /// <summary>
    /// Communicator Attribute
    /// </summary>
    /// <remarks>
    /// Used to denote classes that are used to communicate through RPC
    /// place attribute on class and it will be registered during service load
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class
        , AllowMultiple = true)]
    internal class Communicator : Attribute {}
}