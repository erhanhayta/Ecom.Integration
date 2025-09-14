namespace Ecom.Contracts.Marketplaces
{
    public record BrandDto(int Id, string Name);
    public record CategoryDto(int Id, string Name, int? ParentId, bool Leaf);
    public record CategoryAttributeValueDto(int Id, string Name);
    public record CategoryAttributeDto(
        int AttributeId, string Name,
        bool Required, bool AllowCustom,
        bool Varianter, bool Slicer,
        IReadOnlyList<CategoryAttributeValueDto> Values
    );
}
