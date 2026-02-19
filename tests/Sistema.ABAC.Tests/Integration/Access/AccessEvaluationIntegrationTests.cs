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
using AttributeEntity = Sistema.ABAC.Domain.Entities.Attribute;

namespace Sistema.ABAC.Tests.Integration.Access;

public class AccessEvaluationIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private static readonly System.Text.Json.JsonSerializerOptions _jsonOptions = new(System.Text.Json.JsonSerializerDefaults.Web)
    {
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };

    public AccessEvaluationIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("https://localhost")
        });
    }

    [Fact]
    public async Task Evaluate_WithoutToken_ShouldReturnUnauthorized()
    {
        var request = new EvaluateAccessRequest
        {
            UserId = Guid.NewGuid(),
            ResourceId = Guid.NewGuid(),
            ActionId = Guid.NewGuid()
        };

        var response = await _client.PostAsJsonAsync("/api/access/evaluate", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Evaluate_WithMatchingPermitPolicy_ShouldReturnPermit()
    {
        var user = await AuthenticateAsync();
        var seeded = await SeedEvaluationDataAsync(user.Id, createPermitPolicy: true);

        var request = new EvaluateAccessRequest
        {
            UserId = user.Id,
            ResourceId = seeded.ResourceId,
            ActionId = seeded.ActionId,
            Context = new Dictionary<string, object?>
            {
                ["ipAddress"] = "127.0.0.1"
            }
        };

        var response = await _client.PostAsJsonAsync("/api/access/evaluate", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<AuthorizationResult>(_jsonOptions);
        result.Should().NotBeNull();
        result!.Decision.Should().Be(AuthorizationDecision.Permit);
        result.Reason.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Evaluate_WithoutApplicablePolicies_ShouldReturnDeny()
    {
        var user = await AuthenticateAsync();
        var seeded = await SeedEvaluationDataAsync(user.Id, createPermitPolicy: false);

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
        result.Reason.Should().NotBeNullOrWhiteSpace();
    }

    private async Task<(Guid ActionId, Guid ResourceId)> SeedEvaluationDataAsync(Guid userId, bool createPermitPolicy)
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AbacDbContext>();

        var resource = new Resource
        {
            Id = Guid.NewGuid(),
            Name = "Recurso ABAC",
            Type = "Document",
            Description = "Recurso para pruebas de evaluación",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var action = new ActionEntity
        {
            Id = Guid.NewGuid(),
            Name = "Read",
            Code = "read",
            Description = "Leer recurso",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        dbContext.Resources.Add(resource);
        dbContext.Actions.Add(action);

        if (createPermitPolicy)
        {
            var departmentAttribute = new AttributeEntity
            {
                Id = Guid.NewGuid(),
                Name = "Departamento",
                Key = "department",
                Type = AttributeType.String,
                Description = "Departamento del usuario",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var userAttribute = new UserAttribute
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                AttributeId = departmentAttribute.Id,
                Value = "IT",
                ValidFrom = DateTime.UtcNow.AddDays(-1),
                ValidTo = DateTime.UtcNow.AddDays(1),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var policy = new Policy
            {
                Id = Guid.NewGuid(),
                Name = "Permitir lectura IT",
                Description = "Permite leer recursos a usuarios de IT",
                Effect = PolicyEffect.Permit,
                Priority = 100,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var policyCondition = new PolicyCondition
            {
                Id = Guid.NewGuid(),
                PolicyId = policy.Id,
                AttributeType = "Subject",
                AttributeKey = "department",
                Operator = OperatorType.Equals,
                ExpectedValue = "IT",
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

            dbContext.Attributes.Add(departmentAttribute);
            dbContext.UserAttributes.Add(userAttribute);
            dbContext.Policies.Add(policy);
            dbContext.PolicyConditions.Add(policyCondition);
            dbContext.PolicyActions.Add(policyAction);
        }

        await dbContext.SaveChangesAsync();

        return (action.Id, resource.Id);
    }

    private async Task<UserDto> AuthenticateAsync()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var registerRequest = new RegisterDto
        {
            UserName = $"access_user_{suffix}",
            Email = $"access_{suffix}@abac.test",
            Password = "PasswordValida!1",
            ConfirmPassword = "PasswordValida!1",
            FullName = "Usuario Access",
            Department = "IT"
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
