using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Sistema.ABAC.Application.DTOs;
using Sistema.ABAC.Application.DTOs.Auth;
using Sistema.ABAC.Application.Services.ABAC;
using Sistema.ABAC.Domain.Entities;
using Sistema.ABAC.Domain.Enums;
using Sistema.ABAC.Infrastructure.Persistence;
using ActionEntity = Sistema.ABAC.Domain.Entities.Action;

namespace Sistema.ABAC.Tests.Integration.Access;

public class AccessEdgeCasesIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private static readonly System.Text.Json.JsonSerializerOptions _jsonOptions = new(System.Text.Json.JsonSerializerDefaults.Web)
    {
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };

    public AccessEdgeCasesIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("https://localhost")
        });
    }

    [Fact]
    public async Task Evaluate_WhenUserHasNoAttributes_ShouldReturnDeny()
    {
        var user = await AuthenticateAsync();
        var seeded = await SeedPolicyRequiringDepartmentAsync(requiredDepartment: "IT", withPolicyCondition: true);

        var request = new EvaluateAccessRequest
        {
            UserId = user.Id,
            ResourceId = seeded.ResourceId,
            ActionId = seeded.ActionId
        };

        var response = await _client.PostAsJsonAsync("/api/access/evaluate", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<AuthorizationResult>(_jsonOptions);
        result.Should().NotBeNull();
        result!.Decision.Should().Be(AuthorizationDecision.Deny);
    }

    [Fact]
    public async Task Evaluate_WhenPolicyHasNoConditions_ShouldReturnDeny()
    {
        var user = await AuthenticateAsync();
        var seeded = await SeedPolicyRequiringDepartmentAsync(requiredDepartment: "IT", withPolicyCondition: false);

        var request = new EvaluateAccessRequest
        {
            UserId = user.Id,
            ResourceId = seeded.ResourceId,
            ActionId = seeded.ActionId
        };

        var response = await _client.PostAsJsonAsync("/api/access/evaluate", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<AuthorizationResult>(_jsonOptions);
        result.Should().NotBeNull();
        result!.Decision.Should().Be(AuthorizationDecision.Deny);
    }

    [Fact]
    public async Task Evaluate_WhenResourceDoesNotExist_ShouldReturnNotFound()
    {
        var user = await AuthenticateAsync();
        var actionId = await SeedOnlyActionAsync();

        var request = new EvaluateAccessRequest
        {
            UserId = user.Id,
            ResourceId = Guid.NewGuid(),
            ActionId = actionId
        };

        var response = await _client.PostAsJsonAsync("/api/access/evaluate", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/problem+json");
    }

    private async Task<(Guid ActionId, Guid ResourceId)> SeedPolicyRequiringDepartmentAsync(string requiredDepartment, bool withPolicyCondition)
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AbacDbContext>();

        var resource = new Resource
        {
            Id = Guid.NewGuid(),
            Name = "Edge Resource",
            Type = "Document",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var action = new ActionEntity
        {
            Id = Guid.NewGuid(),
            Name = "Read",
            Code = "read",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var policy = new Policy
        {
            Id = Guid.NewGuid(),
            Name = withPolicyCondition ? "Permit with condition" : "Permit without conditions",
            Effect = PolicyEffect.Permit,
            Priority = 100,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var policyAction = new PolicyAction
        {
            Id = Guid.NewGuid(),
            PolicyId = policy.Id,
            ActionId = action.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        dbContext.Resources.Add(resource);
        dbContext.Actions.Add(action);
        dbContext.Policies.Add(policy);
        dbContext.PolicyActions.Add(policyAction);

        if (withPolicyCondition)
        {
            dbContext.PolicyConditions.Add(new PolicyCondition
            {
                Id = Guid.NewGuid(),
                PolicyId = policy.Id,
                AttributeType = "Subject",
                AttributeKey = "department",
                Operator = OperatorType.Equals,
                ExpectedValue = requiredDepartment,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        await dbContext.SaveChangesAsync();
        return (action.Id, resource.Id);
    }

    private async Task<Guid> SeedOnlyActionAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AbacDbContext>();

        var action = new ActionEntity
        {
            Id = Guid.NewGuid(),
            Name = "Read",
            Code = "read",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        dbContext.Actions.Add(action);
        await dbContext.SaveChangesAsync();

        return action.Id;
    }

    private async Task<UserDto> AuthenticateAsync()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var registerRequest = new RegisterDto
        {
            UserName = $"edge_user_{suffix}",
            Email = $"edge_{suffix}@abac.test",
            Password = "PasswordValida!1",
            ConfirmPassword = "PasswordValida!1",
            FullName = "Usuario Edge",
            Department = "Ventas"
        };

        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var token = await registerResponse.Content.ReadFromJsonAsync<TokenDto>();
        token.Should().NotBeNull();
        token!.AccessToken.Should().NotBeNullOrWhiteSpace();

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);

        token.User.Should().NotBeNull();
        return token.User;
    }
}
