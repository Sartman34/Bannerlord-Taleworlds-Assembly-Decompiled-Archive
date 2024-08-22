using System;
using System.Threading.Tasks;
using TaleWorlds.PlayerServices;

namespace TaleWorlds.PlatformService;

public interface IPlatformInvitationServices
{
	Task OnLogin();

	Task<bool> OnInviteToPlatformSession(PlayerId playerId);

	Task OnLeftParty();

	PlayerId GetInvitationPlayerId();

	Task<Tuple<bool, ulong>> JoinSession();

	Task LeaveSession(bool createNewSession);
}
