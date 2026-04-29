using FluentAssertions;
using Vela.Application.Services;
using Vela.Domain.Entities.Group;
using Vela.Domain.Enums;

namespace Vela.UnitTests.Services;

public class GroupAuthorizationServiceTests
{
    private readonly GroupAuthorizationService _sut = new();

    // ───────────────────── helpers ─────────────────────

    private static Group CreateGroup(params (string UserId, GroupRole Role)[] members)
    {
        var groupId = Guid.NewGuid();
        var group = new Group
        {
            Id = groupId,
            Name = "Test Group",
            Members = members.Select(m => new GroupMember
            {
                GroupId = groupId,
                UserId = m.UserId,
                Role = m.Role
            }).ToList()
        };
        return group;
    }

    // ───────────────────── AuthorizeMembership ─────────────────────

    [Fact]
    public void AuthorizeMembership_WhenCallerIsMember_ReturnsOk()
    {
        var group = CreateGroup(("user-1", GroupRole.Member));

        var result = _sut.AuthorizeMembership(group, "user-1");

        result.Success.Should().BeTrue();
    }

    [Fact]
    public void AuthorizeMembership_WhenCallerIsNotMember_ReturnsFail()
    {
        var group = CreateGroup(("user-1", GroupRole.Owner));

        var result = _sut.AuthorizeMembership(group, "stranger");

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("not a member");
    }

    // ───────────────────── AuthorizeDeleteGroup ─────────────────────

    [Fact]
    public void AuthorizeDeleteGroup_WhenOwnerAndOnlyMember_ReturnsOk()
    {
        var group = CreateGroup(("owner-1", GroupRole.Owner));

        var result = _sut.AuthorizeDeleteGroup(group, "owner-1");

        result.Success.Should().BeTrue();
    }

    [Fact]
    public void AuthorizeDeleteGroup_WhenOwnerButOtherMembersExist_ReturnsFail()
    {
        var group = CreateGroup(
            ("owner-1", GroupRole.Owner),
            ("user-2", GroupRole.Member));

        var result = _sut.AuthorizeDeleteGroup(group, "owner-1");

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("only member left");
    }

    [Fact]
    public void AuthorizeDeleteGroup_WhenCallerIsAdministrator_ReturnsFail()
    {
        var group = CreateGroup(
            ("owner-1", GroupRole.Owner),
            ("admin-1", GroupRole.Administrator));

        var result = _sut.AuthorizeDeleteGroup(group, "admin-1");

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Only the owner");
    }

    [Fact]
    public void AuthorizeDeleteGroup_WhenCallerIsMember_ReturnsFail()
    {
        var group = CreateGroup(
            ("owner-1", GroupRole.Owner),
            ("user-1", GroupRole.Member));

        var result = _sut.AuthorizeDeleteGroup(group, "user-1");

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Only the owner");
    }

    [Fact]
    public void AuthorizeDeleteGroup_WhenCallerIsNotInGroup_ReturnsFail()
    {
        var group = CreateGroup(("owner-1", GroupRole.Owner));

        var result = _sut.AuthorizeDeleteGroup(group, "stranger");

        result.Success.Should().BeFalse();
    }

    // ───────────────────── AuthorizeRemoveMember ─────────────────────

    [Fact]
    public void AuthorizeRemoveMember_WhenOwnerRemovesMember_ReturnsOk()
    {
        var group = CreateGroup(
            ("owner-1", GroupRole.Owner),
            ("user-2", GroupRole.Member));

        var result = _sut.AuthorizeRemoveMember(group, "owner-1", "user-2");

        result.Success.Should().BeTrue();
    }

    [Fact]
    public void AuthorizeRemoveMember_WhenOwnerRemovesAdministrator_ReturnsOk()
    {
        var group = CreateGroup(
            ("owner-1", GroupRole.Owner),
            ("admin-1", GroupRole.Administrator));

        var result = _sut.AuthorizeRemoveMember(group, "owner-1", "admin-1");

        result.Success.Should().BeTrue();
    }

