using FluentAssertions;
using Judgy.Providers.Http;
using Xunit;

namespace Judgy.Providers.Http.Tests;

public class JsonPathExtractorTests
{
    [Fact]
    public void Extract_SimpleProperty()
    {
        var json = """{"response": "hello"}""";

        var result = JsonPathExtractor.Extract(json, "$.response");

        result.Should().Be("hello");
    }

    [Fact]
    public void Extract_NestedProperty()
    {
        var json = """{"data": {"text": "nested"}}""";

        var result = JsonPathExtractor.Extract(json, "$.data.text");

        result.Should().Be("nested");
    }

    [Fact]
    public void Extract_ArrayIndex()
    {
        var json = """{"items": ["first", "second", "third"]}""";

        var result = JsonPathExtractor.Extract(json, "$.items[1]");

        result.Should().Be("second");
    }

    [Fact]
    public void Extract_ArrayIndexWithNestedProperty()
    {
        var json = """{"choices": [{"message": {"content": "result"}}]}""";

        var result = JsonPathExtractor.Extract(json, "$.choices[0].message.content");

        result.Should().Be("result");
    }

    [Fact]
    public void Extract_NumericValue_ReturnsRawText()
    {
        var json = """{"count": 42}""";

        var result = JsonPathExtractor.Extract(json, "$.count");

        result.Should().Be("42");
    }

    [Fact]
    public void Extract_MissingProperty_ThrowsInvalidOperationException()
    {
        var json = """{"data": "value"}""";

        var act = () => JsonPathExtractor.Extract(json, "$.missing");

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*missing*");
    }

    [Fact]
    public void Extract_ArrayIndexOutOfRange_ThrowsInvalidOperationException()
    {
        var json = """{"items": ["only"]}""";

        var act = () => JsonPathExtractor.Extract(json, "$.items[5]");

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*out of range*");
    }

    [Fact]
    public void Extract_PropertyOnArray_ThrowsInvalidOperationException()
    {
        var json = """{"items": [1, 2, 3]}""";

        var act = () => JsonPathExtractor.Extract(json, "$.items.name");

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Expected JSON object*");
    }

    [Fact]
    public void Extract_ArrayIndexOnObject_ThrowsInvalidOperationException()
    {
        var json = """{"data": {"name": "test"}}""";

        var act = () => JsonPathExtractor.Extract(json, "$.data[0]");

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Expected JSON array*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Extract_EmptyJson_ThrowsArgumentException(string? json)
    {
        var act = () => JsonPathExtractor.Extract(json!, "$.path");

        act.Should().Throw<ArgumentException>()
            .Which.ParamName.Should().Be("json");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Extract_EmptyPath_ThrowsArgumentException(string? path)
    {
        var act = () => JsonPathExtractor.Extract("{}", path!);

        act.Should().Throw<ArgumentException>()
            .Which.ParamName.Should().Be("jsonPath");
    }

    [Fact]
    public void Extract_DeeplyNestedPath()
    {
        var json = """{"a": {"b": {"c": {"d": "deep"}}}}""";

        var result = JsonPathExtractor.Extract(json, "$.a.b.c.d");

        result.Should().Be("deep");
    }

    [Fact]
    public void Extract_MultipleArrayIndices()
    {
        var json = """{"matrix": [[1, 2], [3, 4]]}""";

        var result = JsonPathExtractor.Extract(json, "$.matrix[1][0]");

        result.Should().Be("3");
    }
}
