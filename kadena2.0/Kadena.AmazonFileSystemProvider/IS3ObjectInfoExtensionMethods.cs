﻿namespace Kadena.AmazonFileSystemProvider
{
    /// <summary>
    /// Extension methods for <see cref="T:CMS.AmazonStorage.IS3ObjectInfo" />.
    /// </summary>
    public static class IS3ObjectInfoExtensionMethods
    {
        /// <summary>
        /// Returns bucket name for current <see cref="T:CMS.AmazonStorage.IS3ObjectInfo" />.
        /// </summary>
        /// <param name="obj">Representation of a file in Amazon S3 storage.</param>
        public static string GetBucketName(this IS3ObjectInfo obj)
        {
            return S3ObjectInfoProvider.GetBucketName(PathHelper.GetPathFromObjectKey(obj.Key, true));
        }
    }
}
