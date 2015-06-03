using System;

namespace CoreAudioApi
{
    /// <summary>
    ///     PROPERTYKEY is defined in wtypes.h
    /// </summary>
    public struct PropertyKey
    {
        /// <summary>
        ///     Format ID
        /// </summary>
        public Guid FormatId;

        /// <summary>
        ///     Property ID
        /// </summary>
        public int PropertyId;

        /// <summary>
        ///     <param name="formatId"></param>
        ///     <param name="propertyId"></param>
        /// </summary>
        public PropertyKey(Guid formatId, int propertyId)
        {
            FormatId = formatId;
            PropertyId = propertyId;
        }
    }
}