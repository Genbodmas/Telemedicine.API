using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Telemedicine.API.Models.Requests;
using Telemedicine.API.Repository.Interface;
using Telemedicine.API.Services;

namespace Telemedicine.API.Hubs
{
    [Authorize]
    public class TelemedicineHub : Hub
    {
        private readonly IConsultationRepository _repository;
        private readonly EncryptionService _encryptionService;
        private readonly IUserContextService _userContextService;

        public TelemedicineHub(IConsultationRepository repository, EncryptionService encryptionService, IUserContextService userContextService)
        {
            _repository = repository;
            _encryptionService = encryptionService;
            _userContextService = userContextService;
        }

        public async Task JoinRoom(Guid roomId)
        {
            var userId = _userContextService.GetUserId();
            var request = new JoinRoomRequest { RoomId = roomId, UserId = userId };

            var response = await _repository.JoinRoomAsync(request);

            if (response.Succeeded)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, roomId.ToString());
                await Clients.Caller.SendAsync("JoinSuccess", roomId);
                await Clients.Group(roomId.ToString()).SendAsync("UserJoined", userId);
            }
            else
            {
                throw new HubException(response.Message);
            }
        }

        public async Task SendMessage(Guid roomId, string message, string? fileUrl = null)
        {
            var userId = _userContextService.GetUserId();
            var encryptedMessage = _encryptionService.Encrypt(message);

            var request = new AddChatRequest
            {
                RoomId = roomId,
                SenderId = userId,
                Message = encryptedMessage,
                FileUrl = fileUrl
            };

            var response = await _repository.AddChatAsync(request);

            if (response.Succeeded)
            {
                await Clients.Group(roomId.ToString()).SendAsync("ReceiveMessage", userId, message, fileUrl, DateTime.UtcNow);
            }
            else
            {
                throw new HubException($"Failed to send message: {response.Message}");
            }
        }

        public async Task EndSession(Guid roomId)
        {
            var userId = _userContextService.GetUserId();
            var request = new EndSessionRequest { RoomId = roomId, ActionBy = userId };

            var response = await _repository.EndSessionAsync(request);

            if (response.Succeeded)
            {
                await Clients.Group(roomId.ToString()).SendAsync("SessionEnded");
            }
            else
            {
                throw new HubException($"Failed to end session: {response.Message}");
            }
        }

        public async Task SendSignal(Guid roomId, string type, string payload)
        {
            await Clients.Group(roomId.ToString()).SendAsync("ReceiveSignal", _userContextService.GetUserId(), type, payload);
        }

        public async Task GetChatHistory(Guid roomId)
        {
            var response = await _repository.GetChatHistoryAsync(roomId);
            if (response.Succeeded)
            {
                await Clients.Caller.SendAsync("ReceiveHistory", response.Data);
            }
        }
    }
}
