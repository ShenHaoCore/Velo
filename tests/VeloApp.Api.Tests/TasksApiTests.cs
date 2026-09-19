using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace VeloApp.Api.Tests;

public sealed class TasksApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _client;

    public TasksApiTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateTask_WithValidTitle_ReturnsSuccess()
    {
        var response = await _client.PostAsJsonAsync("/tasks", new { title = "Write tests" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<ResultDto<Guid>>(JsonOptions);
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);
        Assert.Null(result.Error);
    }

    [Fact]
    public async Task CreateTask_WithEmptyTitle_ReturnsFailure()
    {
        var response = await _client.PostAsJsonAsync("/tasks", new { title = "   " });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<ResultDto<Guid>>(JsonOptions);
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.Equal("Title is required.", result.Error);
    }

    [Fact]
    public async Task GetTasks_AfterCreate_ContainsCreatedTask()
    {
        var title = $"Task-{Guid.NewGuid():N}";
        var createResponse = await _client.PostAsJsonAsync("/tasks", new { title });
        var created = await createResponse.Content.ReadFromJsonAsync<ResultDto<Guid>>(JsonOptions);
        Assert.NotNull(created);
        Assert.True(created.IsSuccess);

        var listResponse = await _client.GetAsync("/tasks");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);

        var list = await listResponse.Content.ReadFromJsonAsync<ResultDto<List<TaskItemDto>>>(JsonOptions);
        Assert.NotNull(list);
        Assert.True(list.IsSuccess);
        Assert.Contains(list.Value!, t => t.Id == created.Value && t.Title == title && !t.IsCompleted);
    }

    private sealed record ResultDto<T>(bool IsSuccess, T? Value, string? Error);

    private sealed record TaskItemDto(Guid Id, string Title, bool IsCompleted);
}
