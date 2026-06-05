namespace BankingApp.Web.Tests.ViewModels.Shared;

using BankingApp.Web.ViewModels.Shared;

public sealed class PaginationBarViewModelTests
{
    [Fact]
    public void HasPreviousPage_WhenOnFirstPage_ShouldReturnFalse()
    {
        PaginationBarViewModel viewModel = new() { CurrentPage = 1, TotalPages = 5 };

        viewModel.HasPreviousPage.Should().BeFalse();
    }

    [Fact]
    public void HasPreviousPage_WhenPastFirstPage_ShouldReturnTrue()
    {
        PaginationBarViewModel viewModel = new() { CurrentPage = 2, TotalPages = 5 };

        viewModel.HasPreviousPage.Should().BeTrue();
    }

    [Fact]
    public void HasNextPage_WhenOnLastPage_ShouldReturnFalse()
    {
        PaginationBarViewModel viewModel = new() { CurrentPage = 5, TotalPages = 5 };

        viewModel.HasNextPage.Should().BeFalse();
    }

    [Fact]
    public void HasNextPage_WhenBeforeLastPage_ShouldReturnTrue()
    {
        PaginationBarViewModel viewModel = new() { CurrentPage = 4, TotalPages = 5 };

        viewModel.HasNextPage.Should().BeTrue();
    }

    [Fact]
    public void VisiblePageNumbers_WhenSinglePage_ShouldReturnThatPage()
    {
        PaginationBarViewModel viewModel = new() { CurrentPage = 1, TotalPages = 1 };

        viewModel.VisiblePageNumbers.Should().Equal([1]);
    }

    [Fact]
    public void VisiblePageNumbers_WhenCurrentPageNearStart_ShouldStartFromPage1()
    {
        PaginationBarViewModel viewModel = new() { CurrentPage = 2, TotalPages = 10 };

        viewModel.VisiblePageNumbers.Should().Equal([1, 2, 3, 4, 5]);
    }

    [Fact]
    public void VisiblePageNumbers_WhenCurrentPageInMiddle_ShouldCenterWindow()
    {
        PaginationBarViewModel viewModel = new() { CurrentPage = 6, TotalPages = 10 };

        viewModel.VisiblePageNumbers.Should().Equal([4, 5, 6, 7, 8]);
    }

    [Fact]
    public void VisiblePageNumbers_WhenCurrentPageNearEnd_ShouldEndAtLastPage()
    {
        PaginationBarViewModel viewModel = new() { CurrentPage = 9, TotalPages = 10 };

        viewModel.VisiblePageNumbers.Should().Equal([6, 7, 8, 9, 10]);
    }

    [Fact]
    public void VisiblePageNumbers_WhenFewerThanWindowSizePages_ShouldReturnAllPages()
    {
        PaginationBarViewModel viewModel = new() { CurrentPage = 2, TotalPages = 3 };

        viewModel.VisiblePageNumbers.Should().Equal([1, 2, 3]);
    }
}
