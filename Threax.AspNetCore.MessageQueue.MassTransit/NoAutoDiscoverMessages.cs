using System;
using System.Collections.Generic;
using System.Text;

namespace Threax.AspNetCore.MessageQueue.MassTransit
{
    /// <summary>
    /// Mark a consumer or saga with this attribute to ignore it during auto detection.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class NoAutoDiscoverMessages : Attribute
    {
    }
}
