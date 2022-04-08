// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.ClearScript
{
    /// <summary>
    /// Defines properties that comprise ClearScript's global configuration.
    /// </summary>
    public static class HostSettings
    {
        private static CustomAttributeLoader customAttributeLoader;
        private static readonly CustomAttributeLoader defaultCustomAttributeLoader = new CustomAttributeLoader();

        /// <summary>
        /// Enables or disables assembly table usage.
        /// </summary>
        /// <remarks>
        ///<para>
        /// The assembly table is a legacy internal feature intended to accelerate assembly
        /// loading. Because it relies on deprecated platform functionality, this feature is now
        /// disabled by default. Although its replacement is simpler and more efficient, the
        /// feature is still available to provide full compatibility with older ClearScript
        /// releases.
        /// </para>
        ///<para>
        /// The assembly table feature is only available on .NET Framework. This property has no
        /// effect on other platforms.
        /// </para>
        /// </remarks>
        public static bool UseAssemblyTable { get; set; }

        /// <summary>
        /// Gets or sets a semicolon-delimited list of directory paths to search for auxiliary files.
        /// </summary>
        /// <remarks>
        /// This property allows the host to augment ClearScript's algorithm for locating unmanaged
        /// resources such as native assemblies and related data files.
        /// </remarks>
        public static string AuxiliarySearchPath { get; set; }

        /// <summary>
        /// Sets the runtime platform to android and loads android native binaries.
        /// </summary>
        /// <remarks>
        /// This property allows ClearScript to load android native assemblies instead of 
        /// linux assemblies on Android Mono
        /// </remarks>
        public static bool IsAndroid { get; set; }

        /// <summary>
        /// Gets or sets the custom attribute loader for ClearScript.
        /// </summary>
        public static CustomAttributeLoader CustomAttributeLoader
        {
            get => customAttributeLoader ?? defaultCustomAttributeLoader;

            set
            {
                customAttributeLoader = value;
                CustomAttributes.ClearCache();
            }
        }
    }
}
