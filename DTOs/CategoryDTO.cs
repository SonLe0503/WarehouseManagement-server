namespace warehouseManagement.DTOs
{

    public class CategoryDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? ParentId { get; set; }

        public List<CategoryDTO> Children { get; set; } = new();
    }

    public class CategoryDetailDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? ParentId { get; set; }
        public string ParentName { get; set; }
        public List<CategoryDTO> Children { get; set; }
    }


    public class CreateCategoryDTO
    {
        public string Name { get; set; }
        public int? ParentId { get; set; }
    }
    public class UpdateCategoryDTO
    {
        public string Name { get; set; }
        public int? ParentId { get; set; }
    }

}
