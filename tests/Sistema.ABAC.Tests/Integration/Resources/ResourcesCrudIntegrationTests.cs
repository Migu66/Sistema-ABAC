using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Sistema.ABAC.Application.DTOs;
using Sistema.ABAC.Application.DTOs.Auth;
using Sistema.ABAC.Application.DTOs.Common;

namespace Sistema.ABAC.Tests.Integration.Resources;

public class ResourcesCrudIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ResourcesCrudIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("https://localhost")
        });
    }

    [Fact]
    public async Task GetAll_WithoutToken_ShouldReturnUnauthorized()
    {
        _client.DefaultRequestHeaders.Authorization = null;

        var response = await _client.GetAsync("/api/resources");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CrudFlow_WithAuthenticatedUser_ShouldCreateReadUpdateDeleteResource()
    {
        await AuthenticateAsync();

        var suffix = Guid.NewGuid().ToString("N")[..8];
        var createRequest = new CreateResourceDto
        {
            Name = $"Documento-{suffix}",
            Type = "Document",
            Description = "Recurso para prueba de integración"
        };

        var createResponse = await _client.PostAsJsonAsync("/api/resources", createRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await createResponse.Content.ReadFromJsonAsync<ResourceDto>();
        created.Should().NotBeNull();
        created!.Id.Should().NotBe(Guid.Empty);
        created.Name.Should().Be(createRequest.Name);
        created.Type.Should().Be(createRequest.Type);

        var getByIdResponse = await _client.GetAsync($"/api/resources/{created.Id}");
        getByIdResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var byId = await getByIdResponse.Content.ReadFromJsonAsync<ResourceDto>();
        byId.Should().NotBeNull();
        byId!.Id.Should().Be(created.Id);
        byId.Name.Should().Be(createRequest.Name);

        var getAllResponse = await _client.GetAsync($"/api/resources?page=1&pageSize=10&searchTerm={createRequest.Name}");
        getAllResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var paged = await getAllResponse.Content.ReadFromJsonAsync<PagedResultDto<ResourceDto>>();
        paged.Should().NotBeNull();
        paged!.Items.Should().Contain(r => r.Id == created.Id);

        var updateRequest = new UpdateResourceDto
        {
            Name = $"Documento-Actualizado-{suffix}",
            Type = "ApiEndpoint",
            Description = "Descripción actualizada"
        };

        var updateResponse = await _client.PutAsJsonAsync($"/api/resources/{created.Id}", updateRequest);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var updated = await updateResponse.Content.ReadFromJsonAsync<ResourceDto>();
        updated.Should().NotBeNull();
        updated!.Id.Should().Be(created.Id);
        updated.Name.Should().Be(updateRequest.Name);
        updated.Type.Should().Be(updateRequest.Type);
        updated.Description.Should().Be(updateRequest.Description);

        var deleteResponse = await _client.DeleteAsync($"/api/resources/{created.Id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getAfterDeleteResponse = await _client.GetAsync($"/api/resources/{created.Id}");
        getAfterDeleteResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private async Task AuthenticateAsync()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var registerRequest = new RegisterDto
        {
            UserName = $"resource_user_{suffix}",
            Email = $"resource_{suffix}@abac.test",
            Password = "PasswordValida!1",
            ConfirmPassword = "PasswordValida!1",
            FullName = "Usuario Recursos",
            Department = "QA"
        };

        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var token = await registerResponse.Content.ReadFromJsonAsync<TokenDto>();
        token.Should().NotBeNull();
        token!.AccessToken.Should().NotBeNullOrWhiteSpace();

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
    }
}
