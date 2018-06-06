﻿using Kadena.Models.TableSorting;
using Xunit;

namespace Kadena.Tests.Models
{
    public class TableSortingHelperTests
    {
        [Theory]
        [InlineData("/some/url?orderby=column1&orderbydirection=desc", "column1", "desc")]
        [InlineData("/some/url?orderby=column1&orderbydirection=asc", "column1", "asc")]
        public void ExtractOrderByFromUrl_ShouldExtract_WhenParametersAreSpecified(string url, string column, string direction)
        {
            var orderBy = TableSortingHelper.ExtractOrderByFromUrl(url);
            Assert.Equal(column, orderBy.Column);
            Assert.Equal(direction, orderBy.Direction);
        }

        [Theory]
        [InlineData("/some/url?orderby=column1&orderbydirection=invalid")]
        [InlineData("/some/url?orderby=column1")]
        public void ExtractOrderByFromUrl_ShouldClearDirection_WhenDirectionEmptyOrInvalid(string url)
        {
            var orderBy = TableSortingHelper.ExtractOrderByFromUrl(url);
            Assert.Equal("column1", orderBy.Column);
            Assert.Equal(string.Empty, orderBy.Direction);
        }

        [Theory]
        [InlineData("/some/url?orderby=col1&orderbydirection=desc", "col1 desc")]
        [InlineData("/some/url?orderby=col1", "col1")]
        public void GetOrderBy_ShouldReturnOrderByExpression_WhenColumnIsValid(string url, string expression)
        {
            var validColumns = new[] { "col1", "col2", "col3" };
            var result = TableSortingHelper.GetOrderBy(validColumns, url);
            Assert.Equal(expression, result);
        }

        [Fact]
        public void GetOrderBy_ShouldReturnEmptyString_WhenColumnIsInvalid()
        {
            var validColumns = new[] { "col1", "col2", "col3" };
            const string url = "/some/url?orderby=invalid";
            var result = TableSortingHelper.GetOrderBy(validColumns, url);
            Assert.Equal(string.Empty, result);
        }
    }
}