    [Fact]
    public void AuthorizeRemoveMember_WhenOwnerRemovesSelf_ReturnsFail()
    {
        var group = CreateGroup(("owner-1", GroupRole.Owner));

        var result = _sut.AuthorizeRemoveMember(group, "owner-1", "owner-1");

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Owner cannot remove themselves");
    }

    [Fact]
    public void AuthorizeRemoveMember_WhenAdminRemovesMember_ReturnsOk()
    {
        var group = CreateGroup(
            ("owner-1", GroupRole.Owner),
            ("admin-1", GroupRole.Administrator),
            ("user-2", GroupRole.Member));

        var result = _sut.AuthorizeRemoveMember(group, "admin-1", "user-2");

        result.Success.Should().BeTrue();
    }

    [Fact]
    public void AuthorizeRemoveMember_WhenAdminRemovesAdmin_ReturnsFail()
    {
        var group = CreateGroup(
            ("owner-1", GroupRole.Owner),
            ("admin-1", GroupRole.Administrator),
            ("admin-2", GroupRole.Administrator));

        var result = _sut.AuthorizeRemoveMember(group, "admin-1", "admin-2");

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Administrators can only remove members");
    }

    [Fact]
    public void AuthorizeRemoveMember_WhenAdminRemovesOwner_ReturnsFail()
    {
        var group = CreateGroup(
            ("owner-1", GroupRole.Owner),
            ("admin-1", GroupRole.Administrator));

        var result = _sut.AuthorizeRemoveMember(group, "admin-1", "owner-1");

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Administrators can only remove members");
    }

    [Fact]
    public void AuthorizeRemoveMember_WhenMemberRemovesAnyone_ReturnsFail()
    {
        var group = CreateGroup(
            ("owner-1", GroupRole.Owner),
            ("user-1", GroupRole.Member),
            ("user-2", GroupRole.Member));

        var result = _sut.AuthorizeRemoveMember(group, "user-1", "user-2");

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Members cannot remove");
    }

    [Fact]
    public void AuthorizeRemoveMember_WhenTargetNotInGroup_ReturnsFail()
    {
        var group = CreateGroup(
            ("owner-1", GroupRole.Owner),
            ("user-1", GroupRole.Member));

        var result = _sut.AuthorizeRemoveMember(group, "owner-1", "non-existent");

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("No user with ID");
    }

    [Fact]
    public void AuthorizeRemoveMember_WhenCallerNotInGroup_ReturnsFail()
    {
        var group = CreateGroup(
            ("owner-1", GroupRole.Owner),
            ("user-1", GroupRole.Member));

        var result = _sut.AuthorizeRemoveMember(group, "stranger", "user-1");

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("not a member");
    }

    // ───────────────────── AuthorizeChangeMemberRole ─────────────────────

    [Fact]
    public void AuthorizeChangeMemberRole_WhenOwnerPromotesMemberToAdmin_ReturnsOk()
    {
        var group = CreateGroup(
            ("owner-1", GroupRole.Owner),
            ("user-1", GroupRole.Member));

        var result = _sut.AuthorizeChangeMemberRole(group, "owner-1", "user-1", GroupRole.Administrator);

        result.Success.Should().BeTrue();
    }

    [Fact]
    public void AuthorizeChangeMemberRole_WhenOwnerDemotesAdminToMember_ReturnsOk()
    {
        var group = CreateGroup(
            ("owner-1", GroupRole.Owner),
            ("admin-1", GroupRole.Administrator));

        var result = _sut.AuthorizeChangeMemberRole(group, "owner-1", "admin-1", GroupRole.Member);

        result.Success.Should().BeTrue();
    }

    [Fact]
    public void AuthorizeChangeMemberRole_WhenNewRoleIsOwner_ReturnsFail()
    {
        var group = CreateGroup(
            ("owner-1", GroupRole.Owner),
            ("user-1", GroupRole.Member));

        var result = _sut.AuthorizeChangeMemberRole(group, "owner-1", "user-1", GroupRole.Owner);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("transfer-ownership");
    }

    [Fact]
    public void AuthorizeChangeMemberRole_WhenAdminPromotesMemberToAdmin_ReturnsOk()
    {
        var group = CreateGroup(
            ("owner-1", GroupRole.Owner),
            ("admin-1", GroupRole.Administrator),
            ("user-1", GroupRole.Member));

        var result = _sut.AuthorizeChangeMemberRole(group, "admin-1", "user-1", GroupRole.Administrator);

        result.Success.Should().BeTrue();
    }

