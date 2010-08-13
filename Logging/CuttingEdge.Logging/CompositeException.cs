using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;

namespace CuttingEdge.Logging
{
    /// <summary>
    /// Represents errors that occur during application execution.
    /// </summary>
    [Serializable]
    [DebuggerDisplay("Count = {InnerExceptions.Count}")]
    //// NOTE: The public interface of this class is identical to a part of that of of the .NET 4.0
    //// AggregateException. We can't use AggregateException, because CuttingEdge.Logging should be usable
    //// with .NET 2.0. But by having a copy of AggregateException's public API when can later on do two
    //// important things:
    //// 1. Log both AggregateExceptions and CompositeExceptions while keeping the .NET 2.0 requirement, by 
    ////    using reflection.
    //// 2. Letting CompositeException inherit from AggregateException in a later release. This ensures that
    ////    the new release will stay compatible with the old version. Of course that new release is in that
    ////    case dependant on .NET 4.0 (but we could also compile two versions of that release).
    public class CompositeException : Exception
    {
        private ReadOnlyCollection<Exception> innerExceptions;

        /// <summary>Initializes a new instance of the <see cref="CompositeException"/> class.</summary>
        public CompositeException() : base("One or more errors occurred.")
        {
            this.innerExceptions = new ReadOnlyCollection<Exception>(new Exception[0]);
        }

        /// <summary>Initializes a new instance of the <see cref="CompositeException"/> class.</summary>
        /// <param name="message">The message.</param>
        public CompositeException(string message) : base(message)
        {
            this.innerExceptions = new ReadOnlyCollection<Exception>(new Exception[0]);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompositeException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public CompositeException(string message, Exception innerException) : base(message, innerException)
        {
            if (innerException == null)
            {
                throw new ArgumentNullException("innerException");
            }

            this.innerExceptions = new ReadOnlyCollection<Exception>(new Exception[] { innerException });
        }

        /// <summary>Initializes a new instance of the <see cref="CompositeException"/> class.</summary>
        /// <param name="message">The message.</param>
        /// <param name="innerExceptions">The inner exceptions.</param>
        public CompositeException(string message, IEnumerable<Exception> innerExceptions)
            : this(message, 
            innerExceptions == null ? null : ((IList<Exception>)new List<Exception>(innerExceptions)))
        {
        }

        /// <summary>Initializes a new instance of the <see cref="CompositeException"/> class.</summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
        /// <exception cref="ArgumentNullException">
        /// The <paramref name="info"/> parameter is null.
        /// </exception>
        /// <exception cref="SerializationException">
        /// The class name is null or <see cref="P:System.Exception.HResult"/> is zero (0).
        /// </exception>
        [SecurityCritical]
        protected CompositeException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }

            Exception[] list = info.GetValue("InnerExceptions", typeof(Exception[])) as Exception[];

            if (list == null)
            {
                throw new SerializationException("The serialization stream contains no inner exceptions.");
            }

            this.innerExceptions = new ReadOnlyCollection<Exception>(list);
        }

        private CompositeException(string message, IList<Exception> innerExceptions)
            : base(message, innerExceptions != null && innerExceptions.Count > 0 ? innerExceptions[0] : null)
        {
            if (innerExceptions == null)
            {
                throw new ArgumentNullException("innerExceptions");
            }

            Exception[] list = new Exception[innerExceptions.Count];

            for (int i = 0; i < list.Length; i++)
            {
                list[i] = innerExceptions[i];

                if (list[i] == null)
                {
                    throw new ArgumentException("An element of innerExceptions was null.");
                }
            }

            this.innerExceptions = new ReadOnlyCollection<Exception>(list);
        }

        /// <summary>Gets the inner exceptions.</summary>
        /// <value>The inner exceptions.</value>
        public ReadOnlyCollection<Exception> InnerExceptions
        {
            get { return this.innerExceptions; }
        }

        /// <summary>
        /// When overridden in a derived class, sets the <see cref="SerializationInfo"/> with information about the exception.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// The <paramref name="info"/> parameter is a null reference (Nothing in Visual Basic).
        /// </exception>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }

            base.GetObjectData(info, context);
            Exception[] array = new Exception[this.innerExceptions.Count];
            this.innerExceptions.CopyTo(array, 0);
            info.AddValue("InnerExceptions", array, typeof(Exception[]));
        }

        /// <summary>Creates and returns a string representation of the current exception.</summary>
        /// <returns>A string representation of the current exception.</returns>
        public override string ToString()
        {
            string str = base.ToString();

            for (int i = 0; i < this.innerExceptions.Count; i++)
            {
                str = string.Format(CultureInfo.InvariantCulture, 
                    "{0}{1}---> (Inner Exception #{2}) {3}{4}{5}", 
                    str,
                    Environment.NewLine,
                    i,
                    this.innerExceptions[i].ToString(),
                    "<---",
                    Environment.NewLine);
            }

            return str;
        }
    }
}
