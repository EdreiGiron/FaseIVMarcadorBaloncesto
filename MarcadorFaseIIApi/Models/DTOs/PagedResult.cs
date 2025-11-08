using System.Collections.Generic;

namespace MarcadorFaseIIApi.Models.DTOs
{
    public class PagedResult<T>
    {
        public IEnumerable<T> Items { get; set; } = new List<T>();
        public int Total { get; set; }
    }
}
