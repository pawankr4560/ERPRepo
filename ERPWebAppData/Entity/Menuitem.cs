using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.Data.Entity
{
    public class MenuItems
    {
        [Key]
        [Column("F_menu_Index")]
        public int Id { get; set; }

        [Column("F_Parent_menu_index")]
        public int? ParentId { get; set; }

        [Required]
        [Column("T_Title")]
        public string Title { get; set; }

        [Column("F_route")]
        public string Route { get; set; }

        [Column("F_Icon_class")]
        public string IconClass { get; set; }

        [Column("F_order_number")]
        public int OrderNumber { get; set; }

        [Column("F_Active")]
        public bool IsActive { get; set; }
    }
}
