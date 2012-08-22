using System;
using System.ComponentModel.Composition;

namespace AshokGelal.InstallBaker.Events
{
    [Export]
    internal class BaseEventsService : IDisposable
    {
        #region Fields

        protected readonly InstallBakerEventAggregator _eventAggregator;

        #endregion Fields

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether this instance is disposed.
        /// </summary>
        protected bool IsDisposed
        {
            get; set;
        }

        #endregion Properties

        #region Constructors

        public BaseEventsService(InstallBakerEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
        }

        #endregion Constructors

        #region Dispose

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                IsDisposed = true;
            }
        }

        #endregion Dispose
    }
}