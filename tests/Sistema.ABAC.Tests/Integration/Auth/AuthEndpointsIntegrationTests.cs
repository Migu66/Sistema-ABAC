using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Sistema.ABAC.Application.DTOs.Auth;

namespace Sistema.ABAC.Tests.Integration.Auth;

public class AuthEndpointsIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthEndpointsIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("https://localhost")
        });
    }

    [Fact]
    public async Task Register_ShouldReturnCreated_AndToken()
    {
        var registerRequest = BuildRegisterRequest();

        var response = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var payload = await response.Content.ReadFromJsonAsync<TokenDto>();
        payload.Should().NotBeNull();
        payload!.AccessToken.Should().NotBeNullOrWhiteSpace();
        payload.TokenType.Should().Be("Bearer");
        payload.User.Should().NotBeNull();
        payload.User.UserName.Should().Be(registerRequest.UserName);
        payload.User.Email.Should().Be(registerRequest.Email);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnOk_AndToken()
    {
        var registerRequest = BuildRegisterRequest();
        await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        var loginRequest = new LoginDto
        {
            UserName = registerRequest.UserName,
            Password = registerRequest.Password
        };

        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.Content.ReadFromJsonAsync<TokenDto>();
        payload.Should().NotBeNull();
        payload!.AccessToken.Should().NotBeNullOrWhiteSpace();
        payload.User.UserName.Should().Be(registerRequest.UserName);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ShouldReturnBadRequest()
    {
        var registerRequest = BuildRegisterRequest();
        await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        var loginRequest = new LoginDto
        {
            UserName = registerRequest.UserName,
            Password = "PasswordIncorrecta!1"
        };

        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/problem+json");
    }

    [Fact]
    public async Task Profile_WithToken_ShouldReturnOk_AndUserData()
    {
        var registerRequest = BuildRegisterRequest();
        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
        var tokenPayload = await registerResponse.Content.ReadFromJsonAsync<TokenDto>();
        tokenPayload.Should().NotBeNull();

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenPayload!.AccessToken);

        var response = await _client.GetAsync("/api/auth/profile");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var profile = await response.Content.ReadFromJsonAsync<UserDto>();
        profile.Should().NotBeNull();
        profile!.UserName.Should().Be(registerRequest.UserName);
        profile.Email.Should().Be(registerRequest.Email);
    }

    [Fact]
    public async Task Profile_WithoutToken_ShouldReturnUnauthorized()
    {
        _client.DefaultRequestHeaders.Authorization = null;

        var response = await _client.GetAsync("/api/auth/profile");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private static RegisterDto BuildRegisterRequest()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];

        return new RegisterDto
        {
            UserName = $"user_{suffix}",
            Email = $"{suffix}@abac.test",
            Password = "PasswordValida!1",
            ConfirmPassword = "PasswordValida!1",
            FullName = "Usuario Prueba",
            PhoneNumber = "+34123456789",
            Department = "QA"
        };
    }
}
