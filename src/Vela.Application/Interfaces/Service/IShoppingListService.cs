using Vela.Application.Common;
using Vela.Application.DTOs;

namespace Vela.Application.Interfaces.Service;

public interface IShoppingListService
{
   Task<IEnumerable<ShoppingListSummaryDto>> GetAllShoppingListsAsync();
   Task<ShoppingListDto?> GetShoppingListById(Guid id);
   Task<ShoppingListDto> CreateShoppingListAsync(Guid userId, CreateShoppingListDto dto);
   Task<Result> MarkItemAsBoughtAsync(Guid itemId);
}