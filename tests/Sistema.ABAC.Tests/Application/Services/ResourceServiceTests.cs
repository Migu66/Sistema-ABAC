using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Sistema.ABAC.Application.Common.Exceptions;
using Sistema.ABAC.Application.DTOs;
using Sistema.ABAC.Application.Services;
using Sistema.ABAC.Domain.Entities;
using Sistema.ABAC.Domain.Interfaces;
using AutoMapper;
using Sistema.ABAC.Application.Mappings;
using AttributeEntity = Sistema.ABAC.Domain.Entities.Attribute;

namespace Sistema.ABAC.Tests.Application.Services;

public class ResourceServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IResourceRepository> _resourceRepoMock;
    private readonly Mock<IAttributeRepository> _attributeRepoMock;
    private readonly Mock<IRepository<ResourceAttribute>> _resourceAttrRepoMock;
    private readonly IMapper _mapper;
    private readonly ResourceService _sut;

    public ResourceServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _resourceRepoMock = new Mock<IResourceRepository>();
        _attributeRepoMock = new Mock<IAttributeRepository>();
        _resourceAttrRepoMock = new Mock<IRepository<ResourceAttribute>>();

        _unitOfWorkMock.SetupGet(u => u.Resources).Returns(_resourceRepoMock.Object);
        _unitOfWorkMock.SetupGet(u => u.Attributes).Returns(_attributeRepoMock.Object);
        _unitOfWorkMock.SetupGet(u => u.ResourceAttributes).Returns(_resourceAttrRepoMock.Object);

        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = config.CreateMapper();

        _sut = new ResourceService(
            _unitOfWorkMock.Object,
            _mapper,
            NullLogger<ResourceService>.Instance);
    }

    #region GetByIdAsync

    [Fact]
    public async Task GetByIdAsync_WhenResourceExists_ReturnsDto()
    {
        var id = Guid.NewGuid();
        var resource = new Resource { Id = id, Name = "Server", Type = "Infrastructure" };

        _resourceRepoMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(resource);

        var result = await _sut.GetByIdAsync(id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(id);
        result.Name.Should().Be("Server");
    }

    [Fact]
    public async Task GetByIdAsync_WithAttributes_UsesGetWithAttributesAsync()
    {
        var id = Guid.NewGuid();
        var resource = new Resource
        {
            Id = id,
            Name = "Server",
            Type = "Infra",
            ResourceAttributes = new List<ResourceAttribute>()
        };

        _resourceRepoMock.Setup(r => r.GetWithAttributesAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(resource);

        var result = await _sut.GetByIdAsync(id, includeAttributes: true);

        result.Should().NotBeNull();
        _resourceRepoMock.Verify(r => r.GetWithAttributesAsync(id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ReturnsNull()
    {
        _resourceRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Resource?)null);

        var result = await _sut.GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    #endregion

    #region GetAllAsync

    [Fact]
    public async Task GetAllAsync_ReturnsPagedResult()
    {
        var resources = new List<Resource>
        {
            new() { Id = Guid.NewGuid(), Name = "Alpha", Type = "API", CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Name = "Beta", Type = "DB", CreatedAt = DateTime.UtcNow }
        };

        _resourceRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(resources);

        var result = await _sut.GetAllAsync(page: 1, pageSize: 10);

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task GetAllAsync_WithSearchTerm_FiltersResults()
    {
        var resources = new List<Resource>
        {
            new() { Id = Guid.NewGuid(), Name = "Server A", Type = "Infra", CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Name = "Database B", Type = "DB", CreatedAt = DateTime.UtcNow }
        };

        _resourceRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(resources);

        var result = await _sut.GetAllAsync(searchTerm: "Server");

        result.Items.Should().HaveCount(1);
        result.Items.First().Name.Should().Be("Server A");
    }

    [Fact]
    public async Task GetAllAsync_WithTypeFilter_FiltersResults()
    {
        var resources = new List<Resource>
        {
            new() { Id = Guid.NewGuid(), Name = "A", Type = "API", CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Name = "B", Type = "DB", CreatedAt = DateTime.UtcNow }
        };

        _resourceRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(resources);

        var result = await _sut.GetAllAsync(type: "API");

        result.Items.Should().HaveCount(1);
        result.Items.First().Type.Should().Be("API");
    }

    [Fact]
    public async Task GetAllAsync_SortByType_ReturnsSorted()
    {
        var resources = new List<Resource>
        {
            new() { Id = Guid.NewGuid(), Name = "B", Type = "ZZZ", CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Name = "A", Type = "AAA", CreatedAt = DateTime.UtcNow }
        };

        _resourceRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(resources);

        var result = await _sut.GetAllAsync(sortBy: "type");

        result.Items.First().Type.Should().Be("AAA");
    }

    [Fact]
    public async Task GetAllAsync_SortByCreatedAt_ReturnsSorted()
    {
        var older = DateTime.UtcNow.AddDays(-1);
        var newer = DateTime.UtcNow;
        var resources = new List<Resource>
        {
            new() { Id = Guid.NewGuid(), Name = "New", Type = "A", CreatedAt = newer },
            new() { Id = Guid.NewGuid(), Name = "Old", Type = "B", CreatedAt = older }
        };

        _resourceRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(resources);

        var result = await _sut.GetAllAsync(sortBy: "createdat");

        result.Items.First().Name.Should().Be("Old");
    }

    [Fact]
    public async Task GetAllAsync_SortDescending_ReturnsReversed()
    {
        var resources = new List<Resource>
        {
            new() { Id = Guid.NewGuid(), Name = "Alpha", Type = "A", CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Name = "Zeta", Type = "Z", CreatedAt = DateTime.UtcNow }
        };

        _resourceRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(resources);

        var result = await _sut.GetAllAsync(sortDescending: true);

        result.Items.First().Name.Should().Be("Zeta");
    }

    [Fact]
    public async Task GetAllAsync_PaginationNormalization_ClampsValues()
    {
        var resources = Enumerable.Range(1, 5)
            .Select(i => new Resource { Id = Guid.NewGuid(), Name = $"R{i}", Type = "T", CreatedAt = DateTime.UtcNow })
            .ToList();

        _resourceRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(resources);

        var result = await _sut.GetAllAsync(page: -1, pageSize: 0);

        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task GetAllAsync_PageSizeOver100_ClampedTo100()
    {
        var resources = new List<Resource>();
        _resourceRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(resources);

        var result = await _sut.GetAllAsync(pageSize: 150);

        result.PageSize.Should().Be(100);
    }

    [Fact]
    public async Task GetAllAsync_SearchByDescription_FiltersCorrectly()
    {
        var resources = new List<Resource>
        {
            new() { Id = Guid.NewGuid(), Name = "A", Type = "T", Description = "Important server", CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Name = "B", Type = "T", Description = null, CreatedAt = DateTime.UtcNow }
        };

        _resourceRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(resources);

        var result = await _sut.GetAllAsync(searchTerm: "Important");

        result.Items.Should().HaveCount(1);
    }

    #endregion

    #region CreateAsync

    [Fact]
    public async Task CreateAsync_ValidData_ReturnsCreatedResource()
    {
        var dto = new CreateResourceDto { Name = "New Server", Type = "Infra", Description = "desc" };

        _resourceRepoMock
            .Setup(r => r.AddAsync(It.IsAny<Resource>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Resource r, CancellationToken _) => r);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await _sut.CreateAsync(dto);

        result.Should().NotBeNull();
        result.Name.Should().Be("New Server");
        result.Type.Should().Be("Infra");
    }

    #endregion

    #region UpdateAsync

    [Fact]
    public async Task UpdateAsync_WhenExists_UpdatesAndReturns()
    {
        var id = Guid.NewGuid();
        var existing = new Resource { Id = id, Name = "Old", Type = "OldType" };
        var dto = new UpdateResourceDto { Name = "New", Type = "NewType", Description = "desc" };

        _resourceRepoMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var result = await _sut.UpdateAsync(id, dto);

        result.Name.Should().Be("New");
        result.Type.Should().Be("NewType");
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenNotFound_ThrowsNotFoundException()
    {
        _resourceRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Resource?)null);

        var act = async () => await _sut.UpdateAsync(Guid.NewGuid(), new UpdateResourceDto { Name = "x", Type = "y" });

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task UpdateAsync_EmptyDescription_SetsToNull()
    {
        var id = Guid.NewGuid();
        var existing = new Resource { Id = id, Name = "R", Type = "T", Description = "old desc" };
        var dto = new UpdateResourceDto { Name = "R", Type = "T", Description = "  " };

        _resourceRepoMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var result = await _sut.UpdateAsync(id, dto);

        existing.Description.Should().BeNull();
    }

    #endregion

    #region DeleteAsync

    [Fact]
    public async Task DeleteAsync_WhenExists_SoftDeletesAndReturnsTrue()
    {
        var id = Guid.NewGuid();
        var resource = new Resource { Id = id, Name = "R", Type = "T" };
        var resourceWithAttrs = new Resource { Id = id, Name = "R", Type = "T", ResourceAttributes = new List<ResourceAttribute>() };

        _resourceRepoMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(resource);
        _resourceRepoMock.Setup(r => r.GetWithAttributesAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(resourceWithAttrs);

        var result = await _sut.DeleteAsync(id);

        result.Should().BeTrue();
        resource.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_WhenNotFound_ThrowsNotFoundException()
    {
        _resourceRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Resource?)null);

        var act = async () => await _sut.DeleteAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task DeleteAsync_WithAttributes_StillDeletes()
    {
        var id = Guid.NewGuid();
        var resource = new Resource { Id = id, Name = "R", Type = "T" };
        var resourceWithAttrs = new Resource
        {
            Id = id, Name = "R", Type = "T",
            ResourceAttributes = new List<ResourceAttribute>
            {
                new() { Id = Guid.NewGuid(), ResourceId = id, AttributeId = Guid.NewGuid(), Value = "v" }
            }
        };

        _resourceRepoMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(resource);
        _resourceRepoMock.Setup(r => r.GetWithAttributesAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(resourceWithAttrs);

        var result = await _sut.DeleteAsync(id);

        result.Should().BeTrue();
    }

    #endregion

    #region GetAttributesAsync

    [Fact]
    public async Task GetAttributesAsync_WhenResourceExists_ReturnsAttributes()
    {
        var id = Guid.NewGuid();
        var resource = new Resource
        {
            Id = id, Name = "R", Type = "T",
            ResourceAttributes = new List<ResourceAttribute>
            {
                new() { Id = Guid.NewGuid(), ResourceId = id, AttributeId = Guid.NewGuid(), Value = "v1" }
            }
        };

        _resourceRepoMock.Setup(r => r.GetWithAttributesAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(resource);

        var result = await _sut.GetAttributesAsync(id);

        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetAttributesAsync_WhenNotFound_ThrowsNotFoundException()
    {
        _resourceRepoMock.Setup(r => r.GetWithAttributesAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Resource?)null);

        var act = async () => await _sut.GetAttributesAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region AssignAttributeAsync

    [Fact]
    public async Task AssignAttributeAsync_ValidData_ReturnsAssignedAttribute()
    {
        var resourceId = Guid.NewGuid();
        var attributeId = Guid.NewGuid();
        var dto = new AssignResourceAttributeDto { AttributeId = attributeId, Value = "val" };

        _resourceRepoMock.Setup(r => r.GetByIdAsync(resourceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Resource { Id = resourceId, Name = "R", Type = "T" });
        _attributeRepoMock.Setup(r => r.GetByIdAsync(attributeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AttributeEntity { Id = attributeId, Name = "A", Key = "a" });

        ResourceAttribute? capturedEntity = null;
        _resourceAttrRepoMock.Setup(r => r.AddAsync(It.IsAny<ResourceAttribute>(), It.IsAny<CancellationToken>()))
            .Callback<ResourceAttribute, CancellationToken>((ra, _) => capturedEntity = ra)
            .ReturnsAsync((ResourceAttribute ra, CancellationToken _) => ra);

        // First call: no existing assignment. Second call: return the captured entity
        var callCount = 0;
        _resourceAttrRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() =>
            {
                callCount++;
                if (callCount == 1)
                    return new List<ResourceAttribute>();
                return new List<ResourceAttribute> { capturedEntity! };
            });

        var result = await _sut.AssignAttributeAsync(resourceId, dto);

        result.Should().NotBeNull();
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AssignAttributeAsync_ResourceNotFound_ThrowsNotFoundException()
    {
        _resourceRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Resource?)null);

        var act = async () => await _sut.AssignAttributeAsync(Guid.NewGuid(),
            new AssignResourceAttributeDto { AttributeId = Guid.NewGuid(), Value = "v" });

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task AssignAttributeAsync_AttributeNotFound_ThrowsNotFoundException()
    {
        var resourceId = Guid.NewGuid();
        _resourceRepoMock.Setup(r => r.GetByIdAsync(resourceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Resource { Id = resourceId, Name = "R", Type = "T" });
        _attributeRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AttributeEntity?)null);

        var act = async () => await _sut.AssignAttributeAsync(resourceId,
            new AssignResourceAttributeDto { AttributeId = Guid.NewGuid(), Value = "v" });

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task AssignAttributeAsync_AlreadyAssigned_ThrowsValidationException()
    {
        var resourceId = Guid.NewGuid();
        var attributeId = Guid.NewGuid();

        _resourceRepoMock.Setup(r => r.GetByIdAsync(resourceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Resource { Id = resourceId, Name = "R", Type = "T" });
        _attributeRepoMock.Setup(r => r.GetByIdAsync(attributeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AttributeEntity { Id = attributeId, Name = "A", Key = "a" });
        _resourceAttrRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ResourceAttribute>
            {
                new() { ResourceId = resourceId, AttributeId = attributeId, IsDeleted = false }
            });

        var act = async () => await _sut.AssignAttributeAsync(resourceId,
            new AssignResourceAttributeDto { AttributeId = attributeId, Value = "v" });

        await act.Should().ThrowAsync<ValidationException>();
    }

    #endregion

    #region UpdateAttributeAsync

    [Fact]
    public async Task UpdateAttributeAsync_WhenExists_UpdatesValue()
    {
        var resourceId = Guid.NewGuid();
        var attributeId = Guid.NewGuid();
        var existing = new ResourceAttribute
        {
            Id = Guid.NewGuid(), ResourceId = resourceId, AttributeId = attributeId,
            Value = "old", IsDeleted = false
        };

        _resourceAttrRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ResourceAttribute> { existing });

        var result = await _sut.UpdateAttributeAsync(resourceId, attributeId, "new");

        result.Should().NotBeNull();
        existing.Value.Should().Be("new");
    }

    [Fact]
    public async Task UpdateAttributeAsync_WhenNotFound_ThrowsNotFoundException()
    {
        _resourceAttrRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ResourceAttribute>());

        var act = async () => await _sut.UpdateAttributeAsync(Guid.NewGuid(), Guid.NewGuid(), "v");

        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region RemoveAttributeAsync

    [Fact]
    public async Task RemoveAttributeAsync_WhenExists_SoftDeletesAndReturnsTrue()
    {
        var resourceId = Guid.NewGuid();
        var attributeId = Guid.NewGuid();
        var existing = new ResourceAttribute
        {
            Id = Guid.NewGuid(), ResourceId = resourceId, AttributeId = attributeId,
            Value = "v", IsDeleted = false
        };

        _resourceAttrRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ResourceAttribute> { existing });

        var result = await _sut.RemoveAttributeAsync(resourceId, attributeId);

        result.Should().BeTrue();
        existing.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task RemoveAttributeAsync_WhenNotFound_ThrowsNotFoundException()
    {
        _resourceAttrRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ResourceAttribute>());

        var act = async () => await _sut.RemoveAttributeAsync(Guid.NewGuid(), Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion
}
