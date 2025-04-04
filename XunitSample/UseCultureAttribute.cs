﻿using System;
using System.Globalization;
using System.Reflection;
using System.Threading;
using Xunit.v3;

namespace XunitSample
{
    /// <summary>
    /// Apply this attribute to your test method to replace the
    /// <see cref="Thread.CurrentThread" /> <see cref="CultureInfo.CurrentCulture" /> and
    /// <see cref="CultureInfo.CurrentUICulture" /> with another culture.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class UseCultureAttribute : BeforeAfterTestAttribute
    {
        private readonly Lazy<CultureInfo> _culture;
        private readonly Lazy<CultureInfo> _uiCulture;

        private CultureInfo _originalCulture;
        private CultureInfo _originalUiCulture;

        /// <summary>
        /// Replaces the culture and UI culture of the current thread with
        /// <paramref name="culture" />
        /// </summary>
        /// <param name="culture">The name of the culture.</param>
        /// <remarks>
        /// <para>
        /// This constructor overload uses <paramref name="culture" /> for both
        /// <see cref="Culture" /> and <see cref="UICulture" />.
        /// </para>
        /// </remarks>
        public UseCultureAttribute(string culture)
            : this(culture, culture) { }

        /// <summary>
        /// Replaces the culture and UI culture of the current thread with
        /// <paramref name="culture" /> and <paramref name="uiCulture" />
        /// </summary>
        /// <param name="culture">The name of the culture.</param>
        /// <param name="uiCulture">The name of the UI culture.</param>
        public UseCultureAttribute(string culture, string uiCulture)
        {
            _culture = new Lazy<CultureInfo>(() => new CultureInfo(culture, false));
            _uiCulture = new Lazy<CultureInfo>(() => new CultureInfo(uiCulture, false));
        }

        /// <summary>
        /// Gets the culture.
        /// </summary>
        public CultureInfo Culture { get { return _culture.Value; } }

        /// <summary>
        /// Gets the UI culture.
        /// </summary>
        public CultureInfo UICulture { get { return _uiCulture.Value; } }

        /// <summary>
        /// Stores the current <see cref="Thread.CurrentPrincipal" />
        /// <see cref="CultureInfo.CurrentCulture" /> and <see cref="CultureInfo.CurrentUICulture" />
        /// and replaces them with the new cultures defined in the constructor.
        /// </summary>
        public override void Before(MethodInfo methodUnderTest, IXunitTest test)
        {
            _originalCulture = Thread.CurrentThread.CurrentCulture;
            _originalUiCulture = Thread.CurrentThread.CurrentUICulture;

            Thread.CurrentThread.CurrentCulture = Culture;
            Thread.CurrentThread.CurrentUICulture = UICulture;

            CultureInfo.CurrentCulture.ClearCachedData();
            CultureInfo.CurrentUICulture.ClearCachedData();
        }

        /// <summary>
        /// Restores the original <see cref="CultureInfo.CurrentCulture" /> and
        /// <see cref="CultureInfo.CurrentUICulture" /> to <see cref="Thread.CurrentPrincipal" />
        /// </summary>
        public override void After(MethodInfo methodUnderTest, IXunitTest test)
        {
            Thread.CurrentThread.CurrentCulture = _originalCulture;
            Thread.CurrentThread.CurrentUICulture = _originalUiCulture;

            CultureInfo.CurrentCulture.ClearCachedData();
            CultureInfo.CurrentUICulture.ClearCachedData();
        }
    }
}
