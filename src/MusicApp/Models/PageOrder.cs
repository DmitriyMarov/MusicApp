using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MusicApp.Models
{
    /// <summary>
    /// Настройка правил отображения страницы с музыкой
    /// </summary>
    [Table("PageOrder")]
    public class PageOrder
    {
        [Key]
        public int ID { get; set; }

        /// <summary>
        /// ID пользователя
        /// </summary>
        public string UserID { get; set; }

        /// <summary>
        /// Номер страницы в карусели
        /// </summary>
        public int PageNumber { get; set; }

        /// <summary>
        /// Тип контента на странице
        /// </summary>
        public string PageContent { get; set; }

        /// <summary>
        /// Размер контента
        /// (Half - может быть в пол-страницы или во всю, Full - только во всю страницу)
        /// </summary>
        public string ContentSize { get; set; }
    }
}
