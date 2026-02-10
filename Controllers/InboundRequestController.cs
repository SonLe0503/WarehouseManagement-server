using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using warehouseManagement.DTOs;
using warehouseManagement.Models;

namespace warehouseManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InboundRequestController : Controller
    {
        private readonly WmsContext _context;
        private readonly IMapper _mapper;

        public InboundRequestController(WmsContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<InboundRequestDTO>>> GetInboundRequests( )
        {
            var requests = await _context.InboundRequests
                .Include(r => r.InboundItems)
                .ThenInclude(i => i.Product)
                .Include(r => r.CreatedByNavigation)
                .Include(r => r.ApprovedByNavigation)
                .Include(r => r.Warehouse)
                .ToListAsync();

            var requestDtos = _mapper.Map<List<InboundRequestDTO>>(requests);
            return Ok(requestDtos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<InboundRequestDTO>> GetInboundRequestDetails(int id)
        {
            var request = await _context.InboundRequests
                .Include(r => r.InboundItems)
                .ThenInclude(i => i.Product)
                .Include(r => r.CreatedByNavigation)
                .Include(r => r.ApprovedByNavigation)
                .Include(r => r.Warehouse)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null)
            {
                return NotFound();
            }

            var requestDto = _mapper.Map<InboundRequestDTO>(request);
            return Ok(requestDto);
        }
    }
}
 