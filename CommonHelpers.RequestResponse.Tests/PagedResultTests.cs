using System;
using System.Linq;
using FluentAssertions;

namespace CommonHelpers.RequestResponse.Tests
{
    public class PagedResultTests
    {
        [Fact]
        public void Given_Valid_PagedResult_Create_Should_Calculate_TotalPages_And_Navigation()
        {
            // Arrange
            var items = Enumerable.Range(1, 10).Select(i => $"Item {i}").ToList();
            const long totalCount = 45;
            const int pageNumber = 2;
            const int pageSize = 10;

            // Act
            var pagedResult = PagedResult<string>.Create(items, totalCount, pageNumber, pageSize);

            // Assert
            pagedResult.Items.Should().HaveCount(10);
            pagedResult.TotalCount.Should().Be(45);
            pagedResult.PageNumber.Should().Be(2);
            pagedResult.PageSize.Should().Be(10);
            pagedResult.TotalPages.Should().Be(5); // Ceil(45 / 10) = 5
            pagedResult.HasPreviousPage.Should().BeTrue(); // 2 > 1
            pagedResult.HasNextPage.Should().BeTrue(); // 2 < 5
        }

        [Fact]
        public void Given_First_Page_Should_Have_HasPreviousPage_False()
        {
            // Arrange
            var items = new[] { "A", "B" };

            // Act
            var pagedResult = PagedResult<string>.Create(items, 20, 1, 10);

            // Assert
            pagedResult.HasPreviousPage.Should().BeFalse();
            pagedResult.HasNextPage.Should().BeTrue();
        }

        [Fact]
        public void Given_Last_Page_Should_Have_HasNextPage_False()
        {
            // Arrange
            var items = new[] { "A" };

            // Act
            var pagedResult = PagedResult<string>.Create(items, 10, 1, 10);

            // Assert
            pagedResult.HasPreviousPage.Should().BeFalse();
            pagedResult.HasNextPage.Should().BeFalse(); // 1 == 1
        }

        [Fact]
        public void Given_Negative_Or_Zero_Page_Numbers_Should_Normalize_To_One()
        {
            // Act
            var resultZero = new PagedResult<string>(null, 0, pageNumber: 0, pageSize: 0);

            // Assert
            resultZero.PageNumber.Should().Be(1);
            resultZero.PageSize.Should().Be(10);
            resultZero.TotalCount.Should().Be(0);
            resultZero.TotalPages.Should().Be(0);
            resultZero.Items.Should().BeEmpty();
        }

        [Fact]
        public void Given_PagedResult_Empty_Should_Return_Empty_Result()
        {
            // Act
            var empty = PagedResult<int>.Empty(pageNumber: 3, pageSize: 25);

            // Assert
            empty.Items.Should().BeEmpty();
            empty.TotalCount.Should().Be(0);
            empty.PageNumber.Should().Be(3);
            empty.PageSize.Should().Be(25);
            empty.TotalPages.Should().Be(0);
        }

        [Fact]
        public void Given_PagedRequest_Default_Constructor_Should_Have_Page1_And_Size10()
        {
            // Act
            var request = new PagedRequest();

            // Assert
            request.PageNumber.Should().Be(1);
            request.PageSize.Should().Be(10);
            request.IdRequest.Should().NotBeEmpty();
        }

        [Fact]
        public void Given_PagedRequest_With_Invalid_Values_Should_Normalize()
        {
            // Act
            var request = new PagedRequest(pageNumber: -5, pageSize: 500);

            // Assert
            request.PageNumber.Should().Be(1);
            request.PageSize.Should().Be(100); // Max 100
        }
    }
}

