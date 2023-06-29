using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using shopapp.entity;

namespace shopapp.data.Abstract
{
    public interface ICardRepository:IRepository<Card>
    {
        void ClearCard(int cardId);
        void DeleteFromCard(int cardId, int productId);
        Card GetByUserId(string userId);
    }
}