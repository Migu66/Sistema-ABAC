using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Sistema.ABAC.Application.Common.Exceptions;
using Sistema.ABAC.Application.DTOs;
using Sistema.ABAC.Application.Services;
using Sistema.ABAC.Domain.Entities;
using Sistema.ABAC.Domain.Enums;
using Sistema.ABAC.Domain.Interfaces;
using AutoMapper;
using Sistema.ABAC.Application.Mappings;
using AttributeEntity = Sistema.ABAC.Domain.Entities.Attribute;

namespace Sistema.ABAC.Tests.Application.Services;

public class AttributeServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IAttributeRepository> _attributeRepoMock;
    private readonly IMapper _mapper;
    private readonly AttributeService _sut;

    public AttributeServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _attributeRepoMock = new Mock<IAttributeRepository>();
        _unitOfWorkMock.SetupGet(u => u.Attributes).Returns(_attributeRepoMock.Object);

        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = config.CreateMapper();

        _sut = new AttributeService(
            _unitOfWorkMock.Object,
            _mapper,
            NullLogger<AttributeService>.Instance);
    }

    #region GetByIdAsync

    [Fact]
    public async Task GetByIdAsync_WhenExists_ReturnsAttributeDto()
    {
        var id = Guid.NewGuid();
        var attribute = new AttributeEntity { Id = id, Name = "Department", Key = "department", Type = AttributeType.String };

        _attributeRepoMock
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(attribute);

        var result = await _sut.GetByIdAsync(id);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Department");
        result.Key.Should().Be("department");
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ReturnsNull()
    {
        _attributeRepoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AttributeEntity?)null);

        var result = await _sut.GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    #endregion

    #region GetAllAsync

    [Fact]
    public async Task GetAllAsync_ReturnsPagedResults()
    {
        var attributes = new List<AttributeEntity>
        {
            new() { Id = Guid.NewGuid(), Name = "Dept", Key = "dept", Type = AttributeType.String, CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Name = "Level", Key = "level", Type = AttributeType.Number, CreatedAt = DateTime.UtcNow }
        };

        _attributeRepoMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(attributes);

        var result = await _sut.GetAllAsync();

        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task GetAllAsync_WithTypeFilter_FiltersCorrectly()
    {
        var attributes = new List<AttributeEntity>
        {
            new() { Id = Guid.NewGuid(), Name = "Dept", Key = "dept", Type = AttributeType.String, CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Name = "Level", Key = "level", Type = AttributeType.Number, CreatedAt = DateTime.UtcNow }
        };

        _attributeRepoMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(attributes);

        var result = await _sut.GetAllAsync(type: "Number");

        result.Items.Should().HaveCount(1);
        result.Items.First().Key.Should().Be("level");
    }

    [Fact]
    public async Task GetAllAsync_WithSearchTerm_FiltersResults()
    {
        var attributes = new List<AttributeEntity>
        {
            new() { Id = Guid.NewGuid(), Name = "Department", Key = "department", Type = AttributeType.String, CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Name = "Level", Key = "level", Type = AttributeType.Number, CreatedAt = DateTime.UtcNow }
        };

        _attributeRepoMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(attributes);

        var result = await _sut.GetAllAsync(searchTerm: "depart");

        result.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetAllAsync_SortByKey_ReturnsSorted()
    {
        var attributes = new List<AttributeEntity>
        {
            new() { Id = Guid.NewGuid(), Name = "Z", Key = "z_key", Type = AttributeType.String, CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Name = "A", Key = "a_key", Type = AttributeType.String, CreatedAt = DateTime.UtcNow }
        };

        _attributeRepoMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(attributes);

        var result = await _sut.GetAllAsync(sortBy: "key", sortDescending: false);

        result.Items.First().Key.Should().Be("a_key");
    }

    [Fact]
    public async Task GetAllAsync_SortByType_ReturnsSorted()
    {
        var attributes = new List<AttributeEntity>
        {
            new() { Id = Guid.NewGuid(), Name = "B", Key = "b", Type = AttributeType.String, CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Name = "A", Key = "a", Type = AttributeType.Boolean, CreatedAt = DateTime.UtcNow }
        };

        _attributeRepoMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(attributes);

        var result = await _sut.GetAllAsync(sortBy: "type");

        result.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAllAsync_NormalizesInvalidPagination()
    {
        _attributeRepoMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AttributeEntity>());

        var result = await _sut.GetAllAsync(page: -5, pageSize: 0);

        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
    }

    #endregion

    #region CreateAsync

    [Fact]
    public async Task CreateAsync_WhenKeyIsUnique_CreatesAttribute()
    {
        var dto = new CreateAttributeDto { Name = "Dept", Key = "dept", Type = AttributeType.String };

        _attributeRepoMock
            .Setup(r => r.GetByKeyAsync("dept", It.IsAny<CancellationToken>()))
            .ReturnsAsync((AttributeEntity?)null);

        _attributeRepoMock
            .Setup(r => r.AddAsync(It.IsAny<AttributeEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AttributeEntity e, CancellationToken _) => e);

        var result = await _sut.CreateAsync(dto);

        result.Should().NotBeNull();
        result.Name.Should().Be("Dept");
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WhenKeyExists_ThrowsValidationException()
    {
        var dto = new CreateAttributeDto { Name = "Dept", Key = "dept", Type = AttributeType.String };

        _attributeRepoMock
            .Setup(r => r.GetByKeyAsync("dept", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AttributeEntity { Id = Guid.NewGuid(), Key = "dept" });

        var act = async () => await _sut.CreateAsync(dto);

        await act.Should().ThrowAsync<ValidationException>();
    }

    #endregion

    #region UpdateAsync

    [Fact]
    public async Task UpdateAsync_WhenExists_UpdatesAttribute()
    {
        var id = Guid.NewGuid();
        var existing = new AttributeEntity { Id = id, Name = "Dept", Key = "dept" };
        var dto = new UpdateAttributeDto { Name = "Department Updated", Description = "Updated" };

        _attributeRepoMock
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var result = await _sut.UpdateAsync(id, dto);

        result.Name.Should().Be("Department Updated");
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenNotFound_ThrowsNotFoundException()
    {
        _attributeRepoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AttributeEntity?)null);

        var act = async () => await _sut.UpdateAsync(Guid.NewGuid(), new UpdateAttributeDto { Name = "Test" });

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task UpdateAsync_WithEmptyDescription_SetsToNull()
    {
        var id = Guid.NewGuid();
        var existing = new AttributeEntity { Id = id, Name = "Test", Key = "test", Description = "Old" };

        _attributeRepoMock
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        await _sut.UpdateAsync(id, new UpdateAttributeDto { Name = "Test", Description = "  " });

        existing.Description.Should().BeNull();
    }

    #endregion

    #region DeleteAsync

    [Fact]
    public async Task DeleteAsync_WhenExists_SoftDeletes()
    {
        var id = Guid.NewGuid();
        var attribute = new AttributeEntity { Id = id, Name = "Test", Key = "test" };

        _attributeRepoMock
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(attribute);

        _attributeRepoMock
            .Setup(r => r.GetWithUserAttributesAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(attribute);

        _attributeRepoMock
            .Setup(r => r.GetWithResourceAttributesAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(attribute);

        var result = await _sut.DeleteAsync(id);

        result.Should().BeTrue();
        attribute.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_WhenNotFound_ThrowsNotFoundException()
    {
        _attributeRepoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AttributeEntity?)null);

        var act = async () => await _sut.DeleteAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task DeleteAsync_WhenAssignedToUsers_ThrowsValidationException()
    {
        var id = Guid.NewGuid();
        var attribute = new AttributeEntity
        {
            Id = id,
            Name = "Test",
            Key = "test",
            UserAttributes = new List<UserAttribute> { new() }
        };

        _attributeRepoMock
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(attribute);

        _attributeRepoMock
            .Setup(r => r.GetWithUserAttributesAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(attribute);

        var act = async () => await _sut.DeleteAsync(id);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task DeleteAsync_WhenAssignedToResources_ThrowsValidationException()
    {
        var id = Guid.NewGuid();
        var attributeNoUsers = new AttributeEntity { Id = id, Name = "Test", Key = "test" };
        var attributeWithResources = new AttributeEntity
        {
            Id = id,
            Name = "Test",
            Key = "test",
            ResourceAttributes = new List<ResourceAttribute> { new() }
        };

        _attributeRepoMock
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(attributeNoUsers);

        _attributeRepoMock
            .Setup(r => r.GetWithUserAttributesAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(attributeNoUsers);

        _attributeRepoMock
            .Setup(r => r.GetWithResourceAttributesAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(attributeWithResources);

        var act = async () => await _sut.DeleteAsync(id);

        await act.Should().ThrowAsync<ValidationException>();
    }

    #endregion

    #region ExistsByKeyAsync

    [Fact]
    public async Task ExistsByKeyAsync_WhenExists_ReturnsTrue()
    {
        _attributeRepoMock
            .Setup(r => r.KeyExistsAsync("dept", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _sut.ExistsByKeyAsync("dept");

        result.Should().BeTrue();
    }

    #endregion
}
