using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trellura.API.Data;
using Trellura.API.Models;

namespace Trellura.API.Hubs
{
    public class TrelluraHub : Hub
    {
        private TrelluraDbContext _context;

        public TrelluraHub(TrelluraDbContext context)
        {
            _context = context;
        }

        public async Task ObterTodosCards()
        {
            var cards = await _context.Cards.ToListAsync();
            await Clients.All.SendAsync("receberTodosCards", cards);
        }

        // cria um card - será se vou precisar deserializar????
        public async Task CriarCard(Card card)
        {
            _context.Cards.Add(card);
            await _context.SaveChangesAsync();
            // recuperar cards e envia no sendAsync? ou no cliente chama chama novamente o ObterCards? ou manda só o novocard?
            await Clients.All.SendAsync("atualizarBoard", card);
        }

        // update (recebe id + card?)
        public async Task AtualizaCard(Card card)
        {
            _context.Update(card);
            await _context.SaveChangesAsync();
            await Clients.All.SendAsync("atualizarBoard", card);
        }

        public async Task ApagarCard(Card card)
        {
            _context.Remove(card);
            await _context.SaveChangesAsync();
            await Clients.All.SendAsync("atualizarBoard", card);
        }
    }
}
