using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace TL.BaseContracts.Tests
{
    public class SeekPaginationTests
    {
        [Fact]
        public void Given_SeekRequest_With_Valid_Parameters_Should_Store_Values()
        {
            var lastSeenId = Guid.NewGuid();
            var request = new SeekRequest<Guid>(lastSeenId, 25);

            request.LastSeenId.Should().Be(lastSeenId);
            request.PageSize.Should().Be(25);
            request.IdRequest.Should().NotBeEmpty();
        }

        [Theory]
        [InlineData(0, 10)]
        [InlineData(-5, 10)]
        [InlineData(101, 100)]
        [InlineData(500, 100)]
        [InlineData(50, 50)]
        public void Given_SeekRequest_Should_Normalize_PageSize(int inputSize, int expectedSize)
        {
            var request = new SeekRequest<long>
            {
                PageSize = inputSize
            };

            request.PageSize.Should().Be(expectedSize);
        }

        [Fact]
        public void Given_NonGeneric_SeekRequest_Should_Work_With_String_Cursor()
        {
            const string token = "cursor_token_xyz_123";
            var request = new SeekRequest(token, 20);

            request.LastSeenId.Should().Be(token);
            request.PageSize.Should().Be(20);
        }

        [Fact]
        public void Given_SeekResult_With_Strongly_Typed_Cursor_Should_Retain_Metadata_And_Immutability()
        {
            var items = new List<string> { "Item 1", "Item 2", "Item 3" };
            var nextCursor = Guid.NewGuid();

            var result = SeekResult<string, Guid>.Create(items, pageSize: 3, hasNextPage: true, nextCursor: nextCursor);

            result.Items.Should().HaveCount(3);
            result.PageSize.Should().Be(3);
            result.HasNextPage.Should().BeTrue();
            result.NextCursor.Should().Be(nextCursor);

            items.Add("Item 4");
            result.Items.Should().HaveCount(3);
        }

        [Fact]
        public void Given_SeekResult_String_Specialization_When_Empty_Should_Have_Safe_Defaults()
        {
            var result = SeekResult<string>.Empty(pageSize: 15);

            result.Items.Should().BeEmpty();
            result.PageSize.Should().Be(15);
            result.HasNextPage.Should().BeFalse();
            result.NextCursor.Should().BeNull();
        }

        [Fact]
        public void Given_Null_Items_In_SeekResult_Should_Initialize_Empty_List()
        {
            var result = new SeekResult<int, string>(null, 10, false, null);

            result.Items.Should().NotBeNull();
            result.Items.Should().BeEmpty();
        }
    }
}
