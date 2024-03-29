﻿namespace Kadena.AmazonFileSystemProvider
{
    /// <summary>
    /// Factory for creating S3ObjectInfo a S3ObjectInfoProvider.
    /// </summary>
    public class S3ObjectFactory
    {
        private static IS3ObjectInfoProvider mProvider;

        /// <summary>Get or set S3ObjectInfoProvider.</summary>
        public static IS3ObjectInfoProvider Provider
        {
            get
            {
                if (mProvider == null)
                {
                    mProvider = S3ObjectInfoProvider.Current;
                }
                return mProvider;
            }
            set
            {
                mProvider = value;
            }
        }

        /// <summary>Returns new instance of IS3ObjectInfo object.</summary>
        /// <param name="path">Path with file name.</param>
        public static IS3ObjectInfo GetInfo(string path)
        {
            return Provider.GetInfo(path);
        }

        /// <summary>
        /// Initializes new instance of S3 object info with specified bucket name.
        /// </summary>
        /// <param name="path">Path with file name.</param>
        /// <param name="key">Specifies that given path is already object key.</param>
        public static IS3ObjectInfo GetInfo(string path, bool key)
        {
            return Provider.GetInfo(path, key);
        }
    }
}