    [Fact]
    public void AuthorizeChangeMemberRole_WhenAdminDemotesAdmin_ReturnsFail()
    {
        var group = CreateGroup(
            ("owner-1", GroupRole.Owner),
            ("admin-1", GroupRole.Administrator),
            ("admin-2", GroupRole.Administrator));

        var result = _sut.AuthorizeChangeMemberRole(group, "admin-1", "admin-2", GroupRole.Member);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Administrators can only promote members");
    }

    [Fact]
    public void AuthorizeChangeMemberRole_WhenMemberChangesRole_ReturnsFail()
    {
        var group = CreateGroup(
            ("owner-1", GroupRole.Owner),
            ("user-1", GroupRole.Member),
            ("user-2", GroupRole.Member));

        var result = _sut.AuthorizeChangeMemberRole(group, "user-1", "user-2", GroupRole.Administrator);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Members cannot change roles");
    }

    [Fact]
    public void AuthorizeChangeMemberRole_WhenCallerNotInGroup_ReturnsFail()
    {
        var group = CreateGroup(
            ("owner-1", GroupRole.Owner),
            ("user-1", GroupRole.Member));

        var result = _sut.AuthorizeChangeMemberRole(group, "stranger", "user-1", GroupRole.Administrator);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("not a member");
    }

    [Fact]
    public void AuthorizeChangeMemberRole_WhenTargetNotInGroup_ReturnsFail()
    {
        var group = CreateGroup(("owner-1", GroupRole.Owner));

        var result = _sut.AuthorizeChangeMemberRole(group, "owner-1", "ghost", GroupRole.Administrator);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("not a member");
    }

    // ───────────────────── AuthorizeLeaveGroup ─────────────────────

    [Fact]
    public void AuthorizeLeaveGroup_WhenCallerIsMember_ReturnsOk()
    {
        var group = CreateGroup(
            ("owner-1", GroupRole.Owner),
            ("user-1", GroupRole.Member));

        var result = _sut.AuthorizeLeaveGroup(group, "user-1");

        result.Success.Should().BeTrue();
    }

    [Fact]
    public void AuthorizeLeaveGroup_WhenCallerIsAdministrator_ReturnsOk()
    {
        var group = CreateGroup(
            ("owner-1", GroupRole.Owner),
            ("admin-1", GroupRole.Administrator));

        var result = _sut.AuthorizeLeaveGroup(group, "admin-1");

        result.Success.Should().BeTrue();
    }

    [Fact]
    public void AuthorizeLeaveGroup_WhenCallerIsOwner_ReturnsFail()
    {
        var group = CreateGroup(
            ("owner-1", GroupRole.Owner),
            ("user-1", GroupRole.Member));

        var result = _sut.AuthorizeLeaveGroup(group, "owner-1");

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Owner must transfer ownership");
    }

    [Fact]
    public void AuthorizeLeaveGroup_WhenCallerNotInGroup_ReturnsFail()
    {
        var group = CreateGroup(("owner-1", GroupRole.Owner));

        var result = _sut.AuthorizeLeaveGroup(group, "stranger");

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("not a member");
    }

    // ───────────────────── AuthorizeTransferOwnership ─────────────────────

    [Fact]
    public void AuthorizeTransferOwnership_WhenOwnerTransfersToMember_ReturnsOk()
    {
        var group = CreateGroup(
            ("owner-1", GroupRole.Owner),
            ("user-1", GroupRole.Member));

        var result = _sut.AuthorizeTransferOwnership(group, "owner-1", "user-1");

        result.Success.Should().BeTrue();
    }

    [Fact]
    public void AuthorizeTransferOwnership_WhenOwnerTransfersToAdmin_ReturnsOk()
    {
        var group = CreateGroup(
            ("owner-1", GroupRole.Owner),
            ("admin-1", GroupRole.Administrator));

        var result = _sut.AuthorizeTransferOwnership(group, "owner-1", "admin-1");

        result.Success.Should().BeTrue();
    }

