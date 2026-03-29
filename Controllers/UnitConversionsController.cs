using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using warehouseManagement.DTOs;
using warehouseManagement.Services;

namespace warehouseManagement.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class UnitConversionsController : Controller
    {
        private readonly IUnitConversionService _service;
        private readonly IMapper _mapper;

        public UnitConversionsController(IUnitConversionService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        [HttpGet("{productId}")]
        public async Task<IActionResult> GetByProduct(int productId)
        {
            var result = await _service.GetByProductAsync(productId);

            var dto = _mapper.Map<IEnumerable<UnitConversionDTO>>(result);

            return Ok(dto);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateUnitConversionDTO dto)
        {
            var result = await _service.CreateAsync(dto);

            var mapped = _mapper.Map<UnitConversionDTO>(result);

            return Ok(mapped);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateUnitConversionDTO dto)
        {
            var result = await _service.UpdateAsync(id, dto);

            var mapped = _mapper.Map<UnitConversionDTO>(result);

            return Ok(mapped);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Deactivate(int id)
        {
            var result = await _service.DeactivateAsync(id);
            if (!result) return NotFound();
            return Ok();
        }
    }
}
