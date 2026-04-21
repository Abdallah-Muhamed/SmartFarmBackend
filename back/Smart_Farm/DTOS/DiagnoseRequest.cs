using Microsoft.AspNetCore.Http;

namespace Smart_Farm.DTOS
{
    public class DiagnoseRequest
    {
        public IFormFile? Image { get; set; }
    }
}