    [Fact]
    public void AuthorizeTransferOwnership_WhenCallerIsNotOwner_ReturnsFail()
    {
        var group = CreateGroup(
            ("owner-1", GroupRole.Owner),
            ("admin-1", GroupRole.Administrator),
            ("user-1", GroupRole.Member));

        var result = _sut.AuthorizeTransferOwnership(group, "admin-1", "user-1");

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Only the owner");
    }

    [Fact]
    public void AuthorizeTransferOwnership_WhenOwnerTransfersToSelf_ReturnsFail()
    {
        var group = CreateGroup(("owner-1", GroupRole.Owner));

        var result = _sut.AuthorizeTransferOwnership(group, "owner-1", "owner-1");

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("already the owner");
    }

    [Fact]
    public void AuthorizeTransferOwnership_WhenTargetNotInGroup_ReturnsFail()
    {
        var group = CreateGroup(("owner-1", GroupRole.Owner));

        var result = _sut.AuthorizeTransferOwnership(group, "owner-1", "non-existent");

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("not a member");
    }

    [Fact]
    public void AuthorizeTransferOwnership_WhenCallerNotInGroup_ReturnsFail()
    {
        var group = CreateGroup(("owner-1", GroupRole.Owner));

        var result = _sut.AuthorizeTransferOwnership(group, "stranger", "owner-1");

        result.Success.Should().BeFalse();
    }

    // ───────────────────── AuthorizeSendInvite ─────────────────────

    [Fact]
    public void AuthorizeSendInvite_WhenCallerIsOwner_ReturnsOk()
    {
        var group = CreateGroup(("owner-1", GroupRole.Owner));

        var result = _sut.AuthorizeSendInvite(group, "owner-1");

        result.Success.Should().BeTrue();
    }

    [Fact]
    public void AuthorizeSendInvite_WhenCallerIsAdministrator_ReturnsOk()
    {
        var group = CreateGroup(
            ("owner-1", GroupRole.Owner),
            ("admin-1", GroupRole.Administrator));

        var result = _sut.AuthorizeSendInvite(group, "admin-1");

        result.Success.Should().BeTrue();
    }

    [Fact]
    public void AuthorizeSendInvite_WhenCallerIsMember_ReturnsFail()
    {
        var group = CreateGroup(
            ("owner-1", GroupRole.Owner),
            ("user-1", GroupRole.Member));

        var result = _sut.AuthorizeSendInvite(group, "user-1");

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Only owners and administrators");
    }

    [Fact]
    public void AuthorizeSendInvite_WhenCallerNotInGroup_ReturnsFail()
    {
        var group = CreateGroup(("owner-1", GroupRole.Owner));

        var result = _sut.AuthorizeSendInvite(group, "stranger");

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("not a member");
    }

    // ───────────────────── AuthorizeViewInvites ─────────────────────

    [Fact]
    public void AuthorizeViewInvites_WhenCallerIsOwner_ReturnsOk()
    {
        var group = CreateGroup(("owner-1", GroupRole.Owner));

        var result = _sut.AuthorizeViewInvites(group, "owner-1");

        result.Success.Should().BeTrue();
    }

    [Fact]
    public void AuthorizeViewInvites_WhenCallerIsAdministrator_ReturnsOk()
    {
        var group = CreateGroup(
            ("owner-1", GroupRole.Owner),
            ("admin-1", GroupRole.Administrator));

        var result = _sut.AuthorizeViewInvites(group, "admin-1");

        result.Success.Should().BeTrue();
    }

    [Fact]
    public void AuthorizeViewInvites_WhenCallerIsMember_ReturnsFail()
    {
        var group = CreateGroup(
            ("owner-1", GroupRole.Owner),
            ("user-1", GroupRole.Member));

        var result = _sut.AuthorizeViewInvites(group, "user-1");

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Only owners and administrators");
    }

    [Fact]
    public void AuthorizeViewInvites_WhenCallerNotInGroup_ReturnsFail()
    {
        var group = CreateGroup(("owner-1", GroupRole.Owner));

        var result = _sut.AuthorizeViewInvites(group, "stranger");

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("not a member");
    }
}
