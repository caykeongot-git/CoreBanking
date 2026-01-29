using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoreBanking.DAL.Entities
{
    public abstract class BaseEntity
    {
        [Key]
        public int Id { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedDate { get; set; }

        // --- ĐÂY LÀ DÒNG QUAN TRỌNG ĐANG BỊ THIẾU ---
        public bool IsDeleted { get; set; } = false;
    }
}