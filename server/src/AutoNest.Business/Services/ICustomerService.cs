using AutoNest.Business.Contracts;

namespace AutoNest.Business.Services;

public interface ICustomerService
{
    Task<CustomerProfileDto?> ProfileAsync(CancellationToken ct);
    Task<OperationResult> ChangeUsernameAsync(ChangeUsernameRequest request);
    Task<OperationResult> ChangeEmailAsync(ChangeEmailRequest request);
    Task<OperationResult> ChangePasswordAsync(ChangePasswordRequest request);
    Task<OperationResult> DeleteAccountAsync(DeleteAccountRequest request, CancellationToken ct);
    Task<IReadOnlyList<CarSummaryDto>> FavoritesAsync(CancellationToken ct);
    Task<OperationResult> AddFavoriteAsync(int carId, CancellationToken ct);
    Task<OperationResult> RemoveFavoriteAsync(int carId, CancellationToken ct);
    Task<IReadOnlyList<RequestDto>> RequestsAsync(CancellationToken ct);
    Task<OperationResult> CreateRequestAsync(CreateRequestDto request, CancellationToken ct);
    Task<OperationResult> CancelRequestAsync(int id, CancellationToken ct);
    Task<IReadOnlyList<TransactionDto>> TransactionsAsync(CancellationToken ct);
    Task<OperationResult> RateAsync(int transactionId, decimal value, CancellationToken ct);
}
