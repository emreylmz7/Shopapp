using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using shopapp.data.Abstract;
using shopapp.entity;

namespace shopapp.data.Concrete.EfCore
{
    public class EfCoreCardRepository : EfCoreGenericRepository<Card>, ICardRepository
    {
        public EfCoreCardRepository(ShopContext context): base(context)
        {
            
        }
        private ShopContext ShopContext
        {
            get{return context as ShopContext;}
        }



        public void ClearCard(int cardId)
        {
            var cmd = @"delete from CardItems where CardId=@p0 ";
            ShopContext.Database.ExecuteSqlRaw(cmd,cardId);
        }

        public void DeleteFromCard(int cardId, int productId)
        {
            var cmd = @"delete from CardItems where CardId=@p0 and ProductId=@p1";
            ShopContext.Database.ExecuteSqlRaw(cmd,cardId,productId);
        }

        public Card GetByUserId(string userId)
        {
            return ShopContext.Cards
                            .Include(i=>i.CardItems)
                            .ThenInclude(i=>i.Product)
                            .FirstOrDefault(i=>i.UserId==userId);
        }

        public override void Update(Card entity)
        {
                ShopContext.Cards.Update(entity);
        }
    }
}