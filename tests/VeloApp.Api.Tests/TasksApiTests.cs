using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace VeloApp.Api.Tests;

public sealed class TasksApiTests : IClassFixture<VeloWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _client;

    public TasksApiTests(VeloWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateTask_WithValidTitle_ReturnsCreated()
    {
        var response = await _client.PostAsJsonAsync("/tasks", new { title = "Write tests" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<ResultDto<Guid>>(JsonOptions);
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);
        Assert.Null(result.Error);
    }

    [Fact]
    public async Task CreateTask_WithEmptyTitle_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/tasks", new { title = "   " });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<ResultDto<Guid>>(JsonOptions);
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.Equal("Title is required.", result.Error);
    }

    [Fact]
    public async Task CreateTask_WithTooLongTitle_ReturnsBadRequest()
    {
        var title = new string('x', 201);
        var response = await _client.PostAsJsonAsync("/tasks", new { title });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<ResultDto<Guid>>(JsonOptions);
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.Contains("at most 200", result.Error);
    }

    [Fact]
    public async Task CreateTask_WithInvalidJson_ReturnsBadRequest()
    {
        using var content = new StringContent("{ not-json", Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/tasks", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetTasks_AfterCreate_ContainsCreatedTask()
    {
        var title = $"Task-{Guid.NewGuid():N}";
        var createResponse = await _client.PostAsJsonAsync("/tasks", new { title });
        var created = await createResponse.Content.ReadFromJsonAsync<ResultDto<Guid>>(JsonOptions);
        Assert.NotNull(created);
        Assert.True(created.IsSuccess);

        var listResponse = await _client.GetAsync("/tasks?page=1&pageSize=50");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);

        var list = await listResponse.Content.ReadFromJsonAsync<ResultDto<PagedDto<TaskItemDto>>>(JsonOptions);
        Assert.NotNull(list);
        Assert.True(list.IsSuccess);
        Assert.NotNull(list.Value);
        Assert.Contains(list.Value.Items, t => t.Id == created.Value && t.Title == title && !t.IsCompleted);
        Assert.True(list.Value.TotalCount >= 1);
    }

    [Fact]
    public async Task CompleteTask_MarksTaskCompleted()
    {
        var createResponse = await _client.PostAsJsonAsync("/tasks", new { title = $"Complete-{Guid.NewGuid():N}" });
        var created = await createResponse.Content.ReadFromJsonAsync<ResultDto<Guid>>(JsonOptions);
        Assert.NotNull(created);

        var completeResponse = await _client.PostAsync($"/tasks/{created.Value}/complete", null);
        Assert.Equal(HttpStatusCode.OK, completeResponse.StatusCode);

        var completed = await completeResponse.Content.ReadFromJsonAsync<ResultDto<TaskItemDto>>(JsonOptions);
        Assert.NotNull(completed);
        Assert.True(completed.IsSuccess);
        Assert.True(completed.Value!.IsCompleted);
    }

    [Fact]
    public async Task CompleteTask_WhenMissing_ReturnsNotFound()
    {
        var response = await _client.PostAsync($"/tasks/{Guid.NewGuid()}/complete", null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<ResultDto<TaskItemDto>>(JsonOptions);
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.Equal("Task not found.", result.Error);
    }

    [Fact]
    public async Task DeleteTask_RemovesTask()
    {
        var createResponse = await _client.PostAsJsonAsync("/tasks", new { title = $"Delete-{Guid.NewGuid():N}" });
        var created = await createResponse.Content.ReadFromJsonAsync<ResultDto<Guid>>(JsonOptions);
        Assert.NotNull(created);

        var deleteResponse = await _client.DeleteAsync($"/tasks/{created.Value}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var completeResponse = await _client.PostAsync($"/tasks/{created.Value}/complete", null);
        Assert.Equal(HttpStatusCode.NotFound, completeResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteTask_WhenMissing_ReturnsNotFound()
    {
        var response = await _client.DeleteAsync($"/tasks/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private sealed record ResultDto<T>(bool IsSuccess, T? Value, string? Error);

    private sealed record PagedDto<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount);

    private sealed record TaskItemDto(Guid Id, string Title, bool IsCompleted);
}
