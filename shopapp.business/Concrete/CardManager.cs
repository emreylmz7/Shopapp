using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using shopapp.business.Abstract;
using shopapp.data.Abstract;
using shopapp.entity;

namespace shopapp.business.Concrete
{
    public class CardManager : ICardService
    {
        private readonly IUnitOfWork _unitofwork;
        public CardManager(IUnitOfWork unitofwork)
        {
            _unitofwork = unitofwork;    
        }



        public void AddToCard(string userId, int productId, int quantity)
        {
            var card = GetCardByUserId(userId);
            if (card!=null)
            {
                //eklemek isteyen ürün sepette var mı(güncelleme)
                //eklemek istenen ürün sepette var ve yeni kayıt oluştur

                var index = card.CardItems.FindIndex(i=>i.ProductId==productId);
                if (index<0)
                {
                    card.CardItems.Add(new CardItem(){
                        ProductId = productId,
                        Quantity = quantity,
                        CardId = card.Id
                    });
                }
                else
                {
                    card.CardItems[index].Quantity += quantity;
                }

                _unitofwork.Cards.Update(card);
                _unitofwork.Save();
                
            }
        }

        public void ClearCard(int cardId)
        {
            _unitofwork.Cards.ClearCard(cardId);
        }

        public void DeleteFromCard(string userId, int productId)
        {
            var card = GetCardByUserId(userId);
            if (card!=null)
            {
                _unitofwork.Cards.DeleteFromCard(card.Id,productId);
                
            }
        }

        public Card GetCardByUserId(string userId)
        {
            return _unitofwork.Cards.GetByUserId(userId);
        }

        public void InitializeCard(string userId)
        {
            _unitofwork.Cards.Create(new Card(){UserId = userId}); 
            _unitofwork.Save();
        }
    }
}