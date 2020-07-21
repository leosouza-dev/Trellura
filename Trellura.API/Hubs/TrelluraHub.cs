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

        public async Task Entrar(string usuario)
        {
            await Clients.All.SendAsync("atualizarListaUsuario", usuario);
        }

        public async Task ObterTodosCards()
        {
            try
            {
                var cards = await _context.Cards.ToListAsync();
                await Clients.All.SendAsync("receberTodosCards", cards);
            }
            catch (Exception)
            {
                await Clients.Caller.SendAsync("exibeMensagemErro", "Não foi recuperar os cards, tente novamente!");
            }
        }

        // cria um card - será se vou precisar deserializar????
        public async Task CriarCard(Card card)
        {
            try
            {
                _context.Cards.Add(card);
                await _context.SaveChangesAsync();
                // recuperar cards e envia no sendAsync? ou no cliente chama chama novamente o ObterCards? ou manda só o novocard?
                await Clients.All.SendAsync("atualizarBoard", card);
            }
            catch (Exception)
            { 
                await Clients.Caller.SendAsync("exibeMensagemErro", "Não foi possível criar o Card");
            }
        }

        // update (recebe id + card?)
        public async Task AtualizaCard(Card card)
        {
            try
            {
                _context.Update(card);
                await _context.SaveChangesAsync();
                await Clients.All.SendAsync("atualizarBoard", card);
            }
            catch (DbUpdateConcurrencyException)
            {
                await Clients.Caller.SendAsync("exibeMensagemErro", "Não foi possível atualizar o Card");
            }
        }

        public async Task ApagarCard(Card card)
        {
            try
            {
                _context.Remove(card);
                await _context.SaveChangesAsync();
                await Clients.All.SendAsync("atualizarBoard", card);
            }
            catch (Exception)
            {
                await Clients.Caller.SendAsync("exibeMensagemErro", "Não foi possível apagar o Card");
            }
        }
    }
}
