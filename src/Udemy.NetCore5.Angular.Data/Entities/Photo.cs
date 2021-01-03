﻿namespace Udemy.NetCore5.Angular.Data.Entities
{
    public class Photo
    {
        public int Id { get; set; }

        public string Url { get; set; }

        public bool MainPhoto { get; set; }

        public string PublicId { get; set; }
    }
}