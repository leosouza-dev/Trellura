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
    // testar com o front - possível mudar muita coisa
    public class TrelluraHub : Hub
    {
        private TrelluraDbContext _context;

        public TrelluraHub(TrelluraDbContext context)
        {
            _context = context;
        }

        private static int _totalDeClientes;
        private static int _totalDeClientesGrupo;
        private string usuario;
        private static List<string> _usuarios;

        public async override Task OnConnectedAsync()
        {
            _totalDeClientes++;
            await Clients.All.SendAsync("atualizarTotalUsuarios", _totalDeClientes); // atualiza para todos
            await Clients.AllExcept(Context.ConnectionId).SendAsync("usuarioEntrando"); // na home sobe um toast informando novo user para outros usuários
            await base.OnConnectedAsync();
        }

        public async override Task OnDisconnectedAsync(Exception exception)
        {
            // se usuario do grupo simplesmente sair
            if(_usuarios.Contains(this.usuario))
            {
                await this.Sair(this.usuario, "nomeGrupo");
                await base.OnDisconnectedAsync(exception);
                return;
            }

            _totalDeClientes--;
            await Clients.All.SendAsync("atualizarTotalUsuarios", _totalDeClientes); // atualiza na home
            await base.OnDisconnectedAsync(exception);
        }

        public async Task Entrar(string usuario, string nomeGrupo)
        {
            _totalDeClientesGrupo++;
            this.usuario = usuario;
            _usuarios.Add(usuario);

            await Groups.AddToGroupAsync(Context.ConnectionId, nomeGrupo);
            await Clients.Group(nomeGrupo).SendAsync("entrandoNoGrupo", usuario, _totalDeClientesGrupo); // manda para outros usuários - novo usuário

            var cards = await _context.Cards.ToListAsync(); // recupera cards (cliente poderia chamar o obter cards - enviaria só pra quem chamou)
            await Clients.Caller.SendAsync("entrouNoGrupo", cards, _usuarios, _totalDeClientesGrupo); // manda para novo usuario - cards e  lista com todos usuarios

        }

        public async Task Sair(string usuario, string nomeGrupo)
        {
            _totalDeClientesGrupo--;
            _usuarios.Remove(usuario);

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, nomeGrupo);
            await Clients.Group(nomeGrupo).SendAsync("saindoDoGrupo", usuario, _totalDeClientesGrupo);
        }

        public async Task ObterTodosCards(string nomeGrupo)
        {
            try
            {
                var cards = await _context.Cards.ToListAsync();
                await Clients.Group(nomeGrupo).SendAsync("receberTodosCards", cards); // mudar só pro caller?
            }
            catch (Exception)
            {
                await Clients.Caller.SendAsync("exibeMensagemErro", "Não foi recuperar os cards, tente novamente!");
            }
        }

        public async Task CriarCard(Card card, string nomeGrupo)
        {
            try
            {
                _context.Cards.Add(card);
                await _context.SaveChangesAsync();
                await Clients.Caller.SendAsync("cardCriado", "Card criado com sucesso!"); // no formulário envia mensagem com sucesso
                await Clients.GroupExcept(nomeGrupo, Context.ConnectionId).SendAsync("atualizarBoard", card); // para o resto atualiza o board
            }
            catch (Exception)
            { 
                await Clients.Caller.SendAsync("exibeMensagemErro", "Não foi possível criar o Card");
            }
        }

        public async Task AtualizaCard(int cardId, Card card, string nomeGrupo)
        {
            if(cardId != card.Id)
            {
                await Clients.Caller.SendAsync("exibeMensagemErro", "Não foi possível atualizar o Card. Card e Id não coincidem");
                return;
            }

            try
            {
                _context.Update(card);
                await _context.SaveChangesAsync();
                await Clients.Caller.SendAsync("cardAtualizado", "Card atualizado com sucesso!"); // no formulário envia mensagem com sucesso
                await Clients.GroupExcept(nomeGrupo, Context.ConnectionId).SendAsync("atualizarBoard", card); // para o resto atualiza o board
            }
            catch (DbUpdateConcurrencyException)
            {
                await Clients.Caller.SendAsync("exibeMensagemErro", "Não foi possível atualizar o Card. Tente novamente mais tarde");
            }
        }

        public async Task ApagarCard(int cardId, Card card, string nomeGrupo)
        {
            if (cardId != card.Id)
            {
                await Clients.Caller.SendAsync("exibeMensagemErro", "Não foi possível deletar o Card. Card e Id fornecido não coincidem");
                return;
            }

            try
            {
                _context.Remove(card);
                await _context.SaveChangesAsync();
                await Clients.Group(nomeGrupo).SendAsync("atualizarBoard", card);
            }
            catch (Exception)
            {
                await Clients.Caller.SendAsync("exibeMensagemErro", "Não foi possível apagar o Card. Tente novamente mais tarde");
            }
        }
    }
}
