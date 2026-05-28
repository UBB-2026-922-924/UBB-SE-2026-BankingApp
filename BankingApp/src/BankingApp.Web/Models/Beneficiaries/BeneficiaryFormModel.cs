namespace BankingApp.Web.Models.Beneficiaries;

using System.ComponentModel.DataAnnotations;
using BankingApp.Contracts.Features.Beneficiaries.Dtos;

public sealed class BeneficiaryFormModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Name is required.")]
    [MaxLength(100, ErrorMessage = "Name must be 100 characters or fewer.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "IBAN is required.")]
    [MaxLength(34, ErrorMessage = "IBAN must be 34 characters or fewer.")]
    [Display(Name = "IBAN")]
    public string Iban { get; set; } = string.Empty;

    [MaxLength(100, ErrorMessage = "Bank name must be 100 characters or fewer.")]
    [Display(Name = "Bank name (optional)")]
    public string? BankName { get; set; }

    public CreateBeneficiaryRequest ToCreateRequest()
    {
        return new CreateBeneficiaryRequest
        {
            Name = Name,
            Iban = Iban,
            BankName = BankName,
        };
    }

    public UpdateBeneficiaryRequest ToUpdateRequest()
    {
        return new UpdateBeneficiaryRequest
        {
            Id = Id,
            Name = Name,
            Iban = Iban,
            BankName = BankName,
        };
    }

    public static BeneficiaryFormModel FromBeneficiary(BeneficiaryDto beneficiary)
    {
        return new BeneficiaryFormModel
        {
            Id = beneficiary.Id,
            Name = beneficiary.Name ?? string.Empty,
            Iban = beneficiary.Iban ?? string.Empty,
            BankName = beneficiary.BankName,
        };
    }
}
