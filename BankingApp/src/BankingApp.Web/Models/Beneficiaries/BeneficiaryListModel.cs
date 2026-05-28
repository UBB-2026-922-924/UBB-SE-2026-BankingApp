namespace BankingApp.Web.Models.Beneficiaries;

using BankingApp.Contracts.Features.Beneficiaries.Dtos;

public sealed class BeneficiaryListModel
{
    public IReadOnlyList<BeneficiaryListItemModel> Beneficiaries { get; init; } = [];

    public bool HasBeneficiaries => Beneficiaries.Count > 0;

    public static BeneficiaryListModel FromBeneficiaries(IEnumerable<BeneficiaryDto> beneficiaries)
    {
        return new BeneficiaryListModel
        {
            Beneficiaries = beneficiaries
                .OrderBy(beneficiary => beneficiary.Name)
                .Select(beneficiary => new BeneficiaryListItemModel
                {
                    Id = beneficiary.Id,
                    Name = beneficiary.Name ?? string.Empty,
                    Iban = beneficiary.Iban ?? string.Empty,
                    BankName = beneficiary.BankName,
                    TransferCount = beneficiary.TransferCount,
                    TotalAmountSent = beneficiary.TotalAmountSent,
                    LastTransferDate = beneficiary.LastTransferDate,
                })
                .ToArray()
        };


    }
}

public sealed class BeneficiaryListItemModel
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Iban { get; init; } = string.Empty;
    public string? BankName { get; init; }
    public int TransferCount { get; init; }
    public decimal TotalAmountSent { get; init; }
    public DateTime? LastTransferDate { get; init; }
}
