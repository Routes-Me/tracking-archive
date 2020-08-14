﻿using System;
using System.Collections.Generic;

namespace AdvertisementService.Models.DBModels
{
    public partial class Mediametadata
    {
        public Mediametadata()
        {
            Medias = new HashSet<Medias>();
        }

        public int MediaMetadataId { get; set; }
        public float? Size { get; set; }
        public float? Duration { get; set; }

        public virtual ICollection<Medias> Medias { get; set; }
    }
}
