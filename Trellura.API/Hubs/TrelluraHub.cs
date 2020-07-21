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

        private static int _totalDeClientes;
        private static int _totalDeClientesGrupo;

        public async override Task OnConnectedAsync()
        {
            _totalDeClientes++;
            await Clients.All.SendAsync("atualizarTotalUsuarios", _totalDeClientes);
            await base.OnConnectedAsync();
        }

        public async override Task OnDisconnectedAsync(Exception exception)
        {
            _totalDeClientes--;
            await Clients.All.SendAsync("atualizarTotalUsuarios", _totalDeClientes);
            await base.OnDisconnectedAsync(exception);
        }

        public async Task Entrar(string usuario, string nomeGrupo)
        {
            _totalDeClientesGrupo++;
            await Groups.AddToGroupAsync(Context.ConnectionId, nomeGrupo);
            await Clients.Group(nomeGrupo).SendAsync("entrandoNoGrupo", usuario, _totalDeClientesGrupo);
        }

        public async Task Sair(string usuario, string nomeGrupo)
        {
            _totalDeClientesGrupo++;
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, nomeGrupo);
            await Clients.Group(nomeGrupo).SendAsync("saindoDoGrupo", usuario, _totalDeClientesGrupo);
        }

        public async Task ObterTodosCards(string nomeGrupo)
        {
            try
            {
                var cards = await _context.Cards.ToListAsync();
                await Clients.Group(nomeGrupo).SendAsync("receberTodosCards", cards);
            }
            catch (Exception)
            {
                await Clients.Caller.SendAsync("exibeMensagemErro", "Não foi recuperar os cards, tente novamente!");
            }
        }

        // cria um card - será se vou precisar deserializar????
        public async Task CriarCard(Card card, string nomeGrupo)
        {
            try
            {
                _context.Cards.Add(card);
                await _context.SaveChangesAsync();
                // recuperar cards e envia no sendAsync? ou no cliente chama chama novamente o ObterCards? ou manda só o novocard?
                await Clients.Group(nomeGrupo).SendAsync("atualizarBoard", card);
            }
            catch (Exception)
            { 
                await Clients.Caller.SendAsync("exibeMensagemErro", "Não foi possível criar o Card");
            }
        }

        // update (recebe id + card?)
        public async Task AtualizaCard(Card card, string nomeGrupo)
        {
            try
            {
                _context.Update(card);
                await _context.SaveChangesAsync();
                await Clients.Group(nomeGrupo).SendAsync("atualizarBoard", card);
            }
            catch (DbUpdateConcurrencyException)
            {
                await Clients.Caller.SendAsync("exibeMensagemErro", "Não foi possível atualizar o Card");
            }
        }

        public async Task ApagarCard(Card card, string nomeGrupo)
        {
            try
            {
                _context.Remove(card);
                await _context.SaveChangesAsync();
                await Clients.Group(nomeGrupo).SendAsync("atualizarBoard", card);
            }
            catch (Exception)
            {
                await Clients.Caller.SendAsync("exibeMensagemErro", "Não foi possível apagar o Card");
            }
        }
    }
}
