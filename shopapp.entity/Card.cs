using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace shopapp.entity
{
    public class Card
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public List<CardItem> CardItems { get; set; }
    }
}