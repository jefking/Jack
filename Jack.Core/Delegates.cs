using System;

namespace Jack.Core
{
    /// <summary>
    /// Delegate for specifying custom arguments
    /// </summary>
    /// <param name="sender">Sender</param>
    /// <param name="args">Event Arguments</param>
    public delegate void EventHandler<Argument>(object sender
        , EventArguments<Argument> args);
    /// <summary>
    /// Event Handler For single Paramater, and custom return type
    /// </summary>
    /// <typeparam name="Argument">Argument</typeparam>
    /// <typeparam name="Return">Return Type</typeparam>
    /// <param name="args">Arguments</param>
    /// <returns>Return Data</returns>
    public delegate Return EventHandler<Argument, Return>(Argument args);
    /// <summary>
    /// Event Handler which returns Void
    /// </summary>
    /// <typeparam name="Argument">Argument</typeparam>
    /// <param name="args">Argument</param>
    public delegate void EventHandlerNoReturn<Argument>(Argument args);
    /// <summary>
    /// Event Handler which takes no arguments, but returns X
    /// </summary>
    /// <typeparam name="Argument">Argument</typeparam>
    /// <param name="args">Argument</param>
    public delegate Argument EventHandlerWithReturn<Argument>();
}