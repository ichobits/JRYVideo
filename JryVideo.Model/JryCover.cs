﻿using System;
using System.Diagnostics;
using System.IO;
using MongoDB.Bson.Serialization.Attributes;

namespace JryVideo.Model
{
    public sealed partial class JryCover : RootObject
    {
        public CoverType CoverType { get; set; }

        [BsonIgnoreIfDefault]
        public byte[] BinaryData { get; set; }

        [BsonIgnore]
        public Stream BinaryStream => this.BinaryData.ToMemoryStream();

        /// <summary>
        /// (value / 10) was opacity.
        /// </summary>
        [BsonIgnoreIfDefault]
        public int? Opacity { get; set; }

        protected override bool InnerTestHasError()
        {
            if (base.InnerTestHasError()) return true;

            if (this.BinaryData == null || this.BinaryData.Length == 0)
            {
                JasilyLogger.Current.WriteLine<JryCover>(JasilyLogger.LoggerMode.Debug, "cover data can not be empty.");
                return true;
            }

            return false;
        }

        public override void CheckError()
        {
            base.CheckError();
            DataChecker.NotNull(this.BinaryData);
        }
    }
}