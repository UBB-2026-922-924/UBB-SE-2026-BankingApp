namespace BankingApp.Contracts.Features.Investments.Dtos;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Domain.Aggregates.InvestmentAggregate.Entities;

/// <summary>Represents an aggregated view of a user's investment holdings.</summary>
public class PortfolioDto
{
    [Key]
    public int Id { get; set; }

    public int UserId { get; set; }

    /// <summary>Gets or sets the total market value of all holdings.</summary>
    public decimal TotalValue { get; set; }

    /// <summary>Gets or sets the absolute gain or loss amount.</summary>
    public decimal TotalGainLoss { get; set; }

    /// <summary>Gets or sets the gain or loss percentage.</summary>
    public decimal GainLossPercent { get; set; }

    /// <summary>Gets or sets the collection of constituent holdings.</summary>
    public List<InvestmentHolding> Holdings { get; set; } = [];
}
